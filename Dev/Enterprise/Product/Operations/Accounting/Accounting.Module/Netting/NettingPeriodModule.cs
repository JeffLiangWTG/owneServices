using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class NettingPeriodModule : ZFilterGridModule
	{
		public NettingPeriodModule()
		{
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.NettingPeriod);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new NettingPeriodFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new NettingPeriodFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new NettingSystemPeriodCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.NettingPeriod; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}
	}
}
