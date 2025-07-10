using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	[TestedType(typeof(BatchForwardingShipmentValueObjectDataAdapter))]
	sealed class BatchForwardingShipmentValueObjectDataAdapterTest : ForwardingShipmentValueObjectDataAdapterTest
	{
		public new void TestFindBusinessObject()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneePK = Consignee.PK;
			shipment.ConsignorPK = Consignor.PK;
			shipment.JS_HouseBill = "HOUSEBILL";
			shipment.JS_GoodsDescription = "goods";
			Factory.Save();

			SystemDataRegistry.Instance.UpdateShipmentsDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentAutomaticImportOptions.Code.NoImport);

			Xsd.Shipment xsdShipment = GetNewXsdShipment("HOUSEBILL");
			xsdShipment.ShipmentDetails.PortOfOrigin.Port.Value = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);

			Assert(Notify.AsString.Contains("House bill number 'HOUSEBILL' already exists. Shipment file has not been imported."));

			Notify.Clear();
			SystemDataRegistry.Instance.UpdateShipmentsDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentAutomaticImportOptions.Code.Update);
			int shipmentCount = Factory.GetDatabaseCount(typeof(ForwardingShipment));

			AssertNotEquals(shipment.JS_TransportMode, "SEA");
			Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			Factory.Save();

			AssertEquals("New Shipment should have NOT been created", shipmentCount, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			AssertEquals(shipment.JS_TransportMode, "SEA");

			SystemDataRegistry.Instance.UpdateShipmentsDuringAutomaticImport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ShipmentAutomaticImportOptions.Code.Create);
			Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			Factory.Save();

			AssertEquals("Shipment should not have changed", false, shipment.HasChanges);
			AssertEquals("New Shipment should have been created", shipmentCount + 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			ForwardingShipment newShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.PK, SQLComparisonOperator.NotEqual, shipment.PK));
			AssertNotNull(newShipment);
			AssertEquals("House bill number:", "HOUSEBILL", newShipment.JS_HouseBill);
		}

		public void TestFindBusinessObject_NewObject()
		{
			int shipmentCount = Factory.GetDatabaseCount(typeof(ForwardingShipment));
			Xsd.Shipment xsdShipment = GetNewXsdShipment("HOUSEBILL");
			xsdShipment.ShipmentDetails.PortOfOrigin.Port.Value = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			Factory.Save();

			AssertEquals("New Shipment should have been created", shipmentCount + 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
			ForwardingShipment newShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery());
			AssertNotNull(newShipment);
			AssertEquals("House bill number:", "HOUSEBILL", newShipment.JS_HouseBill);
		}

		public void TestSaveUsesBranchContext()
		{
			SetUpBranchesAndImportRules();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Xsd.Shipment xsdShipment = GetNewXsdShipment("HBL001");
			xsdShipment.ShipmentDetails.PortOfOrigin.Port.Value = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			Factory.Save();

			ForwardingShipment shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL001"));
			AssertEquals("should use SYD code as it's a current branch", "SSYD00000001", shipment.JS_UniqueConsignRef);

			xsdShipment = GetNewXsdShipment("HBL002");
			xsdShipment.ShipmentDetails.PortofDestination.Port.Value = "PLGDN";
			Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			Factory.Save();

			shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL002"));
			AssertEquals("should use GDN branch from destination port", "SGDN00000001", shipment.JS_UniqueConsignRef);

			xsdShipment = GetNewXsdShipment("HBL003");
			xsdShipment.ShipmentDetails.PortofDestination.Port.Value = "PLGDN";
			xsdShipment.ShipmentDetails.PortOfOrigin.Port.Value = "HKHGK";
			Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			Factory.Save();

			shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL003"));
			AssertEquals("should use HGK branch from origin port", "SHGK00000001", shipment.JS_UniqueConsignRef);

			xsdShipment = GetNewXsdShipment("HBL004");
			xsdShipment.ShipmentDetails.PortofDestination.Port.Value = "PLGDN";
			xsdShipment.ShipmentDetails.PortOfOrigin.Port.Value = "CNSHA";
			Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			Factory.Save();

			shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL004"));
			AssertEquals("should use HGK branch because if has CNSHA as extra port", "SHGK00000002", shipment.JS_UniqueConsignRef);
		}

		public void TestImportUsesBranchContext()
		{
			SetUpBranchesAndImportRules();

			AssertEquals("current branch should be SYD", "SYD", GlbBranch.CurrentBranch.GB_Code);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Xsd.Shipment xsdShipment = GetNewXsdShipment("HBL001");
			xsdShipment.ShipmentDetails.PortofDestination.Port.Value = "PLGDN";

			xsdShipment.ShipmentDetails.LocalClient = new Xsd.Organisation();
			xsdShipment.ShipmentDetails.LocalClient.EDICode = "SMO";
			xsdShipment.ShipmentDetails.LocalClient.OrganisationDetails = new Xsd.OrganisationDetail();
			xsdShipment.ShipmentDetails.LocalClient.OrganisationDetails.Name = "LOCAL CLIENT";
			xsdShipment.ShipmentDetails.LocalClient.IsSpecified = true;

			Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			Factory.Save();

			ForwardingShipment shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL001"));
			JobHeader.Loader loader = new JobHeader.Loader(shipment);
			JobHeader jobHeader = loader.Load();

			AssertEquals("should use GDN branch from destination port during import", "GDN", jobHeader.Branch.GB_Code);
			AssertEquals("should use GDN branch from destination port during save", "SGDN00000001", shipment.JS_UniqueConsignRef);
		}

		public void TestDoNotCreateShipmentWhenBranchNotFound()
		{
			ImportBranchRule rule = new ImportBranchRule();
			rule.UseBranchFromXml = true;
			rule.DefaultToBranchRelatedToOriginLoadPort = 0;
			rule.DefaultToBranchRelatedToDestinationDischargePort = 0;
			rule.FallbackRule = ImportBranchRule.FallbackCodes.DoNotCreate;

			SetUpBranchesAndImportRules(rule);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Xsd.Shipment xsdShipment = GetNewXsdShipment("HBL001");
			xsdShipment.ShipmentDetails.PortofDestination.Port.Value = "CRAPO";
			Adapter.CreateOrUpdateFromValueObject(xsdShipment, context);
			Factory.Save();

			ForwardingShipment shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL001"));
			AssertNull("shipent should not be created", shipment);
			AssertContains("Shipment file has not been imported because branch could not be located. Refer to registry settings System -> Data Import Settings -> Shipment -> Shipment Import Branch Rules", Notify.AsString);
		}

		public void TestImportMultipleShipmentsMachingDifferentBranches()
		{
			SetUpBranchesAndImportRules();

			AssertEquals("prerequisite - current branch", "SYD", GlbBranch.CurrentBranch.GB_Code);

			Xsd.Shipment xsdShipment1 = GetNewXsdShipment("HBL001");
			xsdShipment1.ShipmentDetails.PortOfOrigin.Port.Value = "PLGDN";

			Xsd.Shipment xsdShipment2 = GetNewXsdShipment("HBL002");
			xsdShipment2.ShipmentDetails.PortOfOrigin.Port.Value = "HKHGK";

			BusinessObjectFactory factory = NewFactory();

			ValueObjectImportContext context = new ValueObjectImportContext(factory, Notify);
			Adapter.CreateOrUpdateFromValueObject(xsdShipment1, context);
			Adapter.CreateOrUpdateFromValueObject(xsdShipment2, context);

			AssertNotEquals("adapter should create new and save old factory", factory, context.FactoryProvider.Current);

			context.Factory.Save();

			ForwardingShipment shipment1 = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL001"));
			AssertEquals("shipment1 should use GDN branch from origin", "SGDN00000001", shipment1.JS_UniqueConsignRef);

			ForwardingShipment shipment2 = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL002"));
			AssertEquals("shipment2 should use HGK branch from origin", "SHGK00000001", shipment2.JS_UniqueConsignRef);
		}

		public void TestImportMultipleShipmentsMachingDifferentBranchesFirstOneIsCurrent()
		{
			SetUpBranchesAndImportRules();

			AssertEquals("prerequisite - current branch", "SYD", GlbBranch.CurrentBranch.GB_Code);

			Xsd.Shipment xsdShipment1 = GetNewXsdShipment("HBL001");
			xsdShipment1.ShipmentDetails.PortOfOrigin.Port.Value = "AUSYD";

			Xsd.Shipment xsdShipment2 = GetNewXsdShipment("HBL002");
			xsdShipment2.ShipmentDetails.PortOfOrigin.Port.Value = "HKHGK";

			BusinessObjectFactory factory = NewFactory();

			ValueObjectImportContext context = new ValueObjectImportContext(factory, Notify);
			Adapter.CreateOrUpdateFromValueObject(xsdShipment1, context);
			Adapter.CreateOrUpdateFromValueObject(xsdShipment2, context);

			AssertNotEquals("adapter should create new and save old factory", factory, context.FactoryProvider.Current);

			context.Factory.Save();

			ForwardingShipment shipment1 = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL001"));
			AssertEquals("shipment1 should use SYD branch from origin", "SSYD00000001", shipment1.JS_UniqueConsignRef);

			ForwardingShipment shipment2 = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, "HBL002"));
			AssertEquals("shipment2 should use HGK branch from origin", "SHGK00000001", shipment2.JS_UniqueConsignRef);
		}

		public void TestSaveAndCreateNewFactoryOnlyWhenDifferentContextIsFound()
		{
			SetUpBranchesAndImportRules();

			AssertEquals("prerequisite - current branch", "SYD", GlbBranch.CurrentBranch.GB_Code);

			Xsd.Shipment xsdShipment1 = GetNewXsdShipment("HBL001");
			xsdShipment1.ShipmentDetails.PortOfOrigin.Port.Value = "HKHGK";

			Xsd.Shipment xsdShipment2 = GetNewXsdShipment("HBL002");
			xsdShipment2.ShipmentDetails.PortofDestination.Port.Value = "HKHGK";

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			Adapter.CreateOrUpdateFromValueObject(xsdShipment1, context);
			AssertEquals(Factory, context.Factory);
			AssertEquals("HGK", GlbBranch.CurrentBranch.GB_Code);

			Adapter.CreateOrUpdateFromValueObject(xsdShipment2, context);
			AssertEquals(Factory, context.Factory);
			AssertEquals("HGK", GlbBranch.CurrentBranch.GB_Code);

			Factory.Save();

			AssertEquals("SYD", GlbBranch.CurrentBranch.GB_Code);
		}

		void SetUpBranchesAndImportRules(ImportBranchRule rule = null)
		{
			if (rule == null)
			{
				rule = new ImportBranchRule();
				rule.UseBranchFromXml = true;
				rule.DefaultToBranchRelatedToOriginLoadPort = 1;
				rule.DefaultToBranchRelatedToDestinationDischargePort = 2;
				rule.FallbackRule = ImportBranchRule.FallbackCodes.DefaultToAny;
			}

			SystemDataRegistry.Instance.ShipmentImportBranchRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			JobBranchDefaultOrderRule jobBranchDefaultOrderRule = new JobBranchDefaultOrderRule();

			jobBranchDefaultOrderRule.DefaultToBlank = 1;
			jobBranchDefaultOrderRule.DefaultToBranchRelatedToPortOrWarehouseBranch = 0;
			jobBranchDefaultOrderRule.DefaultToBranchOfOrganisation = 0;
			jobBranchDefaultOrderRule.DefaultToLoginUserDefault = 0;

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
			branchHKG.GB_RL_NKHomePort = "HKHGK";
			branchHKG.GB_Code = "HGK";
			branchHKG.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "CNSHA";

			GlbBranch branchSIN = company.Branches.AddNew();
			branchSIN.GB_RL_NKHomePort = "MOMFM";
			branchSIN.GB_Code = "MFM";

			Factory.Save();

			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();
			BillOfLadingNumberCustomisationElement element = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.BranchCode];
			element.Include = true;
			element.Order = 1;
			element.Fountain = true;

			FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
		}

		protected override void TestWarningsReturnedIfUpdateInactiveShipmentCore()
		{
			// For batch import, it will never update existing shipment. ie the test is not applicable on this case
			Assert(true);
		}

		#region Implementation

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

			ImportBranchRule rule = new ImportBranchRule();
			rule.UseBranchFromXml = false;
			rule.DefaultToBranchRelatedToOriginLoadPort = 0;
			rule.DefaultToBranchRelatedToDestinationDischargePort = 0;
			rule.FallbackRule = ImportBranchRule.FallbackCodes.DefaultToAny;

			SystemDataRegistry.Instance.ShipmentImportBranchRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);
		}

		protected override void AssertAddImportOnSubsequentImport(StmALog[] logs)
		{
			AssertEquals("As second import does not take place during atutomatic import, a new DIM event should not have been added.", 1, logs.Length);
		}

		protected override void AssertNotifyOnImport()
		{
			Assert(Notify.AsString.Contains("House bill number 'HOUSEBILL' already exists. Shipment file has not been imported."));
		}

		protected override Type GetDataAdapterType()
		{
			return typeof(BatchForwardingShipmentValueObjectDataAdapter);
		}

		protected override ValueObjectDataAdapter<ForwardingShipment, Xsd.Shipment> GetNewBizObjXmlDataAdapter()
		{
			return new BatchForwardingShipmentValueObjectDataAdapter();
		}

		protected override ShipmentValueObjectDataAdapter<ForwardingShipment> GetNewShipmentValueObjectDataAdapter()
		{
			return new BatchForwardingShipmentValueObjectDataAdapter();
		}

		#endregion
	}
}
