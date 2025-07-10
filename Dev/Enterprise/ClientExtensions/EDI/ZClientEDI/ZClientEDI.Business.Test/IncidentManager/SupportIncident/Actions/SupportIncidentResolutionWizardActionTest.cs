using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentResolutionWizardAction))]
	public class SupportIncidentResolutionWizardActionTest : SupportIncidentActionTestCase
	{
		public void TestResolutionCommentAddingToEConversationMaxLength()
		{
			var action = new SupportIncidentResolutionWizardAction(Factory.New<SupportIncident>());
			AssertEquals("MaxLength", 32000, action.CommentInfo.MaxLength);
		}

		public void TestPerformAction_WhenCompletelySolved()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR5_Training;
			var action = new SupportIncidentResolutionWizardAction(incident);
			action.Comment = "Completely Solved";
			action.CompletelySolvedOption = true;

			action.SynchroniseToIncident();
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, action.Incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingReferredToLearningMaterials, action.Incident.IM_ClosureResolution);
			Assert(action.Incident.EConversation.GetTimeOrderedMessages().Any(x => x.Body == "Completely Solved"));
			var ll = incident.Logs.GetAllLogs().Cast<StmALog>();
			Assert(incident.Logs.GetAllLogs().Cast<StmALog>().Any
				(x => x.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode && x.SL_Reference.Equals("|CAV=Y|DES=CR5 Resolution Assistant Result")));
		}

		public void TestPerformAction_WhenPartlySolved()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR5_Training;
			var action = new SupportIncidentResolutionWizardAction(incident);
			action.Comment = "Partly Solved";
			action.PartlySolvedOption = true;
			action.ContentBeDevelopNotWorthOption = true;
			action.LinksToExistingContent = "123";
			action.ComplexEdgeCaseOption = true;

			action.SynchroniseToIncident();
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingFlaggedForContentDevelopment, action.Incident.IM_ResolutionCode);
			Assert(action.Incident.EConversation.GetTimeOrderedMessages().Any(x => x.Body.Contains("Content Escalation Notes")));
			Assert(action.Incident.EConversation.GetTimeOrderedMessages().Any(x => x.Body == "Partly Solved"));
			Assert(incident.Logs.GetAllLogs().Cast<StmALog>().Any
				(x => x.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode && x.SL_Reference.Equals("|CAV=PARTLY|DES=CR5 Resolution Assistant Result|PRI=1")));
		}

		public void TestPerformAction_WhenPartlySolvedAndShouldContentBeDevelopedYes()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR5_Training;
			var action = new SupportIncidentResolutionWizardAction(incident);
			action.Comment = "Partly Solved";
			action.PartlySolvedOption = true;
			action.ContentBeDevelopYesOption = true;
			action.LinksToExistingContent = "123";
			action.RecommendToContentText = "recommend";
			action.ContentEasierText = "content easier";

			action.SynchroniseToIncident();
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingFlaggedForContentDevelopment, action.Incident.IM_ResolutionCode);
			Assert(action.Incident.EConversation.GetTimeOrderedMessages().Any(x => x.Body.Contains("Content Escalation Notes")));
			Assert(action.Incident.EConversation.GetTimeOrderedMessages().Any(x => x.Body == "Partly Solved"));
			Assert(incident.Logs.GetAllLogs().Cast<StmALog>().Any
				(x => x.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode && x.SL_Reference.Equals("|CAV=PARTLY|DES=CR5 Resolution Assistant Result|PRI=2")));
		}

		public void TestPerformAction_WhenContentCouldNotBeFound()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR5_Training;
			var action = new SupportIncidentResolutionWizardAction(incident);
			action.Comment = "Content could not be found";
			action.ContentCouldNotBeFoundOption = true;
			action.ContentIsIrrelevantOption = true;
			action.ResolutionMethod = SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved;

			action.SynchroniseToIncident();
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, action.Incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, action.Incident.IM_ClosureResolution);
			Assert(action.Incident.EConversation.GetTimeOrderedMessages().Any(x => x.Body == "Content could not be found"));
			Assert(incident.Logs.GetAllLogs().Cast<StmALog>().Any
				(x => x.SL_SE_NKEvent == AutoEvents.MiscellaneousEventCode && x.SL_Reference.Equals("|CAV=N|DES=CR5 Resolution Assistant Result|PRI=0")));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var incident = Factory.New<SupportIncident>();
			return new SupportIncidentResolutionWizardAction(incident);
		}
	}
}
