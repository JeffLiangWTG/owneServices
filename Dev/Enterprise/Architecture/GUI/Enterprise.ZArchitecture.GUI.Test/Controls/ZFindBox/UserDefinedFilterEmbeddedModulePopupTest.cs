using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class UserDefinedFilterEmbeddedModulePopupTest : TestCaseWithFactory
	{
		public void TestModuleUserDefinedFilter_SaveButtonEnable()
		{
			FilterStripsTestHelper.CreateUserDefinedFilterStrip_TwoProperties("User Defined Filter 2", true);

			var newStaff = Factory.LoadTop1<IGlbStaff>(new ZQuery(GlbStaffSchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentUserPK));
			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertRightMessage(EnvProxy.Instance.Security.PublishUserDefinedFilters);
				AssertRightMessage(EnvProxy.Instance.Security.EditUserDefinedFilters);
			}
		}

		void AssertRightMessage(ISecurityCheckpoint checkpoint)
		{
			try
			{
				checkpoint.IsAllowed = false;
				using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				using (var form = (ZForm)module.ShowPopup())
				{
					Application.DoEvents();

					var filterControl = form.FindSingle<ZFilterStripBaseControl>();
					var filterStrip = filterControl.FindSingle<ZFilterStrip>();
					filterStrip.FilterDescriptionDropEdit.CodeBox.Text = ModuleUserDefinedFilter.GetSuffixedDescription("User Defined Filter 2");
					module.Grid.Focus();
					var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();

					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
					{
						var popup = dialog as UserDefinedFilterEmbeddedModulePopup;

						popup.Shown += (_, x_) =>
						{
							var embeddedFilterControl = popup.Module.DisplayGrid.GetParentFilterControl();
							AssertEquals(true, embeddedFilterControl.IsFilterReadonly);
							AssertEquals("View User Defined Filter 2", popup.Text);
							AssertEquals(false, popup.OKButton_Exposed.Visible);
						};
					});

					filterFindBox.PopupButton.PerformClick();
					Application.DoEvents();
					Assert(UnitTestUserNotification.Instance.LastMessage.Contains(checkpoint.ErrorMessageForNotAllowed));
				}
			}
			finally
			{
				checkpoint.IsAllowed = true;
			}
		}

		public void TestModuleUserDefinedFilter_PerformSaveButton()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy4 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Description = "Walla Walla";
			dummy2.Z0_Description = "Keyokuk";
			dummy3.Z0_Description = "Keyokuk";
			dummy4.Z0_Description = "Keyokuk";
			dummy1.Z0_Code = "AAA";
			dummy2.Z0_Code = "BBB";
			dummy3.Z0_Code = "BBB";
			dummy4.Z0_Code = "CCC";
			dummy1.Z0_Number = 1;
			dummy2.Z0_Number = 2;
			dummy3.Z0_Number = 3;
			dummy4.Z0_Number = 4;

			Factory.Save();

			FilterStripsTestHelper.CreateUserDefinedFilterStrip_TwoProperties("Me filter", isPublished: true, isPublishedGlobal: true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = @"Me filter [+]";
				module.Grid.Focus();

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();
				FilterStripsTestHelper.AssertFindBoxText(null, filterFindBox, "2 filters applied");

				var embeddedResults = new List<BusinessObject>();

				SetUpModulePopup(true, DialogResult.OK, embeddedResults);
				filterFindBox.PopupButton.PerformClick();

				var newStaff = Factory.LoadTop1<IGlbStaff>(new ZQuery(GlbStaffSchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentUserPK));
				using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					EnvProxy.Instance.Security.EditUserDefinedFilters.IsAllowed = false;

					EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed = true;
					EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowedForAllBranches = true;
					Application.DoEvents();
					AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Edit User-Defined Filters", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestGloballyPublishedUserDefinedFilterStripPropagatesChanges()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy4 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Description = "Walla Walla";
			dummy2.Z0_Description = "Keyokuk";
			dummy3.Z0_Description = "Keyokuk";
			dummy4.Z0_Description = "Keyokuk";
			dummy1.Z0_Code = "AAA";
			dummy2.Z0_Code = "BBB";
			dummy3.Z0_Code = "BBB";
			dummy4.Z0_Code = "CCC";
			dummy1.Z0_Number = 1;
			dummy2.Z0_Number = 2;
			dummy3.Z0_Number = 3;
			dummy4.Z0_Number = 4;

			Factory.Save();

			FilterStripsTestHelper.CreateUserDefinedFilterStrip_TwoProperties("Me filter", isPublishedGlobal: true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = @"Me filter [+]";
				module.Grid.Focus();

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();
				FilterStripsTestHelper.AssertFindBoxText(null, filterFindBox, "2 filters applied");

				var embeddedResults = new List<BusinessObject>();

				SetUpModulePopup(true, DialogResult.OK, embeddedResults);
				filterFindBox.PopupButton.PerformClick();
				Application.DoEvents();
				AssertEquals("Any changes made to the user-defined filter [Me filter] will be saved. These changes will affect all saved layouts that use this filter. Would you like to save your changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContainsExactElementsInAnyOrder(new[] { dummy2.PK }, embeddedResults.Select(x => x.PK));

				filterControl.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder("The changes were saved, so the search results should use the new filters, and yet...", new[] { dummy2.PK }, module.GridCollection.ToArray().Select(x => x.PK));
				FilterStripsTestHelper.AssertFindBoxText("The changes should have been saved, so the filter strip should be updated immediately, and yet...", filterFindBox, "3 filters applied");
			}

			var newFactory = new BusinessObjectFactory();
			var moduleFilters = newFactory.Load<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_FilterName, "Me filter"));
			CombineAssertions(() =>
			{
				AssertEquals("Should not create an extra StmModule filter row", 1, moduleFilters.Length);
				AssertEquals("Should not be any rows that not published across companies", 0, moduleFilters.Where(f => !f.S9_GC.IsEmpty).Count());
			});

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = @"Me filter [+]";
				module.Grid.Focus();

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();

				FilterStripsTestHelper.AssertFindBoxText("The saved changes should have been propagated over to the new module. SAD.", filterFindBox, "3 filters applied");
			}
		}

		public void TestModuleUserDefinedFilter_ShouldUpdate_WhenNestedLayoutChangedExternally()
		{
			// make data to display in module grid
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Description = "Walla Walla";
			dummy1.Z0_Code = "AAA";
			dummy1.Z0_Number = 1;

			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Description = "Keyokuk";
			dummy2.Z0_Code = "BBB";
			dummy2.Z0_Number = 2;

			// make a filter layout that returns dummy2
			var myLayout = FilterStripsTestHelper.CreateUserDefinedFilterStrip_TwoProperties();
			var myLayoutDescription = myLayout.S9_FilterName;
			var myOuterLayoutName = "outerLayout";

			Factory.Save();

			// save layout nested under another layout
			FilterStripsTestHelper.AddUserDefinedFilterLayoutToAnotherFilterLayout(myLayoutDescription, myOuterLayoutName);

			Factory.Save();

			// assert that our outerLayout returns dummy2, as it currently matches our inner layout
			EmbeddedModuleTestHelper.AssertModuleGridContentsAfterChangingFilter(myOuterLayoutName, dummy2.PK);

			// change our inner layout to match dummy1
			myLayout = FilterStripsTestHelper.CreateUserDefinedFilterStrip_TwoProperties(property1Value: dummy1.Z0_Description, property2Value: dummy1.Z0_Code);

			Factory.Save();

			// assert we find dummy1 because our outerLayout should update to the new nested filter
			EmbeddedModuleTestHelper.AssertModuleGridContentsAfterChangingFilter(myOuterLayoutName, dummy1.PK);
		}

		public void TestModuleUserDefinedFilter_ShouldUpdate_WhenNestedLayoutChangedInternally()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Description = "Walla Walla";
			dummy1.Z0_Code = "AAA";
			dummy1.Z0_Number = 1;

			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Description = "Keyokuk";
			dummy2.Z0_Code = "BBB";
			dummy2.Z0_Number = 2;

			var myInnerLayoutName = "innerLayout";
			var myOuterLayoutName = "outerLayout";
			var myInnerLayout = FilterStripsTestHelper.CreateUserDefinedFilterStrip(myInnerLayoutName, propertyName: "Z0_Code", propertyValue: "AAA");

			Factory.Save();

			FilterStripsTestHelper.AddUserDefinedFilterLayoutToAnotherFilterLayout(myInnerLayoutName, myOuterLayoutName);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddFilterStrip<ModuleUserDefinedFilter>(ModuleUserDefinedFilter.GetPrefixedDescription(myOuterLayoutName));

				form.Show();
				Application.DoEvents();

				var filterQueryResults = EmbeddedModuleTestHelper.PerformFindOnModulePopupAndGetResults(module, form);
				AssertEquals("PRE: We only found one bizo", 1, filterQueryResults.Length);
				AssertEquals("PRE: We only find Dummy1, as we haven't mutated the filters yet", dummy1.PK, filterQueryResults.First().PK);

				foreach (var filter in filterBizo.ActiveModuleFilters)
				{
					filter.IsActive = false;
				}

				form.Show();
				Application.DoEvents();

				SetupModulePopups(dummy1, dummy2, myInnerLayoutName);

				var outerLayout = filterBizo.ModuleFilters.First(filter => filter.Description.Contains(myOuterLayoutName));
				EmbeddedModuleTestHelper.OpenUserDefinedFilterStripPopupButton(outerLayout as ModuleUserDefinedFilter, form);

				form.Show();
				Application.DoEvents();

				Factory.Save();

				filterQueryResults = EmbeddedModuleTestHelper.PerformFindOnModulePopupAndGetResults(module, form);

				AssertEquals("We only found one bizo", 1, filterQueryResults.Length);
				AssertEquals("We get dummy2 from our filter query, as the innermost layout's change is detected", dummy2.PK, filterQueryResults.First().PK);
			}
		}

		void SetupModulePopups(DummyBusinessObject dummy1, DummyBusinessObject dummy2, string innerLayoutName)
		{
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown((dialog) =>
			{
				if (dialog is UserDefinedFilterEmbeddedModulePopup popup)
				{
					if (popup.Text.Contains("Edit outerLayout"))
					{
						EmbeddedModuleTestHelper.OpenUserDefinedFilterStripPopupButton(innerLayoutName, popup);

						popup.Show();
						Application.DoEvents();

						popup.CancelButtonForTest.PerformClick();
					}
					else if (popup.Text.Contains("Edit innerLayout"))
					{
						popup.Show();
						Application.DoEvents();

						var filterStrip = popup.FindAll<ZFilterStrip>().First();
						var filterText = filterStrip.FindAll<ZTextBox>().First();

						AssertEquals("PRE: our filter was set to the dummy1 code", dummy1.Z0_Code, filterText.Text);

						filterText.Text = dummy2.Z0_Code;

						AssertEquals("PRE: we changed our innermost filter to dummy2's code", dummy2.Z0_Code, filterText.Text);
						popup.Show();
						Application.DoEvents();

						UnitTestUserNotification.Instance.AddOKAnswer();
						var currentFilter = filterStrip.CurrentDataItem.CurrentModuleFilter as ModuleTextFilter;
						currentFilter.Property = "BBB";
						currentFilter.FilterBusinessObject.Factory.Save(); // manually save the popup. For whatever reason, nested delegates don't save/close properly
						popup.ExposedOKButtonForTesting.PerformClick();
						Application.DoEvents();
					}

					popup.Show();
					Application.DoEvents();
				}
			});
		}

		public void TestUserDefinedFilter_WhenPopupOpened_ShouldHideManageLayoutButton()
		{
			EmbeddedModuleTestHelper.PopupUserDefinedFilter(popup =>
			{
				var embeddedFilterControl = popup.Module.DisplayGrid.GetParentFilterControl();

				AssertEquals(false, embeddedFilterControl.ToolStripManageDropButton_ForTest.Visible);
				AssertEquals(true, embeddedFilterControl.ToolStripSaveLayoutButton_ForTest.Visible);
			});
		}

		public void TestOkButton_Text_ShouldBeSave()
		{
			EmbeddedModuleTestHelper.PopupUserDefinedFilter(popup =>
			{
				AssertEquals("Save", popup.ExposedOKButtonForTesting.Text);
			});
		}

		public void TestUserDefinedFilter_WhenPopupOpened_ShouldHaveCustomTitleBarText()
		{
			FilterStripsTestHelper.CreateUserDefinedFilterStrip_TwoProperties();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = @"Me filter [+]";
				module.Grid.Focus();
				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var popup = (EmbeddedModulePopup)dialog;
					popup.Shown += (_, x_) =>
					{
						AssertEquals("Edit Me filter", popup.Text);
					};
				});

				filterFindBox.PopupButton.PerformClick();
				Application.DoEvents();

				filterFindBox.ReadOnly = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(dialog =>
				{
					var popup = (EmbeddedModulePopup)dialog;
					AssertEquals("View Me filter", popup.Text);
				});

				filterFindBox.PopupButton.PerformClick();
				Application.DoEvents();
			}
		}

		public void TestClickOk_ShouldPromptUserToSaveChanges()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy4 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Description = "Walla Walla";
			dummy2.Z0_Description = "Keyokuk";
			dummy3.Z0_Description = "Keyokuk";
			dummy4.Z0_Description = "Keyokuk";
			dummy1.Z0_Code = "AAA";
			dummy2.Z0_Code = "BBB";
			dummy3.Z0_Code = "BBB";
			dummy4.Z0_Code = "CCC";
			dummy1.Z0_Number = 1;
			dummy2.Z0_Number = 2;
			dummy3.Z0_Number = 3;
			dummy4.Z0_Number = 4;

			Factory.Save();
			FilterStripsTestHelper.CreateUserDefinedFilterStrip_TwoProperties();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = @"Me filter [+]";
				module.Grid.Focus();

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();
				FilterStripsTestHelper.AssertFindBoxText(null, filterFindBox, "2 filters applied");

				// First popup: don't change any filters
				var embeddedResults = new List<BusinessObject>();
				SetUpModulePopup(false, DialogResult.OK, embeddedResults);
				filterFindBox.PopupButton.PerformClick();
				Application.DoEvents();
				AssertEquals("No changes were made, but the user should still be prompted to save changes, and yet...", "Any changes made to the user-defined filter [Me filter] will be saved. These changes will affect all saved layouts that use this filter. Would you like to save your changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				FilterStripsTestHelper.AssertFindBoxText("The user said yes to save changes, but there were no actual changes, so the number of filters applied should not have changed, and yet...", filterFindBox, "2 filters applied");

				// Second popup: change filters but press cancel on the message
				SetUpModulePopup(true, DialogResult.Cancel, embeddedResults);
				filterFindBox.PopupButton.PerformClick();
				Application.DoEvents();
				AssertEquals("Any changes made to the user-defined filter [Me filter] will be saved. These changes will affect all saved layouts that use this filter. Would you like to save your changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContainsExactElementsInAnyOrder(new[] { dummy2.PK }, embeddedResults.Select(x => x.PK));

				filterControl.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder("The changes were not saved, so the search results should use the original filters, and yet...", new[] { dummy2.PK, dummy3.PK }, module.GridCollection.ToArray().Select(x => x.PK));
				FilterStripsTestHelper.AssertFindBoxText("The changes weren't saved, so there shouldn't be any change to the find box's text, and yet...", filterFindBox, "2 filters applied");

				var popup = ZApplication.GetOpenForms().OfType<EmbeddedModulePopup>().Single(x => x.Text == @"Edit Me filter");
				popup?.CancelButtonForTest?.PerformClick();
				Application.DoEvents();

				// Third popup: change filters and press OK on the message
				SetUpModulePopup(true, DialogResult.OK, embeddedResults);
				filterFindBox.PopupButton.PerformClick();
				Application.DoEvents();
				AssertEquals("Any changes made to the user-defined filter [Me filter] will be saved. These changes will affect all saved layouts that use this filter. Would you like to save your changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContainsExactElementsInAnyOrder(new[] { dummy2.PK }, embeddedResults.Select(x => x.PK));

				filterControl.FirePerformSearch();
				AssertContainsExactElementsInAnyOrder("The changes were saved, so the search results should use the new filters, and yet...", new[] { dummy2.PK }, module.GridCollection.ToArray().Select(x => x.PK));
				FilterStripsTestHelper.AssertFindBoxText("The changes should have been saved, so the filter strip should be updated immediately, and yet...", filterFindBox, "3 filters applied");
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = @"Me filter [+]";
				module.Grid.Focus();

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();
				FilterStripsTestHelper.AssertFindBoxText("The changes should have been saved, even when the filter is added on a new module, and yet...", filterFindBox, "3 filters applied");
			}
		}

		static void SetUpModulePopup(bool shouldAddNewFilterStrip, DialogResult popupAnswer, List<BusinessObject> findButtonResults)
		{
			findButtonResults.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(popupAnswer);
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				var popup = (UserDefinedFilterEmbeddedModulePopup)dialog;
				popup.FilterStripsLoaded += (_, x_) =>
				{
					var embeddedFilterControl = popup.Module.DisplayGrid.GetParentFilterControl();
					var filterStrips = embeddedFilterControl.FindAll<ZFilterStrip>().ToArray();
					AssertEquals(2, filterStrips.Length);
					AssertContainsExactElementsInAnyOrder("The original filter strips should be displayed, and yet...", new[] { "Z0_Description", "Z0_Code" }, filterStrips.Select(x => x.FilterDescriptionDropEdit.CodeBox.Text));
				};

				popup.Shown += (_, x_) =>
				{
					var embeddedFilterControl = popup.Module.DisplayGrid.GetParentFilterControl();

					if (shouldAddNewFilterStrip)
					{
						var strip = popup.Module.FilterBusinessObject.FilterStrips.AddNew("Z0_Number");
						var filter = (ModuleNumberRangeFilter)strip.CurrentModuleFilter;
						filter.Property1 = 2;
						filter.Property2 = 2;
						embeddedFilterControl.AddFilterStrip(strip);
						Application.DoEvents();
						var filterStrips = embeddedFilterControl.FindAll<ZFilterStrip>().ToArray();
						AssertEquals(3, filterStrips.Length);
					}

					embeddedFilterControl.FirePerformSearch();
					AssertEquals(shouldAddNewFilterStrip ? 1 : 2, embeddedFilterControl.GridCollection.Count);
					Application.DoEvents();
					findButtonResults.AddRange(popup.Module.GridCollection.ToArray());

					popup.ExposedOKButtonForTesting.PerformClick();
				};
			});
		}

		public void TestUserDefinedFilter_WhenPopupOpened_WithValidationErrors_ShouldShowErrorAndNotSave()
		{
			FilterStripsTestHelper.CreateUserDefinedFilterStrip_TwoProperties();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = @"Me filter [+]";
				module.Grid.Focus();

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var popup = (UserDefinedFilterEmbeddedModulePopup)dialog;
					popup.Shown += (_, x_) =>
					{
						var embeddedFilterControl = popup.Module.DisplayGrid.GetParentFilterControl();

						var strip = popup.Module.FilterBusinessObject.FilterStrips.AddNew("[USR]Me filter");
						embeddedFilterControl.AddFilterStrip(strip);
						Application.DoEvents();

						popup.ExposedOKButtonForTesting.PerformClick();
					};
				});

				filterFindBox.PopupButton.PerformClick();
				Application.DoEvents();
				AssertEquals("Error - IsUserDefinedFilter: A user-defined filter strip cannot contain a filter strip that references itself. Please remove the [Me filter] filter strip before saving.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = @"Me filter [+]";
				module.Grid.Focus();

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();
				FilterStripsTestHelper.AssertFindBoxText("The user-defined filter should not have been saved with the additional filter strip, and yet...", filterFindBox, "2 filters applied");
			}
		}

		public void TestSaveButton_ShouldPrePopulateFilterNameBox_AndTickCorrectCheckBoxes()
		{
			AssertSaveLayoutWindow("One filter", isUserDefinedFilterPublished: false);
			AssertSaveLayoutWindow("Another filter", isUserDefinedFilterPublished: true);
		}

		static void AssertSaveLayoutWindow(string filterName, bool isUserDefinedFilterPublished)
		{
			EmbeddedModuleTestHelper.PopupUserDefinedFilter(popup =>
			{
				var embeddedFilterControl = popup.Module.DisplayGrid.GetParentFilterControl();

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var form = dialog as SaveLayoutForm;

					if (form != null)
					{
						var saveBizo = (SaveLayoutBizO)form.BusinessEntity;
						AssertEquals(filterName, saveBizo.LayoutName);
						AssertEquals(isUserDefinedFilterPublished, saveBizo.PublishLayout);
						AssertEquals(true, saveBizo.IsUserDefinedFilter);
					}
				});

				embeddedFilterControl.ToolStripSaveLayoutButton_ForTest.PerformClick();
				Application.DoEvents();
			}, filterName, isUserDefinedFilterPublished);
		}

		public void TestUseSaveFormToOverwriteLayout_FromUserDefinedFilterEmbeddedModulePopup_ShouldOverwriteAndNotShowError()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Description = "Walla Walla";
			dummy2.Z0_Description = "Keyokuk";

			Factory.Save();

			FilterStripsTestHelper.CreateUserDefinedFilterStrip("sCOALmo", propertyName: "Z0_Description", propertyValue: "W", isPublished: true, isPublishedGlobal: true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();

				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = @"sCOALmo [+]";
				module.Grid.Focus();
				Application.DoEvents();

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();
				FilterStripsTestHelper.AssertFindBoxText(null, filterFindBox, "1 filter applied");

				module.PerformSearch_ForTest();
				Application.DoEvents();

				var results = module.GridCollection.Cast<DummyBusinessObject>();
				AssertContainsExactElementsInAnyOrder(new[] { "Walla Walla" }, results.Select(x => x.Z0_Description));

				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (dialog is UserDefinedFilterEmbeddedModulePopup userDefinedPopup)
					{
						var popupFilterControl = userDefinedPopup.FindSingle<ZFilterStripBaseControl>();

						var popupFilterStrip = popupFilterControl.FindSingle<ZFilterStrip>();
						AssertEquals("Z0_Description", popupFilterStrip.FilterDescriptionDropEdit.CodeBox.Text);

						((ModuleTextFilter)popupFilterStrip.CurrentDataItem.CurrentModuleFilter).Property = "K";

						popupFilterControl.ToolStripSaveLayoutButton_ForTest.PerformClick();
						Application.DoEvents();
					}
					else if (dialog is SaveLayoutForm saveForm)
					{
						var saveLayout = (SaveLayoutBizO)saveForm.BusinessEntity;
						saveLayout.PublishAcrossAllCompanies = false;

						UnitTestUserNotification.Instance.AddYesAnswer();
						var saveButton = saveForm.FindSingle<ZButton>("SaveFilterButton");
						saveButton.PerformClick();
						Application.DoEvents();
					}
				});

				filterFindBox.PopupButton.PerformClick();
				Application.DoEvents();

				AssertEquals("Attempting to overwrite an existing User-Defined Filter should not show an error about index violations, but rather should ask the user to overwrite the existing layout. SAD!",
					"The user-defined filter [sCOALmo] already exists, do you want to overwrite the existing user-defined filter?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = @"sCOALmo [+]";
				module.Grid.Focus();
				Application.DoEvents();

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();
				FilterStripsTestHelper.AssertFindBoxText(null, filterFindBox, "1 filter applied");

				module.PerformSearch_ForTest();
				Application.DoEvents();

				var results = module.GridCollection.Cast<DummyBusinessObject>();
				AssertContainsExactElementsInAnyOrder("The layout should have been overwritten, so the search results should now be different. SAD!", new[] { "Keyokuk" }, results.Select(x => x.Z0_Description));
			}
		}

		public void TestUseSaveFormToOverwriteLayout_FromUserDefinedFilterEmbeddedModulePopup_ShouldOverwriteAndNotShowError_Twice()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Description = "Walla Walla";
			dummy2.Z0_Description = "Keyokuk";
			dummy3.Z0_Description = "Cucamonga";

			Factory.Save();

			FilterStripsTestHelper.CreateUserDefinedFilterStrip("sCOALmo", propertyName: "Z0_Description", propertyValue: "W", isPublished: true, isPublishedGlobal: true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();

				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = @"sCOALmo [+]";
				module.Grid.Focus();
				Application.DoEvents();

				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();

				FilterStripsTestHelper.AssertFindBoxText(null, filterFindBox, "1 filter applied");

				module.PerformSearch_ForTest();
				Application.DoEvents();

				var results = module.GridCollection.Cast<DummyBusinessObject>();
				AssertContainsExactElementsInAnyOrder(new[] { "Walla Walla" }, results.Select(x => x.Z0_Description));

				Exception testFailureException = null;
				var showUserDefinedFilterEmbeddedModulePopupCount = 0;

				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (dialog is UserDefinedFilterEmbeddedModulePopup userDefinedPopup)
					{
						showUserDefinedFilterEmbeddedModulePopupCount++;

						try
						{
							var popupFilterControl = userDefinedPopup.FindSingle<ZFilterStripBaseControl>();
							var popupFilterStrip = popupFilterControl.FindSingle<ZFilterStrip>();
							AssertEquals("Z0_Description", popupFilterStrip.FilterDescriptionDropEdit.CodeBox.Text);

							var filter = (ModuleTextFilter)popupFilterStrip.CurrentDataItem.CurrentModuleFilter;
							filter.Property = "K";

							popupFilterControl.ToolStripSaveLayoutButton_ForTest.PerformClick();
							Application.DoEvents();

							AssertEquals("Attempting to overwrite an existing User-Defined Filter should not show an error about index violations, but rather should ask the user to overwrite the existing layout. SAD!",
								"The user-defined filter [sCOALmo] already exists, do you want to overwrite the existing user-defined filter?", UnitTestUserNotification.Instance.LastMessage.Text);

							AssertFilterStripChanges("Keyokuk");

							filter.Property = "C";
							popupFilterControl.ToolStripSaveLayoutButton_ForTest.PerformClick();
							Application.DoEvents();

							AssertEquals("Clicking Save a second time should show the save window with all the same settings as the first time, because we should have refreshed the filterPk after the original was overwritten. SAD!",
								"The user-defined filter [sCOALmo] already exists, do you want to overwrite the existing user-defined filter?", UnitTestUserNotification.Instance.LastMessage.Text);

							AssertFilterStripChanges("Cucamonga");

							if (showUserDefinedFilterEmbeddedModulePopupCount == 1)
							{
								userDefinedPopup.Close();
							}
						}
						catch (Exception ex)
						{
							testFailureException = testFailureException ?? ex;
						}
					}

					if (dialog is SaveLayoutForm saveForm)
					{
						try
						{
							var saveLayout = (SaveLayoutBizO)saveForm.BusinessEntity;
							AssertEquals("The original user-defined filter was published, so this checkbox should be ticked by default. SAD!", true, saveLayout.PublishLayout);
							AssertEquals("The original user-defined filter was published for all companies, so this checkbox should be ticked by default. SAD!", true, saveLayout.PublishAcrossAllCompanies);
							AssertEquals("The original layout was a user-defined filter, so this checkbox should be ticked by default. SAD!", true, saveLayout.IsUserDefinedFilter);

							UnitTestUserNotification.Instance.AddYesAnswer();
							var saveButton = saveForm.FindSingle<ZButton>("SaveFilterButton");
							saveButton.PerformClick();
							Application.DoEvents();
						}
						catch (Exception ex)
						{
							testFailureException = ex;
						}
					}
				});

				AssertNoExceptionThrown("Shouldn't throw exception when show popup first time", () =>
				{
					filterFindBox.PopupButton.PerformClick();
					Application.DoEvents();
				});

				AssertNoExceptionThrown("Shouldn't throw exception when show popup second time", () =>
				{
					filterFindBox.PopupButton.PerformClick();
					Application.DoEvents();
				});

				if (testFailureException != null)
				{
					throw new Exception($"An exception was throw in {showUserDefinedFilterEmbeddedModulePopupCount} call.", testFailureException); // all this crazy trying/catching/throwing is to make the errors readable, because otherwise the architecture catches them and error reports, which is difficult to read.
				}
			}
		}

		static void AssertFilterStripChanges(string expectedDummy)
		{
			using (var otherModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var otherForm = (ZForm)otherModule.ShowPopup())
			{
				Application.DoEvents();

				var otherFilterControl = otherForm.FindSingle<ZFilterStripBaseControl>();
				var otherFilterStrip = otherFilterControl.FindSingle<ZFilterStrip>();
				otherFilterStrip.FilterDescriptionDropEdit.CodeBox.Text = @"sCOALmo [+]";
				otherModule.Grid.Focus();
				Application.DoEvents();

				otherModule.PerformSearch_ForTest();
				Application.DoEvents();

				var otherResults = otherModule.GridCollection.Cast<DummyBusinessObject>();
				AssertContainsExactElementsInAnyOrder("The layout should have been overwritten, so the search results should now be different. SAD!", new[] { expectedDummy }, otherResults.Select(x => x.Z0_Description));
			}
		}
	}
}
