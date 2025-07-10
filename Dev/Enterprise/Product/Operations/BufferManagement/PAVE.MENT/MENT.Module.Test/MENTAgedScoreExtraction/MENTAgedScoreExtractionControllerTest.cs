using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Module.Test;
using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Module.Test
{
	[TestedType(typeof(MENTAgedScoreExtractionController))]
	class MENTAgedScoreExtractionControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.MENTAgedScoreExtraction;
		}

		public void TestViewForm_ShouldHideVisualizationTab()
		{
			BMSRegistry.Instance.EnableMENTSections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var extraction = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			var controller = new MENTAgedScoreExtractionController();
			Factory.Save();
			using (var form = controller.ShowViewForm(extraction))
			{
				AssertType(typeof(MENTAgedScoreQueryForm), form);

				Application.DoEvents();

				var mentTabs = ((MENTAgedScoreQueryForm)form).FindAll<MENTNavigationTabPage>();
				Assert("visualization tab should not be found", !mentTabs.Any());
			}
		}

		public void TestViewForm_ShouldOpenMENTAgedScoreQueryForm_AndSelectTheExtraction()
		{
			var extraction = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			var controller = new MENTAgedScoreExtractionController();
			Factory.Save();
			using (var form = controller.ShowViewForm(extraction))
			{
				AssertType(typeof(MENTAgedScoreQueryForm), form);

				Application.DoEvents();

				var mentTab = ((MENTAgedScoreQueryForm)form).FindSingle<MENTNavigationTabPage>();
				var zTabControl = mentTab.Parent as ZTabControl;
				Assert("the selected tab should be the visualization tab", zTabControl.SelectedTab == mentTab);

				var visualisationControl = mentTab.FindSingle<VisualisationConfigurationControl>();
				Assert("the extraction selected should match", ((MENTAgedScoreExtraction)visualisationControl.ExtractionsGrid_ForTest.ListManager.Current).PK == extraction.PK);
			}
		}

		public void TestEditForm_ShouldOpenMENTAgedScoreQueryForm_AndSelectTheExtraction()
		{
			var extraction = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			var controller = new MENTAgedScoreExtractionController();
			Factory.Save();
			using (var form = controller.ShowEditForm(extraction))
			{
				AssertType(typeof(MENTAgedScoreQueryForm), form);

				Application.DoEvents();

				var mentTab = ((MENTAgedScoreQueryForm)form).FindSingle<MENTNavigationTabPage>();
				var zTabControl = mentTab.Parent as ZTabControl;
				Assert("the selected tab should be the visualization tab", zTabControl.SelectedTab == mentTab);

				var visualisationControl = mentTab.FindSingle<VisualisationConfigurationControl>();
				Assert("the extraction selected should match", ((MENTAgedScoreExtraction)visualisationControl.ExtractionsGrid_ForTest.ListManager.Current).PK == extraction.PK);
			}
		}

		public override void TestNewForm()
		{
			Assert(true); // Actions not supported
		}

		public override void TestViewForm()
		{
			Assert(true); // Actions not supported
		}

		public override void TestEditForm()
		{
			Assert(true); // Actions not supported
		}

		public override void TestDeleteForm()
		{
			Assert(true); // Actions not supported
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.EnableMENTSections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}
}
