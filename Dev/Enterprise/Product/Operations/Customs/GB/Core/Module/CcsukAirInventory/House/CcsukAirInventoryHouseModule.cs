using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukAirInventoryHouseModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public CcsukAirInventoryHouseModule()
			: base()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public OperationalActionSupporter OperationalActionSupporter
		{
			get { return new CcsukOperationalActionSupporterHawb(); }
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new CcsukAirInventoryHouseFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var hawb = selectedBusinessObject as CusHAWB;
			if (hawb != null && hawb.Shipment != null)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.GB.CcsukAirInventoryHouseInShipment);
			}
			else
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.GB.CcsukAirInventoryHouse);
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CcsukAirInventoryHouseFilterStripBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusHAWBCollectionNonDependent(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.EU.GB.CcsukAirInventoryHouse; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AirCcsukBase; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AirCcsukHouse; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.CustomsHouseAirCargoCode; }
		}
	}
}
