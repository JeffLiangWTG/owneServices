using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class EDIEmailBuilderTest : TestCaseWithFactory
	{
		public void TestGetInstance()
		{
			AssertNoExceptionThrown(() =>
			{
				IEDIEmailTriggeringRules rules = null;
				var builder = EDIEmailBuilder.GetInstance(rules);
				builder.BuildEmailDefByTemplate(new DefaultNotificationEmailTemplate());
				builder.BuildHtmlEmailDefByTemplate(new DefaultNotificationEmailTemplate());
			});

			AssertNoExceptionThrown(() =>
			{
				DummyEDIEmailBizo bizo = null;
				var builder = EDIEmailBuilder.GetInstance(bizo);
				builder.BuildEmailDefByTemplate(new DefaultNotificationEmailTemplate());
				builder.BuildHtmlEmailDefByTemplate(new DefaultNotificationEmailTemplate());
			});

			AssertNoExceptionThrown(() =>
			{
				var bizo = new DummyEDIEmailBizo();
				var builder = EDIEmailBuilder.GetInstance(bizo);
				builder.BuildEmailDefByTemplate(new DefaultNotificationEmailTemplate());
				builder.BuildHtmlEmailDefByTemplate(new DefaultNotificationEmailTemplate());
			});
		}

		public void TestSuppressingEmail()
		{
			var incident = Factory.New<SupportIncident>();
			var jobHeader = ProcessJobHeader.GetForParent(incident, incident.Factory);
			var templateSuppressed = new NewIncidentRaisedInternalEmailContentBuilder(incident, "NTZ");
			var templateUnsuppressed = new SupportIncidentCorrespondenceEmailContentBuilder(incident);
			var blnGroup = Factory.New<TagDefinition>();
			blnGroup.TGD_Code = "BLN";
			var tag = blnGroup.Magnitudes.AddNew();
			tag.TGM_Code = templateSuppressed.TemplateCode;
			jobHeader.AddTag(tag);
			Factory.Save();

			var builder = EDIEmailBuilder.GetInstance(incident);
			var email = builder.BuildEmailDefByTemplate(templateUnsuppressed);
			var  htmlEmail = builder.BuildHtmlEmailDefByTemplate(templateUnsuppressed);
			var log = incident.Logs.MostRecentLogByEventTime(Enterprise.ZArchitecture.Business.Events.EmailSent);
			AssertNull(log);
			AssertNotNull(email);
			AssertNotNull(htmlEmail);

			AssertContains("correspondence", email.Body);
			AssertContains("correspondence", htmlEmail.Body);

			AssertContains("on Incident", email.Subject);
			AssertContains("on Incident", htmlEmail.Subject);

			builder = EDIEmailBuilder.GetInstance(incident);
			email = builder.BuildEmailDefByTemplate(templateSuppressed);
			htmlEmail = builder.BuildHtmlEmailDefByTemplate(templateSuppressed);
			log = incident.Logs.MostRecentLogByEventTime(Enterprise.ZArchitecture.Business.Events.EmailSent);
			AssertNotNull(log);
			AssertEquals(string.Format(EDIEmailBuilder.LogReference, templateSuppressed.TemplateDescription, templateSuppressed.TemplateCode), log.SL_Reference);
			AssertNull(email);
			AssertNull(htmlEmail);
		}

		public void TestSubjectLineProductCodePrefix_MatchesIncidentProductCode()
		{
			var incident = Factory.New<SupportIncident>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_OH_Client = org1.PK;
			incident.Client.OH_FullName = "organisation";
			var template = new NewIncidentRaisedInternalEmailContentBuilder(incident, "NTZ");
			Factory.Save();
			var builder = EDIEmailBuilder.GetInstance(incident);
			var email = builder.BuildEmailDefByTemplate(template);

			AssertContains("[ENT] A CR2 incident raised by client organisation", email.Subject);
		}

		class DummyEDIEmailBizo : IEDIEmailTriggeringRulesProvider
		{
			public IEDIEmailTriggeringRules TriggeringRules => null;
		}
	}
}
