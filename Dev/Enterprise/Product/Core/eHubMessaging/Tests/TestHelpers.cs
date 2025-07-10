using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests
{
	class TestHelpers : TestCase
	{
		public static GlbCompany ValidCompanyForTest(BusinessObjectFactory factory)
		{
			var companies = factory.Load<GlbCompany>(new ZQuery());
			return companies.FirstOrDefault(x => x.FirstActiveBranch != null) ?? companies.First(x => x.Branches[0] != null);
		}

		public static GlbStaff CreateStaff(string staffInfo, BusinessObjectFactory factory)
		{
			var result = factory.New<GlbStaff>();
			result.GS_Code = staffInfo;
			result.GS_LoginName = staffInfo;
			result.GS_IsSystemAccount = false;
			result.GS_EmailAddress = staffInfo + "@" + staffInfo + ".com";
			factory.Save();
			return result;
		}

		public static GlbGroup CreateGroup(string groupInfo, BusinessObjectFactory factory)
		{
			var result = factory.New<GlbGroup>();
			result.GG_Code = groupInfo;
			factory.Save();
			return result;
		}

		public static GlbGroupLink CreateGroupLink(GlbGroup group, GlbStaff staff, BusinessObjectFactory factory)
		{
			var result = factory.New<GlbGroupLink>();
			result.GK_GG = group.PK;
			result.GK_GS = staff.PK;
			factory.Save();
			return result;
		}

		public static void AssertErrorEmail(string containsMessage)
		{
			AssertEquals("No error email found", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault();
			Env.OutgoingMailManager.EmailsCreated.Remove(email);
			AssertNotNull("Unexpected null email", email);
			Assert(string.Format("Wrong error email found.{0}{0}Could not find text:{0}{0}{1}{0}{0}In email body:{0}{0}{2}{0}{0}", System.Environment.NewLine, containsMessage, email.Body), email.Body.Contains(containsMessage));
		}

		public static void AssertErrorEmails(params string[] containsMessages)
		{
			AssertEquals("No error email found", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault();
			Env.OutgoingMailManager.EmailsCreated.Remove(email);
			AssertNotNull("Unexpected null email", email);
			foreach (var containsMessage in containsMessages)
			{
				Assert(string.Format("Wrong error email found.{0}{0}Could not find text:{0}{0}{1}{0}{0}In email body:{0}{0}{2}{0}{0}", System.Environment.NewLine, containsMessage, email.Body), email.Body.Contains(containsMessage));
			}
		}

		public static void AssertNoErrorEmail()
		{
			var email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault();
			if (email != null)
			{
				Fail(string.Format("No error email expected.{0}{0}Email body contains:{0}{0}{1}{0}{0}", System.Environment.NewLine, email.Body));
			}
			else
			{
				Assert(true);
			}
		}

		public static void CleanErrorReporter()
		{
			ErrorReporter.Clear();
		}

		public static void DropSequenceIfExists(string sequenceName)
		{
			var sqlText = $@"
if (OBJECT_ID('{sequenceName}', N'SO') is not null)
begin
	drop sequence [{sequenceName}]
end";
			using var connection = Db.NewExtraConnectionToMainDb();
			connection.ExecuteNonQuery(sqlText);
		}
	}
}
