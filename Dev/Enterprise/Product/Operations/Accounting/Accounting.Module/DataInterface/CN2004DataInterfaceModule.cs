using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class CN2004DataInterfaceModule : ZPopupModule
	{
		public CN2004DataInterfaceModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CN2004DataInterface; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected override ZPopupController GetNewController()
		{
			return new CN2004DataInterfaceController();
		}
	}
}
