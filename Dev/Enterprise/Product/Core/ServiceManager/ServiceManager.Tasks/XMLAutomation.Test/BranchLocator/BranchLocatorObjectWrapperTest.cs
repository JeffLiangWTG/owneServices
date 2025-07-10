using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class BranchLocatorObjectWrapperTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var notification = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notification);

			var consolXsd = new Xsd.Consol();
			consolXsd.ConsolDetail.PortOfDischarge = new Xsd.Movement() { Port = Xsd.UNLOCO.FromPortCode(Factory, "YEBLH") };
			consolXsd.ConsolDetail.PortOfLoading = new Xsd.Movement() { Port = Xsd.UNLOCO.FromPortCode(Factory, "ZAAMZ") };

			var shipmentXsd = new Xsd.Shipment();
			shipmentXsd.ShipmentDetails.PortofDestination = new Xsd.Movement() { Port = Xsd.UNLOCO.FromPortCode(Factory, "HKHKG") };
			shipmentXsd.ShipmentDetails.PortOfOrigin = new Xsd.Movement() { Port = Xsd.UNLOCO.FromPortCode(Factory, "USCHI") };

			var branchLocatorWrapper = new BranchLocatorObjectWrapper(context, consolXsd);

			AssertEquals("Discharge Port", consolXsd.DischargePort.Value, branchLocatorWrapper.DestinationOrDischargePort);
			AssertEquals("Load Port", consolXsd.LoadPort.Value, branchLocatorWrapper.OriginOrLoadPort);
			AssertEquals("Import Rule", SystemDataRegistry.Instance.ConsolImportBranchRules.Value.FallbackRule, branchLocatorWrapper.ImportRule.FallbackRule);

			branchLocatorWrapper = new BranchLocatorObjectWrapper(context, shipmentXsd);

			AssertEquals("Destination Port", shipmentXsd.DestinationPort.Value, branchLocatorWrapper.DestinationOrDischargePort);
			AssertEquals("Origin Port", shipmentXsd.OriginPort.Value, branchLocatorWrapper.OriginOrLoadPort);
			AssertEquals("Import Rule", SystemDataRegistry.Instance.ShipmentImportBranchRules.Value.FallbackRule, branchLocatorWrapper.ImportRule.FallbackRule);

			notification.Clear();
			var xmlInterchange = new Xsd.XmlInterchange();
			xmlInterchange.InterchangeInfo.EDIOrganisation.EDICode = GlbCompany.CurrentCompany.OrgProxy.OH_Code;

			context = new ValueObjectImportContext(Factory, xmlInterchange, notification);
			shipmentXsd.ShipmentDetails.PortofDestination = new Xsd.Movement() { Port = Xsd.UNLOCO.FromPortCode(Factory, "ABC") };
			shipmentXsd.ShipmentDetails.PortOfOrigin = new Xsd.Movement() { Port = Xsd.UNLOCO.FromPortCode(Factory, "SYD") };

			branchLocatorWrapper = new BranchLocatorObjectWrapper(context, shipmentXsd);

			var uschiUNLOCO = Xsd.UNLOCO.FromPortCode(Factory, "USCHI");
			var ausydUNLOCO = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");

			AssertEquals("Destination Port", uschiUNLOCO.Value, branchLocatorWrapper.DestinationOrDischargePort);
			AssertEquals("Origin Port", ausydUNLOCO.Value, branchLocatorWrapper.OriginOrLoadPort);
			AssertEquals("Import Rule", SystemDataRegistry.Instance.ShipmentImportBranchRules.Value.FallbackRule, branchLocatorWrapper.ImportRule.FallbackRule);

			consolXsd.ConsolDetail.PortOfDischarge = new Xsd.Movement() { Port = Xsd.UNLOCO.FromPortCode(Factory, "SYD") };
			consolXsd.ConsolDetail.PortOfLoading = new Xsd.Movement() { Port = Xsd.UNLOCO.FromPortCode(Factory, "ABC") };
			notification.Clear();

			branchLocatorWrapper = new BranchLocatorObjectWrapper(context, consolXsd);

			AssertEquals("Loading Port", ausydUNLOCO.Value, branchLocatorWrapper.DestinationOrDischargePort);
			AssertEquals("Discharge Port", uschiUNLOCO.Value, branchLocatorWrapper.OriginOrLoadPort);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupCompany();
			SetupRegistry();
		}

		void SetupRegistry()
		{
			ImportBranchRule rule = new ImportBranchRule()
			{
				UseBranchFromXml = true,
				DefaultToBranchRelatedToOriginLoadPort = 0,
				DefaultToBranchRelatedToDestinationDischargePort = 0,
				FallbackRule = ImportBranchRule.FallbackCodes.DefaultToAny
			};

			SystemDataRegistry.Instance.ConsolImportBranchRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			rule = new ImportBranchRule()
			{
				UseBranchFromXml = true,
				DefaultToBranchRelatedToOriginLoadPort = 0,
				DefaultToBranchRelatedToDestinationDischargePort = 1,
				FallbackRule = ImportBranchRule.FallbackCodes.DoNotCreate
			};

			SystemDataRegistry.Instance.ShipmentImportBranchRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);
		}

		void SetupCompany()
		{
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			if (orgProxy == null)
			{
				orgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery());
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgProxy.PK;
			}

			var portCodePatternMatch = Factory.New<OrgPatternMatchOverride>();
			portCodePatternMatch.OO_OH = orgProxy.PK;
			portCodePatternMatch.OO_ForeignCode = "ABC";
			portCodePatternMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			portCodePatternMatch.OO_LocalGuid = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USCHI")).PK;

			portCodePatternMatch = Factory.New<OrgPatternMatchOverride>();
			portCodePatternMatch.OO_OH = orgProxy.PK;
			portCodePatternMatch.OO_ForeignCode = "SYD";
			portCodePatternMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			portCodePatternMatch.OO_LocalGuid = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD")).PK;

			Factory.Save();
		}
	}
}
