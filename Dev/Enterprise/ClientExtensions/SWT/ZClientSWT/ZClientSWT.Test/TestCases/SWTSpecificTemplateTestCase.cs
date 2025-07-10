using CargoWise.Definitions;
using Enterprise.ReportTesting;

namespace Enterprise.Client.SWT
{
	public static class SWTSpecificTemplateTestCase
	{
		[TemplateName("SWT Not Yet Arrived Report")]
		public class SWTNotYetArrivedTemplateTestCase : ClientSpecificTemplateTestCase
		{
			protected override Clients ClientCode
			{
				get
				{
					return Clients.SWT;
				}
			}

			protected override bool ReportRequiresColumnHeadings
			{
				get
				{
					return false;
				}
			}
		}
		// add new class for each report here... similar to the above
	}
}
