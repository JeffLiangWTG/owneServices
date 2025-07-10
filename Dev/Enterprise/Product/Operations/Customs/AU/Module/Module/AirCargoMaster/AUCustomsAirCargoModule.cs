using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module.AirCargo
{
	public class AUCustomsAirCargoModule : ZFilterGridModule
	{
		public AUCustomsAirCargoModule()
		{
		}

		public override ModuleIdentifier ID => ModuleIDs.Customs.AU.AirCargo;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ACAMaster;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => JobInvoicingConsumerTypes.CusMAWB.Code;

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddInterfaceConnectorCSVImportMenuItem("From CSV File", new EventHandler(OnCSVFileImport));
		}

		internal ZController GetNewControllerInternal(BusinessObject selectedBusinessObject) => GetNewController(selectedBusinessObject);

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			CusMAWB mAWB = selectedBusinessObject as CusMAWB;
			if (mAWB != null && mAWB.Consol != null)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargoConsolController);
			}
			else
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargo);
			}
		}

		protected override IFilterControl GetNewFilterControl() => new AUCustomsAirCargoFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ModuleMAWBCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new AUCustomsAirCargoFilterStripBusinessObject();

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AirCargoReport;

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("Held Shipments Scanning", new EventHandler(HeldShipmentsScanning_Click)));
			return result.ToArray();
		}

		void OnCSVFileImport(object sender, EventArgs args)
		{
			using (DataImporterForm form = DataImporterForm.Create(BillingInterfaceName.AirCargoCsvImport))
			{
				form.Importer = new AirCargoFlatFileDataImporter();
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		void HeldShipmentsScanning_Click(object sender, EventArgs e)
		{
			if (Env.Security.ACAMasterHeldShipmentsScanning.IsAllowed)
			{
				ZFormModaliser.ShowDialogAndDispose(new ScanForHeldShipmentsWizard(new AirScanForOutturnHeldShipmentManager(new BusinessObjectFactory())));
			}
			else
			{
				Globals.Message.Show(HaveNotSecurityRightsForHeldScanning);
			}
		}

		internal const string HaveNotSecurityRightsForHeldScanning = "You do not have the appropriate security rights to run this function.\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: Operations -> Customs -> Air Cargo Report -> Held Shipment Scanning.";
	}
}
