using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ReceivReports : ZReportModule
	{
		public ReceivReports()
		{
		}

		public override ModuleIdentifier ID
		{
			get
			{
				return ModuleIDs.ReceivReports;
			}
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get
			{
				return Env.Security.ReceivablesReports;
			}
		}
	}
}
