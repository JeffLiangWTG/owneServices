using Enterprise.DocumentEngine.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class CashBookReports : ZReportModule
	{
		public CashBookReports()
		{
		}

		public override ModuleIdentifier ID
		{
			get
			{
				return ModuleIDs.CashBookReports;
			}
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get
			{
				return Env.Security.CashBookReports;
			}
		}
	}
}
