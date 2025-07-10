using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	public class PAVEFeatureUsageCollectorTest : BMSTestCaseWithFactory
	{
		public void TestReportTaskStatusChange_ShouldLogCorrectly()
		{
			//Arrange
			var collector = new PAVEFeatureUsageCollector();
			var changeEvents = new Dictionary<string, string>
			{
				["TMU"] = "WRK",
				["TDA"] = "SUS"
			};

			//Act
			collector.ReportTaskStatusChange(changeEvents);

			//Assert
			var messages = LoadMessages();
			AssertEquals("Expected 1 message to be present in Logs, but was: " + messages.Length, 1, messages.Length);

			var currentBranch = Env.CurrentBranch.Code;
			var contents = messages[0].EM_MessageText;

			AssertContains("Message must contain FeatureCode: STC", "\"FeatureCode\": \"STC\"", contents);
			AssertContains("Message must contain Module: PAV", "\"Module\": \"PAV\"", contents);
			AssertEquals("Message must contain BranchCode: " + currentBranch, currentBranch, messages[0].Branch.Code);
			AssertContains("Message must contain the first change event TMU: WRK", "\"TMU\": \"WRK\"", contents);
			AssertContains("Message must contain the second change event TDA: SUS", "\"TDA\": \"SUS\"", contents);
		}

		public void TestReportVisualBoardOpen_ShouldLogCorrectly()
		{
			//Arrange
			var collector = new PAVEFeatureUsageCollector();
			var changeEvents = new Dictionary<string, string>
			{
				["PK"] = "BoardPK",
				["Name"] = "BoardName",
				["CMP"] = "1",
				["MOD"] = "2",
				["BUF"] = "3",
				["BUC"] = "4",
			};

			//Act
			collector.ReportVisualBoardOpen(changeEvents);

			//Assert
			var currentBranch = Env.CurrentBranch.Code;
			var messages = LoadMessages();
			AssertEquals("Expected 1 message to be present in Logs, but was: " + messages.Length, 1, messages.Length);

			var contents = messages[0].EM_MessageText;

			AssertContains("Message must contain FeatureCode: VBO", "\"FeatureCode\": \"VBO\"", contents);
			AssertContains("Message must contain Module: PAV", "\"Module\": \"PAV\"", contents);
			AssertEquals("Message must contain BranchCode: " + currentBranch, currentBranch, messages[0].Branch.Code);
			AssertContains("Message must contain PK: BoardPK", "\"PK\": \"BoardPK\"", contents);
			AssertContains("Message must contain Name: BoardName", $"\"Name\": \"BoardName\"", contents);
			AssertContains("Message must contain CMP: 1", "\"CMP\": \"1\"", contents);
			AssertContains("Message must contain MOD: 2", "\"MOD\": \"2\"", contents);
			AssertContains("Message must contain BUF: 3", "\"BUF\": \"3\"", contents);
			AssertContains("Message must contain BUC: 4", "\"BUC\": \"4\"", contents);
		}

		public void TestReportDeferralWithESDOrADDRemoved_ShouldLogCorrectly()
		{
			//Arrange
			var collector = new PAVEFeatureUsageCollector();
			var changeEvents = new Dictionary<string, string>
			{
				["PK"] = "WorkflowPK",
				["Name"] = "WorkflowName",
				["FromVisualBoard"] = "YES",
				["OriginalESDDefaultsFrom"] = "ARR",
			};

			//Act
			collector.ReportDeferralWithESDOrADDRemoved(changeEvents);

			//Assert
			var currentBranch = Env.CurrentBranch.Code;
			var messages = LoadMessages();
			AssertEquals("Expected 1 message to be present in Logs, but was: " + messages.Length, 1, messages.Length);

			var contents = messages[0].EM_MessageText;

			AssertContains("Message must contain FeatureCode: DEA", "\"FeatureCode\": \"DEA\"", contents);
			AssertContains("Message must contain Module: PAV", "\"Module\": \"PAV\"", contents);
			AssertEquals("Message must contain BranchCode: " + currentBranch, currentBranch, messages[0].Branch.Code);
			AssertContains("Message must contain PK: WorkflowPK", "\"PK\": \"WorkflowPK\"", contents);
			AssertContains("Message must contain Name: WorkflowName", "\"Name\": \"WorkflowName\"", contents);
			AssertContains("Message must contain FromVisualBoard: YES", "\"FromVisualBoard\": \"YES\"", contents);
			AssertContains("Message must contain OriginalESDDefaultsFrom: ARR", "\"OriginalESDDefaultsFrom\": \"ARR\"", contents);
		}

		#region Helper
		public IUsageEDIMessage[] LoadMessages()
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, "USG");
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, "TRX");
			query.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			query.AddToFilter(EDIMessageSchema.EM_Status, "CAP");
			query.AddToFilter(EDIMessageSchema.EM_MessageType, "USG");

			return Factory.Load<IUsageEDIMessage>(query);
		}

		#endregion
	}
}
