using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class PayablesReports : ZReportModule
	{
		public PayablesReports()
		{
		}

		public override ModuleIdentifier ID
		{
			get
			{
				return ModuleIDs.PayablesReports;
			}
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get
			{
				return Env.Security.PayablesReports;
			}
		}
	}
}
