using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.ZAudit.PlugIn
{
	public class AuditModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Audit; }
		}

		protected override ZPopupController GetNewController()
		{
			return new AuditController();
		}

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AuditServices; }
		}

		#endregion
	}
}
