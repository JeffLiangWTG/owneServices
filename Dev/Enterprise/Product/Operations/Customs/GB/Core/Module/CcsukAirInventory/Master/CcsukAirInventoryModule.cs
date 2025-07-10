using System;
using System.Windows.Forms;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukAirInventoryModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public CcsukAirInventoryModule()
			: base()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public OperationalActionSupporter OperationalActionSupporter
		{
			get { return new CcsukOperationalActionSupporterMawb(); }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CcsukAirInventoryFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override ZController GetNewController(CargoWise.EntityFramework.BusinessObject selectedBusinessObject)
		{
			var mawbOrBasic = selectedBusinessObject as CusMAWB;
			if (mawbOrBasic != null && mawbOrBasic.Consol != null)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.GB.CcsukAirInventoryInConsol);
			}
			else
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.GB.CcsukAirInventory);
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CcsukAirInventoryFilterStripBusinessObject();
		}

		protected override CargoWise.EntityFramework.IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusMAWBCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.EU.GB.CcsukAirInventory; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AirCcsukBase; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AirCcsukMaster; }
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			MenuItem[] result = base.GetNewStandardMenuItems();
			if (NewMenuItem != null)
			{
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("1C53F424-AE40-464C-84FD-4F44AE71B028", "New AWB"), HandleNewClick));
				if (LicenceAndPimaHelper.ShedEnabled)
				{
					NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("B0CB771A-9A5A-45DC-9AEE-95659ED2A9D8", "New UFO"), CreateUFO));
				}
			}
			return result;
		}

		void CreateUFO(object sender, EventArgs e)
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.InitialiseUFO();
			var controller = (CcsukAirInventoryControllerUFO)ZControllerFactory.Create(ControllerIDs.Customs.GB.CcsukAirInventoryUFO);
			controller.SetNewBusinessObjectToReturn(mawb);
			controller.ShowNewForm();
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }  // This doesn't really do anything.   What's the point? 
		}

		public override string WorkflowType
		{
			get { return JobInvoicingConsumerTypes.CusMAWB.Code; }
		}
	}
}
