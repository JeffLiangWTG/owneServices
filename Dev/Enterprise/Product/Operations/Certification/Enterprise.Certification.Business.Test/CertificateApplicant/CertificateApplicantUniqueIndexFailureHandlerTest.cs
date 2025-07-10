using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Certification.Business.Testing
{
	sealed class CertificateApplicantUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestUniqueIndexToHandle()
		{
			AssertEquals(HRJobApplicantSchema.Constants.Indexes.NR_UX__HA_EmailAddress, new CertificateApplicantUniqueIndexFailureHandler(null).HandledUniqueIndexNames.Single());
		}

		public void TestNotifyUserAndAttemptToResolve()
		{
			CertificateApplicant existingApplicant = Factory.New<CertificateApplicant>();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			existingApplicant.Logs.AddNew(AutoEvents.EditedARecord, "Approved by The Big Boss (BB)");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			existingApplicant.HA_FullName = "Jordan Smith";
			existingApplicant.HA_EmailAddress = "newuser@cargowise.com";
			Factory.Save();

			CertificateApplicant newApplicant = Factory.New<CertificateApplicant>();
			newApplicant.HA_FullName = "John Smith";
			newApplicant.HA_EmailAddress = "newuser@cargowise.com";

			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				AssertEquals("The anonymous user (John Smith - newuser@cargowise.com) you are trying to approve already exists in the database. Previously approved by BB.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
