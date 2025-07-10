using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.VisualBoards.Business.Test
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
	public class VisualBoardsTestCase : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			EnableBMSInRegistry();
		}

		protected MockRepository Mocks
		{
			get { return mocks ?? (mocks = new MockRepository(MockBehavior.Default)); }
		}

		MockRepository mocks;

		protected override void TearDown()
		{
			base.TearDown();
			mocks = null;
		}

		public static void EnableBMSInRegistry()
		{
			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.PlanningManagement);
			BMSRegistry.Instance.AlwaysViewWorkflowManagementTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		public static void DisableBMSInRegistry()
		{
			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.BasicWorkflow);
			BMSRegistry.Instance.AlwaysViewWorkflowManagementTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		#region Use Testable Card Content

		public IDisposable UseBizoCardContents()
		{
			return VisualBoardsTestHelper.UseBizoCardContents();
		}

		#endregion

		#region Async

		public static IDisposable DisableAsyncBehaviour()
		{
			return ApplyAsyncStrategy(new MockAsyncStrategy());
		}

		public static IDisposable ApplyAsyncStrategy(IAsyncStrategy strategy)
		{
			timerSubstitution?.Dispose();

			var useMockTimer = strategy is MockAsyncStrategy || strategy is TriggerableAsyncStrategy;
			timerSubstitution = useMockTimer ? ObjectFactory.Substitute<IWindowsTimer>(() => new MockTimer()) : null;

			var originalOverriddenStrategy = AsyncStrategy.OverriddenStrategy_ForTest.Value;
			AsyncStrategy.OverriddenStrategy_ForTest.Value = strategy;

			return new DisposableAction(() =>
			{
				timerSubstitution?.Dispose();
				timerSubstitution = null;
				(strategy as IDisposable)?.Dispose();
				AsyncStrategy.OverriddenStrategy_ForTest.Value = originalOverriddenStrategy;
			});
		}

		static IDisposable timerSubstitution;

		public static void RunOnBackgroundAndWait(Action action)
		{
			Task.Factory.StartNew(action).Wait();
		}

		#endregion

		#region Helper Methods

		protected BMSystem CreateSystem(params string[] workflowTypes)
		{
			return VisualBoardsTestHelper.CreateSystem(Factory, workflowTypes);
		}

		public static BMBoard CreateBoard(BMSystem system, string name = "board", string description = "")
		{
			return VisualBoardsTestHelper.CreateBoard(system, name, description);
		}

		public static BMComponent CreateBucket(BMSystem system, string name = "bucket", int sequence = 0, int offsetMinutes = 0)
		{
			return VisualBoardsTestHelper.CreateBucket(system, name, offsetMinutes: offsetMinutes, sequence: sequence);
		}

		public static BMComponent CreateBuffer(BMSystem system, string name = "buffer", int timespanMinutes = 96 * 60, byte loadLimitPercent = 50, int sequence = 0)
		{
			return VisualBoardsTestHelper.CreateBuffer(system, name, timespanMinutes, loadLimitPercent, sequence);
		}

		public static BMComponent CreateConstraint(BMComponent buffer, string name = "constraint", int offsetMinutes = 0, int sequence = 0)
		{
			return VisualBoardsTestHelper.CreateConstraint(buffer, name, offsetMinutes, sequence);
		}

		public static BMComponent CreateDecouple(BMComponent buffer, string name = "decouple", int offsetMinutes = 0, int sequence = 0)
		{
			return VisualBoardsTestHelper.CreateDecouple(buffer, name, offsetMinutes, sequence);
		}

		public static BMComponent CreateSubBuffer(BMComponent buffer, string name = "sub-buffer", int timespanMinutes = 96 * 60, int offsetMinutes = 0, int sequence = 0)
		{
			return VisualBoardsTestHelper.CreateSubBuffer(buffer, name, timespanMinutes, offsetMinutes, sequence);
		}

		public static BMComponentLink LinkComponents(BMComponent from, BMComponent to, byte sequence = 0, bool? isReleaseGate = null)
		{
			return VisualBoardsTestHelper.LinkComponents(from, to, sequence, isReleaseGate);
		}

		public static BMZoneCapacityMultiplier CreateZoneMultiplier(BMComponent buffer, decimal zone3Multiplier = 1, decimal zone2Multiplier = 1, decimal zone1Multiplier = 2, decimal zone0Multiplier = 3, ZGuid releaseGroupPK = default(ZGuid))
		{
			return VisualBoardsTestHelper.CreateZoneMultiplier(buffer, zone3Multiplier, zone2Multiplier, zone1Multiplier, zone0Multiplier, releaseGroupPK);
		}

		public static BMSystemReleaseGroup CreateReleaseGroup(BMSystem system, GlbGroup group, BMComponent constrainedModeComponent = null)
		{
			return VisualBoardsTestHelper.CreateReleaseGroup(system, group, constrainedModeComponent);
		}

		protected ProcessJobHeader CreateJobHeader<T>(bool addDefaultProcessHeaderIfNone = true, string description = null)
			where T : BusinessObject, IWorkflowProvider
		{
			return VisualBoardsTestHelper.CreateJobHeader<T>(Factory, addDefaultProcessHeaderIfNone, description);
		}

		public static ProcessHeader Create1HourWorkflow(ProcessJobHeader job, string staffCode = "")
		{
			var workflow = job.ProcessHeaders.AddNew();
			CreateTask(workflow, staffCode, 60, estVariationFactor: 1);
			return workflow;
		}

		public static ProcessHeader CreateWorkflow(ProcessJobHeader jobHeader, string completionStatement, BMComponent currentComponent = null, ZDateTime? releaseDateTime = null, ZGuid? releaseGroupPK = null)
		{
			return VisualBoardsTestHelper.CreateWorkflow(jobHeader, completionStatement, currentComponent, releaseDateTime, releaseGroupPK);
		}

		public static ProcessHeader[] CreateWorkflows(BMComponent component, int numberOfWorkflows, int numberOfTasksPerWorkflow = 0, GlbGroup releaseGroup = null, GlbStaff staff = null)
		{
			return VisualBoardsTestHelper.CreateWorkflows(component, numberOfWorkflows, numberOfTasksPerWorkflow, releaseGroup, staff);
		}

		public static ProcessTask CreateTask(ProcessHeader workflow, string staffCode, int lowEstMinutes, string taskType = "UDF", string taskStatus = "ASN", GlbCapability capability = null, ZInt? sequence = null, int estVariationFactor = 2, string description = "")
		{
			return VisualBoardsTestHelper.CreateTask(workflow, staffCode, lowEstMinutes, taskType, taskStatus, capability, sequence, estVariationFactor, description);
		}

		public static ProcessTask CreateTaskWithoutBMS(IWorkflowProvider provider, string description)
		{
			return VisualBoardsTestHelper.CreateTask(provider, description: description);
		}

		public static void MakeCompletionStatementTaskType(string workflowType, string completionStatementTaskType)
		{
			VisualBoardsTestHelper.MakeCompletionStatementTaskType(workflowType, completionStatementTaskType);
		}

		public static BMBoardSection CreateBoardSection(BMComponent component, BMBoard board = null, int row = 0, int col = 0, int rowHeightPercent = 100, int colWidthPercent = 100, string backColour = null, string panelLayoutStyle = null, int? cellsPerSubsection = null, bool automaticallySetReleaseGroupIfRequired = true, ZGuid? customReleaseGroupPK = null)
		{
			return VisualBoardsTestHelper.CreateBoardSection(component, board, row, col, rowHeightPercent, colWidthPercent, backColour, panelLayoutStyle, cellsPerSubsection, automaticallySetReleaseGroupIfRequired, customReleaseGroupPK);
		}

		public static BoardSectionViewModel CreateBoardSectionViewModel(IBMBoardSection section, bool isPreview = false)
		{
			return new BoardSectionViewModel(section, VisualBoardsTestHelper.CreateBoardViewModel(section.Factory.Load<IBMBoard>(section.MS_MB_Board), isPreview: isPreview));
		}

		public static BMBoardSection CreateReleaseSchedulerBoardSection(BMComponent component, GlbGroup releaseGroup, BMBoard board = null)
		{
			return VisualBoardsTestHelper.CreateReleaseSchedulerBoardSection(component, releaseGroup, board);
		}

		protected GlbCapability CreateCapability(string code, string description)
		{
			return VisualBoardsTestHelper.CreateCapability(Factory, code, description);
		}

		protected GlbStaff CreateStaffInCurrentBranchDept(string code, string fullName, params GlbCapability[] capabilities)
		{
			return VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, code, fullName, capabilities);
		}

		public static BMComponentAcceptabilityBand CreateAcceptabilityBand_WorkflowsInComponent(BMComponent component, int cautionMin, int goodMin, int excellentMin, int excellentMax, int goodMax, int cautionMax)
		{
			return VisualBoardsTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(component, cautionMin, goodMin, excellentMin, excellentMax, goodMax, cautionMax);
		}

		public static BMComponentAcceptabilityBand CreateAcceptabilityBand_AverageNumberOfTasksPerWorkflow(BMComponent component, int cautionMin, int goodMin, int excellentMin, int excellentMax, int goodMax, int cautionMax)
		{
			return VisualBoardsTestHelper.CreateAcceptabilityBand_AverageNumberOfTasksPerWorkflow(component, cautionMin, goodMin, excellentMin, excellentMax, goodMax, cautionMax);
		}

		public static BMComponentAcceptabilityBand CreateAcceptabilityBand(BMComponent component, int cautionMin, int goodMin, int excellentMin, int excellentMax, int goodMax, int cautionMax, string name, string sql = "", string type = AcceptabilityBandTypes.Codes.SQL)
		{
			return VisualBoardsTestHelper.CreateAcceptabilityBand(component, cautionMin, goodMin, excellentMin, excellentMax, goodMax, cautionMax, name, sql, type);
		}

		public static void AssertAcceptabilityBandSubheadingDetails(BMBoardSectionViewModel viewModel, BMBoardSection section, string expectedSubHeading, string expectedSubHeadingMouseoverText, Color expectedSubheadingBackColor)
		{
			AssertAcceptabilityBandSubheadingDetails(viewModel, section, string.Empty, expectedSubHeading, expectedSubHeadingMouseoverText, expectedSubheadingBackColor);
		}

		public static void AssertAcceptabilityBandSubheadingDetails(BMBoardSectionViewModel viewModel, BMBoardSection section, string message, string expectedSubHeading, string expectedSubHeadingMouseoverText, Color expectedSubheadingBackColor)
		{
			VisualBoardsTestHelper.AssertAcceptabilityBandSubheadingDetails(viewModel, section, message, expectedSubHeading, expectedSubHeadingMouseoverText, expectedSubheadingBackColor);
		}

		public static void AssertAcceptabilityBandSubheadingDetails(ISectionSubHeadingAppearance viewModel, string message, string expectedSubHeading, string expectedSubHeadingMouseoverText, Color expectedSubheadingBackColor)
		{
			VisualBoardsTestHelper.AssertAcceptabilityBandSubheadingDetails(viewModel, message, expectedSubHeading, expectedSubHeadingMouseoverText, expectedSubheadingBackColor);
		}

		#endregion

		#region SetupAndClearTables

		public static void SetupAndClearTables()
		{
			TestCaseHelper.ClearTable(BMZoneCapacityMultiplierSchema.Constants.TableName);
			TestCaseHelper.ClearTable(ViewStmNumsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(ProcessHeaderLinkSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMNCNAttachmentSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMNCNScheduleSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMNCNShapeSchema.Constants.TableName);
			TestCaseHelper.ClearTable(ProcessTaskNotificationSchema.Constants.TableName);
			TestCaseHelper.ClearTable(ProcessTemplateTriggerSchema.Constants.TableName);
			TestCaseHelper.ClearTable(ProcessTasksSchema.Constants.TableName);
			TestCaseHelper.ClearTable(ProcessHeaderSchema.Constants.TableName);
			TestCaseHelper.ClearTable(ProcessTaskTemplateSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMBoardSectionChannelSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMComponentAcceptabilityBandSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMSystemReleaseGroupSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMSystemWorkflowDeterminerSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMBoardSectionSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMComponentLinkSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMComponentResourceLinkSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMComponentReleaseGroupLinkSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMComponentSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMBoardSlideshowPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMBoardSchema.Constants.TableName);
			TestCaseHelper.ClearTable(BMSystemSchema.Constants.TableName);
			TestCaseHelper.ClearTable(GlbResourceCapabilityPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(GlbCapabilitySchema.Constants.TableName);

			EnableBMSInRegistry();
		}

		#endregion

		#region Assertion

		public static void AssertTagsApplied(ITagable tagable, params TagMagnitude[] magnitudes)
		{
			AssertTagsApplied(string.Empty, tagable, magnitudes);
		}

		public static void AssertTagsApplied(string message, ITagable tagable, params TagMagnitude[] magnitudes)
		{
			AssertContainsExactElementsInAnyOrder(message, magnitudes, tagable.GetApplicableTags());
		}

		public static void AssertSamePK(IBusiness expected, IBusiness actual)
		{
			AssertSamePK(string.Empty, expected, actual);
		}

		public static void AssertSamePK(string message, IBusiness expected, IBusiness actual)
		{
			AssertNotNull("expected", expected);
			AssertNotNull("actual", actual);

			if (expected.Identifier != actual.Identifier)
			{
				AssertEquals(message, expected, actual);
			}
			else
			{
				Assert(true);
			}
		}

		public static void AssertImagePixelsEqual(Bitmap expected, Bitmap actual)
		{
			AssertImagePixelsEqual(string.Empty, expected, actual);
		}

		public static void AssertImagePixelsEqual(string message, Bitmap expected, Bitmap actual)
		{
			var width = Math.Min(expected.Width, actual.Width);
			var height = Math.Min(expected.Height, actual.Height);

			for (var x = 0; x <= width - 1; x++)
			{
				for (var y = 0; y <= height - 1; y++)
				{
					if (!expected.GetPixel(x, y).Equals(actual.GetPixel(x, y)))
					{
						Fail(string.Format("Images do not match at position {0},{1}: {2}", x, y, message));
						return;
					}
				}
			}

			Assert(true);
		}

		public static void AssertSaved(ContinueWithSave saveResult, IBusiness formBizo)
		{
			AssertSaved(saveResult, (BusinessObject)formBizo);
		}

		public static void AssertSaved(ContinueWithSave saveResult, BusinessObject formBizo = null)
		{
			AssertSaved(string.Empty, saveResult, formBizo);
		}

		public static void AssertSaved(string message, ContinueWithSave saveResult, BusinessObject formBizo = null)
		{
			if (string.IsNullOrEmpty(message))
			{
				message += System.Environment.NewLine;
			}

			message += string.Format("Expected form to have saved, but it wasn't, and the following message was shown: [{0}]", UnitTestUserNotification.Instance.LastMessage.Text);

			CombineAssertions(message, () =>
			{
				AssertEquals("saveResult", ContinueWithSave.Yes, saveResult);

				if (formBizo != null)
				{
					AssertNoErrors(formBizo);
				}
			});
		}

		public static void AssertCcpmRtrTagApplied(ITagable tagable)
		{
			var tag = TagProvider.GetCCPMReadyToReleaseTag(TagProvider.GetCCPMReleaseTagGroup(tagable.Factory));

			AssertTagApplied(tagable, tag);
		}

		public static void AssertCcpmRblTagApplied(ITagable tagable)
		{
			var tag = TagProvider.GetCCPMReleaseBlockedTag(TagProvider.GetCCPMReleaseTagGroup(tagable.Factory));

			AssertTagApplied(tagable, tag);
		}

		public static void AssertCcpmRtrTagNotApplied(ITagable tagable)
		{
			var tag = TagProvider.GetCCPMReadyToReleaseTag(TagProvider.GetCCPMReleaseTagGroup(tagable.Factory));

			AssertTagNotApplied(tagable, tag);
		}

		public static void AssertTagApplied(ITagable tagable, TagMagnitude tag, bool reloadInNewFactory = false, int? sequence = null, decimal? magnitude = null)
		{
			AssertTagApplied(string.Format("[{0}] should have tag [{1}] applied but didn't", tagable, tag.DisplayText), tagable, tag, reloadInNewFactory, sequence, magnitude);
		}

		public static void AssertTagApplied(string message, ITagable tagable, TagMagnitude tag, bool reloadInNewFactory = false, int? sequence = null, decimal? magnitude = null)
		{
			if (reloadInNewFactory)
			{
				tagable = ReloadInNewFactory(tagable);
			}

			tagable.Factory.ClearQueryCache();

			var links = tagable.TagLinks.Cast<TagLink>().Where(l => l.TGL_TGM_Magnitude == tag.PK).ToArray();

			if (links.Length == 0)
			{
				var messageBuilder = new StringBuilder(message);

				if (tagable.TagLinks.Count > 0)
				{
					messageBuilder.AppendLine();
					messageBuilder.Append("Tags applied:");
					messageBuilder.AppendLine();
					messageBuilder.Append(string.Join("\r\n", tagable.TagLinks.Cast<TagLink>().Select(l => "• " + l.DisplayText)));
				}
				else
				{
					messageBuilder.AppendLine();
					messageBuilder.Append("No tags were applied");
				}

				Fail(messageBuilder.ToString());
			}
			else
			{
				Assert(true);
			}

			if (sequence != null)
			{
				foreach (var link in links)
				{
					AssertEquals(string.Format("Expected tag link for tag [{0}] to have Sequence number {1}", tag.DisplayText, sequence.Value), sequence.Value, link.TGL_Sequence);
				}
			}

			if (magnitude != null)
			{
				foreach (var link in links)
				{
					AssertEquals(FormattableString.Invariant($"Expected tag link for tag [{tag.DisplayText}] to have Magnitude {magnitude.Value}"), magnitude.Value, link.TGL_Magnitude);
				}
			}
		}

		public static void AssertTagNotApplied(ITagable tagable, TagMagnitude tag, bool reloadInNewFactory = false)
		{
			AssertTagNotApplied(string.Format("[{0}] should NOT have tag [{1}] applied but did", tagable, tag.DisplayText), tagable, tag, reloadInNewFactory);
		}

		public static void AssertTagNotApplied(string message, ITagable tagable, TagMagnitude tag, bool reloadInNewFactory = false)
		{
			if (reloadInNewFactory)
			{
				tagable = ReloadInNewFactory(tagable);
			}

			AssertEquals(message, false, tagable.TagLinks.Cast<TagLink>().Any(l => l.TGL_TGM_Magnitude == tag.PK));
		}

		static ITagable ReloadInNewFactory(ITagable tagable)
		{
			var factory = new BusinessObjectFactory();
			var reloaded = factory.Load(tagable.GetType(), tagable.PK);

			return (ITagable)reloaded;
		}

		#region SQL Commands

		public static void AssertExecutedCommandCountMatching(string partialCommandText, int expectedCountOfExecutedCommandsMatchingText)
		{
			AssertExecutedCommandCountMatching(string.Empty, partialCommandText, expectedCountOfExecutedCommandsMatchingText);
		}

		public static void AssertExecutedCommandCountMatching(string message, string partialCommandText, int expectedCountOfExecutedCommandsMatchingText)
		{
			if (Db.Connection.ExecutedCommands == null)
			{
				Fail($"Wrap your calls to expensive methods with {nameof(Db)}.{nameof(Db.Connection)}.{nameof(Db.Connection.TrackExecutedCommands)} so the connection starts tracking all executed commands.");
			}

			var matchingCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains(partialCommandText)).ToArray();

			if (expectedCountOfExecutedCommandsMatchingText == matchingCommands.Length)
			{
				Assert(true);
			}
			else
			{
				var failureMessage = new StringBuilder(message);

				failureMessage.Append("<br />");

				failureMessage.Append($"Expected number of statements matching the text [{partialCommandText}]:");
				failureMessage.Append(HtmlFormatGoodValue(expectedCountOfExecutedCommandsMatchingText));
				failureMessage.Append("There were:");
				failureMessage.Append(HtmlFormatBadValue(matchingCommands.Length));

				if (matchingCommands.Length > 0)
				{
					failureMessage.Append("<br />");
					failureMessage.Append("Matching statements:");
					failureMessage.Append("<br />");

					foreach (var command in matchingCommands)
					{
						failureMessage.Append("<br />");
						failureMessage.Append(new string('-', 200));
						failureMessage.Append("<br />");
						failureMessage.Append(command.Replace("\r\n", "<br />").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;"));
						failureMessage.Append("<br />");
					}
				}

				HtmlFail(failureMessage.ToString());
			}
		}

		public static void AssertDoesNotContainInlinedPKsInSQL(string sql)
		{
			sql = GetSQLWithNoParamValues(sql);
			var uncommentedSQL = GetUncommentedSQL(sql);
			var pkRegex = new Regex("([0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12})");
			var match = pkRegex.Match(uncommentedSQL);

			if (match.Success)
			{
				Assert($@"The query should be parameterised and should not contain PKs. Found PKs:
{string.Join(System.Environment.NewLine, pkRegex.Matches(uncommentedSQL).Cast<System.Text.RegularExpressions.Match>().Select(m => m.Value))}

The query:
{sql}", false);
			}
			else
			{
				Assert(true);
			}
		}

		public static string GetSQLWithNoParamValues(string sql)
		{
			var index = sql.IndexOf(System.Environment.NewLine + "Params" + System.Environment.NewLine);
			Assert("Should contain parameter values", index >= 0);
			return sql.Substring(0, index);
		}

		public static string GetUncommentedSQL(string sql)
		{
			sql = RemoveMultilineCommentsFromSQL(sql);
			return RemoveSinglelineCommentsFromSQL(sql);
		}

		public static string RemoveMultilineCommentsFromSQL(string sql)
		{
			var multilineSqlCommentsRegex = new Regex(@"((\/)([\*][\*]?)(.|\n)*?\3\2)", RegexOptions.IgnoreCase); // matches multiline sql comments - insert the expression into https://regex101.com/ for the explanation
			return multilineSqlCommentsRegex.Replace(sql, string.Empty);
		}

		public static string RemoveSinglelineCommentsFromSQL(string sqlWithNoMultilineComments)
		{
			return string.Join(System.Environment.NewLine, sqlWithNoMultilineComments.SplitByLine().Select(l =>
			{
				var index = l.IndexOf("--");
				return index == -1 ? l : l.Substring(0, index);
			}));
		}

		#endregion

		#endregion
	}
}
