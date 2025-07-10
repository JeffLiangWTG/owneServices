using Enterprise.Accounting.GUI.PeriodManagement;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
#if DEBUG
	[TestExcludeZFilterGridModulesAllHaveModuleBashers]
#endif
	public class ComPayRegisteredOrganisationsModule : ZEmbeddedModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ComPayRegisteredOrganisations; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#region Implementation

		protected override System.Windows.Forms.Control GetNewEmbeddedControl()
		{
			ComPayRegisteredOrganisationsControl formToReturn = new ComPayRegisteredOrganisationsControl();
			return formToReturn;
		}

		#endregion
	}
}
