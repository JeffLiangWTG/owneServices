using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI.Test
{
	class MENTAgedScoreExtractionFormFactoryTest : TestCaseWithFactory
	{
		public void TestForMENTAgedScoreExtraction_ViewAndEdit_ShouldOpenMENTAgedScoreQueryForm()
		{
			var mentAgedScoreExtraction = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			using (var form = MENTAgedScoreExtractionFormFactory.ShowForm(mentAgedScoreExtraction))
			{
				AssertType(typeof(MENTAgedScoreQueryForm), form);
			}
		}

		public void TestShowForm_ForMENTAgedScoreExtraction_WithExtractions_ShouldOpenTheVisualizationTab_AndSelectTheExtraction()
		{
			BMSRegistry.Instance.EnableMENTSections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var mentQueryScore = Factory.NewWithValidTestData(typeof(MENTAgedScoreQuery)) as MENTAgedScoreQuery;

			var extraction1 = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			var extraction2 = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			var extraction3 = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();

			mentQueryScore.Extractions.Add(extraction1);
			mentQueryScore.Extractions.Add(extraction2);
			mentQueryScore.Extractions.Add(extraction3);

			using (var form = (MENTAgedScoreQueryForm)MENTAgedScoreExtractionFormFactory.ShowForm(extraction2))
			{
				form.Show();
				MENTAgedScoreExtractionFormFactory.NavigateToExtractionItem(form, extraction2);

				Application.DoEvents();

				var mentTab = form.FindSingle<MENTNavigationTabPage>();
				var zTabControl = mentTab.Parent as ZTabControl;
				Assert("the selected tab should be the visualization tab", zTabControl.SelectedTab == mentTab);

				var visualisationControl = mentTab.FindSingle<VisualisationConfigurationControl>();
				Assert("the extraction selected should match", ((MENTAgedScoreExtraction)visualisationControl.ExtractionsGrid_ForTest.ListManager.Current).PK == extraction2.PK);
			}
		}
	}
}
