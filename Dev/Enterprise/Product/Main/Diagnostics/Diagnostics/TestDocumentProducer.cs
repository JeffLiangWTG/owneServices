using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Diagnostics
{
	public class TestDocumentProducer
	{
		public TestDocumentProducer()
		{
		}

		readonly BusinessObjectFactory Factory = new BusinessObjectFactory();

		public void RunPrintTestDocument()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			DocumentWrappers.DocShipment shipmentWrapper = Enterprise.DocumentWrappers.DocShipment.New(shipment, Factory);
			ExcelTemplate template = new ExcelTemplateReadFromExcelTemplatesSolution(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames.PrintTest);

			RunPack(template, null, "Test Document");
		}

		public void RunCoverSheetDocument()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			DocumentWrappers.DocShipment shipmentWrapper = Enterprise.DocumentWrappers.DocShipment.New(shipment, Factory);
			StmMenuTemplatePivot coverSheetMenuTemplatePivot = GetCoverSheetMenuTemplatePivot();
			RunPack(coverSheetMenuTemplatePivot.Template, shipmentWrapper, "Test Cover Sheet");
		}

		public void RunHAWBTestDocument()
		{
			ShipmentExportAWBHeader aWBHeader = Factory.New<ShipmentExportAWBHeader>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			shipment.Consols.AddNew();

			Transport transport = shipment.Consols[0].Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			transport.JW_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			OrgHeader consignor = Factory.New<OrgHeader>();

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignor.PK;

			aWBHeader.EH_ParentID = shipment.PK;
			OrgHeader sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "Name";
			sendingForwarder.MainAddress.OA_Address1 = "Address1";
			sendingForwarder.MainAddress.OA_Address2 = "Address2";
			aWBHeader.Consol.SetDefaultSendingForwarderAddress(sendingForwarder);
			aWBHeader.AWBOtherCharges.AddNew();
			aWBHeader.AWBOtherCharges.AddNew();
			aWBHeader.AWBAccountingInformations.AddNew();
			aWBHeader.AWBAccountingInformations.AddNew();

			DocumentWrappers.DocAWB aWBWrapper = Enterprise.DocumentWrappers.DocAWB.New(aWBHeader, Factory);

			StmMenuTemplatePivot hAWBMenuTemplatePivot = GetHAWBMenuTemplatePivot();
			RunPack(hAWBMenuTemplatePivot.Template, aWBWrapper, "Test HAWB");
		}

		StmMenuTemplatePivot GetCoverSheetMenuTemplatePivot()
		{
			ZQuery menuItemFilter = new DocumentZQuery("Shipment", "Cover Sheet");
			menuItemFilter.AddToFilter(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuPath, "Legacy Documents");

			StmMenuItem[] menuItems = Factory.Load<StmMenuItem>(menuItemFilter);

			ZQuery pivotFilter = new ZQuery(ZArchitecture.Schema.StmMenuTemplatePivotSchema.SI_SU, menuItems[0].PK);
			StmMenuTemplatePivot[] pivots = (StmMenuTemplatePivot[])Factory.Load(typeof(StmMenuTemplatePivot), pivotFilter);
			return pivots[0];
		}

		StmMenuTemplatePivot GetHAWBMenuTemplatePivot()
		{
			ZQuery menuItemFilter = new ZQuery(ZArchitecture.Schema.StmMenuItemSchema.SU_MenuName, "Laser HAWB");
			menuItemFilter.AddToFilter(JoinCondition.And, ZArchitecture.Schema.StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.Equal, "Shipment");
			menuItemFilter.AddToFilter(JoinCondition.And, ZArchitecture.Schema.StmMenuItemSchema.SU_IsSystemDefined, SQLComparisonOperator.Equal, true);
			StmMenuItem[] menuItems = (StmMenuItem[])Factory.Load(typeof(StmMenuItem), menuItemFilter);

			ZQuery pivotFilter = new ZQuery(ZArchitecture.Schema.StmMenuTemplatePivotSchema.SI_SU, menuItems[0].PK);
			pivotFilter.AddToFilter(JoinCondition.And, ZArchitecture.Schema.StmMenuTemplatePivotSchema.SI_DocumentTitle, SQLComparisonOperator.Contains, "Original 1");
			StmMenuTemplatePivot[] pivots = (StmMenuTemplatePivot[])Factory.Load(typeof(StmMenuTemplatePivot), pivotFilter);
			return pivots[0];
		}

		void RunPack(StmTemplate template, DocumentWrapper wrapper, string documentName)
		{
			ExcelTemplate excelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);

			RunPack(excelTemplate, wrapper, documentName);
		}

		void RunPack(ExcelTemplate excelTemplate, DocumentWrapper wrapper, string documentName)
		{
			using (Factory.GetDocWrapperContextManager().SetContextValuesTemporarily(DocumentDirection.ANY, ContactType.All))
			using (PrintTask printTask = new PrintTask())
			{
				DocumentPack documentPack = new DocumentPack();
				documentPack.Add(new Report(documentPack, excelTemplate, wrapper, documentName, null, DocumentDirection.ANY, false));

				printTask.Add(documentPack);
				printTask.Run(EnvProxy.Instance.Security.None);
			}
		}
	}
}
