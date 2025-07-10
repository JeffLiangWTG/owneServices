using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class ClientFeatureRequestValueAndContributionUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestUniqueIndexToHandle()
		{
			AssertEquals(ClientFeatureRequestValueAndContributionSchema.Constants.Indexes.NR_UC__T9_ParentID, new ClientFeatureRequestValueAndContributionUniqueIndexFailureHandler().HandledUniqueIndexNames.Single());
		}

		public void TestNotifyUserAndAttemptToResolve()
		{
			ClientFeatureRequestValueAndContribution featureRequestValueAndContribution = Factory.New<ClientFeatureRequestValueAndContribution>();
			ZGuid supportIncidentPK = ZGuid.NewZGuid();
			featureRequestValueAndContribution.T9_Contribution = 100;
			featureRequestValueAndContribution.T9_ParentID = supportIncidentPK;
			Factory.Save();

			ClientFeatureRequestValueAndContribution newFeatureRequestValueAndContribution = Factory.New<ClientFeatureRequestValueAndContribution>();
			newFeatureRequestValueAndContribution.T9_Contribution = 100;
			newFeatureRequestValueAndContribution.T9_ParentID = supportIncidentPK;
			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("An EBV / Contribution data has previously been created for this form. Please close and reopen the form again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
