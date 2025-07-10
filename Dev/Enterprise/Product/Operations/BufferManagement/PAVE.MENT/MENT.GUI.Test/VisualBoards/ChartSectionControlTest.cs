using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.Business.Test;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.GUI.Test
{
	public class ChartSectionControlTest : TestCaseWithFactory
	{
		public void TestEnableMENTSectionsDefault()
		{
			BMSRegistry.Instance.EnableMENTSections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var extraction = MENTTestHelper.CreateExtraction(Factory, "Grumble", isInstantaneous: true);
			extraction.RelatedQuery.MAQ_IsActive = false;
			var section = MENTTestHelper.CreateMENTBoardSection(Factory, null, extraction.PK);
			var sectionViewModel = new BoardSectionViewModel(section, VisualBoardsTestHelper.CreateBoardViewModel(Factory.NewWithValidTestData<BMBoard>(), new MENTTestHelper.TestExtractorFactoryProvider()));
			Factory.Save();

			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new ChartSectionControl(sectionViewModel))
			{
				form.Show();
				var label = control.FindSingle<ZLabel>();
				AssertEquals("MENT sections are no longer supported", label.Text);
			}
		}

		public void TestControlShowsMessageInActiveInstantaneousQueries()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Grumble", isInstantaneous: true);
			extraction.RelatedQuery.MAQ_IsActive = false;
			var section = MENTTestHelper.CreateMENTBoardSection(Factory, null, extraction.PK);

			var sectionViewModel = new BoardSectionViewModel(section, VisualBoardsTestHelper.CreateBoardViewModel(Factory.NewWithValidTestData<BMBoard>(), new MENTTestHelper.TestExtractorFactoryProvider()));

			Factory.Save();

			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new ChartSectionControl(sectionViewModel))
			{
				form.Show();
				form.Controls.Add(control);
				Application.DoEvents();
				((IBoardSectionControl)control).Refresh(BoardRefreshEventArgs.Empty);
				Application.DoEvents();

				var label = control.FindAll<ZLabel>().Single();

				AssertEquals("Queries must be active to be Instantaneous.", label.Text);
			}
		}

		#region Refresh

		public void TestControlRefresh()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Grumble");
			var section = MENTTestHelper.CreateMENTBoardSection(Factory, null, extraction.PK);

			var sectionViewModel = new BoardSectionViewModel(section, VisualBoardsTestHelper.CreateBoardViewModel(Factory.NewWithValidTestData<BMBoard>(), new MENTTestHelper.TestExtractorFactoryProvider()));

			Factory.Save();

			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new ChartSectionControl(sectionViewModel))
			{
				form.Show();
				form.Controls.Add(control);
				Application.DoEvents();

				var oldChartControl = control.chartControl;

				((IBoardSectionControl)control).Refresh(BoardRefreshEventArgs.Empty);
				Application.DoEvents();

				AssertNotEquals(oldChartControl, control.chartControl);
			}
		}

		public void TestControlRefresh_ShouldOnlyCalledOnce()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Grumble");
			var section = MENTTestHelper.CreateMENTBoardSection(Factory, null, extraction.PK);

			var sectionViewModel = new BoardSectionViewModel(section, VisualBoardsTestHelper.CreateBoardViewModel(Factory.NewWithValidTestData<BMBoard>(), new MENTTestHelper.TestExtractorFactoryProvider()));

			Factory.Save();

			int refreshCounter = 0;
			using (var form = new ZForm())
			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var control = new ChartSectionControl(sectionViewModel))
			{
				control.RefreshCompleted += (s, e) => refreshCounter++;
				form.Show();
				form.Controls.Add(control);
				Application.DoEvents();

				((IBoardSectionControl)control).Refresh(BoardRefreshEventArgs.Empty);
				Application.DoEvents();

				AssertEquals("After added ChartSectionControl to form, it should only fired refresh event once.", 1, refreshCounter);
			}
		}

		public void TestRefresh_MENTSection()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system, "bucket", offsetMinutes: 0, sequence: 0);
			var board = BMSTestHelper.CreateBoard(system, "board", "board");
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Grumble");
			var section = MENTTestHelper.CreateMENTBoardSection(Factory, board, extraction.PK);
			var sectionViewModel = new BoardSectionViewModel(section, VisualBoardsTestHelper.CreateBoardViewModel(Factory.NewWithValidTestData<BMBoard>(), new MENTTestHelper.TestExtractorFactoryProvider()));

			Factory.Save();

			var viewModel = VisualBoardFormBasherTest.GetViewModel(section.Board);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var chartSectionControl = form.FindAll<ChartSectionControl>().Single();

				bool refreshed = false;
				form.RefreshStarted += delegate
				{
					refreshed = true;
				};

				VisualBoardsFormTestHelper.PressHotkeys(chartSectionControl, Keys.F5);
				Application.DoEvents();

				Assert("Form.KeyDown should be called on pressing F5 on ChartSectionControl", refreshed);
			}
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			BMSRegistry.Instance.EnableMENTSections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}

	class ChartSectionControlNonTransactionedTest : NonTransactionedTestCase
	{
		public void TestControlRefresh_ShouldCallCompletedWhenQueryStarts()
		{
			var extraction = MENTTestHelper.CreateInstantaneousExtraction(Factory, "Poodle", "select count(*) Score, null ReleaseGroup, null Component, '' AttributeValue, '' Staff from dbo.StmALog a join dbo.StmALog b on a.SL_PK = b.SL_PK");
			var section = MENTTestHelper.CreateMENTBoardSection(Factory, null, extraction.PK);

			var sectionViewModel = new BoardSectionViewModel(section, VisualBoardsTestHelper.CreateBoardViewModel(Factory.NewWithValidTestData<BMBoard>(), new MENTTestHelper.TestExtractorFactoryProvider()));

			Factory.Save();

			bool refreshCompleted = false;

			var triggerable = new TriggerableAsyncStrategy();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(triggerable))
			using (var form = new ZForm())
			using (var control = new ChartSectionControl(sectionViewModel))
			{
				control.RefreshCompleted += (s, e) => refreshCompleted = true;
				form.Show();
				form.Controls.Add(control);
				Application.DoEvents();

				((IBoardSectionControl)control).Refresh(BoardRefreshEventArgs.Empty);

				AssertEquals(true, refreshCompleted);
				AssertNull("When the background thread starts there is no control created", control.chartControl);

				triggerable.DoAllActions();
				AssertNotNull("After background thread completes the control should be created", control.chartControl);
			}
		}
	}

	[TestedType(typeof(ChartSectionControl))]
	class ChartSectionControlIBoardSectionControlTest : BoardSectionControlTestCase<ChartSectionControl>
	{
		protected override ChartSectionControl GetControl()
		{
			var section = Factory.New<IBMBoardSection>();
			section.MS_SectionType = MENTConstants.ChartSectionType;
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Grumble");

			var configuration = (ChartSectionConfiguration)section.Configuration;
			configuration.ExtractionPK = extraction.PK;
			var sectionViewModel = new BoardSectionViewModel(section, VisualBoardsTestHelper.CreateBoardViewModel(Factory.NewWithValidTestData<BMBoard>(), new MENTTestHelper.TestExtractorFactoryProvider()));

			Factory.Save();

			return new ChartSectionControl(sectionViewModel);
		}
	}
}
