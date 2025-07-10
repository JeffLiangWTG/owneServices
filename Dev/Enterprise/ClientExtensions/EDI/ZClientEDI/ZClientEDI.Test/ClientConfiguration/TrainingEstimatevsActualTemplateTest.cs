using CargoWise.Definitions;
using Enterprise.ReportTesting;

namespace Enterprise.Client.EDI.Test.Reports
{
	[TemplateName("Training Estimated vs Actual Report")]
	public class TrainingEstimatevsActualTemplateTest : ClientSpecificTemplateTestCase
	{
		protected override Clients ClientCode => Clients.EDI;
		protected override bool ReportRequiresColumnHeadings => false;
	}
}
