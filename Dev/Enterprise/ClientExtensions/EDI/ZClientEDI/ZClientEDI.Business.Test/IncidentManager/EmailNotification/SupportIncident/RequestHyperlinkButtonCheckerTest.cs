using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(RequestHyperlinkButtonChecker))]
	public class RequestHyperlinkButtonCheckerTest : TestCaseWithFactory
	{
		public void TestCanAddButton()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();

			var emailEmpty = SupportIncidentEmail.New(incident);

			var emailOnlyUrl = SupportIncidentEmail.New(incident);
			emailOnlyUrl.Body = SupportIncidentEmailBodyGeneralControls.GetIncidentGlowUrl(incident);

			var emailFullButton = SupportIncidentEmail.New(incident);
			emailFullButton.Body = SupportIncidentEmailBodyGeneralControls.GetReplyViaEConversationButton(incident);

			var emailWithAnotherIncident = SupportIncidentEmail.New(incident);
			emailWithAnotherIncident.Body = SupportIncidentEmailBodyGeneralControls.GetIncidentGlowUrl(incident2);

			AssertEquals(true, RequestHyperlinkButtonChecker.CanAddButton(incident, emailEmpty.Body));
			AssertEquals(true, RequestHyperlinkButtonChecker.CanAddButton(incident, emailOnlyUrl.Body));
			AssertEquals(false, RequestHyperlinkButtonChecker.CanAddButton(incident, emailFullButton.Body));
			AssertEquals(true, RequestHyperlinkButtonChecker.CanAddButton(incident, emailWithAnotherIncident.Body));
		}

		public void TestCheckAndGetEmailBodyWithButton()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var url = SupportIncidentEmailBodyGeneralControls.GetIncidentGlowUrl(incident);
			var button = SupportIncidentEmailBodyGeneralControls.GetReplyViaEConversationButton(incident);

			var emailEmpty = string.Empty;
			var emailOnlyUrl =  SupportIncidentEmailBodyGeneralControls.GetIncidentGlowUrl(incident);
			var emailFullButton = SupportIncidentEmailBodyGeneralControls.GetReplyViaEConversationButton(incident);
			var emailOnlyIdentifier = SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID + incident.Number;
			var emailWithErrorIncidentUrl = SupportIncidentEmailBodyGeneralControls.GetIncidentGlowUrl(incident2);

			AssertEquals($"<p>{button}</p>Regards,<br/>", RequestHyperlinkButtonChecker.CheckAndGetEmailBodyWithButton(incident, emailEmpty));
			AssertContains("Button should be added even if it's body only contains url already", SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID, RequestHyperlinkButtonChecker.CheckAndGetEmailBodyWithButton(incident, emailOnlyUrl));
			AssertEquals("Button should be only one", 1, Regex.Matches(RequestHyperlinkButtonChecker.CheckAndGetEmailBodyWithButton(incident, emailFullButton), SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID).Count);
			AssertNotContains("Checker should not add button for email even if it only has button identifier", url, RequestHyperlinkButtonChecker.CheckAndGetEmailBodyWithButton(incident, emailOnlyIdentifier));
			AssertContains("Checker should add button for the email which is taking another incident's url", url, RequestHyperlinkButtonChecker.CheckAndGetEmailBodyWithButton(incident, emailWithErrorIncidentUrl));
		}

		public void TestCheckAndGetEmailBodyWithCreateFollowUpERequestButton()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var url = SupportIncidentEmailBodyGeneralControls.GetIncidentGlowUrl(incident);
			var button = SupportIncidentEmailBodyGeneralControls.GetCreateFollowUpERequestButton(incident);

			var emailOnlyUrl = SupportIncidentEmailBodyGeneralControls.GetIncidentGlowUrl(incident);
			var emailFullButton = SupportIncidentEmailBodyGeneralControls.GetReplyViaEConversationButton(incident);
			var emailOnlyIdentifier = SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID + incident.Number;
			var emailWithErrorIncidentUrl = SupportIncidentEmailBodyGeneralControls.GetIncidentGlowUrl(incident2);

			AssertContains("Button should be added even if it's body only contains url already", SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID, RequestHyperlinkButtonChecker.CheckAndGetEmailBodyWithButton(incident, emailOnlyUrl));
			AssertEquals("Button should be only one", 1, Regex.Matches(RequestHyperlinkButtonChecker.CheckAndGetEmailBodyWithButton(incident, emailFullButton), SupportIncidentEmailBodyGeneralControls.EConversationHyperlinkButtonID).Count);
			AssertNotContains("Checker should not add button for email even if it only has button identifier", url, RequestHyperlinkButtonChecker.CheckAndGetEmailBodyWithButton(incident, emailOnlyIdentifier));
			AssertContains("Checker should add button for the email which is taking another incident's url", url, RequestHyperlinkButtonChecker.CheckAndGetEmailBodyWithButton(incident, emailWithErrorIncidentUrl));
		}

		public void TestConfirmResolvedButton()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			var emailWithConfirmResolvedButton = SupportIncidentEmailBodyGeneralControls.GetConfirmResolvedButton(incident);

			var token = Factory.Load<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.IncidentEmailActionLink)).Single();
			AssertEquals(incident.PK, token.SAT_ParentId);
			AssertEquals(IncidentMainSchema.Constants.Prefix, token.SAT_ParentTableCode);
			AssertEquals(1, token.SAT_RemainingUseCount);
			AssertEquals(false, token.SAT_IsPermanentToken);
			AssertEquals(DateTime.UtcNow.AddDays(7).Date, token.SAT_ExpiresAt.Date);

			AssertContains(SupportIncidentEmailBodyGeneralControls.ConfirmResolvedButtonID, emailWithConfirmResolvedButton);
			AssertContains(token.SAT_Token, emailWithConfirmResolvedButton);
		}

		public void TestGetIncidentGlowUrl()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "PYNTSTORG";
			var contact = org.Contacts.AddNew();

			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;

			Factory.Save();

			var incidentGlowUrl = SupportIncidentEmailBodyGeneralControls.GetIncidentGlowUrl(incident);
			AssertContains("Incident glow url should contain orgCode", "OrgCode=PYNTSTORG", incidentGlowUrl);
		}
	}
}
