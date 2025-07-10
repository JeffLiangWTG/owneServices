using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using NUnit.Framework;
using ZClientEDI.Business.Test.IncidentManager.EmailNotification;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(CustomerServiceFinalClosureAutoReplyEmailContentBuilder))]
	public class CustomerServiceFinalClosureAutoReplyEmailContentBuilderTest : EmailContentBuilderTest<CustomerServiceFinalClosureAutoReplyEmailContentBuilder>
	{
		protected override string ExpectedEmailBody => @$"Incident {Incident.Number} - {Incident.IM_Description} - is Closed.
Should you need any further help with regards to this incident, please create a follow-up eRequest using the button below.";

		protected override string ExpectedEmailSubject => $"eRequest: {Incident.Number} is Closed.";

		protected override CustomerServiceFinalClosureAutoReplyEmailContentBuilder GetEmailTemplateBuilder()
		{
			return new CustomerServiceFinalClosureAutoReplyEmailContentBuilder(Incident, false, Incident.IM_Product, "DFT");
		}

		SupportIncident Incident
		{
			get
			{
				if (incident == null)
				{
					incident = Factory.NewWithValidTestData<SupportIncident>();
					incident.IM_Product = "ENT";
					Factory.Save();
				}

				return incident;
			}
		}
		SupportIncident incident;
	}
}
