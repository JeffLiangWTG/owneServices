using System.Linq;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(FilterCollectionEmbeddedModulePopup))]
	public class FilterCollectionEmbeddedModulePopupBasherTest : EmbeddModulePopupBasherTest
	{
		public void TestValidateSelectedFilters()
		{
			using (var parentModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessTasks))
			{
				var parentStrip = parentModule.FilterBusinessObject.FilterStrips.AddNew("Parent Job");
				var parentFilter = (ModuleGuidModuleSpecifiedFilter)parentStrip.CurrentModuleFilter;
				parentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				parentFilter.SelectedModule = ModuleIDs.Organisation.Name;

				using (var findBox = new ZFilterCollectionFindBoxTest.ZFilterCollectionFindBoxForTest(parentFilter))
				using (var popup = findBox.PopupForm_Exposed())
				{
					popup.Show();
					var helper = new EmbeddedModuleTestHelper(popup);
					var strip = helper.AddFilter<ModuleNkFilter>("Creating User", f => f.Property = "XYI");
					Application.DoEvents();

					var isOpen = true;
					popup.Closed += (sender, args) => isOpen = false;

					UnitTestUserNotification.Instance.AddOKAnswer();
					popup.ExposedOKButtonForTesting.PerformClick();
					Application.DoEvents();

					AssertEquals("There are errors. Please correct these before continuing.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, isOpen);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var filter = (ModuleNkFilter)strip.CurrentModuleFilter;
					filter.Property = Env.CurrentUser.Initials;

					popup.ExposedOKButtonForTesting.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

					Application.DoEvents();
					AssertEquals(false, isOpen);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		public void TestPopup_ShouldNotShowRecentItemsPanel()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ProcessHeader))
			using (var moduleForm = (Form)module.ShowPopup())
			{
				var parentRecentItemsPanel = moduleForm.FindSingle<ZPanel>("RecentItemsPanel");
				AssertEquals("Recent items should be shown on the main module windows, and yet...", true, parentRecentItemsPanel.Visible);

				var strip = module.FilterBusinessObject.FilterStrips.AddNew("Release Group");
				var filter = (ModuleGuidFilter)strip.CurrentModuleFilter;
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

				var stripControl = moduleForm.FindSingle<StripControl>();
				stripControl.AddFilterStrip(strip);
				var findBox = stripControl.FindSingle<ZFilterCollectionFindBox>();
				Form popup = null;

				try
				{
					findBox.PopupButton.PerformClick();
					popup = Application.OpenForms.OfType<FilterCollectionEmbeddedModulePopup>().Single();
					var recentItemsPanel = popup.FindSingle<ZPanel>("RecentItemsPanel");
					AssertEquals("Popups for filters match should not show recent items because you're not selecting an item, you're selecting filters. And yet...", false, recentItemsPanel.Visible);
				}
				finally
				{
					popup?.Dispose();
				}

				var comparisonOperatorDrop = stripControl.FindSingle<ZDropEdit>(x => x.Name == "OperatorDropEdit");
				comparisonOperatorDrop.Text = ModuleTextFilter.ComparisonConstants.Exact;
				var newFindBox = stripControl.FindSingle<ZCodeFindBox>(x => x.Name == "PropertyFindBox");
				newFindBox.Visible = true;

				try
				{
					newFindBox.PopupButton.PerformClick();
					popup = Application.OpenForms.OfType<EmbeddedModulePopup>().Single(f => f.Text == "Group");
					var recentItemsPanel = popup.FindSingle<ZPanel>("RecentItemsPanel");
					AssertEquals("Normal popups should show recent items because the user can select an item from that list as well as the grid, and yet...", true, recentItemsPanel.Visible);
				}
				finally
				{
					popup?.Dispose();
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new FilterCollectionEmbeddedModulePopup(new DummyFilterGridModule());
		}
	}
}
