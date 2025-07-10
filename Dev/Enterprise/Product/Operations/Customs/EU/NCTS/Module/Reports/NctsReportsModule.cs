using Enterprise.Customs.Module;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.Module
{
	public class NctsReportsModule : CustomsReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.EU.NctsReportsModule; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.NCTS; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.EuNctsReports; }
		}
	}
}
