using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class CNDataInterfaceModule : ZPopupModule
	{
		public CNDataInterfaceModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CNDataInterface; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ChinaDataInterface; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected override ZPopupController GetNewController()
		{
			return new CNDataInterfaceController();
		}
	}
}
