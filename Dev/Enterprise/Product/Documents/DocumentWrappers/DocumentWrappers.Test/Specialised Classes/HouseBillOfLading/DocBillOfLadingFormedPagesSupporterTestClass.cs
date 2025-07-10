using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocBillOfLading))]
	sealed class DocBillOfLadingFormedPagesSupporterTestClass : NonPersistentBusinessObjectTestCase
	{
		public void TestStandardShipment()
		{
			CreateStandardShipment();

			Constants["IncludePackageCountInBOLGoodsDescription"] = 0;

			AssertEquals("1 shipment", 1, Supporter.Shipments.Count);

			DocFormedPagesShipment formedPagesShipment = Supporter.Shipments[0];
			AssertEquals((ZString)"Description of goods", formedPagesShipment.GoodsDescription);
			AssertEquals((ZString)"Marks & Numbers", formedPagesShipment.MarksAndNumbers);
			AssertEquals((ZString)"180 KG", formedPagesShipment.Weight);
			AssertEquals((ZString)"2.4 M3", formedPagesShipment.Volume);
			AssertEquals((ZString)"44 Bag(s)", formedPagesShipment.PackageCount);
		}

		public void TestStandardShipmentBOL()
		{
			CreateStandardShipment();
			Shipment.JS_PackingMode = "FCL";

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 3;
			Constants["MarksAndNumbersWidth"] = 20;
			Constants["MarksAndNumbersAndGoodsDescriptionGap"] = 1;
			Constants["GoodsDescriptionWidth"] = 20;
			Constants["GoodsDescriptionAndGrossWeightGap"] = 1;
			Constants["GrossWeightWidth"] = 12;
			Constants["GrossWeightAndMeasurementGap"] = 1;
			Constants["VolumeMeasurementWidth"] = 12;
			Constants["ShowDetailHeadingInMainBody"] = "Y";
			Constants["ShowContainerHeadingInMainBody"] = "Y";

			const string expectedDetailHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks & Numbers      Goods Description        Gross Wt       Volume\n" +
				"";

			const string expectedDetailBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks & Numbers      44 Bag(s)                  180 KG       2.4 M3\n" +
				"                     Description of goods                          \n" +
				"";

			const string expectedContainersSection =
				"Cn. No          Seal                 Type           Net (kg)  Tare (kg) Gross (kg)    Volume (M3)        Packs\n" +
				"-               -                    -                   180          -        180            2.4       44 BAG\n";

			AssertEquals("Should only have 1 formed page", 1, DocBOL.FormedPages.Count);

			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, DocBOL.FormedPages.DetailsSectionHeader);

			AssertMultilineASCIIEquals("Detail Body",
				expectedDetailHeading +
				expectedDetailBody1,
				DocBOL.FormedPages[0].MainBodyDetailsSection);

			AssertMultilineASCIIEquals("FollowOn",
				expectedContainersSection +
				"\n",
				ZString.Join("\n", DocBOL.FollowOnSection));
		}

		void CreateStandardShipment()
		{
			Shipment.JS_ShipmentType = "STD";
			Shipment.JS_GoodsDescription = "Description of goods";
			Shipment.JS_MarksAndNumbers = "Marks & Numbers";
			Shipment.JS_ActualWeight = 180;
			Shipment.JS_UnitOfWeight = "KG";
			Shipment.JS_ActualVolume = 2.4;
			Shipment.JS_UnitOfVolume = "M3";
			Shipment.JS_OuterPacks = 44;
			Shipment.JS_F3_NKPackType = "BAG";
		}

		public void TestCoLoadMasterShipment()
		{
			CreateMasterAndSubShipments();

			AssertEquals("1 shipment", 3, Supporter.Shipments.Count);

			DocFormedPagesShipment formedPagesShipment = Supporter.Shipments[0];
			AssertMultilineASCIIEquals("Description1", "12 Box(s)\nSub1 goods description\n\nA mandatory statement", formedPagesShipment.GoodsDescription);
			AssertEquals("Sub1 M&N", formedPagesShipment.MarksAndNumbers);
			AssertEquals("25 KG", formedPagesShipment.Weight);
			AssertEquals("2.4 M3", formedPagesShipment.Volume);
			AssertEquals("12 Box(s)", formedPagesShipment.PackageCount);

			formedPagesShipment = Supporter.Shipments[1];
			AssertMultilineASCIIEquals("Description1", "22 Bag(s)\nSub2 goods description\n\nA mandatory statement", formedPagesShipment.GoodsDescription);
			AssertEquals("Sub2 M&N", formedPagesShipment.MarksAndNumbers);
			AssertEquals("33 KG", formedPagesShipment.Weight);
			AssertEquals("2.9 M3", formedPagesShipment.Volume);
			AssertEquals("22 Bag(s)", formedPagesShipment.PackageCount);

			formedPagesShipment = Supporter.Shipments[2];
			AssertMultilineASCIIEquals("Description1", "A mandatory statement", formedPagesShipment.GoodsDescription);
			AssertEquals(ZString.Empty, formedPagesShipment.MarksAndNumbers);
			AssertEquals(ZString.Empty, formedPagesShipment.Weight);
			AssertEquals(ZString.Empty, formedPagesShipment.Volume);
			AssertEquals(ZString.Empty, formedPagesShipment.PackageCount);
		}

		public void TestCoLoadMasterShipmentBOL()
		{
			CreateMasterAndSubShipments();
			Shipment.JS_PackingMode = "FCL";

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 4;
			Constants["MarksAndNumbersWidth"] = 20;
			Constants["MarksAndNumbersAndGoodsDescriptionGap"] = 1;
			Constants["GoodsDescriptionWidth"] = 20;
			Constants["GoodsDescriptionAndGrossWeightGap"] = 1;
			Constants["GrossWeightWidth"] = 12;
			Constants["GrossWeightAndMeasurementGap"] = 1;
			Constants["VolumeMeasurementWidth"] = 12;
			Constants["ShowDetailHeadingInMainBody"] = "Y";

			const string expectedDetailHeading =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Marks & Numbers      Goods Description        Gross Wt       Volume\n" +
				"";

			const string expectedDetailBody1 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"Sub1 M&N             12 Box(s)                   25 KG       2.4 M3\n" +
				"                     Sub1 goods                                    \n" +
				"                     description                                   \n" +
				"";

			const string expectedDetailBody2 =
				//         1    1    2    2    3    3    4    4    5    5    6    6    7    7
				//1...5....0....5....0....5....0....5....0....5....0....5....0....5....0....5
				"                                                                   \n" +
				"                     A mandatory                                   \n" +
				"                     statement                                     \n" +
				"-------------------- -------------------- ------------ ------------\n" +
				"Sub2 M&N             22 Bag(s)                   33 KG       2.9 M3\n" +
				"                     Sub2 goods                                    \n" +
				"                     description                                   \n" +
				"                                                                   \n" +
				"                     A mandatory                                   \n" +
				"                     statement                                     \n" +
				"-------------------- -------------------- ------------ ------------\n" +
				"                     A mandatory                                   \n" +
				"                     statement                                     \n" +
				"\n";

			const string expectedContainersSection =
				"Cn. No          Seal                 Type           Net (kg)  Tare (kg) Gross (kg)    Volume (M3)        Packs\n" +
				"-               -                    -                    33          -         33            2.9       22 BAG\n";

			AssertEquals("Should only have 1 formed page", 1, DocBOL.FormedPages.Count);

			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, DocBOL.FormedPages.DetailsSectionHeader);

			AssertMultilineASCIIEquals("Detail Body",
				expectedDetailHeading +
				expectedDetailBody1,
				DocBOL.FormedPages[0].MainBodyDetailsSection);

			AssertMultilineASCIIEquals("FollowOn",
				expectedDetailHeading +
				expectedDetailBody2 +
				expectedContainersSection +
				"\n",
				ZString.Join("\n", DocBOL.FollowOnSection));

			Constants["MarksAndNumbersAndGoodsDescriptionHeight"] = 18;

			AssertMultilineASCIIEquals("Detail Heading", expectedDetailHeading, DocBOL.FormedPages.DetailsSectionHeader);

			AssertMultilineASCIIEquals("Detail Body",
				expectedDetailHeading +
				expectedDetailBody1 +
				expectedDetailBody2 +
				"\n",
				DocBOL.FormedPages[0].MainBodyDetailsSection);

			AssertMultilineASCIIEquals("FollowOn",
				expectedContainersSection +
				"\n",
				ZString.Join("\n", DocBOL.FollowOnSection));
		}

		public void TestNestedCoLoadMasterShipmentsDontGetStuckInAnInfiniteLoop()
		{
			CreateMasterAndSubShipments();

			var subAsColoadMaster = Shipment.CoLoadShipments.Cast<ForwardingShipment>().FirstOrDefault();
			subAsColoadMaster.OuterPackLines.RemoveAndDeleteAll();
			subAsColoadMaster.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			subAsColoadMaster.CoLoadShipments.AddNew();

			AssertEquals("Expected 4 shipments. Should not get stuck in an infinite loop with masters having masters", 4, Supporter.Shipments.Count);
		}

		void CreateMasterAndSubShipments()
		{
			CountryExportStatementSettingCollection countrySettings = new CountryExportStatementSettingCollection();
			CountryExportStatementSetting sedSetting = countrySettings.AddNew();
			sedSetting.CountryCode = Core.Constants.CountryCodes.Australia;
			sedSetting.Statements.Add(new ExportStatementSetting(sedSetting, "MAT", "A mandatory statement", "", "", "", "MAN", true, true, true, true, true, true));
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettings);

			Shipment.JS_ShipmentType = "CLD";
			Shipment.JS_GoodsDescription = "Master goods description";
			Shipment.JS_MarksAndNumbers = "Master M&N";
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "DEHAM";
			Shipment.Consols.Add(Consol);

			ForwardingShipment sub1 = Shipment.CoLoadShipments.AddNew();
			sub1.JS_ShipmentType = "STD";
			sub1.JS_GoodsDescription = "Sub1 goods description";
			sub1.JS_MarksAndNumbers = "Sub1 M&N";
			sub1.JS_ActualWeight = 25;
			sub1.JS_ActualVolume = 2.4;
			sub1.JS_OuterPacks = 12;
			sub1.JS_F3_NKPackType = "BOX";

			ForwardingShipment sub2 = Shipment.CoLoadShipments.AddNew();
			sub2.JS_ShipmentType = "STD";
			sub2.JS_GoodsDescription = "Sub2 goods description";
			sub2.JS_MarksAndNumbers = "Sub2 M&N";
			sub2.JS_ActualWeight = 33;
			sub2.JS_ActualVolume = 2.9;
			sub2.JS_OuterPacks = 22;
			sub2.JS_F3_NKPackType = "BAG";
		}

		public void TestContainers()
		{
			CreateMasterAndSubShipments();

			ForwardingShipment sub1 = Shipment.CoLoadShipments[0];
			ForwardingShipment sub2 = Shipment.CoLoadShipments[1];

			PackLine pack1 = sub1.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 20;

			PackLine pack2 = sub2.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 43;

			PackLine pack3 = sub2.OuterPackLines.AddNew();
			pack3.JL_PackageCount = 38;

			ForwardingContainer container1 = Consol.Containers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container1.JC_ContainerNum = "ABCD12345";

			ForwardingContainer container2 = Consol.Containers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			container2.JC_ContainerNum = "ABCD67890";

			pack1.SetContainer(container1.PK);
			pack2.SetContainer(container1.PK);
			pack3.SetContainer(container2.PK);

			AssertEquals("2 containers", 2, Supporter.Containers.Count);
		}

		public void TestCharges()
		{
			CreateStandardShipment();

			Shipment.JS_INCO = "CFR";

			var header = GetNewHeader(Shipment);

			CreateLineCharge(header, header.LocalChargesPK, 10000, "FRT", "AUD");
			CreateLineCharge(header, header.AgentCollectPK, 500, "OLAB", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, 750, "DLAB", "EUR");

			AssertEquals("All charges", 3, Supporter.AllCharges.Count);
			var collection = Supporter.CollectCharges;
			AssertEquals("Collect charges", 1, collection.Count);

			var newSupporter = GetNewBusinessObject() as IFormedPagesSupporter;
			AssertNotNull(newSupporter);
			AssertNotSame("Different Supporter", newSupporter, Supporter);
			AssertSame("Same collection cached on Factory level by ShipmentWrapper PK", collection, newSupporter.CollectCharges);

			Factory.Save();
			AssertNotSame("Cached value was cleared", collection, Supporter.CollectCharges);
		}

		JobHeader GetNewHeader(IJobHeaderParent parent)
		{
			var header = new JobHeader.Loader(parent).TryLoadOrCreate();

			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.JH_ParentID = parent.PK;
			header.JH_GB = GlbBranch.CurrentBranch.PK;
			header.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			header.JH_JobNum = parent.JobNumber;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.AgentCollectPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			header.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			return header;
		}

		JobCharge CreateLineCharge(JobHeader header, ZGuid sellAccountPK, ZDecimal osSellAmount, ZString chargeCode, ZString currencyCode)
		{
			var accChargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.Equal, chargeCode);
			accChargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, Env.CurrentCompanyPK);

			var accChargeCode = Factory.LoadTop1<AccChargeCode>(accChargeCodeQuery);
			var lineCharge = Factory.New<JobCharge>();

			lineCharge.JR_JH = header.PK;
			lineCharge.JR_GE = header.JH_GE;
			lineCharge.JR_GB = header.JH_GB;
			lineCharge.JR_OSSellAmt = osSellAmount;
			lineCharge.JR_OH_SellAccount = sellAccountPK;
			lineCharge.JR_AC = accChargeCode.PK;
			lineCharge.JR_RX_NKSellCurrency = currencyCode;

			return lineCharge;
		}

		public void TestDisplayContainers()
		{
			Shipment.Consols.Add(Consol);
			AssertEquals("Do not display containers if none are present", false, ((IFormedPagesSupporter)DocBOL).DisplayContainers);

			var container = Consol.Containers.AddNew();
			var packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);

			AssertEquals(1, ShipmentWrapper.Containers.Count);
			AssertEquals("Show containers if they are present", true, ((IFormedPagesSupporter)DocBOL).DisplayContainers);
		}

		public void TestHideContainerWeights()
		{
			Assert("Pre-condition: expecting the registry to be set to true", Env.Registry.DisplayTareAndGrossWeightOnHBOL);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Should be hidden as the packing mode is not FCL", true, Supporter.HideContainerGrossWeight);
			AssertEquals("Should be hidden as the packing mode is not FCL", true, Supporter.HideContainerTareWeight);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("Should not be hidden as the packing mode is FCL", false, Supporter.HideContainerGrossWeight);
			AssertEquals("Should not be hidden as the packing mode is FCL", false, Supporter.HideContainerTareWeight);

			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("Should be hidden as the packing mode is not FCL", true, Supporter.HideContainerGrossWeight);
			AssertEquals("Should be hidden as the packing mode is not FCL", true, Supporter.HideContainerTareWeight);

			try
			{
				Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
				Env.Registry.DisplayTareAndGrossWeightOnHBOL = false;

				AssertEquals("Set registry to override: Gross", true, Supporter.HideContainerGrossWeight);
				AssertEquals("Set registry to override: Tare", true, Supporter.HideContainerTareWeight);

				Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
				AssertEquals("Should be hidden regardless of packing mode due to the registry setting", true, Supporter.HideContainerGrossWeight);
				AssertEquals("Should be hidden regardless of packing mode due to the registry setting", true, Supporter.HideContainerTareWeight);
			}
			finally
			{
				Env.Registry.DisplayTareAndGrossWeightOnHBOL = true;
			}
		}

		public void TestHideInterleavedPackLines()
		{
			AssertEquals("Hidden by default", true, Supporter.HidePackLinesInContainersSection);

			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Change the registry setting to enable", false, Supporter.HidePackLinesInContainersSection);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			base.SetUp();
		}

		ForwardingConsol Consol
		{
			get { return consol ?? (consol = Factory.New<ForwardingConsol>()); }
		}
		ForwardingConsol consol;

		ForwardingShipment Shipment
		{
			get { return shipment ?? (shipment = Factory.New<ForwardingShipment>()); }
		}
		ForwardingShipment shipment;

		DocForwardingShipment ShipmentWrapper
		{
			get
			{
				DocForwardingShipment wrapper = DocForwardingShipment.New(Shipment, Factory);

				if (constants != null)
				{
					wrapper.SetTemplateConstants(constants);
				}

				return wrapper;
			}
		}

		DocBillOfLading DocBOL
		{
			get { return ShipmentWrapper.BillOfLading; }
		}

		IFormedPagesSupporter Supporter
		{
			get { return DocBOL; }
		}

		Dictionary<string, object> Constants
		{
			get { return constants ?? (constants = new Dictionary<string, object>()); }
		}
		Dictionary<string, object> constants;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocBillOfLading(ShipmentWrapper);
		}

		#endregion

	}
}
