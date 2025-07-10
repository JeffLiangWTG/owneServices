using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Billing.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.NIP.Business.ConsolAndShipmentImport.Testing
{
	sealed class NIPConsolAndShipmentDataImporterTest : FlatFileDataImporterTestCase
	{
		public void TestImportData()
		{
			AssertEquals("Precondition: DB should not contains any consols", 0, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
			AssertEquals("Precondition: DB should not contains any decs", 0, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));

			var importer = (NIPConsolAndShipmentDataImporter)GetDataImporter();
			importer.ImportData(PathToTestFile, new NotificationBuffer(), SourceInfo.EmptySourceInfo);

			AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
			var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery());
			AssertEquals(1, consol.Shipments.Count);
			AssertEquals(2, consol.Containers.Count);
			var decs = Factory.Load<BaseJobDeclaration>(new ZQuery());
			AssertEquals(2, decs.Length);
			AssertEquals(1, decs[0].CusContainers.Count);
			AssertEquals(1, decs[1].CusContainers.Count);
		}

		public void TestCargoMessageCreatedSea()
		{
			ObjectFactory.New<Integration.Customs.AU.ICertificateManagerHelper>(Factory).CreateCustomsCertificates();
			Factory.Save();

			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();

			using (SystemDataRegistry.Instance.AutomaticallySendAirCargoMessage.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.AutomaticallySendSeaCargoMessage.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, true))
			using (Env.Registry.RawRegistry.AUCCompanyCertificateData.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, new byte[] { 122, 44, 33 }))
			using (Env.Registry.RawRegistry.AUCCompanyCertificatePassword.DataType.SuspendValidation())
			using (Env.Registry.RawRegistry.AUCCompanyCertificatePassword.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, "pwd"))
			{
				var importer = (NIPConsolAndShipmentDataImporter)GetDataImporter();
				importer.ImportData(PathToTestFile, new NotificationBuffer(), SourceInfo.EmptySourceInfo);

				var factory2 = new BusinessObjectFactory();
				var ocean = factory2.Load<CusSCAOceanBill>(new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, "SYDYOK000055"))[0];
				var house = ocean.HouseBills[0];
				AssertEquals("SYYOSRE07595", house.CA_HouseBill);
				var message = house.Messages[0];
				AssertContains("CUSCAR:D:99B:UN'BGM+933:::SEACR", message.EM_MessageText);
				AssertContains("RFF+BH:SYYOSRE07595'RFF+MB:SYDYOK000055'", message.EM_MessageText);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2009,05,25)]
		public void TestCargoMessageCreatedAir()
		{
			ObjectFactory.New<Integration.Customs.AU.ICertificateManagerHelper>(Factory).CreateCustomsCertificates();
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			orgProxy.PrimaryRegistrationNumber.Number = "41065894724";
			Factory.Save();

			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();

			using (SystemDataRegistry.Instance.AutomaticallyCreateAirCargoJob.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.AutomaticallySendAirCargoMessage.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, true))
			using (SystemDataRegistry.Instance.AutomaticallySendSeaCargoMessage.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, true))
			using (Env.Registry.RawRegistry.AUCCompanyCertificateData.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, new byte[] { 122, 44, 33 }))
			using (Env.Registry.RawRegistry.AUCCompanyCertificatePassword.DataType.SuspendValidation())
			using (Env.Registry.RawRegistry.AUCCompanyCertificatePassword.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, "pwd"))
			{
				var notifications = new NotificationBuffer();
				var airTestFilePath = BaseSourcePath + @"Enterprise\ClientExtensions\NIP\ZClientNIP\ZClientNIP.Test\ImportConsolAndShipmentData\TestFiles\TestFileAir.csv";
				var importer = (NIPConsolAndShipmentDataImporter)GetDataImporter();
				importer.ImportData(airTestFilePath, notifications, SourceInfo.EmptySourceInfo);

				var factory2 = new BusinessObjectFactory();
				var mawbs = factory2.Load<Customs.Business.CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "11223300056"));
				AssertEquals(1, mawbs.Length);
				var hawb = mawbs[0].ChildBills[0];
				AssertEquals("SYYOSRE07595", hawb.CS_HAWB);
				var message = hawb.Messages[0];
				AssertContains("CUSCAR:D:99B:UN'BGM+933:::AIRCR", message.EM_MessageText);
				AssertContains("RFF+HWB:SYYOSRE07595'RFF+MWB:11223300056'", message.EM_MessageText);

				AssertContains("Messages successfully sent.", notifications.AsString);
			}
		}

		protected override FlatFileDataImporter GetDataImporter() => new NIPConsolAndShipmentDataImporter();

		string csvTestfilePath;
		protected override string PathToTestFile => csvTestfilePath ?? (csvTestfilePath = resourceRetriever.SaveResourceToFile("ImportConsolAndShipmentData.TestFiles.TestFile.csv"));

		protected override void SetUp()
		{
			base.SetUp();
			var override1 = Factory.New<OrgPatternMatchOverride>();
			override1.OO_LocalGuid = new RefContainer.Loader(Factory).LoadFromCode("20FR").PK;
			override1.OO_ForeignCode = "4020FR";
			override1.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.ContainerType;
			override1.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			var override2 = Factory.New<OrgPatternMatchOverride>();
			override2.OO_LocalGuid = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			override2.OO_ForeignCode = "4030GP";
			override2.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.ContainerType;
			override2.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			var override3 = Factory.New<OrgPatternMatchOverride>();
			override3.OO_LocalGuid = new RefContainer.Loader(Factory).LoadFromCode("40FR").PK;
			override3.OO_ForeignCode = "4040FR";
			override3.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.ContainerType;
			override3.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			var override4 = Factory.New<OrgPatternMatchOverride>();
			override4.OO_LocalGuid = new RefContainer.Loader(Factory).LoadFromCode("40FR").PK;
			override4.OO_ForeignCode = "4050GP";
			override4.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.ContainerType;
			override4.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			Factory.Save();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}

		EmbeddedResourceRetriever resourceRetriever;
		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}
	}
}
