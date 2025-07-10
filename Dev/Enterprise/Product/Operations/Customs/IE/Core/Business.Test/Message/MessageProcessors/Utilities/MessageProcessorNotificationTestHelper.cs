using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	public static class MessageProcessorNotificationTestHelper
	{
		public static void AssertEmail(string expectingSubject, string[] expectingBodyTexts, string[] expectingRecipients)
		{
			var outgoingEmails = Env.OutgoingCustomsMailManager.EmailsCreated;
			Assertion.Assert("No Emails Created", outgoingEmails.Count > 0);
			Assertion.AssertNotNullOrEmpty("expectingSubject cannot be empty", expectingSubject);
			EmailDef outgoingEmail = null;
			var subjects = new List<string>();
			foreach (var email in outgoingEmails)
			{
				var subject = email.Subject;
				subjects.Add(subject);
				if (outgoingEmail == null && subject.StartsWith(expectingSubject))
				{
					outgoingEmail = email;
					break;
				}
			}
			Assertion.AssertNotNull("No outgoing email found; found subjects:\r\n" + string.Join("\r\n", subjects), outgoingEmail);
			var body = outgoingEmail.Body;
			Assertion.AssertNotEquals("expectingBodyTexts cannot be empty", 0, expectingBodyTexts.Length);
			foreach (var expectedBodyText in expectingBodyTexts)
			{
				Assertion.AssertContains("Email body should contain", expectedBodyText, body);
			}

			Assertion.AssertContainsExactElementsInAnyOrder("Recipients", expectingRecipients, outgoingEmail.Recipients.Cast<RecipientDef>().Select(x => x.Email));
		}

		public static void AssertNoEmailsSent()
		{
			var outgoingEmails = Env.OutgoingCustomsMailManager.EmailsCreated;
			Assertion.Assert("Emails were Created", outgoingEmails.Count == 0);
		}

		public static GlbStaff SetupStaffData(BusinessObjectFactory factory) => CreateStaff(factory, "!1@", "Staff 1", "staff1@where.com");

		public static GlbStaff CreateStaff(BusinessObjectFactory factory, ZString code, ZString name, ZString email)
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_FullName = name;
			staff.GS_EmailAddress = email;
			return staff;
		}

		public static GlbGroup SetupStaffGroup(BusinessObjectFactory factory)
		{
			var group = factory.New<GlbGroup>();
			group.GG_Desc = "Test";
			group.GG_Code = "TEST";
			group.Staff.Add(CreateStaff(factory, "!3@", "Staff 3", "staff3@where.com"));
			group.Staff.Add(CreateStaff(factory, "!4@", "Staff 4", "staff4@where.com"));
			group.Staff.Add(CreateStaff(factory, "!5@", "Staff 5", "staff5@where.com"));
			return group;
		}
	}
}
