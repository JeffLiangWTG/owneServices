using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	[TestedType(typeof(BatchForwardingConsolValueObjectDataAdapter))]
	sealed class BatchForwardingConsolValueObjectDataAdapterTest : ForwardingConsolValueObjectDataAdapterTest
	{
		public void TestImportConsolNumberFromXml()
		{
			SetUpBranchesAndImportRules();

			var notification = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notification);

			SystemDataRegistry.Instance.ImportConsolNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			Xsd.Consol xsdConsol = GetNewXsdConsol("MAWB001");
			xsdConsol.ConsolDetail.PortOfLoading.Port.Value = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			xsdConsol.ConsolDetail.AgentReference = "C00012345";

			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol, context);
			Factory.Save();

			ForwardingConsol consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "MAWB001"));
			AssertEquals("Since registry 'import consol no from xml' is activated, the consol no should be import from xml. It will ignore the setting on the consol branch registry", "C00012345", consol.JK_UniqueConsignRef);

			SystemDataRegistry.Instance.ImportConsolNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			xsdConsol.ConsolIdentifier.Clear();
			Xsd.ConsolIdentifier identifier = xsdConsol.ConsolIdentifier.AddNew();
			identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			identifier.Value = "MAWB002";

			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol, context);
			Factory.Save();

			consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "MAWB002"));
			AssertEquals("should use SYD code as it's a current branch", "CSYD00000001", consol.JK_UniqueConsignRef);
		}

		public void TestSaveUsesBranchContext()
		{
			SetUpBranchesAndImportRules();

			var notification = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notification);

			Xsd.Consol xsdConsol = GetNewXsdConsolWithAttachedShipment("MAWB001", "HBL001");
			xsdConsol.ConsolDetail.PortOfLoading.Port.Value = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol, context);
			Factory.Save();

			ForwardingConsol consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "MAWB001"));
			ForwardingShipment shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL001"));
			AssertEquals("should use SYD code as it's a current branch", "SSYD00000001", shipment.JS_UniqueConsignRef);
			AssertEquals("should use SYD code as it's a current branch", "CSYD00000001", consol.JK_UniqueConsignRef);

			xsdConsol = GetNewXsdConsolWithAttachedShipment("MAWB002", "HBL002");
			xsdConsol.ConsolDetail.PortOfDischarge.Port.Value = "PLGDN";

			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol, context);
			Factory.Save();

			consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "MAWB002"));
			shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL002"));
			AssertEquals("should use GDN branch from destination poort", "CGDN00000001", consol.JK_UniqueConsignRef);
			AssertEquals("should use GDN branch from destination port", "SGDN00000001", shipment.JS_UniqueConsignRef);

			xsdConsol = GetNewXsdConsolWithAttachedShipment("MAWB003", "HBL003");
			xsdConsol.ConsolDetail.PortOfDischarge.Port.Value = "PLGDN";
			xsdConsol.ConsolDetail.PortOfLoading.Port.Value = "HKHKG";
			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol, context);
			Factory.Save();

			consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "MAWB003"));
			shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL003"));
			AssertEquals("should use HGK branch from origin port", "CHKG00000001", consol.JK_UniqueConsignRef);
			AssertEquals("should use HGK branch from origin port", "SHKG00000001", shipment.JS_UniqueConsignRef);

			xsdConsol = GetNewXsdConsolWithAttachedShipment("MAWB004", "HBL004");
			xsdConsol.ConsolDetail.PortOfDischarge.Port.Value = "PLGDN";
			xsdConsol.ConsolDetail.PortOfLoading.Port.Value = "CNSHA";
			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol, context);
			Factory.Save();

			consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "MAWB004"));
			shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL004"));
			AssertEquals("should use HGK branch because it has CNSHA as extra port", "CHKG00000002", consol.JK_UniqueConsignRef);
			AssertEquals("should use HGK branch because it has CNSHA as extra port", "SHKG00000002", shipment.JS_UniqueConsignRef);
		}

		public void TestImportUsesBranchContext()
		{
			SetUpBranchesAndImportRules();

			SystemDataRegistry.Instance.AllowBillingImportIntoShipment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var notification = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notification);

			Xsd.Consol xsdConsol = GetNewXsdConsolWithAttachedShipment("MAWB001", "HBL001");
			xsdConsol.ConsolDetail.PortOfLoading.Port.Value = "PLGDN";
			Xsd.Shipment xsdShipment = xsdConsol.Shipments[0];
			xsdShipment.ShipmentDetails.LocalClient = new Xsd.Organisation();
			xsdShipment.ShipmentDetails.LocalClient.EDICode = "SMO";
			xsdShipment.ShipmentDetails.LocalClient.OrganisationDetails = new Xsd.OrganisationDetail();
			xsdShipment.ShipmentDetails.LocalClient.OrganisationDetails.Name = "LOCAL CLIENT";
			xsdShipment.ShipmentDetails.LocalClient.IsSpecified = true;

			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol, context);
			Factory.Save();

			AssertEquals("current branch should be SYD", "SYD", GlbBranch.CurrentBranch.GB_Code);

			ForwardingConsol consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "MAWB001"));
			ForwardingShipment shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL001"));
			JobHeader.Loader loader = new JobHeader.Loader(shipment);
			JobHeader jobHeader = loader.Load();

			AssertEquals("should use GDN branch from destination port during save", "CGDN00000001", consol.JK_UniqueConsignRef);
			AssertEquals("should use GDN branch from destination port during import", "GDN", jobHeader.Branch.GB_Code);
			AssertEquals("should use GDN branch from destination port during save", "SGDN00000001", shipment.JS_UniqueConsignRef);
		}

		public void TestDoNotCreateConsolAndShipmentWhenBranchNotFound()
		{
			ImportBranchRule rule = new ImportBranchRule();
			rule.UseBranchFromXml = true;
			rule.DefaultToBranchRelatedToOriginLoadPort = 0;
			rule.DefaultToBranchRelatedToDestinationDischargePort = 0;
			rule.FallbackRule = ImportBranchRule.FallbackCodes.DoNotCreate;

			var notification = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notification);

			SetUpBranchesAndImportRules(rule);

			Xsd.Consol xsdConsol = GetNewXsdConsolWithAttachedShipment("MAWB001", "HBL001");
			xsdConsol.ConsolDetail.PortOfLoading.Port.Value = "PLGDN";
			Xsd.Shipment xsdShipment = xsdConsol.Shipments[0];
			xsdConsol.ConsolDetail.PortOfDischarge.Port.Value = "CRAPO";

			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol, context);
			Factory.Save();

			ForwardingConsol consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "MAWB001"));
			ForwardingShipment shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL001"));

			AssertNull("consol should not be created", consol);
			AssertNull("shipment should not be created", shipment);
			AssertContains("Consol file has not been imported because branch could not be located. Refer to registry settings System -> Data Import Settings -> Consols -> Consol Import Branch Rules", notification.AsString);
		}

		public void TestImportMultipleConsolsMatchingDifferentBranches()
		{
			SetUpBranchesAndImportRules();

			var notification = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notification);

			AssertEquals("prerequisite - current branch", "SYD", GlbBranch.CurrentBranch.GB_Code);

			Xsd.Consol xsdConsol1 = GetNewXsdConsolWithAttachedShipment("MAWB001", "HBL001");
			xsdConsol1.ConsolDetail.PortOfLoading.Port.Value = "PLGDN";

			Xsd.Consol xsdConsol2 = GetNewXsdConsol("MAWB002");
			xsdConsol2.ConsolDetail.PortOfLoading.Port.Value = "HKHGK";

			BusinessObjectFactory factory = NewFactory();

			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol1, context);
			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol2, context);

			AssertNotEquals("adapter should create new and save old factory", factory, context.FactoryProvider.Current);

			context.Factory.Save();

			ForwardingConsol consol1 = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "MAWB001"));
			ForwardingShipment shipment1 = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL001"));

			AssertEquals("consol1 should use GDN branch from origin", "CGDN00000001", consol1.JK_UniqueConsignRef);
			AssertEquals("shipment1 should use GDN branch from origin", "SGDN00000001", shipment1.JS_UniqueConsignRef);

			ForwardingConsol consol2 = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "MAWB002"));
			AssertEquals("shipment2 should use HGK branch from origin", "CHKG00000001", consol2.JK_UniqueConsignRef);
		}

		public void TestImportMultipleShipmentsMatchingDifferentBranchesFirstOneIsCurrent()
		{
			SetUpBranchesAndImportRules();

			var notification = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notification);

			AssertEquals("prerequisite - current branch", "SYD", GlbBranch.CurrentBranch.GB_Code);

			Xsd.Consol xsdConsol1 = GetNewXsdConsol("MAWB001");
			xsdConsol1.ConsolDetail.PortOfLoading.Port.Value = "AUSYD";

			Xsd.Consol xsdConsol2 = GetNewXsdConsolWithAttachedShipment("MAWB002", "HBL002");
			xsdConsol2.ConsolDetail.PortOfLoading.Port.Value = "HKHKG";

			BusinessObjectFactory factory = NewFactory();

			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol1, context);
			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol2, context);

			AssertNotEquals("adapter should create new and save old factory", factory, context.FactoryProvider.Current);

			context.Factory.Save();

			ForwardingConsol consol1 = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "MAWB001"));

			AssertEquals("shipment1 should use SYD branch from origin", "CSYD00000001", consol1.JK_UniqueConsignRef);

			ForwardingConsol consol2 = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "MAWB002"));
			ForwardingShipment shipment2 = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL002"));

			AssertEquals("consol2 should use HKG branch from origin", "CHKG00000001", consol2.JK_UniqueConsignRef);
			AssertEquals("shipment2 should use HKG branch from origin", "SHKG00000001", shipment2.JS_UniqueConsignRef);
		}

		public void TestSaveAndCreateNewFactoryOnlyWhenDifferentContextIsFound()
		{
			SetUpBranchesAndImportRules();

			var notification = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notification);

			AssertEquals("prerequisite - current branch", "SYD", GlbBranch.CurrentBranch.GB_Code);

			Xsd.Consol xsdConsol1 = GetNewXsdConsol("MAWB001");
			xsdConsol1.ConsolDetail.PortOfLoading.Port.Value = "HKHKG";

			Xsd.Consol xsdConsol2 = GetNewXsdConsolWithAttachedShipment("MAWB002", "HBL002");
			xsdConsol2.ConsolDetail.PortOfDischarge.Port.Value = "HKHKG";

			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol1, context);
			AssertEquals(Factory, context.Factory);
			AssertEquals("HKG", GlbBranch.CurrentBranch.GB_Code);

			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol2, context);
			AssertEquals(Factory, context.Factory);
			AssertEquals("HKG", GlbBranch.CurrentBranch.GB_Code);

			Factory.Save();

			AssertEquals("SYD", GlbBranch.CurrentBranch.GB_Code);
		}

		public void TestImportUsesBranchContextWhenSpecifiedInTargetOfXml()
		{
			SetUpBranchesAndImportRules();

			SystemDataRegistry.Instance.AllowBillingImportIntoShipment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.ImportConsolNoFromXml.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var notification = new NotificationBuffer();
			var xmlInterchange = new Xsd.XmlInterchange();
			xmlInterchange.InterchangeInfo.Target.BranchCode = "HKG";

			var context = new ValueObjectImportContext(Factory, xmlInterchange, notification);

			Xsd.Consol xsdConsol = GetNewXsdConsolWithAttachedShipment("MAWB001", "HBL001");
			DataAdapter.CreateOrUpdateFromValueObject(xsdConsol, context);
			Factory.Save();

			AssertEquals("prerequisite - current branch", "SYD", GlbBranch.CurrentBranch.GB_Code);

			var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, "MAWB001"));
			var shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL001"));

			AssertEquals("should use HKG branch from target in xml", "CHKG00000001", consol.JK_UniqueConsignRef);
			AssertEquals("should use HKG branch from target in xml", "SHKG00000001", shipment.JS_UniqueConsignRef);
		}

		#region Implementation

		void SetUpBranchesAndImportRules(ImportBranchRule rule = null)
		{
			if (rule == null)
			{
				rule = new ImportBranchRule()
				{
					UseBranchFromXml = true,
					DefaultToBranchRelatedToOriginLoadPort = 1,
					DefaultToBranchRelatedToDestinationDischargePort = 2,
					FallbackRule = ImportBranchRule.FallbackCodes.DefaultToAny
				};
			}

			SystemDataRegistry.Instance.ConsolImportBranchRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			JobBranchDefaultOrderRule jobBranchDefaultOrderRule = new JobBranchDefaultOrderRule()
			{
				DefaultToBlank = 1,
				DefaultToBranchRelatedToPortOrWarehouseBranch = 0,
				DefaultToBranchOfOrganisation = 0,
				DefaultToLoginUserDefault = 0
			};

			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobBranchDefaultOrderRule);

			GlbCompany company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

			XMLAutomationTestHelper.DeleteAllBranchesExceptCurrentBranch(company);

			GlbBranch branchSYD = company.Branches[0];
			branchSYD.GB_RL_NKHomePort = "AUSYD";
			branchSYD.GB_Code = "SYD";

			GlbBranch branchGDN = company.Branches.AddNew();
			branchGDN.GB_RL_NKHomePort = "PLGDN";
			branchGDN.GB_Code = "GDN";

			GlbBranch branchHKG = company.Branches.AddNew();
			branchHKG.GB_RL_NKHomePort = "HKHKG";
			branchHKG.GB_Code = "HKG";
			branchHKG.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "CNSHA";

			GlbBranch branchSIN = company.Branches.AddNew();
			branchSIN.GB_RL_NKHomePort = "MOMFM";
			branchSIN.GB_Code = "MFM";

			Factory.Save();

			var customisation = new BillOfLadingNumberCustomisation();
			var element = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.BranchCode];
			element.Include = true;
			element.Order = 1;
			element.Fountain = true;

			FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
			FreightConfigurationRegistry.Instance.ConsolNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
		}

		Xsd.Consol GetNewXsdConsol(string masterbill)
		{
			Xsd.Consol xsdConsol = new Xsd.Consol();
			xsdConsol.ConsolDetail.TransportMode = Xsd.ConsolTransportMode.SEA;

			if (!string.IsNullOrEmpty(masterbill))
			{
				Xsd.ConsolIdentifier identifier = xsdConsol.ConsolIdentifier.AddNew();
				identifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
				identifier.Value = masterbill;
			}

			return xsdConsol;
		}

		Xsd.Consol GetNewXsdConsolWithAttachedShipment(string masterbill, string housebill)
		{
			Xsd.Consol xsdConsol = GetNewXsdConsol(masterbill);
			xsdConsol.Shipments.Add(GetNewXsdShipment(housebill));
			return xsdConsol;
		}

		Xsd.Shipment GetNewXsdShipment(string houseBill)
		{
			Xsd.Shipment xsdShipment = new Xsd.Shipment();
			xsdShipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;
			xsdShipment.ShipmentDetails.Consignee.EDICode = GlbCompany.CurrentCompany.OrgProxy.OH_Code;

			if (!string.IsNullOrEmpty(houseBill))
			{
				Xsd.ShipmentIdentifier identifier = xsdShipment.ShipmentIdentifier.AddNew();
				identifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
				identifier.Value = houseBill;
			}

			return xsdShipment;
		}

		protected override void SetUp()
		{
			base.SetUp();

			ImportBranchRule rule = new ImportBranchRule()
			{
				UseBranchFromXml = false,
				DefaultToBranchRelatedToOriginLoadPort = 0,
				DefaultToBranchRelatedToDestinationDischargePort = 0,
				FallbackRule = ImportBranchRule.FallbackCodes.DefaultToAny
			};

			SystemDataRegistry.Instance.ConsolImportBranchRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);
		}

		protected override Type GetDataAdapterType()
		{
			return typeof(BatchForwardingConsolValueObjectDataAdapter);
		}

		protected override ValueObjectDataAdapter<ForwardingConsol, Xsd.Consol> GetNewBizObjXmlDataAdapter()
		{
			return new BatchForwardingConsolValueObjectDataAdapter();
		}

		BatchForwardingConsolValueObjectDataAdapter DataAdapter
		{
			get { return dataAdapter ?? (dataAdapter = new BatchForwardingConsolValueObjectDataAdapter()); }
		}
		BatchForwardingConsolValueObjectDataAdapter dataAdapter;

		#endregion
	}
}
