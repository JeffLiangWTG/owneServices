using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class NettingStatementModule : ZPopupModule
	{
		protected override ZPopupController GetNewController()
		{
			return (ZSingletonController)ZControllerFactory.Create(ControllerIDs.NettingStatement);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.NettingStatement; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}
	}
}
