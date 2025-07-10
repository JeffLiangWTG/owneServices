using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class FilterRuleFilterStripControlTest : TransactionedTestCase
	{
		public void TestOnDataItemChangeSourceHandlesTestData()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var control = new FilterRuleFilterStripControl(filterStrip, "", ""))
			{
				form.Controls.Add(control);

				form.Show();

				control.SetDataBinding(new object(), "");

				AssertNull(control.Source);
			}
		}

		public void TestPreviewButtonShowsAndClicks()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var control = new FilterRuleFilterStripControlForTest(filterStrip, ""))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(false, control.ToolStripPreviewDropButton_Exposed.Visible);

				control.SetIsPreviewAllowed("Dummy");
				AssertEquals(true, control.ToolStripPreviewDropButton_Exposed.Visible);

				UnitTestUserNotification.Instance.AddOKAnswer();
				control.ToolStripPreviewDropButton_Exposed.PerformClick();
				AssertEquals(true, control.PreviewClickedFlag);
			}
		}

		public void TestResetFiltersSetHasChanges()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var control = new FilterRuleFilterStripControlForTest(filterStrip, ""))
			{
				form.Controls.Add(control);
				form.Show();

				Assert("Precondition: HasChanges is false", !filterStrip.HasChanges);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				control.ToolStripResetLayoutButton_Exposed.PerformClick();
				Assert("Reset Filter button should set filter HasChanges property to true", filterStrip.HasChanges);
			}
		}

		public void TestFilterPanelSizeAdjusted()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var control = new FilterRuleFilterStripControlForTest(filterStrip, ""))
			{
				form.Controls.Add(control);
				form.Show();
				control.Size = new System.Drawing.Size(500, 400);
				var initialSize = control.MaxFilterStripPanelHeightExposed;
				control.Size = new System.Drawing.Size(500, 900);
				var sizeAfterControlResizing = control.MaxFilterStripPanelHeightExposed;
				Assert(sizeAfterControlResizing > initialSize);
			}
		}

		#region Preview

		public void TestPreviewButtonShowsCorrectModule()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var control = new FilterRuleFilterStripControlForTest(filterStrip, ""))
			{
				form.Controls.Add(control);
				form.Show();

				control.SetIsPreviewAllowed("Dummy");
				UnitTestUserNotification.Instance.AddOKAnswer();
				control.ToolStripPreviewDropButton_Exposed.PerformClick();

				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);

				var popup = ZFormModaliser.LastFormShownDialogForTest as EmbeddedModulePopup;
				AssertNotNull(popup);
				AssertEquals(DummyModuleIDs.Dummy, popup.Module.ID);
				AssertEquals(false, popup.ExposedOKButtonForTesting.Visible);
				AssertEquals("Close", popup.CancelButtonForTest.Text);
			}
		}

		public void TestPreviewPopupFlags()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var control = new FilterRuleFilterStripControlForTest(filterStrip, ""))
			{
				form.Controls.Add(control);
				form.Show();

				control.SetIsPreviewAllowed("Dummy");
				UnitTestUserNotification.Instance.AddOKAnswer();
				control.ToolStripPreviewDropButton_Exposed.PerformClick();

				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);

				var popup = ZFormModaliser.LastFormShownDialogForTest as EmbeddedModulePopup;
				AssertNotNull(popup);
				AssertEquals("The preview window should not show an error when the user clicks OK and nothing is selected, and yet...", false, popup.IsSelectionMandatory);

				var filterControl = (ZFilterStripCommonControl)popup.Module.EmbeddedControl;

				AssertNotNull(filterControl);

				AssertEquals(true, filterControl.IsFilterReadonly);
				AssertEquals(true, filterControl.RunSearchOnEnteringAModuleOverride);
			}
		}

		[ExpectNoExceptions]
		public void TestPreviewButtonDoesntThrowForIncorrectModule()
		{
			var filterStrip = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var control = new FilterRuleFilterStripControlForTest(filterStrip, ""))
			{
				form.Controls.Add(control);
				form.Show();

				control.SetIsPreviewAllowed("ПЫЩЬ ПЫЩЬ");
				UnitTestUserNotification.Instance.AddOKAnswer();
				control.ToolStripPreviewDropButton_Exposed.PerformClick();

				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestSetIsPreviewAllowed_WithMenuOptions_ShouldCreateDropDownButton()
		{
			var factory = new BusinessObjectFactory();
			var dummyA = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyA.Z0_Description = "A";
			var dummyB = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyB.Z0_Description = "B";

			var previewable = factory.NewWithValidTestData<FilterPreviewableDummy>();
			var filterBizo = new DummyFilterBusinessObject();

			factory.Save();

			using (var form = new ZForm(previewable) { Size = ControlDpiScalingHelper.NewScaledSize(800, 600) })
			using (var control = new FilterRuleFilterStripControlForTest(filterBizo, ""))
			{
				form.Controls.Add(control);

				var list = new CodeDescriptionPairList();
				list.AddPair("A", "Preview - A");
				list.AddPair("B", "Preview - B");
				control.SetIsPreviewAllowed(DummyModuleIDs.Dummy.Name, list);

				form.Show();
				Application.DoEvents();

				var previewButton = control.ToolStripPreviewDropButton_Exposed;
				var menuItem = previewButton.DropDownItems[0];

				var result = new List<DummyBusinessObject>();
				ZFormModaliser.ShowDialogsInTest = true;
				EmbeddedModuleTestHelper.SetListToStoreSearchResultsWhenPopupShown(result);

				menuItem.PerformClick();
				Application.DoEvents();
				AssertContainsExactElementsInAnyOrder(new[] { "A" }, result.Select(x => x.Z0_Description.ToString()));

				menuItem = previewButton.DropDownItems[1];
				menuItem.PerformClick();
				Application.DoEvents();
				AssertContainsExactElementsInAnyOrder(new[] { "B" }, result.Select(x => x.Z0_Description.ToString()));
			}
		}

		#endregion

		public void TestControlsShouldBeMovedToSplitContainer()
		{
			var filterBizo = new FilterStripBusinessObjectForTest();
			using (var control = new FilterRuleFilterStripControlForTest(filterBizo, ""))
			{
				var container = control.SplitContainer_Exposed;

				AssertNotNull(container.Panel1.Controls.Find("FilterStripsPanel", true).SingleOrDefault());
				AssertNotNull(container.Panel2.Controls.Find("ToolStrip", true).SingleOrDefault());
				AssertNotNull(container.Panel2.Controls.Find("ToolStripHelp", true).SingleOrDefault());
				AssertNotNull(container.Panel2.Controls.Find("CoveringLabel", true).SingleOrDefault());
				AssertNotNull(container.Panel2.Controls.Find("ToolStripColourPicker", true).SingleOrDefault());
				AssertNotNull(container.Panel2.Controls.Find("ToolStripRecordsFoundLabel", true).SingleOrDefault());
				AssertNotNull(container.Panel2.Controls.Find("AddStripButton", true).SingleOrDefault());
			}
		}

		public void TestToolStripTop_ShouldAlwaysBeZero()
		{
			var filterBizo = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var control = new FilterRuleFilterStripControlForTest(filterBizo, ""))
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(0, control.ToolStripTop_Exposed);
				AssertEquals(0, control.ToolStripGroupTop_Exposed);

				control.AddFilterStrip(filterBizo.FilterStrips.AddNew("desc"));

				AssertEquals(0, control.ToolStripTop_Exposed);
				AssertEquals(0, control.ToolStripGroupTop_Exposed);

				control.AddGroupFilterControl("Shalala");
				control.AddStrip();

				AssertEquals(0, control.ToolStripTop_Exposed);
				AssertEquals(0, control.ToolStripGroupTop_Exposed);
			}
		}

		public void TestAddFilterStrip_ShouldMoveSplitterDistance()
		{
			var filterBizo = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var control = new FilterRuleFilterStripControlForTest(filterBizo, ""))
			{
				form.Controls.Add(control);
				form.Show();

				control.AddFilterStrip(filterBizo.FilterStrips.AddNew("desc"));
				var distance = control.SplitContainer_Exposed.SplitterDistance;

				control.AddFilterStrip(filterBizo.FilterStrips.AddNew("desc"));

				AssertNotEquals(distance, control.SplitContainer_Exposed.SplitterDistance);
				AssertEquals(true, control.SplitContainer_Exposed.SplitterDistance > distance);

				distance = control.SplitContainer_Exposed.SplitterDistance;
				control.AddGroupFilterControl("Shalala");
				control.AddStrip();

				AssertNotEquals(distance, control.SplitContainer_Exposed.SplitterDistance);
				AssertEquals(true, control.SplitContainer_Exposed.SplitterDistance > distance);
			}
		}

		public void TestSplitterDistance_ShouldNotExceedMaxPanelHeight()
		{
			var filterBizo = new FilterStripBusinessObjectForTest();

			using (var form = new ZForm())
			using (var control = new FilterRuleFilterStripControlForTest(filterBizo, ""))
			{
				form.Controls.Add(control);
				form.Show();

				for (var i = 0; i < 30; i++)
				{
					control.AddFilterStrip(filterBizo.FilterStrips.AddNew("desc"));
				}

				AssertEquals(true, control.SplitContainer_Exposed.SplitterDistance <= control.MaxFilterStripPanelHeightExposed);
			}
		}

		public void TestSplitContainer_ShouldHaveFixedSplitter()
		{
			var filterBizo = new FilterStripBusinessObjectForTest();
			using (var control = new FilterRuleFilterStripControlForTest(filterBizo, ""))
			{
				AssertEquals(true, control.SplitContainer_Exposed.IsSplitterFixed);
			}
		}

		public void TestFilterStripsPanel_ShouldHaveDockSetToFill()
		{
			var filterBizo = new FilterStripBusinessObjectForTest();
			using (var control = new FilterRuleFilterStripControlForTest(filterBizo, ""))
			{
				AssertEquals(DockStyle.Fill, control.FilterStripsPanel_Exposed.Dock);
			}
		}

		public void TestSplitterDistance_ShouldNotGoTooHighOrTooLow_ForAnyNumberOfFilterStrips()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var splitterDistanceCommands = new List<Tuple<int, int, int>>(); // min, max, actual 
				var filterBizo = module.FilterBusinessObject;

				void ShowControlAndAssertDistancesWereWithinAllowedRange()
				{
					using (var control = new FilterRuleFilterStripControlForTest(filterBizo, ""))
					using (var form = new Form { Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
					{
						control.SplitterDistanceSetting += (sender, args) =>
						{
							// Setting the splitter outside the allowed values will thrown an InvalidOperationException: "SplitterDistance must be between Panel1MinSize and Width - Panel2MinSize." (when they say Width they also mean Height in this case)
							var splitter = ((FilterRuleFilterStripControlForTest)sender).SplitContainer_Exposed;
							var min = splitter.Panel1MinSize;
							var max = splitter.Height - splitter.Panel2MinSize;

							splitterDistanceCommands.Add(Tuple.Create(min, max, args.Distance));
						};
						form.Controls.Add(control);

						form.Show();
						Application.DoEvents();

						AssertEquals(true, splitterDistanceCommands.Any());

						CombineAssertions(() =>
						{
							foreach (var command in splitterDistanceCommands)
							{
								var min = command.Item1;
								var max = command.Item2;
								var distance = command.Item3;

								AssertEquals(string.Format(CultureInfo.InvariantCulture, "The split container should not have attempted to set the distance ({0}) lower than the minimum ({1}). Sad!", distance, min), true, distance >= min);
								AssertEquals(string.Format(CultureInfo.InvariantCulture, "The split container should not have attempted to set the distance ({0}) higher than the maximum ({1}). Sad!", distance, max), true, distance <= max);
							}
						});
					}
				}

				ShowControlAndAssertDistancesWereWithinAllowedRange();

				for (var i = 0; i < 20; i++)
				{
					filterBizo.AddTextFilterStrip("Description", "Please 'vote' Yes.");
				}

				ShowControlAndAssertDistancesWereWithinAllowedRange();
			}
		}

		public void TestSplitterDistance_ShouldNotGoTooHighOrTooLow_WhenPanel2MinSizeManuallyManipulated()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var filterBizo = module.FilterBusinessObject;

				using (var control = new FilterRuleFilterStripControlForTest(filterBizo, ""))
				using (var form = new Form { Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
				{
					control.SplitContainer_Exposed.Panel2MinSize = 1000;

					AssertNoExceptionThrown(() => form.Controls.Add(control));
					form.Show();
					Application.DoEvents();
				}
			}
		}

		public void TestReadOnly_ShouldDisableEverythingExceptPanelAndPreview()
		{
			using (var module = (ZFilterModule)ZModule.GetZModule(ModuleIDs.ProcessTasks))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Description");
				strip.GroupName = "First Group";
				strip = filterBizo.FilterStrips.AddNew("Description");
				strip.GroupName = "First Group";
				strip = filterBizo.FilterStrips.AddNew("Description");
				strip.GroupName = "Second Group";
				strip = filterBizo.FilterStrips.AddNew("Description");
				strip.GroupName = "Second Group";

				using (var wrapperControl = new WrapperControl_ForTest(filterBizo) { Dock = DockStyle.Fill, IsPreviewAllowed = true })
				using (var form = new Form() { Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
				{
					form.Controls.Add(wrapperControl);
					form.Show();
					Application.DoEvents();
					var control = wrapperControl.StripControl;

					AssertEquals(true, control.GroupExists("First Group"));
					AssertEquals(true, control.GroupExists("Second Group"));
					AssertChildControlState("Controls should be enabled by default, and yet...", control, shouldBeEnabled: true);

					control.SetReadOnly(true);
					AssertChildControlState("Controls should be disabled when the parent control is read-only, and yet...", control, shouldBeEnabled: false);

					control.SetReadOnly(false);
					AssertEquals("ReadOnly is a reversible process, so no report is raised on changing it", string.Empty, ErrorReporter.LastKeyReported);
					AssertChildControlState("Controls should be enabled when the parent control is set to not read-only, and yet...", control, shouldBeEnabled: true);
				}
			}

			ErrorReporter.Clear();
		}

		static void AssertChildControlState(string message, FilterRuleFilterStripControlForTest control, bool shouldBeEnabled)
		{
			AssertEquals("The FilterStripsPanel should always be enabled (even when read-only) so the scrollbar still works, and yet...", true, control.FilterStripsPanel_Exposed.Enabled);

			var strips = control.FilterStripsPanel_Exposed.FindAll<GroupStripControl>().Cast<Control>().Concat(control.FilterStripsPanel_Exposed.FindAll<ZFilterStrip>());

			foreach (var strip in strips)
			{
				AssertEquals(message, shouldBeEnabled, strip.FindAll<TextBox>().First().Enabled);
				var typesExcludedFromOverlapTest = new[] { typeof(GroupPanel), typeof(KPanel) };
				AssertControlsDoNotOverlap(strip.Controls.OfType<Control>().Where(x => x.Visible && !typesExcludedFromOverlapTest.Contains(x.GetType())));

				var deleteButton = strip.Controls.Find("DeleteStripButton", true).Single(x => x.Parent == strip);
				AssertEquals("The delete button's appearance should reflect its enabled state, and yet...", Icons.GetImage(shouldBeEnabled ? IconTypes.MinusButtonRest : IconTypes.MinusButtonDisabled), deleteButton.BackgroundImage);

				var groupStrip = strip as GroupStripControl;

				if (groupStrip != null)
				{
					AssertEquals("The add button's appearance should reflect its enabled state, and yet...", Icons.GetImage(shouldBeEnabled ? IconTypes.AddButtonRest : IconTypes.AddButtonDisabled), groupStrip.AddStripButton.BackgroundImage);
					AssertEquals(shouldBeEnabled, groupStrip.FilterCategoriesToolStrip.Enabled);
				}

				foreach (var button in strip.FindAll<ZButton>())
				{
					AssertEquals(message, shouldBeEnabled, button.Enabled);
				}
			}

			foreach (var toolStripControl in control.ToolStripControls_Exposed.Where(x => x != control.ToolStrip_Exposed))
			{
				AssertEquals(message, shouldBeEnabled, toolStripControl.Enabled);
			}

			foreach (ToolStripItem item in control.ToolStrip_Exposed.Items)
			{
				if (item == control.ToolStripPreviewDropButton_Exposed)
				{
					AssertEquals("The Preview Button should always be enabled, as it is a readonly action", true, item.Enabled);
				}
				else
				{
					AssertEquals($"Non-Preview Buttons should be enabled based on the form's readonly status, but the {item.Name} button isn't doing that!", shouldBeEnabled, item.Enabled);
				}
			}
		}

		static void AssertControlsDoNotOverlap(IEnumerable<Control> controls)
		{
			var orderedControls = controls.OrderBy(x => x.Left);
			Control previousControl = null;

			foreach (var control in orderedControls)
			{
				if (previousControl != null)
				{
					var message = string.Format(CultureInfo.InvariantCulture, "Some of the visible controls are overlapping when filter strips are added to a FilterRuleFilterStripControl. The overlapping controls are [{0}: {1}] and [{2}: {3}]", previousControl.GetType().Name, previousControl.Name, control.GetType().Name, control.Name);
					AssertEquals(message, true, control.Left > previousControl.Right);
				}

				previousControl = control;
			}
		}

		internal class FilterRuleFilterStripControlForTest : FilterRuleFilterStripControl
		{
			public FilterRuleFilterStripControlForTest(FilterStripBusinessObject filterStrip, string newFilterStripString)
				: base(filterStrip, newFilterStripString, "")
			{
			}

			protected override void OnPreviewClicked(string dropDownCode = null)
			{
				PreviewClickedFlag = true;
				base.OnPreviewClicked(dropDownCode);
			}

			protected override void SetSplitterDistance(int distance)
			{
				SplitterDistanceSetting?.Invoke(this, new SplitterDistanceSettingEventArgs(distance));

				base.SetSplitterDistance(distance);
			}

			internal event EventHandler<SplitterDistanceSettingEventArgs> SplitterDistanceSetting;

			internal class SplitterDistanceSettingEventArgs : EventArgs
			{
				public int Distance { get; }

				public SplitterDistanceSettingEventArgs(int distance)
				{
					Distance = distance;
				}
			}

			public bool PreviewClickedFlag { get; private set; }

			public int MaxFilterStripPanelHeightExposed => MaxFilterStripPanelHeight;
			public KSplitContainer SplitContainer_Exposed => SplitContainer;
			public int ToolStripTop_Exposed => ToolStripTop;
			public int ToolStripGroupTop_Exposed => ToolStripGroupTop;
			public KPanel FilterStripsPanel_Exposed => FilterStripsPanel;
			public ZToolStrip ToolStrip_Exposed => ToolStrip;
			public ZToolStripDropDownButton ToolStripPreviewDropButton_Exposed => ToolStripPreviewDropButton;
			public ZToolStripButton ToolStripResetLayoutButton_Exposed => ToolStripResetLayoutButton;
			public IEnumerable<Control> FilterStripControls_Exposed => FilterStripControls;
			public IEnumerable<Control> ToolStripControls_Exposed => ToolStripControls;
		}

		class FilterStripBusinessObjectForTest : FilterStripBusinessObject
		{
			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				var filters = new ModuleFilterCollection();
				filters.AddTextFilter("desc", delegate
				{
					return new ZQuery();
				});
				return filters;
			}
		}

		internal class WrapperControl_ForTest : FilterStripWrapperControl
		{
			public WrapperControl_ForTest(FilterStripBusinessObject filterBizo)
			{
				this.filterBizo = filterBizo;
				SetDataBinding(filterBizo.SaveLayout("MyLayout"), string.Empty);
			}

			readonly FilterStripBusinessObject filterBizo;

			protected override FilterRuleFilterStripControl GetNewFilterStripControl()
			{
				return new FilterRuleFilterStripControlForTest(filterBizo, NewFilterStripString);
			}

			public FilterRuleFilterStripControlForTest StripControl => (FilterRuleFilterStripControlForTest)stripControl;
		}

		class FilterPreviewableDummy : DummyBusinessObject, IFilterPreviewable
		{
			public FilterPreviewableDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZQuery GetAdditionalPreviewFilter(string moduleId, string dropDownCode)
			{
				return new ZQuery(DummyBizoSchema.Z0_Description, dropDownCode);
			}
		}
	}
}
