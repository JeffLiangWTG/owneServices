using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	[TestedType(typeof(SaveLayoutForm))]
	sealed class SaveLayoutFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var filterBizObj = new DummyFilterStripBusinessObject();

			return new SaveLayoutForm(new SaveLayoutBizO(filterBizObj), true, true);
		}

		public void TestSaveLayoutButtonGrowsToAccomodateTheTextLabelWidth()
		{
			var filterBizObj = new DummyFilterStripBusinessObject();
			var layoutBizObj = new SaveLayoutBizO(filterBizObj);
			using (var form = new SaveLayoutFormTestClass(layoutBizObj, true, true))
			{
				form.Show();
				{
					using (var cg = form.CreateGraphics())
					{
						var saveButton = form.SaveButtonExposed;
						saveButton.Text = "Save Layout";
						var originalTextMeasuredSize = cg.MeasureString(saveButton.Text, saveButton.Font);
						var originalButtonWidth = saveButton.Width;

						saveButton.Text = "A rather long text string";
						var newTextMeasuredSize = cg.MeasureString(saveButton.Text, saveButton.Font);
						var newButtonWidth = saveButton.Width;

						var growthExpected = (int)(newTextMeasuredSize.Width - originalTextMeasuredSize.Width);
						var growth = newButtonWidth - originalButtonWidth;

						AssertCloseEnough(growth, growthExpected, 20);
					}
				}
			}
		}

		public void TestSaveColumnCheckBoxVisibility()
		{
			var filterBizObj = new DummyFilterStripBusinessObject();

			var layoutBizObj = new SaveLayoutBizO(filterBizObj);

			using (var form = new SaveLayoutFormTestClass(layoutBizObj, true, true))
			{
				form.Show();
				AssertEquals("SaveColumn should be visible", true, form.SaveColumnsCheckBoxExposed.Visible);
			}

			using (var form = new SaveLayoutFormTestClass(layoutBizObj, false, false))
			{
				form.Show();
				AssertEquals("SaveColumn should be invisible", false, form.SaveColumnsCheckBoxExposed.Visible);
			}

			using (var form = new SaveLayoutFormTestClass(layoutBizObj, false, true))
			{
				form.Show();
				AssertEquals("IsUserDefinedFilterCheckBox should be visible", true, form.IsUserDefinedFilterCheckBoxExposed.Visible);
			}

			using (var form = new SaveLayoutFormTestClass(layoutBizObj, false, false))
			{
				form.Show();
				AssertEquals("IsUserDefinedFilterCheckBox should not be invisible", false, form.IsUserDefinedFilterCheckBoxExposed.Visible);
			}
		}

		public void TestIsOkToSave()
		{
			var filterBizObj = new DummyFilterStripBusinessObject();

			var layoutBizObj = new SaveLayoutBizO(filterBizObj);

			using (var form = new SaveLayoutFormTestClass(layoutBizObj, true, true))
			{
				form.Show();

				layoutBizObj.LayoutName = "";//error condition
				form.SaveButtonExposed.PerformClick();

				AssertEquals(false, form.IsOkToSave);

				layoutBizObj.LayoutName = "iourwe7890";
				form.SaveButtonExposed.PerformClick();
				AssertEquals(true, form.IsOkToSave);
			}

			new DataGridLayoutManager().SavePreconfiguredLayout(filterBizObj, "iourwe7890", false, false, SaveColumnLayout.No);

			layoutBizObj = new SaveLayoutBizO(filterBizObj);
			using (var form = new SaveLayoutFormTestClass(layoutBizObj, true, true))
			{
				form.Show();

				layoutBizObj.LayoutName = "iourwe7890";//error condition as it is duplicate
				form.SaveButtonExposed.PerformClick();
				AssertEquals(false, form.IsOkToSave);

				form.CancelButtonExposed.PerformClick();
				AssertEquals(false, form.IsOkToSave);
			}
		}

		public void TestLanguageEditingSecurity()
		{
			var filterBizObj = new DummyFilterStripBusinessObject();
			var layoutBizObj = new SaveLayoutBizO(filterBizObj);
			EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed = false;
			try
			{
				using (var form = new SaveLayoutFormTestClass(layoutBizObj, true, true))
				{
					form.Show();
					AssertEquals(false, form.FilterNameTextBox.IsLanguageEditingEnabled);
				}
			}
			finally
			{
				EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed = true;
			}
		}

		public void TestLanguageEditingEnabledOnPublishedFilters()
		{
			var filterBizObj = new DummyFilterStripBusinessObject();
			var layoutBizObj = new SaveLayoutBizO(filterBizObj);
			using (var form = new SaveLayoutFormTestClass(layoutBizObj, true, true))
			{
				form.Show();
				AssertEquals(false, form.FilterNameTextBox.IsLanguageEditingEnabled);
				form.PublishLayoutCheckBox.Checked = true;
				AssertEquals(true, form.FilterNameTextBox.IsLanguageEditingEnabled);
			}

			filterBizObj = new DummyFilterStripBusinessObject();
			layoutBizObj = new SaveLayoutBizO(filterBizObj);
			layoutBizObj.PublishLayout = true;
			using (var form = new SaveLayoutFormTestClass(layoutBizObj, true, true))
			{
				form.Show();
				AssertEquals(true, form.FilterNameTextBox.IsLanguageEditingEnabled);
				form.PublishLayoutCheckBox.Checked = false;
				AssertEquals(false, form.FilterNameTextBox.IsLanguageEditingEnabled);
			}
		}

		#region User-defined Filters

		public void TestSaveColumnsCheckBox_WhenSaveAsUserDefinedFilterStripSelected_ShouldBeReadOnly()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var layoutBizObj = new SaveLayoutBizO(module.FilterBusinessObject);
				using (var form = new SaveLayoutForm(layoutBizObj, true, true))
				{
					form.Show();
					Application.DoEvents();

					var columnCheckBox = form.FindSingle<ZCheckBox>(x => x.Name == "SaveColumnsCheckBox");
					AssertEquals(false, columnCheckBox.ReadOnly);

					columnCheckBox.Checked = true;
					AssertEquals(true, columnCheckBox.Checked);

					var udfsCheckBox = form.FindSingle<ZCheckBox>(x => x.Name == "IsUserDefinedFilterCheckBox");
					AssertEquals(false, udfsCheckBox.Checked);
					AssertEquals(false, udfsCheckBox.ReadOnly);

					udfsCheckBox.Checked = true;
					AssertEquals("Ticking the UDFS checkbox should disable the column checkbox, and yet...", true, columnCheckBox.ReadOnly);
					AssertEquals("Ticking the UDFS checkbox should untick the column checkbox, and yet...", false, columnCheckBox.Checked);
				}
			}
		}

		public void TestSaveForUserDefinedFilterStrip_WithExistingUserDefinedFilterStrip_WithSameNameAndModule_ShouldOverwriteWithPrompt()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "I named it meself", false, false, isUserDefinedFilter: true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var layoutBizObj = new SaveLayoutBizO(module.FilterBusinessObject);

				using (var form = new SaveLayoutFormTestClass(layoutBizObj, true, true))
				{
					form.Show();

					layoutBizObj.LayoutName = "I named it meself";
					layoutBizObj.IsUserDefinedFilter = true;

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					form.SaveButtonExposed.PerformClick();
					AssertEquals("The user-defined filter [I named it meself] already exists, do you want to overwrite the existing user-defined filter?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(false, form.IsOkToSave);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.SaveButtonExposed.PerformClick();
					AssertEquals("The user-defined filter [I named it meself] already exists, do you want to overwrite the existing user-defined filter?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, form.IsOkToSave);
				}
			}
		}

		public void TestSaveUserDefinedFilter_WhenUserDefinedFilterWithSameNameAlreadyExistsForSameModule_WithDifferentPublicitySettings_ShouldWarnAndPromptAndActuallyReplaceOldFilter()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Z0_Description", "sCOALmo");

				FilterStripsTestHelper.SaveFilterLayout(filterBizo, "COALition", true, true, true);
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;

				using (var popup = (EmbeddedModulePopup)module.ShowPopup())
				{
					Application.DoEvents();

					var stripControl = popup.FindSingle<StripControl>();
					var filterStrip = filterBizo.FilterStrips.AddNew("Z0_Description");
					var filter = (ModuleTextFilter)filterStrip.CurrentModuleFilter;
					filter.Property = "Climate Changed";
					filter.IsActive = true;
					stripControl.AddFilterStrip(filterStrip);
					Application.DoEvents();

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
					{
						var saveLayoutForm = (SaveLayoutForm)dialog;
						var saveLayoutBizo = (SaveLayoutBizO)saveLayoutForm.BusinessEntity;

						saveLayoutBizo.LayoutName = "COALition";
						saveLayoutBizo.IsUserDefinedFilter = true;
						saveLayoutBizo.PublishLayout = true;
						saveLayoutBizo.PublishAcrossAllCompanies = false; // This is the key difference between this and the previously saved layout.

						AssertHasWarning(saveLayoutBizo.LayoutNameInfo, "The user-defined filter [COALition] already exists, do you want to overwrite the existing user-defined filter?");

						UnitTestUserNotification.Instance.AddYesAnswer();
						var saveLayoutButton = saveLayoutForm.FindSingle<ZButton>("SaveFilterButton");
						saveLayoutButton.PerformClick();
						Application.DoEvents();
					});

					var saveButton = stripControl.ToolStripSaveLayoutButton_ForTest;

					AssertNoExceptionThrown("The filter should save without throwing an exception about violating a filter. SAD!", () =>
					{
						saveButton.PerformClick();
						Application.DoEvents();
					});
				}
			}

			var query = new ZQuery(StmModuleFilterSchema.S9_FilterName, "COALition");
			query.AddToFilter(StmModuleFilterSchema.S9_ModuleID, DummyModuleIDs.Dummy.Name);
			var savedLayouts = Factory.Load<StmModuleFilter>(query);

			CombineAssertions(() =>
			{
				AssertEquals("The user-defined filter [COALition] already exists, do you want to overwrite the existing user-defined filter?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The original filter should have been replaced, not duplicated. SAD!", 1, savedLayouts.Length);

				if (savedLayouts.Length == 1)
				{
					AssertEquals("The replacement should have succeeded, even with the unique index " + StmModuleFilterSchema.Constants.Indexes.NR_UX__S9_ModuleID_S9_FilterName, EnvProxy.Instance.CurrentCompany.PK, savedLayouts.Single().S9_GC);
				}
			});
		}

		public void TestSaveForUserDefinedFilterStrip_WithExistingUserDefinedFilterStrip_WithSameNameButDifferentModule_ShouldSaveWithoutPrompt()
		{
			AssertSaveWithSameName(existingFilterIsUserDefinedFilterStrip: true, newFilterIsUserDefinedFilterStrip: true, isSavingInDifferentModule: true, shouldSave: true, expectedError: null);
		}

		public void TestSaveForUserDefinedFilterStrip_WithExistingRegularSavedLayout_WithSameNameAndModule_ShouldNotSave()
		{
			AssertSaveWithSameName(existingFilterIsUserDefinedFilterStrip: false, newFilterIsUserDefinedFilterStrip: true, isSavingInDifferentModule: false, shouldSave: false,
				expectedError: "There are errors that need correcting before saving your filter layout.");
		}

		public void TestSaveForUserDefinedFilterStrip_WithExistingRegularSavedLayout_WithSameNameButDifferentModule_ShouldSaveWithoutPrompt()
		{
			AssertSaveWithSameName(existingFilterIsUserDefinedFilterStrip: false, newFilterIsUserDefinedFilterStrip: true, isSavingInDifferentModule: true, shouldSave: true, expectedError: null);
		}

		public void TestSaveForRegularLayout_WithExistingUserDefinedFilterStrip_WithSameNameAndModule_ShouldNotSave()
		{
			AssertSaveWithSameName(existingFilterIsUserDefinedFilterStrip: true, newFilterIsUserDefinedFilterStrip: false, isSavingInDifferentModule: false, shouldSave: false,
				expectedError: "There are errors that need correcting before saving your filter layout.");
		}

		public void TestSaveForRegularLayout_WithExistingUserDefinedFilterStrip_WithSameNameButDifferentModule_ShouldSaveWithoutPrompt()
		{
			AssertSaveWithSameName(existingFilterIsUserDefinedFilterStrip: true, newFilterIsUserDefinedFilterStrip: false, isSavingInDifferentModule: true, shouldSave: true, expectedError: null);
		}

		static void AssertSaveWithSameName(bool existingFilterIsUserDefinedFilterStrip, bool newFilterIsUserDefinedFilterStrip, bool isSavingInDifferentModule, bool shouldSave, string expectedError)
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "I named it meself", false, false, existingFilterIsUserDefinedFilterStrip);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(isSavingInDifferentModule ? DummyModuleIDs.DummyDependent : DummyModuleIDs.Dummy))
			{
				var layoutBizObj = new SaveLayoutBizO(module.FilterBusinessObject);

				using (var form = new SaveLayoutFormTestClass(layoutBizObj, true, false))
				{
					form.Show();

					layoutBizObj.LayoutName = "I named it meself";
					layoutBizObj.IsUserDefinedFilter = newFilterIsUserDefinedFilterStrip;

					form.SaveButtonExposed.PerformClick();
					AssertEquals(shouldSave, form.IsOkToSave);
					AssertEquals(expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestSaveUserDefinedFilter_ThenAddThatFilterToModuleLayout_ThenOpenSaveForm_WhenValidationRuns_ShouldNotThrowExceptions()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (module.ShowPopup())
			{
				Application.DoEvents();
				module.FilterBusinessObject.AddTextFilterStrip(DummyBizoSchema.Constants.Z0_Description, "Test");
				ModuleUserDefinedFilter userDefinedFilter = null;

				ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
				{
					if (userDefinedFilter == null)
					{
						var form = (SaveLayoutForm)dialog;
						var saveBizo = (SaveLayoutBizO)form.BusinessEntity;

						saveBizo.LayoutName = "UDF 1";
						saveBizo.IsUserDefinedFilter = true;

						form.FindSingle<ZButton>("SaveFilterButton").PerformClick();
						Application.DoEvents();
					}
				});

				var saveButton = ((StripControl)module.EmbeddedControl).ToolStripSaveLayoutButton_ForTest;
				saveButton.PerformClick();
				Application.DoEvents();

				userDefinedFilter = module.FilterBusinessObject.AddFilterStrip<ModuleUserDefinedFilter>("[USR]UDF 1");
				AssertNotNull(userDefinedFilter);

				AssertNoExceptionThrown("Showing the save form for a layout with a new user-defined filter should not throw any exceptions when validating. SAD!", () =>
				{
					saveButton.PerformClick();
					Application.DoEvents();
				});
			}
		}

		public void TestSaveUserDefinedFilter_WhenUsingExistingFilterName_ShouldRemoveAllReferencesToOldFilter()
		{
			const string commonFilterName = "coalalition";

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Z0_Description", "sCOALmo");

				FilterStripsTestHelper.SaveFilterLayout(filterBizo, commonFilterName, true, true, true);
			}

			Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var filterStrips = filterBizo.FilterStrips;

				foreach (var filter in filterBizo.ModuleFilters)
				{
					AssertEquals(false, filter.IsDeleted);
				}

				using (var popup = (EmbeddedModulePopup)module.ShowPopup())
				{
					var setup = true;
					popup.Show();
					Application.DoEvents();

					EmbeddedModuleTestHelper.ForceSelectFilterFromDropDown("Z0_Description", popup);

					popup.Show();
					Application.DoEvents();

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
					{
						var saveLayoutForm = (SaveLayoutForm)dialog;
						var saveLayoutBizo = (SaveLayoutBizO)saveLayoutForm.BusinessEntity;

						saveLayoutForm.Show();
						Application.DoEvents();

						if (setup)
						{
							saveLayoutBizo.LayoutName = commonFilterName;
							saveLayoutBizo.IsUserDefinedFilter = true;
							saveLayoutBizo.PublishLayout = true;
							saveLayoutBizo.PublishAcrossAllCompanies = true;

							saveLayoutForm.Show();
							Application.DoEvents();

							AssertHasWarning(saveLayoutBizo.LayoutNameInfo, $"The user-defined filter [{commonFilterName}] already exists, do you want to overwrite the existing user-defined filter?");

							UnitTestUserNotification.Instance.AddYesAnswer();
							var saveLayoutButton = saveLayoutForm.FindSingle<ZButton>("SaveFilterButton");
							saveLayoutButton.PerformClick();
							Application.DoEvents();

							saveLayoutForm.Show();
							Application.DoEvents();
						}
						else
						{
							saveLayoutForm.Close();
						}
					});

					AssertNoExceptionThrown("The filter should save without throwing an exception about violating a filter. SAD!", () =>
					{
						((StripControl)module.EmbeddedControl).ToolStripSaveLayoutButton_ForTest.PerformClick();
						popup.Show();
						Application.DoEvents();
					});

					var deleteSecondStripButton = popup.FindAll<ZButton>().First(button => button.Name.Equals("DeleteStripButton"));
					deleteSecondStripButton.PerformClick();
					setup = false;

					EmbeddedModuleTestHelper.ForceSelectFilterFromDropDown(ModuleUserDefinedFilter.GetSuffixedDescription(commonFilterName), popup);

					UnitTestUserNotification.Instance.ClearMessages();

					AssertNoExceptionThrown("We shouldn't get an 'accessing deleted object' exception OR error reported for doing this action", () =>
					{
						var filterControl = popup.FindSingle<ZFilterStripBaseControl>();
						var filterStrip = filterControl.FindSingle<ZFilterStrip>();
						var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();
						filterFindBox.PopupButton.PerformClick();
					});

					AssertNull("We opened the popup form for the filterstrip without incident", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			var query = new ZQuery(StmModuleFilterSchema.S9_FilterName, commonFilterName);
			query.AddToFilter(StmModuleFilterSchema.S9_ModuleID, DummyModuleIDs.Dummy.Name);
			var savedLayouts = Factory.Load<StmModuleFilter>(query);

			AssertEquals("The original filter should have been replaced, not duplicated. SAD!", 1, savedLayouts.Length);
		}

		#endregion

		public void TestLayoutNameMaxLength()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var layoutBizObj = new SaveLayoutBizO(module.FilterBusinessObject);

				using (var form = new SaveLayoutFormTestClass(layoutBizObj, true, true))
				{
					form.Show();
					Application.DoEvents();

					var formTextBox = (IGridControl)form.FilterNameTextBox;
					AssertEquals(StmModuleFilterSchema.S9_FilterName.MaxLength, formTextBox.MaxLength);
				}
			}
		}
	}
}
