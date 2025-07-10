using System;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ZClientEDI.Business.Test.IncidentManager.EmailNotification;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentCriticalityChangedEmailContentBuilder))]
	public class SupportIncidentCriticalityChangedEmailContentBuilderTest : EmailContentBuilderTest<SupportIncidentCriticalityChangedEmailContentBuilder>
	{
		protected override string ExpectedEmailBody => $@"This is an update to {SupportIncidentEmailBodyGeneralControls.GetIncidentGlowHyperlink(Incident)}<br/>Test Only EmailBody<br/>";

		protected override string ExpectedEmailSubject => "Test Only Subject";

		protected override SupportIncidentCriticalityChangedEmailContentBuilder GetEmailTemplateBuilder()
		{
			return new SupportIncidentCriticalityChangedEmailContentBuilder(Incident, false, Incident.IM_Product, "DEF");
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

		protected override void SetUp()
		{
			base.SetUp();
			var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
			var templatePair = collection.AddNew();
			templatePair.Code = "DEF";
			templatePair.Product = "ENT";
			templatePair.Description = (NoResString)"Default Email Template";
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "Test Only Subject";
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "Test Only EmailBody";
			templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailSubject = "Test Only Subject";
			templatePair.EmailTemplates.ERequestV2EmailTemplate.EmailBody = "Test Only EmailBody";

			EDIDataRegistry.Instance.IncidentCriticalityChangedRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}
	}
}
