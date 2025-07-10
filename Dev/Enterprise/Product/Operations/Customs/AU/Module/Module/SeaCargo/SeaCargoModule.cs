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

namespace Enterprise.Customs.AU.Module
{
	/// <summary>
	/// Module for SeaCargo.
	/// </summary>
	public class SeaCargoModule : CMRModule
	{
		public SeaCargoModule()
		{
			AddInterfaceConnectorCSVImportMenuItem("From CSV file", new EventHandler(OnCSVFileImport));
		}

		#region Checkpoints

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AUCustomsSCA; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.SeaCargoReport; }
		}

		#endregion

		#region Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.AU.SeaCargo; }
		}

		internal ZController GetNewControllerInternal(BusinessObject selectedBusinessObject) => GetNewController(selectedBusinessObject);

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			CusSCAOceanBill oceanBill = selectedBusinessObject as CusSCAOceanBill;
			if (oceanBill != null && oceanBill.Consol != null)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.AU.SeaCargo);
			}
			else
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.AU.SeaCargoStandAloneController);
			}
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new SeaCargoFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusSCAOceanBillCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new SeaCargoFilterStripBusinessObject();
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.CusSCAOceanBillWorkflowDescriptorCode; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("Held Shipments Scanning", new EventHandler(HeldShipmentsScanning_Click)));
			return result.ToArray();
		}

		void HeldShipmentsScanning_Click(object sender, EventArgs e)
		{
			if (Env.Security.AUCustomsSCAHeldShipmentsScanning.IsAllowed)
			{
				ZFormModaliser.ShowDialogAndDispose(new ScanForHeldShipmentsWizard(new SeaScanForOutturnHeldShipmentManager(new BusinessObjectFactory())));
			}
			else
			{
				Globals.Message.Show(HaveNotSecurityRightsForHeldScanning);
			}
		}
		internal const string HaveNotSecurityRightsForHeldScanning = "You do not have the appropriate security rights to run this function.\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: Operations -> Customs -> Sea Cargo Report -> Held Shipment Scanning.";

		#endregion

		#region Events

		void OnCSVFileImport(object sender, EventArgs e)
		{
			using (DataImporterForm form = DataImporterForm.Create(BillingInterfaceName.SeaCargoCsvImport))
			{
				form.Importer = new SeaCargoFlatFileDataImporter();
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		#endregion
	}
}
