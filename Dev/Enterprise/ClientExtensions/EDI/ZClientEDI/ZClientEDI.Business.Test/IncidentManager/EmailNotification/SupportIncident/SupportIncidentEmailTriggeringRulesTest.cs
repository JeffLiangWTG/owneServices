using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class SupportIncidentEmailTriggeringRulesTest : TestCaseWithFactory
	{
		public void TestRule_AllowGeneratingByCode()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var jobHeader = ProcessJobHeader.GetForParent(incident, incident.Factory);
			AssertNotNull(jobHeader);
			Assert(!jobHeader.Tags.Any());

			var abcGroup = Factory.New<TagDefinition>();
			abcGroup.TGD_Code = "ABC";
			var blnGroup = Factory.New<TagDefinition>();
			blnGroup.TGD_Code = "BLN";

			var abcTag = abcGroup.Magnitudes.AddNew();
			abcTag.TGM_Code = "ABC";
			jobHeader.AddTag(abcTag);
			Factory.Save();

			Assert(jobHeader.Tags.Count() == 1);
			Assert("Should be allowed because Job doesn't contain BLN group", SupportIncidentEmailTriggeringRules.AllowGeneratingByCode(incident, "ABC"));

			var qweTag = blnGroup.Magnitudes.AddNew();
			qweTag.TGM_Code = "QWE";
			var opqTag = blnGroup.Magnitudes.AddNew();
			opqTag.TGM_Code = "OPQ";
			jobHeader.AddTag(qweTag);
			Factory.Save();
			Assert(jobHeader.Tags.Count() == 2);
			Assert("Should not be allowed because Job has QWE tag", !SupportIncidentEmailTriggeringRules.AllowGeneratingByCode(incident, "QWE"));
			Assert("Should be allowed because Job doesn't have OPQ tag", SupportIncidentEmailTriggeringRules.AllowGeneratingByCode(incident, "OPQ"));
		}

		public void TestSuppressAll()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var jobHeader = ProcessJobHeader.GetForParent(incident, incident.Factory);
			AssertNotNull(jobHeader);
			Assert(!jobHeader.Tags.Any());
			var blnGroup = Factory.New<TagDefinition>();
			blnGroup.TGD_Code = "BLN";

			var magnitude = blnGroup.Magnitudes.AddNew();
			magnitude.TGM_Code = SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification;
			magnitude.TGM_IsActive = false;
			Factory.Save();

			Assert("Should not be suppressed because ALL is not active", !SupportIncidentEmailTriggeringRules.SuppressAll(incident));

			magnitude.TGM_IsActive = true;
			Factory.Save();

			Assert("Should be suppressed.", SupportIncidentEmailTriggeringRules.SuppressAll(incident));

			var allTagLink = jobHeader.Tags.Single(x => x.TagMagnitude.TGM_Code == SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertNotNull(allTagLink);

			Assert("Should return true again", SupportIncidentEmailTriggeringRules.SuppressAll(incident));
			AssertNoExceptionThrown("Should not add tag again", () => jobHeader.Tags.Single(x => x.TagMagnitude.TGM_Code == SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification));
		}

		public void TestUnsuppressAll()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var jobHeader = ProcessJobHeader.GetForParent(incident, incident.Factory);
			AssertNotNull(jobHeader);
			Assert(!jobHeader.Tags.Any());
			var blnGroup = Factory.New<TagDefinition>();
			blnGroup.TGD_Code = "BLN";
			var magnitude = blnGroup.Magnitudes.AddNew();
			magnitude.TGM_Code = SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification;
			magnitude.TGM_IsActive = true;
			Factory.Save();

			Assert("Should be suppressed.", SupportIncidentEmailTriggeringRules.SuppressAll(incident));

			var allTagLink = jobHeader.Tags.Single(x => x.TagMagnitude.TGM_Code == SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertNotNull(allTagLink);

			SupportIncidentEmailTriggeringRules.UnsuppressAll(incident);
			Assert("ALL tag should be removed", !jobHeader.Tags.Any(x => x.TagMagnitude.TGM_Code == SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification));

			AssertNoExceptionThrown(() => SupportIncidentEmailTriggeringRules.UnsuppressAll(incident));
		}

		public void TestGetTagRuleStatus()
		{
			var result = SupportIncidentEmailTriggeringRules.GetTagRuleStatus(null, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertEquals("Should return NotExists because of null data", IncidentEmailTagRuleStatus.NotExists, result);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.Factory.Save();

			result = SupportIncidentEmailTriggeringRules.GetTagRuleStatus(incident, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertEquals("Should return NotExists because of missing Group", IncidentEmailTagRuleStatus.NotExists, result);

			var blnGroup = Factory.New<TagDefinition>();
			blnGroup.TGD_Code = "BLN";

			var magnitude = blnGroup.Magnitudes.AddNew();
			magnitude.TGM_Code = SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification;
			magnitude.TGM_IsActive = true;
			Factory.Save();

			result = SupportIncidentEmailTriggeringRules.GetTagRuleStatus(incident, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertEquals("Should return NotExists", IncidentEmailTagRuleStatus.NotExists, result);

			Assert(SupportIncidentEmailTriggeringRules.SuppressAll(incident));

			result = SupportIncidentEmailTriggeringRules.GetTagRuleStatus(incident, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertEquals(IncidentEmailTagRuleStatus.New, result);

			Factory.Save();
			result = SupportIncidentEmailTriggeringRules.GetTagRuleStatus(incident, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertEquals("After saving, its status should be Existing", IncidentEmailTagRuleStatus.Existing, result);

			SupportIncidentEmailTriggeringRules.SuppressAll(incident);
			result = SupportIncidentEmailTriggeringRules.GetTagRuleStatus(incident, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertEquals("Should still be Existing because of no changes", IncidentEmailTagRuleStatus.Existing, result);

			SupportIncidentEmailTriggeringRules.UnsuppressAll(incident);
			result = SupportIncidentEmailTriggeringRules.GetTagRuleStatus(incident, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertEquals(IncidentEmailTagRuleStatus.Deleted, result);

			Factory.Save();
			result = SupportIncidentEmailTriggeringRules.GetTagRuleStatus(incident, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertEquals("Status should be NotExists after saving, because the link was deleted before saving", IncidentEmailTagRuleStatus.NotExists, result);

			SupportIncidentEmailTriggeringRules.SuppressAll(incident);
			result = SupportIncidentEmailTriggeringRules.GetTagRuleStatus(incident, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertEquals("New link was added", IncidentEmailTagRuleStatus.New, result);

			SupportIncidentEmailTriggeringRules.UnsuppressAll(incident);
			result = SupportIncidentEmailTriggeringRules.GetTagRuleStatus(incident, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertEquals("The status should be NotExists, because the link is not in DB", IncidentEmailTagRuleStatus.NotExists, result);

			Factory.Save();
			result = SupportIncidentEmailTriggeringRules.GetTagRuleStatus(incident, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertEquals(IncidentEmailTagRuleStatus.NotExists, result);

			SupportIncidentEmailTriggeringRules.SuppressAll(incident);
			SupportIncidentEmailTriggeringRules.UnsuppressAll(incident);
			SupportIncidentEmailTriggeringRules.SuppressAll(incident);
			result = SupportIncidentEmailTriggeringRules.GetTagRuleStatus(incident, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertEquals(IncidentEmailTagRuleStatus.New, result);

			Factory.Save();
			result = SupportIncidentEmailTriggeringRules.GetTagRuleStatus(incident, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification);
			AssertEquals(IncidentEmailTagRuleStatus.Existing, result);
		}
	}
}
