using CargoWise.Types;
using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.SWT
{
	public static class SWTSpecificReportTestCase
	{
		#region SWTNotYetArrivedReportTestCase
		public class SWTNotYetArrivedReportTestCase : ClientSpecificReportTestCase
		{
			public override ClientSpecificTemplateTestCase GetClientSpecificTemplateTestCase()
			{
				return new SWTSpecificTemplateTestCase.SWTNotYetArrivedTemplateTestCase();
			}

			public override string MenuName
			{
				get
				{
					return "Not Yet Arrived";
				}
			}

			public override ModuleIdentifier ModuleIdToTest
			{
				get
				{
					return ModuleIDs.OrdersReport;
				}
			}

			public override string Hint
			{
				get
				{
					return ZString.Empty;
				}
			}
		}
		#endregion
		// add new class for each report here... similar to the above
	}
}
