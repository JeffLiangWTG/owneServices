using CargoWise.Definitions;
using Enterprise.MarketingManager.Business;
using Enterprise.ReportTesting;

namespace Enterprise.Client.EDI.Test.Reports
{
	[TemplateName("Customer Service Survey Result Report")]
	public class CustomerServiceSurveyResultReportTemplateTest : ClientSpecificTemplateTestCase
	{
		protected override Clients ClientCode => Clients.EDI;
		protected override void SetUp()
		{
			base.SetUp();
			Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();
		}
	}
}
