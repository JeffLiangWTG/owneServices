using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ToBillOfLadingImporterTest : FlatFileDataImporterTestCase
	{
		public void TestEndToEnd()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "AAAAAAAA";
			chargeCode1.AC_GC = GlbCompany.CurrentCompany.PK;

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "CCCCCCCC";
			chargeCode2.AC_GC = GlbCompany.CurrentCompany.PK;

			var orgPatternMatchOverride = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			orgPatternMatchOverride.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			orgPatternMatchOverride.OO_ForeignCode = "BBBBBBBB";
			orgPatternMatchOverride.OO_LocalCode = "CCCCCCCC";

			var currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = "WWW";

			var exchangeRate = currency.ExchangeRates.AddNew();
			exchangeRate.RE_SellRate = 0.5;
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;

			var importer = new ToBillOfLadingImporter();

			using (TextReader reader = File.OpenText(PathToTestFile))
			{
				var notifications = new NotificationBuffer();
				AssertNull("precondition: bill of lading DLCBNE01P5BB01A doesn't exist", Factory.LoadTop1<BillOfLading>(new ZQuery(JobShipmentSchema.JS_HouseBill, "DLCBNE01P5BB01A")));
				AssertNull("precondition: bill of lading NKGBNE01P5FF105A doesn't exist", Factory.LoadTop1<BillOfLading>(new ZQuery(JobShipmentSchema.JS_HouseBill, "NKGBNE01P5FF105A")));

				var refContainer1 = Factory.New<RefContainer>();
				refContainer1.RC_IsIso = false;
				refContainer1.RC_Code = "20DH";

				var refContainer2 = Factory.New<RefContainer>();
				refContainer2.RC_IsIso = false;
				refContainer2.RC_Code = "20DC";

				Factory.Save();

				var importResult = importer.ImportData(reader, "ImportIMM2.txt", notifications, SourceInfo.EmptySourceInfo);

				var importFailureString = string.Format(
					/* Message: */ "Errors encountered importing 'ImportIMM2.txt'\r\n" +
					"proxy org: {0}\r\n" +
					"mapping org: {1}\r\n" +
					"system mapping org: {2}\r\n" +
					"import failed:\r\n" +
					"{3}",
					GlbCompany.CurrentCompany.GC_OH_OrgProxy,
					new ValueObjectImportContext(Factory, notifications).Converter.MappingOrgPK,
					StringToBusinessObjectFieldConverter.InstanceForCurrentCompany.MappingOrgPK,
					notifications.AsString
					);

				Assert(importFailureString, importResult);
				{
					var bill = Factory.LoadTop1<BillOfLading>(new ZQuery(JobShipmentSchema.JS_HouseBill, "DLCBNE01P5BB01A"));

					AssertEquals("AUBNE", bill.JS_RL_NKDestination);
					AssertEquals("AUBNE", bill.JS_NKDischargePort);
					AssertEquals("CNDLC", bill.JS_RL_NKOrigin);
					AssertEquals("CNDLC", bill.JS_NKLoadPort);
					AssertEquals(Core.Constants.ContainerModes.BreakBulk, bill.JS_PackingMode);
					AssertEquals(318, bill.JS_OuterPacks);
					AssertEquals(Core.Constants.PkgUnit.Package, bill.JS_F3_NKPackType);
					AssertEquals(752015m, bill.JS_ActualWeight);
					AssertEquals(4449.04m, bill.JS_ActualVolume);

					AssertEquals(1, bill.TopLevelPacks.Count);
				}

				{
					var bill = Factory.LoadTop1<BillOfLading>(new ZQuery(JobShipmentSchema.JS_HouseBill, "NKGBNE01P5FF105A"));
					AssertNotNull("found bill of lading NKGBNE01P5FF105A after import", bill);

					AssertEquals("AUBNE", bill.JS_RL_NKDestination);
					AssertEquals("AUBNE", bill.JS_NKDischargePort);
					AssertEquals("CNNKG", bill.JS_RL_NKOrigin);
					AssertEquals("CNSHA", bill.JS_NKLoadPort);
					AssertEquals(Core.Constants.ContainerModes.FCL, bill.JS_PackingMode);
					AssertEquals(20, bill.JS_OuterPacks);
					AssertEquals(Core.Constants.PkgUnit.Bag, bill.JS_F3_NKPackType);
					AssertEquals(19530m, bill.JS_ActualWeight);
					AssertEquals(15m, bill.JS_ActualVolume);

					AssertEquals(1, bill.RealContainers.Count);
					var container = bill.RealContainers[0];
					AssertEquals(refContainer1, container.RefContainer);
					AssertEquals("GESU2856217", container.JC_ContainerNum);
					AssertEquals(false, container.JC_IsEmptyContainer);
					AssertEquals(false, container.JC_IsShipperOwned);
					AssertEquals(19530m, container.JC_GrossWeight);
					AssertEquals(2750m, container.JC_TareWeight);
				}
			}
		}

		public void TestImportingTwiceMatchesObjects()
		{
			var importer = new ToBillOfLadingImporter();

			var countBeforeImport = GetBillOfLadingsCount();
			var numberOfQueries = 0;

			var notifications = new NotificationBufferForTest((IQueryUserEventArgs e) =>
			{
				numberOfQueries++;

				var yesNoArgs = (QueryUserYesNoYesAllNoAllEventArgs)e;
				AssertEquals("Query message", "An existing Bill of Lading (V00001000, DLCBNE01P5BB01A) has been found, do you wish to update it?", yesNoArgs.Message);
				AssertEquals("Default response", true, yesNoArgs.Response);
			});

			ImportBillOfLading(notifications);
			ImportBillOfLading(notifications);

			var countAfterImport = GetBillOfLadingsCount();
			AssertEquals("Only one record was created", countBeforeImport + 1, countAfterImport);
			AssertEquals("We got one", 1, numberOfQueries);
		}

		public void TestFieldIsUpdated()
		{
			ImportBillOfLading();

			var bill = LoadImportedBillOfLading();
			AssertEquals(1, bill.TopLevelPacks.Count);
			AssertEquals((short)318, bill.TopLevelPacks[0].JC_ContainerCount);
			bill.TopLevelPacks[0].JC_ContainerCount = 13;

			ImportBillOfLading((IQueryUserEventArgs eventArgs) =>
			{
				var yesNoArgs = (QueryUserYesNoYesAllNoAllEventArgs)eventArgs;
				yesNoArgs.Response = true;
			});

			AssertEquals(1, bill.TopLevelPacks.Count);
			AssertEquals((short)318, bill.TopLevelPacks[0].JC_ContainerCount);
		}

		public void TestFieldIsNotUpdatedWhenAnsweringFalse()
		{
			ImportBillOfLading();

			var bill = LoadImportedBillOfLading();
			AssertEquals(1, bill.TopLevelPacks.Count);
			AssertEquals((short)318, bill.TopLevelPacks[0].JC_ContainerCount);
			bill.TopLevelPacks[0].JC_ContainerCount = 13;

			ImportBillOfLading((IQueryUserEventArgs eventArgs) =>
			{
				var yesNoArgs = (QueryUserYesNoYesAllNoAllEventArgs)eventArgs;
				yesNoArgs.Response = false;
			});

			AssertEquals(1, bill.TopLevelPacks.Count);
			AssertEquals((short)13, bill.TopLevelPacks[0].JC_ContainerCount);
		}

		protected override string PathToTestFile => embeddedResourceRetriever.SaveResourceToFile("Enterprise.Customs.AU.Declaration.Business.Testing.Data.Import.TestFiles.ImportIMM2.TXT");

		protected override FlatFileDataImporter GetDataImporter() => new ToBillOfLadingImporter();

		protected override void SetUp()
		{
			base.SetUp();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever.Dispose();
		}
		EmbeddedResourceRetriever embeddedResourceRetriever;

		void ImportBillOfLading(NotificationBuffer notifications)
		{
			var importer = new ToBillOfLadingImporter();
			using (var reader = File.OpenText(embeddedResourceRetriever.SaveResourceToFile("Enterprise.Customs.AU.Declaration.Business.Testing.Data.Import.TestFiles.OneBillShipnet.txt")))
			{
				Assert(importer.ImportData(reader, "OneBillShipnet.txt", notifications, SourceInfo.EmptySourceInfo));
			}
		}

		void ImportBillOfLading(NotificationBufferForTest.QueryUserDelegate queryDelegate)
		{
			var notifications = new NotificationBufferForTest(queryDelegate);
			ImportBillOfLading(notifications);
		}

		void ImportBillOfLading()
		{
			var notifications = new NotificationBuffer();
			ImportBillOfLading(notifications);
		}

		BillOfLading LoadImportedBillOfLading() => Factory.LoadTop1<BillOfLading>(new ZQuery(JobShipmentSchema.JS_HouseBill, "DLCBNE01P5BB01A"));

		int GetBillOfLadingsCount() => Factory.GetDatabaseCount(typeof(BillOfLading));

		sealed class NotificationBufferForTest : NotificationBuffer
		{
			public NotificationBufferForTest(QueryUserDelegate queryUser)
				: base()
			{
				this.queryUser = queryUser;
			}

			public delegate void QueryUserDelegate(IQueryUserEventArgs e);

			protected override void QueryUser(IQueryUserEventArgs e)
			{
				queryUser(e);
			}

			readonly QueryUserDelegate queryUser;
		}
	}
}
