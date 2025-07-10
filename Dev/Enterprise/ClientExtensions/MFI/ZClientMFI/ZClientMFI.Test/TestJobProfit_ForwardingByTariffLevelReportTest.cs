using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.MFI.Testing
{
	public class TestJobProfit_ForwardingByTariffLevelReportTest : ClientSpecificReportTestCase
	{
		public override ClientSpecificTemplateTestCase GetClientSpecificTemplateTestCase()
		{
			return new TestJobProfit_ForwardingByTariffLevelRoutine();
		}

		public override ModuleIdentifier ModuleIdToTest
		{
			get
			{
				return ModuleIDs.JobCostingReport;
			}
		}

		public override string MenuName
		{
			get
			{
				return "MFI Job Profit - Forwarding by Tariff Level";
			}
		}

		public override string Hint
		{
			get
			{
				return "";
			}
		}
	}
}
