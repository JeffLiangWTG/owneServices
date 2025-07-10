using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class GLAccountFormatModule : ZPopupModule
	{
		public GLAccountFormatModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GLAccountFormat; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#region Implementation

		protected override ZPopupController GetNewController()
		{
			return (ZSingletonController)ZControllerFactory.Create(ControllerIDs.GLAccountFormat);
		}

		#endregion
	}
}
