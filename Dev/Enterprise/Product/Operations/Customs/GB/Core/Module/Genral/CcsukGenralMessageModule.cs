using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukGenralMessageModule : ZFilterGridModule
	{
		protected override IFilterControl GetNewFilterControl()
		{
			return new CcsukGenralMessageFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.GB.CcsukGenralMessage);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CcsukGenralMessageFilterStripBusinessObject();
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GenralEdiMessageCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.EU.GB.CcsukGenralMessage; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AirCcsukBase; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AirCcsukGenral; }
		}
	}
}

