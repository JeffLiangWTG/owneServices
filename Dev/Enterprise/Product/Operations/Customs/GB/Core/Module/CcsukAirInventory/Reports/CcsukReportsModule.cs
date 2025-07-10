using Enterprise.Customs.Module;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukReportsModule : CustomsReportModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.EU.GB.GbCcsukReports; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AirCcsukBase; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AirCcsukReports; }
		}
	}
}
