using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	public class MetaSupportIncidentProviderTest : TestCaseWithFactory
	{
		ZGuid incidentMainGuid;
		ZGuid workItemGuid;
		DateTime now;
		protected override void SetUp()
		{
			base.SetUp();
			now = DateTime.UtcNow;

			var incidentRequest = Factory.New<IncidentRequest>();
			incidentRequest.FillWithValidTestData();

			var incidentMain = Factory.New<IncidentMainBase>();
			incidentMain.FillWithValidTestData();
			incidentMain.IM_SystemCreateTimeUtc = now;
			incidentMain.IM_SystemLastEditTimeUtc = now;
			incidentMain.IM_Description = "IM_Description";
			incidentMain.IM_Module = "Mod";
			incidentMain.IM_Priority = "PRI";
			incidentMain.IM_Product = "PRO";
			incidentMain.IM_ProgramArea = "PGA";
			incidentMain.IM_INC_Request = incidentRequest.PK;

			var stmNote = Factory.New<StmNote>();
			stmNote.FillWithValidTestData();
			stmNote.ST_NoteText = "ST_Description";
			stmNote.ST_ParentID = new Guid(incidentMain.PK.ToString());
			stmNote.ST_Description = "Incident Detail";
			stmNote.ST_Table = "table";

			var jobConversation = Factory.New<JobConversation>();
			jobConversation.FillWithValidTestData();
			jobConversation.JCC_ParentID = incidentRequest.PK;
			jobConversation.JCC_ParentTableCode = "IM";

			var jobConversationMessage = Factory.New<JobConversationMessage>();
			jobConversationMessage.FillWithValidTestData();
			jobConversationMessage.JCM_JCC_Conversation = jobConversation.PK;
			jobConversationMessage.JCM_IsSystem = false;
			jobConversationMessage.JCM_Body = "JCM_Body";
			jobConversationMessage.JCM_PostedTimeUtc = now;

			var workItem = Factory.New<WorkItem>();
			workItem.FillWithValidTestData();
			workItem.WKI_WorkItemNumber = "WKI_WorkItemNumber";
			workItem.WKI_Summary = "WKI_Summary";
			workItem.WKI_Details = new ZBlob(Encoding.ASCII.GetBytes("PlainTextWKI_Details"));
			workItem.WKI_SystemCreateTimeUtc = now;

			var genPivot = Factory.New<GenPivot>();
			genPivot.FillWithValidTestData();
			genPivot.XX_Relation1ID = incidentMain.PK;
			genPivot.XX_Relation2ID = workItem.PK;

			incidentMainGuid = incidentMain.PK;
			workItemGuid = workItem.PK;
			Factory.Save();
		}

		public void TestMetaSupportIncidentProvider_FromGuid()
		{
			//Arrange
			var metaSupportIncidentProvider = new MetaSupportIncidentProvider();
			//Act
			var msi = metaSupportIncidentProvider.FromGuid(new Guid(incidentMainGuid.ToString()));
			//Assert;

			AssertEquals("IM_Description", msi.Description);
			AssertEquals("Mod", msi.Module);
			AssertEquals("ST_Description", msi.NoteText);
			AssertEquals(incidentMainGuid, msi.PK);
			AssertEquals("PRI", msi.Priority);
			AssertEquals("PRO", msi.Product);
			AssertEquals("PGA", msi.ProgramArea);
			AssertZDatesWithin5Minutes("SystemCreateTimeUtc to be within one second of 'now'.", new ZDateTime(now), msi.SystemCreateTimeUtc);
			AssertZDatesWithin5Minutes("SystemLastEditTimeUtc to be within one second of 'now'.", new ZDateTime(now), msi.SystemLastEditTimeUtc);

			AssertEquals("JCM_Body", msi.ConversationItems.First().Body);
			AssertZDatesWithin5Minutes("Conversation item SystemCreateTimeUtc to be within one second of 'now'.", new ZDateTime(now), msi.ConversationItems.First().PostedTimeUtc);

			AssertEquals(workItemGuid, msi.WorkItems.First().PK);
			AssertZDatesWithin5Minutes("Work item SystemCreateTimeUtc to be within one second of 'now'.", new ZDateTime(now), msi.WorkItems.First().SystemCreateTimeUtc);
			AssertEquals("WKI_WorkItemNumber", msi.WorkItems.First().WorkItemNumber);
			AssertEquals("WKI_Summary", msi.WorkItems.First().Summary);
			AssertEquals("PlainTextWKI_Details", msi.WorkItems.First().Details);
		}
	}
}
