using CargoWise.Definitions;
namespace Enterprise.Client.EDI.Test.Reports
{
	using Enterprise.ReportTesting;

	[TemplateName("Installation Projects Status Report")]
	public class TestInstallationProjectsStatusReport : ClientSpecificTemplateTestCase
	{
		protected override Clients ClientCode => Clients.EDI;
	}
}
