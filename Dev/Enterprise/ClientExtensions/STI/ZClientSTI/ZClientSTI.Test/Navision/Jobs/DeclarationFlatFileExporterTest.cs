using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	[TestedType(typeof(DeclarationFlatFileExporter))]
	public class DeclarationFlatFileExporterTest : FlatFileDataExporterTestCase
	{
		public override FlatFileDataExporter GetDataExporter()
		{
			return new DeclarationFlatFileExporter(Factory, Instructions, "blah", "");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetDataExporter();
		}

		public override IBusinessObjectCollection GetPopulatedCollectionToSaveAndExport()
		{
			BaseJobDeclarationCollection declarations = new BaseJobDeclarationCollection(Factory);
			BaseJobDeclaration declaration = declarations.AddNew();
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>(TestBusinessObjectKind.MinimumRequiredToSave);
			shipment.JS_UniqueConsignRef = "S0001000";
			ZDateTime departure = new ZDateTime(2006, 2, 22, 10, 14, 38);
			ZDateTime arrival = new ZDateTime(2006, 2, 23, 12, 32, 19);
			shipment.JS_E_ARV = arrival;
			shipment.JS_E_DEP = departure;
			shipment.JS_RL_NKOrigin = "NGASD";
			shipment.JS_RL_NKDestination = "USLAX";
			declaration.JE_AgentsReference = "8YEWHKLHF";
			declaration.JE_DeclarationReference = "B000000938";
			declaration.JE_MasterBill = "M0000375987";
			declaration.JE_HouseBill = "HUOSE12830928";
			declaration.JE_OH_Importer = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_IsConsignee, ZBool.True)).PK;
			declaration.JE_OH_Supplier = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_IsConsignor, ZBool.True)).PK;
			declaration.JE_OwnerRef = "owner reference";
			declaration.JE_GoodsDescription = "oil for australian wheat";
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Qantas";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "Air";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "QF1234";
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_E_DEP = departure;
			origin.JA_A_DEP = departure;
			origin.JA_RL_NKPortOfLoading = "USLAX";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_E_ARV = arrival;
			destination.JB_A_ARV = arrival;
			destination.JB_RL_NKPortOfDischarge = "GBLON";
			voyage.GenerateSailings();
			shipment.JS_JX = voyage.Sailings[0].PK;
			declaration.JE_JS = shipment.PK;
			declaration.JE_ExportDate = departure;
			declaration.JE_DateOfArrival = arrival;
			declaration.JE_DateAtOrigin = departure;
			declaration.JE_DateAtFinalDestination = arrival;
			declaration.JE_RL_NKPortOfLoading = "GBLON";
			declaration.JE_RL_NKPortOfArrival = "USLAX";
			Factory.Save();
			return declarations;
		}

		#region Instructions
		ExportInstructions Instructions
		{
			get
			{
				if (fInstructions == null)
				{
					fInstructions = new ExportInstructions();
					fInstructions.BasePath = Env.TempPath;
					fInstructions.FileExtension = FileExtensionType.Csv;
					fInstructions.MethodOfExport = ExportType.File;
				}

				return fInstructions;
			}
		}

		ExportInstructions fInstructions;
		#endregion
	}
}
