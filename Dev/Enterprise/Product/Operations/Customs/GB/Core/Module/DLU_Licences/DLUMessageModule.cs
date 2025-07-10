using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Chief.Messaging.DLU;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module.DLU
{
	public class DLUMessageModule : ZFilterGridModule
	{
		protected override IFilterControl GetNewFilterControl()
		{
			return new DLUMessageFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.GB.DLUController);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DLUMessageFilterStripBusinessObject();
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowView
		{
			get { return true; }
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new DLUMessageCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.EU.GB.DLUMessage; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CustomsFiles; }
		}
	}
}
