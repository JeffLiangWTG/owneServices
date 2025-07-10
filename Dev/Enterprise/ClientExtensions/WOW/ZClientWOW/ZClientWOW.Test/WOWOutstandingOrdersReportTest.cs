using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.Wow.Testing
{
	public class WOWOutstandingOrdersReportTest : ClientSpecificReportTestCase
	{
		public override ClientSpecificTemplateTestCase GetClientSpecificTemplateTestCase()
		{
			return new WOWOutstandingOrdersReportTemplateTest();
		}

		public override string Hint
		{
			get
			{
				return string.Empty;
			}
		}

		public override string MenuName
		{
			get
			{
				return "Outstanding Orders Report";
			}
		}

		public override ModuleIdentifier ModuleIdToTest
		{
			get
			{
				return ModuleIDs.OrdersReport;
			}
		}
	}
}
