using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.Business.Test;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.GUI.Test
{
	[TestedType(typeof(PreviewVisualisationForm))]
	class PreviewVisualisationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Turtledoves");
			MENTTestHelper.TurnAllSeriesAndCategoryColumnsOnForExtraction(extraction);
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "dudexxxx", extraction);

			Factory.Save();
			var viewModelProvider = new VisualisationViewModelProvider(extraction, visualisation);

			return new PreviewVisualisationForm(viewModelProvider, new MENTTestHelper.TestExtractorFactoryProvider());
		}

		protected override void SetUp()
		{
			base.SetUp();
			disposable = VisualBoardsTestCase.DisableAsyncBehaviour();
		}

		protected override void TearDown()
		{
			disposable.Dispose();
			base.TearDown();
		}

		IDisposable disposable;
	}

	class PreviewVisualisationFormTestNonTransactionTest : NonTransactionedTestCase
	{
		public void TestLoadingLabel()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Turtledoves");
			MENTTestHelper.TurnAllSeriesAndCategoryColumnsOnForExtraction(extraction);
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "dudexxxx", extraction);

			Factory.Save();
			var viewModelProvider = new VisualisationViewModelProvider(extraction, visualisation);

			var triggerable = new TriggerableAsyncStrategy();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(triggerable))
			using (var form = new PreviewVisualisationForm(viewModelProvider, new MENTTestHelper.TestExtractorFactoryProvider()))
			{
				form.Show();

				AssertEquals("Loading, please wait...", form.loadingLabel.Text);

				triggerable.DoAllActions();

				AssertNull(form.loadingLabel);
				AssertNotNull(form.FindAll<MENTChartWindowsControl>().FirstOrDefault());
			}
		}

		public void TestBottomBarRemoved()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Turtledoves");
			MENTTestHelper.TurnAllSeriesAndCategoryColumnsOnForExtraction(extraction);
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "dudexxxx", extraction);

			Factory.Save();
			var viewModelProvider = new VisualisationViewModelProvider(extraction, visualisation);

			var triggerable = new TriggerableAsyncStrategy();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(triggerable))
			using (var form = new PreviewVisualisationForm(viewModelProvider, new MENTTestHelper.TestExtractorFactoryProvider()))
			{
				form.Show();

				AssertEquals("Loading, please wait...", form.loadingLabel.Text);

				triggerable.DoAllActions();

				AssertNull(form.loadingLabel);
				AssertEquals("There should be no bottom bar", 1, form.Controls.Count);
				AssertEquals("There should be no bottom bar", 1, form.Controls.OfType<MENTChartWindowsControl>().Count());
			}
		}
	}
}
