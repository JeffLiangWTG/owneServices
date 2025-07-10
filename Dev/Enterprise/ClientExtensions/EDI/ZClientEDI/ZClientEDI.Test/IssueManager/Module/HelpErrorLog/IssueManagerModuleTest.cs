using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Module
{
	[TestedType(typeof(IssueManagerModule))]
	public class IssueManagerModuleTest : ZModuleBasherTest
	{
		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert(true);
		}

		protected override void SetUp()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;
			base.SetUp();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.IssueManager;
		}

		protected override void BashModule(ZFilterModule module)
		{
			AssertNotNull(module.ToolBarButtons); // poke the toolbar to avoid null reference exceptions
			base.BashModule(module);
		}

		public void TestCreateWorkItem()
		{
			GlbStaff johnDoe = Factory.New<GlbStaff>();
			johnDoe.GS_LoginName = "~~~johndoe";
			johnDoe.GS_Code = "~JD";
			GlbStaff janeRoe = Factory.New<GlbStaff>();
			janeRoe.GS_LoginName = "~~~janeroe";
			janeRoe.GS_Code = "~JR";
			EdiHelpErrorLog logOne = Factory.New<EdiHelpErrorLog>();
			logOne.HE_ExceptionSource = "ZRSGenerator";
			HelpErrorLogOccurrence occurrenceOne = Factory.New<HelpErrorLogOccurrence>();
			occurrenceOne.HO_XMLData = XmlDocString;
			occurrenceOne.HO_HE = logOne.PK;
			EdiHelpErrorLog logTwo = Factory.New<EdiHelpErrorLog>();
			logTwo.HE_ExceptionSource = "ZRSGenerator";
			HelpErrorLogOccurrence occurrenceTwo = Factory.New<HelpErrorLogOccurrence>();
			occurrenceTwo.HO_XMLData = XmlDocString.Replace("Enterprise", "Boris");
			occurrenceTwo.HO_HE = logTwo.PK;
			Factory.Save();
			Assert("One should not have work item", !logOne.HasWorkItems);
			Assert("Two should not have work item", !logTwo.HasWorkItems);
			using (IssueManagerModule module = new IssueManagerModule())
			{
				module.AutoCreateWorkItems(new BusinessObject[] { logOne, logTwo });
			}

			Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertEquals("2 WorkItem been created within 2 issues.", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("One should have work item", logOne.HasWorkItems);
			Assert("Two should have work item", logTwo.HasWorkItems);
		}

		public void TestView()
		{
			using (ZForm form = new ZForm())
			using (IssueManagerModule module = new IssueManagerModule())
			{
				AssertNotNull("View button should be shown", module.ToolBarButtons.FindByText("View"));
			}
		}

		public void TestCloseIssues()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			Factory.Save();
			GlbStaff team = Factory.NewWithValidTestData<GlbStaff>();
			EdiHelpErrorLog log1 = Factory.New<EdiHelpErrorLog>();
			EdiHelpErrorLog log2 = Factory.New<EdiHelpErrorLog>();
			Factory.Save();
			Assert("One should not be closed", log1.HE_FixedDate.IsEmpty);
			Assert("Two should not be closed", log2.HE_FixedDate.IsEmpty);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (ZForm form = new ZForm())
			using (IssueManagerModule module = new IssueManagerModule())
			{
				module.Close(new BusinessObject[] { log1, log2 });
				AssertNotEquals(ZDate.Empty, log1.HE_FixedDate);
				AssertNotEquals(ZDate.Empty, log2.HE_FixedDate);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("2 issues have been closed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMergeSecurity()
		{
			EDISecurityCheckpoints.IssueManagerMergeTool.IsAllowed = false;
			using (IssueManagerModule module = new IssueManagerModule())
			{
				module.MergeMenuItem_Click(null, EventArgs.Empty);
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("You do not have the appropriate security rights to run this function"));
			}

			EDISecurityCheckpoints.IssueManagerMergeTool.IsAllowed = true;
			using (IssueManagerModule module = new IssueManagerModule())
			{
				module.MergeMenuItem_Click(null, EventArgs.Empty);
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("You must select more than one issue to merge"));
			}
		}

		public void TestExceptionText()
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(XmlDocString);
			using (IssueManagerModule module = new IssueManagerModule())
			{
				AssertEquals(ExpectedResult, module.ExceptionText(xmlDoc));
			}
		}

		public void TestExceptionLongText()
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(XmlDocString.Replace(">Test Message", $">Test Message + super long + {new string('\\', 1024 * 1024 + 8)} + {new string('a', 1024 * 1024 + 100)}"));
			using (IssueManagerModule module = new IssueManagerModule())
			{
				AssertEquals(ExpectedResult.Replace(": Test Message", $": Test Message + super long + {new string('\\', 4)} + {new string('a', 1024 * 1024 - 35)}")
					, module.ExceptionText(xmlDoc));
			}
		}

		const string ExpectedResult = @"Enterprise.TestException2 (Inner 1): Inner Test Message

	at Enterprise.Test.TestMethod3();
	at Enterprise.Test.TestMethod4();

