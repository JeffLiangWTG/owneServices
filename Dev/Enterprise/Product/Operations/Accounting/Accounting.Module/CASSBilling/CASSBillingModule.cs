using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class CASSBillingModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CASSCostFileImport; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Accountant; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.APCASSCostFileImport; }
		}

		protected override ZPopupController GetNewController()
		{
			return (ZSingletonController)ZControllerFactory.Create(ControllerIDs.CASSCostFileImport);
		}
	}
}