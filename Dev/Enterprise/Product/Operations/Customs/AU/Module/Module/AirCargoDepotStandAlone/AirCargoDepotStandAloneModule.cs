using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module.AirCargo
{
	/// <summary>
	/// Module Controller for AUCustomsAirCargo.
	/// </summary>
	public class AirCargoDepotStandAloneModule : ZFilterGridModule
	{
		public AirCargoDepotStandAloneModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.AU.AirCargoDepot; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			CusMAWB mAWB = selectedBusinessObject as CusMAWB;
			if (mAWB != null && mAWB.Consol != null)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargoConsolController);
			}
			else
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargoDepot);
			}
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AirCargoDepotStandAloneFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ModuleMAWBCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AirCargoDepotStandAloneFilterStripBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AirCargoDepot; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ACAMasterDepot; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return JobInvoicingConsumerTypes.CusMAWB.Code; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("Held Shipments Scanning", new EventHandler(HeldShipmentsScanning_Click)));
			return result.ToArray();
		}

		void HeldShipmentsScanning_Click(object sender, EventArgs e)
		{
			if (Env.Security.ACAMasterDepotHeldShipmentsScanning.IsAllowed)
			{
				ZFormModaliser.ShowDialogAndDispose(new ScanForHeldShipmentsWizard(new AirScanForOutturnHeldShipmentManager(new BusinessObjectFactory())));
			}
			else
			{
				Env.Security.ACAMasterDepotHeldShipmentsScanning.ShowError();
			}
		}
	}
}