Enterprise.TestException: Test Message

	at Enterprise.Test.TestMethod();
	at Enterprise.Test.TestMethod2();
";
		const string XmlDocString = @"
				<EDI_Exception_Report>
					<ExceptionDetails>
						<ExceptionType>Enterprise.TestException</ExceptionType>
						<Message>Test Message</Message>
						<StackTrace>
							<Call>  at Enterprise.Test.TestMethod();</Call>
							<Call>	at Enterprise.Test.TestMethod2();</Call>
						</StackTrace>
						<InnerException>
							<ExceptionType>Enterprise.TestException2</ExceptionType>
							<Message>Inner Test Message</Message>
							<StackTrace>
								<Call>  at Enterprise.Test.TestMethod3();</Call>
								<Call>	at Enterprise.Test.TestMethod4();" + "\r" + @"</Call>
							</StackTrace>
						</InnerException>
					</ExceptionDetails>
				</EDI_Exception_Report>";

		public void TestModuleGridDbHits()
		{
			const int numberOfIssues = 10;
			const int numberOfOccurrencesPerIssue = 3;
			for (var i = 0; i < numberOfIssues; i++)
			{
				CreateIssue(i, numberOfOccurrencesPerIssue);
			}
			Factory.Save();

			using (var module = new IssueManagerModule())
			using (var form = new ZForm { Width = 2000, Height = 600 })
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.DisplayGrid.ExposeAllColumns();
				module.PerformSearch_ForTest();

				var hits = new Dictionary<string, int>
				{
					{ GenPivotSchema.Constants.TableName, 10 },
					{ HelpErrorLogKeySchema.Constants.TableName, 1 },
					{ HelpErrorLogSchema.Constants.TableName, 1 },
					{ HelpErrorLogOccurrenceSchema.Constants.TableName, 1 },
					{ IncidentMainSchema.Constants.TableName, 1 },
					{ WorkItemSchema.Constants.TableName, 1 },
				};
				using (AssertDbHitsWithUsefulQueryInformation(hits, module.GridCollection.Factory))
				{
					Application.DoEvents();
					foreach (var businessObject in module.GridCollection)
					{
						Assert(businessObject is EdiHelpErrorLog);
						Assert(((EdiHelpErrorLog)businessObject).HasWorkItems);
					}
				}
			}
		}

		void CreateIssue(int issueNumber, int numberOfOccurrencesPerIssue)
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			org.LicenceEnterpriseCode = "ORG";

			var contact = org.Contacts.AddNew();
			contact.FillWithValidTestData();

			var db = org.LicCompany.LicDatabases.AddNew();
			db.LD_OC_ContractInstallerOrInternalTechContact = contact.PK;
			db.LD_ServerCode = issueNumber.ToString();

			var clientCompany = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany.LCC_LD = db.PK;
			clientCompany.LCC_Code = org.LicCompany.LC_CompanyCode;

			var issue = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			for (var i = 0; i < numberOfOccurrencesPerIssue; i++)
			{
				var occurrence = issue.Occurrences.AddNew();
				occurrence.HO_LD = db.PK;
				occurrence.HO_LCC = clientCompany.PK;
			}

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();

			incident.RelatedItems.Add(issue);
			incident.RelatedItems.Add(workItem);
			workItem.RelatedItems.Add(issue);
		}
	}
}
