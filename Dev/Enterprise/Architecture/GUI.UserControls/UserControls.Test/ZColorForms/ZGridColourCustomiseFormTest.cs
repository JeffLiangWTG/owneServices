using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZGridColourCustomiseForm))]
	public class ZGridColourCustomiseFormTest : ZFormBasherTest
	{
		IDisposable OpenForEdit(GridColourScheme scheme, out ZGridColourCustomiseFormForTest form)
		{
			var disposables = new DisposableList(3);
			try
			{
				var factory = new BusinessObjectFactory { RefreshEnabled = false };
				var reloadedModuleFilter = factory.Load<StmModuleFilter>(scheme.ColourStrips[0].StmModuleFilter.PK);
				var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest(factory);
				strip.LoadLayout(reloadedModuleFilter);

				var schemeReloaded = factory.Load<GridColourScheme>(scheme.PK);
				schemeReloaded.SetStripsFromFilter(strip, null);

				var control = new FilterStripControlForTest(null, strip);
				disposables.Add(control);

				form = new ZGridColourCustomiseFormForTest(schemeReloaded, strip, control);
				disposables.Add(form);

				form.Show();
				Application.DoEvents();
				Application.DoEvents();

				form.Focus();

				return disposables;
			}
			catch
			{
				disposables.Dispose();
				throw;
			}
		}

		public void TestRemoveRule_2()
		{
			var scheme = Factory.New<GridColourScheme>();

			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();

			using (var control = new FilterStripControlForTest(null, strip))
			{
				scheme.S9_ModuleID = "test_module";
				scheme.S9_FilterName = "my colour scheme";

				var colourStripBO1 = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO1.FilterStrips.AddNew();
				colourStripBO1.RuleName = "rule1";
				((IFilterStripBusinessObjectInternals)colourStripBO1).LayoutContext = "LC" + GridColourFactory.ColorStripCode;

				var filter1 = Factory.New<StmModuleFilter>();
				filter1.S9_FilterName = "rule1";
				filter1.S9_RelatedEntityID = scheme.PK;
				filter1.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				filter1.S9_ModuleID = "LC" + GridColourFactory.ColorStripCode;

				colourStripBO1.StmModuleFilter = filter1;
				scheme.ColourStrips.Add(colourStripBO1);

				var colourStripBO5 = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO5.FilterStrips.AddNew();
				colourStripBO5.RuleName = "Never delete me";
				((IFilterStripBusinessObjectInternals)colourStripBO5).LayoutContext = "LC" + GridColourFactory.ColorStripCode;

				var filter5 = Factory.New<StmModuleFilter>();
				filter5.S9_FilterName = "Never delete me";
				filter5.S9_RelatedEntityID = scheme.PK;
				filter5.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				filter5.S9_ModuleID = "LC" + GridColourFactory.ColorStripCode;

				colourStripBO5.StmModuleFilter = filter5;
				scheme.ColourStrips.Add(colourStripBO5);

				Factory.Save();

				using (var disposable = OpenForEdit(scheme, out var testForm))
				{
					//can't test this since we don't hit AddNewRule(), but tested functionally and it's there
					/*Assert("Added Rule Name: 'rule1'",
	((StmModuleFilter)testForm.BusinessEntity).Logs.GetAllLogs().OfType<StmALog>().Where(
		x => x.SL_SE_NKEvent == AutoEvents.EditedARecordCode && x.SL_Reference == "Added Rule Name: 'rule1'").Any());*/

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					AssertEquals(3, testForm.RulesTabControl.TabCount);
					AssertEquals("rule1", testForm.RulesTabControl.TabPages[0].Text);
					var pkToCheckLater = filter1.PK;
					AssertEquals(1,
						((StmModuleFilter)testForm.BusinessEntity).Logs.GetAllLogs().OfType<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.AddedARecordToTheSystemCode));

					testForm.ClickRemoveRuleButton();

					AssertEquals(2, testForm.RulesTabControl.TabCount);
					Assert("Deleted Rule Name: 'rule1'",
						((StmModuleFilter)testForm.BusinessEntity).Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.EditedARecordCode && x.SL_Reference == "Deleted Rule Name: 'rule1'"));

					testForm.ClickSaveSchemeButton();

					AssertEquals(null, new BusinessObjectFactory().Load<StmModuleFilter>(pkToCheckLater));
				}
			}
		}

		public void TestRemoveRule()
		{
			var scheme = Factory.New<GridColourScheme>();

			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();

			using (var control = new FilterStripControlForTest(null, strip))
			{
				scheme.S9_ModuleID = "test_module";
				scheme.S9_FilterName = "my colour scheme";

				var colourStripBO1 = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO1.FilterStrips.AddNew();
				colourStripBO1.RuleName = "rule1";
				((IFilterStripBusinessObjectInternals)colourStripBO1).LayoutContext = "LC" + GridColourFactory.ColorStripCode;

				var filter1 = Factory.New<StmModuleFilter>();
				filter1.S9_FilterName = "rule1";
				filter1.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				filter1.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				filter1.S9_ModuleID = "LC" + GridColourFactory.ColorStripCode;

				colourStripBO1.StmModuleFilter = filter1;
				scheme.ColourStrips.Add(colourStripBO1);

				var colourStripBO2 = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO2.FilterStrips.AddNew();
				colourStripBO2.RuleName = "I am a rulename";
				((IFilterStripBusinessObjectInternals)colourStripBO2).LayoutContext = "LC" + GridColourFactory.ColorStripCode;

				var filter2 = Factory.New<StmModuleFilter>();
				filter2.S9_FilterName = "I am a rulename";
				filter2.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				filter2.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				filter2.S9_ModuleID = "LC" + GridColourFactory.ColorStripCode;

				colourStripBO2.StmModuleFilter = filter2;
				scheme.ColourStrips.Add(colourStripBO2);

				var colourStripBO3 = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO3.FilterStrips.AddNew();
				colourStripBO3.RuleName = "The '&' char should disappear when turned into tab";
				((IFilterStripBusinessObjectInternals)colourStripBO3).LayoutContext = "LC" + GridColourFactory.ColorStripCode;

				var filter3 = Factory.New<StmModuleFilter>();
				filter3.S9_FilterName = "The '&' char should disappear when turned into tab";
				filter3.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				filter3.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				filter3.S9_ModuleID = "LC" + GridColourFactory.ColorStripCode;

				colourStripBO3.StmModuleFilter = filter3;
				scheme.ColourStrips.Add(colourStripBO3);

				var colourStripBO4 = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO4.FilterStrips.AddNew();
				colourStripBO4.RuleName = "I like dogs & cats.";
				((IFilterStripBusinessObjectInternals)colourStripBO4).LayoutContext = "LC" + GridColourFactory.ColorStripCode;

				var filter4 = Factory.New<StmModuleFilter>();
				filter4.S9_FilterName = "I like dogs & cats.";
				filter4.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				filter4.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				filter4.S9_ModuleID = "LC" + GridColourFactory.ColorStripCode;

				colourStripBO4.StmModuleFilter = filter4;
				scheme.ColourStrips.Add(colourStripBO4);

				var colourStripBO5 = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO5.FilterStrips.AddNew();
				colourStripBO5.RuleName = "Never delete me";
				((IFilterStripBusinessObjectInternals)colourStripBO5).LayoutContext = "LC" + GridColourFactory.ColorStripCode;

				var filter5 = Factory.New<StmModuleFilter>();
				filter5.S9_FilterName = "Never delete me";
				filter5.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				filter5.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				filter5.S9_ModuleID = "LC" + GridColourFactory.ColorStripCode;

				colourStripBO5.StmModuleFilter = filter5;
				scheme.ColourStrips.Add(colourStripBO5);

				Factory.Save();

				using (var testForm = new ZGridColourCustomiseFormForTest(scheme, strip, control))
				{
					//can't test this since we don't hit AddNewRule(), but tested functionally and it's there
					/*Assert("Added Rule Name: 'rule1'",
	((StmModuleFilter)testForm.BusinessEntity).Logs.GetAllLogs().OfType<StmALog>().Where(
		x => x.SL_SE_NKEvent == AutoEvents.EditedARecordCode && x.SL_Reference == "Added Rule Name: 'rule1'").Any());*/

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					AssertEquals(6, testForm.RulesTabControl.TabCount);
					AssertEquals("rule1", testForm.RulesTabControl.TabPages[0].Text);
					var pkToCheckLater = filter1.PK;

					testForm.ClickRemoveRuleButton();

					AssertEquals(true, filter1.IsDeleted);
					AssertEquals(5, testForm.RulesTabControl.TabCount);
					Assert("Deleted Rule Name: 'rule1'",
						((StmModuleFilter)testForm.BusinessEntity).Logs.GetAllLogs().OfType<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.EditedARecordCode && x.SL_Reference == "Deleted Rule Name: 'rule1'"));

					testForm.ClickRemoveRuleButton();
					AssertEquals(4, testForm.RulesTabControl.TabCount);
					AssertEquals(true, filter2.IsDeleted);

					testForm.RulesTabControl.SelectTab(1);
					testForm.RulesTabControl.TabPages[1].Text = "I like dogs only.";
					testForm.ClickRemoveRuleButton();

					AssertEquals(3, testForm.RulesTabControl.TabCount);
					AssertEquals("Rule should be deleted even with different name to tab", true, filter4.IsDeleted);

					testForm.RulesTabControl.SelectTab(0);
					testForm.RulesTabControl.TabPages[0].Text = "I am a modified rulename.";
					testForm.ClickRemoveRuleButton();

					AssertEquals(2, testForm.RulesTabControl.TabCount);
					AssertEquals("One left", "Never delete me", testForm.RulesTabControl.TabPages[0].Text);
					AssertEquals("Rule should be deleted even with different name to tab", true, filter3.IsDeleted);

					testForm.ClickSaveSchemeButton();

					AssertEquals(null, new BusinessObjectFactory().Load<StmModuleFilter>(pkToCheckLater));
				}
			}
		}

		public void TestRenameRule()
		{
			var scheme = Factory.New<GridColourScheme>();

			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();

			using (var control = new FilterStripControlForTest(null, strip))
			{
				scheme.S9_ModuleID = "test_module";

				var colourStripBO1 = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO1.FilterStrips.AddNew();
				colourStripBO1.RuleName = "rule1";
				((IFilterStripBusinessObjectInternals)colourStripBO1).LayoutContext = "LC" + GridColourFactory.ColorStripCode;

				var filter1 = Factory.New<StmModuleFilter>();
				filter1.S9_FilterName = "rule1";
				filter1.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				filter1.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				filter1.S9_ModuleID = "LC" + GridColourFactory.ColorStripCode;

				colourStripBO1.StmModuleFilter = filter1;
				scheme.ColourStrips.Add(colourStripBO1);

				var colourStripBO2 = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO2.FilterStrips.AddNew();
				colourStripBO2.RuleName = "Rule 2";
				((IFilterStripBusinessObjectInternals)colourStripBO2).LayoutContext = "LC" + GridColourFactory.ColorStripCode;

				var filter2 = Factory.New<StmModuleFilter>();
				filter2.S9_FilterName = "Rule 2";
				filter2.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				filter2.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				filter2.S9_ModuleID = "LC" + GridColourFactory.ColorStripCode;

				colourStripBO2.StmModuleFilter = filter2;
				scheme.ColourStrips.Add(colourStripBO2);

				var colourStripBO3 = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO3.FilterStrips.AddNew();
				colourStripBO3.RuleName = "Rule 3";
				((IFilterStripBusinessObjectInternals)colourStripBO3).LayoutContext = "LC" + GridColourFactory.ColorStripCode;

				var filter3 = Factory.New<StmModuleFilter>();
				filter3.S9_FilterName = "Rule 3";
				filter3.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				filter3.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				filter3.S9_ModuleID = "LC" + GridColourFactory.ColorStripCode;

				colourStripBO3.StmModuleFilter = filter3;
				scheme.ColourStrips.Add(colourStripBO3);

				var colourStripBO4 = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO4.FilterStrips.AddNew();
				colourStripBO4.RuleName = "Rule 4 that has '&' character.";
				((IFilterStripBusinessObjectInternals)colourStripBO4).LayoutContext = "LC" + GridColourFactory.ColorStripCode;

				var filter4 = Factory.New<StmModuleFilter>();
				filter4.S9_FilterName = "Rule 4 that has '&' character.";
				filter4.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				filter4.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				filter4.S9_ModuleID = "LC" + GridColourFactory.ColorStripCode;

				colourStripBO4.StmModuleFilter = filter4;
				scheme.ColourStrips.Add(colourStripBO4);

				Factory.Save();

				using (var testForm = new ZGridColourCustomiseFormForTest(scheme, strip, control))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
					{
						if (dialog is ZColorSchemeRuleForm ruleNameForm)
						{
							ruleNameForm.RuleNameTextBox.Text = "This is the new rule name.";
							Application.DoEvents();
						}
					});
					testForm.ClickRenameRuleButton();
					AssertEquals("Rulename should have changed", "This is the new rule name.", testForm.RulesTabControl.TabPages[0].Text);

					testForm.RulesTabControl.TabPages[1].Text = "Different rule 2 name.";
					testForm.RulesTabControl.SelectTab(1);
					ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
					{
						if (dialog is ZColorSchemeRuleForm ruleNameForm)
						{
							ruleNameForm.RuleNameTextBox.Text = "New Rule 2 Name";
							Application.DoEvents();
						}
					});
					testForm.ClickRenameRuleButton();
					AssertEquals("Rule should have successfully changed name.", "New Rule 2 Name", testForm.RulesTabControl.TabPages[1].Text);

					testForm.RulesTabControl.SelectTab(3);
					ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
					{
						if (dialog is ZColorSchemeRuleForm ruleNameForm)
						{
							ruleNameForm.RuleNameTextBox.Text = "New Rule 4 Name";
							Application.DoEvents();
						}
					});
					testForm.ClickRenameRuleButton();
					AssertEquals("Rule should have successfully changed name.", "New Rule 4 Name", testForm.RulesTabControl.TabPages[3].Text);

					testForm.RulesTabControl.SelectTab(2);
					ZFormModaliser.SetDelegateToCallOnFormShown(dialog =>
					{
						if (dialog is ZColorSchemeRuleForm ruleNameForm)
						{
							ruleNameForm.RuleNameTextBox.Text = "New Rule 3 Name";
							Application.DoEvents();
						}
					});
					testForm.ClickRenameRuleButton();
					AssertEquals("Rule should have successfully changed name.", "New Rule 3 Name", testForm.RulesTabControl.TabPages[2].Text);
				}
			}
		}

		public void TestSwapRules()
		{
			var scheme = Factory.New<GridColourScheme>();

			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();

			using (var control = new FilterStripControlForTest(null, strip))
			{
				var colourStripBO1 = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO1.FilterStrips.AddNew();
				colourStripBO1.RuleName = "Rule1";
				((IFilterStripBusinessObjectInternals)colourStripBO1).LayoutContext = "LC" + GridColourFactory.ColorStripCode;

				var colourStripBO2 = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO2.FilterStrips.AddNew();
				colourStripBO2.RuleName = "Rule2";
				((IFilterStripBusinessObjectInternals)colourStripBO2).LayoutContext = "LC" + GridColourFactory.ColorStripCode;

				var filter1 = Factory.New<StmModuleFilter>();
				filter1.S9_FilterName = "Scheme1";
				filter1.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				filter1.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				filter1.S9_ModuleID = "LC" + GridColourFactory.ColorStripCode;

				colourStripBO1.StmModuleFilter = filter1;
				colourStripBO2.StmModuleFilter = filter1;

				scheme.ColourStrips.Add(colourStripBO1);
				scheme.ColourStrips.Add(colourStripBO2);
				scheme.S9_ModuleID = "test_module";

				Factory.Save();

				using (var testForm = new ZGridColourCustomiseFormForTest(scheme, strip, control))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					AssertEquals("Rule1", testForm.RulesTabControl.TabPages[0].Text);
					AssertEquals("Rule2", testForm.RulesTabControl.TabPages[1].Text);

					testForm.SwapTabPages(testForm.RulesTabControl, testForm.RulesTabControl.TabPages[0], testForm.RulesTabControl.TabPages[1]);

					AssertEquals("Rule2", testForm.RulesTabControl.TabPages[0].Text);
					AssertEquals("Rule1", testForm.RulesTabControl.TabPages[1].Text);

					AssertEquals(colourStripBO2, scheme.ColourStrips[0]);
					AssertEquals(colourStripBO1, scheme.ColourStrips[1]);
				}
			}
		}

		public void TestRemoveScheme()
		{
			var scheme = Factory.New<GridColourScheme>();

			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();
			scheme.S9_ModuleID = "test_module";

			using (var control = new FilterStripControlForTest(null, strip))
			using (var testForm = new ZGridColourCustomiseFormForTest(scheme, strip, control))
			{
				var colourStripBO = new GridColourStripBusinessObject(control.FilterBusinessObject, scheme, null);
				colourStripBO.FilterStrips.AddNew();
				colourStripBO.RuleName = "rule1";
				((IFilterStripBusinessObjectInternals)colourStripBO).LayoutContext = "LC" + GridColourFactory.ColorStripCode;

				var filter1 = Factory.New<StmModuleFilter>();
				filter1.S9_FilterName = "rule1";
				filter1.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				filter1.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				filter1.S9_ModuleID = "LC" + GridColourFactory.ColorStripCode;

				colourStripBO.StmModuleFilter = filter1;

				scheme.ColourStrips.Add(colourStripBO);

				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testForm.ClickRemoveSchemeButton();
				Factory.Save();
				AssertEquals(true, scheme.IsDeleted);
				AssertEquals(true, filter1.IsDeleted);
			}
		}

		[DatCapabilityRequirementLatestAvailableSqlServer]
		public void TestSaveInvalidCustomSQLFilter()
		{
			var scheme = Factory.New<GridColourScheme>();

			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();
			scheme.S9_ModuleID = "test_module";

			using (var filterControl = new FilterStripControlForTest(null, strip))
			using (var testForm = new ZGridColourCustomiseFormForTest(scheme, strip, filterControl))
			{
				var colourStripBO = new GridColourStripBusinessObject(filterControl.FilterBusinessObject, null, typeof(DummyLogged), true);
				colourStripBO.FilterStrips.AddNew();
				colourStripBO.RuleName = "rule1";

				var result = new ModuleSQLFilter("moo", typeof(DummyBusinessObject));
				result.Property1 = "(GetCustomField(Custom Test) = 'Y'";
				result.IsActive = true;

				colourStripBO.ModuleFilters.AddCustomFilter(result);
				colourStripBO.RegisterEditableChildObject(result);

				scheme.ColourStrips.Add(colourStripBO);

				var dialogResult = testForm.ShowPreSaveDialogs_Exposed();
				AssertEquals(dialogResult, ContinueWithSave.No);

				// This error message comes from ModuleSQLFilter.QueryHasCorrectFormatCheck's exception handler.
				// And inner error message purely comes from sql server engine, which also means same query can lead to different messages.
				// In Sql2019, the error is Incorrect syntax near 'Test'.
				// In Sql2022, the error is Incorrect syntax near ')'.
				// Then we bind DAT capability to latest SQL Server available on DAT.
				var errorMessage = @"The Custom SQL Filter should be in the format of F1 = 'Value' AND F2 = 'Value 2'.
What you have typed is: (GetCustomField(Custom Test) = 'Y'
Error message: Incorrect syntax near ')'.
Filters: (GetCustomField(Custom Test) = 'Y'";
				AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldLoadAllStripsWhenSaveSchemeWithValidateErrors()
		{
			var scheme = Factory.New<GridColourScheme>();
			scheme.S9_ModuleID = "MOD";
			scheme.S9_FilterName = "Name";

			var bo = new ColorGridFactoryTest.FilterStripBusinessObjectForTest();
			using (var fStripControl = new FilterStripControlForTest(null, bo))
			{
				var gridColorStrip1 = new GridColourStripBusinessObject(fStripControl.FilterBusinessObject, scheme, null);
				gridColorStrip1.RuleName = "rule1";

				var gridColorStrip2 = new GridColourStripBusinessObject(fStripControl.FilterBusinessObject, scheme, null);
				gridColorStrip2.RuleName = "rule2";

				using (var testForm = new ZGridColourCustomiseFormForTest(scheme, bo, fStripControl))
				using (var ruleNameForm1 = new ZColorSchemeRuleForm(gridColorStrip1))
				using (var ruleNameForm2 = new ZColorSchemeRuleForm(gridColorStrip1))
				{
					var ruleTab1 = new ZTabPage();
					var colourStripControl1 = new ZGridColourStripControl(gridColorStrip1, fStripControl);
					ruleTab1.Controls.Add(colourStripControl1);
					testForm.RulesTabControl.TabPages.Insert(ruleTab1, testForm.RulesTabControl.TabPages.Count - 1);
					scheme.ColourStrips.Add(gridColorStrip1);

					var ruleTab2 = new ZTabPage();
					var colourStripControl2 = new ZGridColourStripControl(gridColorStrip2, fStripControl);
					ruleTab2.Controls.Add(colourStripControl2);
					testForm.RulesTabControl.TabPages.Insert(ruleTab2, testForm.RulesTabControl.TabPages.Count - 1);
					scheme.ColourStrips.Add(gridColorStrip2);

					var errorText = "Error message";
					var filter = gridColorStrip2.AddTextFilterStrip("desc");
					filter.IsActive = true;
					filter.Property = string.Empty;
					filter.PropertyValidation = info =>
					{
						if (info.Value.IsEmpty)
						{
							info.AddError(errorText);
						}
					};

					testForm.Show();

					Assert("ruleTab1 is not already loaded", !colourStripControl1.AlreadyLoadedForTest);
					Assert("ruleTab2 is not already loaded", !colourStripControl2.AlreadyLoadedForTest);

					testForm.ClickSaveSchemeButton();

					Assert("ruleTab1 is already loaded", colourStripControl1.AlreadyLoadedForTest);
					Assert("ruleTab2 is already loaded", colourStripControl2.AlreadyLoadedForTest);
				}
			}
		}

		public void TestCanHandleZSaveException()
		{
			var scheme1 = Factory.New<GridColourScheme>();
			scheme1.S9_FilterName = "test1";
			scheme1.S9_RelatedEntityID = Env.CurrentUserPK;
			scheme1.S9_GC = Env.CurrentCompanyPK;

			var scheme2 = Factory.New<GridColourScheme>();
			scheme2.S9_FilterName = "test2";
			scheme2.S9_RelatedEntityID = Env.CurrentUserPK;
			scheme2.S9_GC = Env.CurrentCompanyPK;

			Factory.Save();

			// Open test1 to edit
			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();
			using (var filterControl = new FilterStripControlForTest(null, strip))
			{
				using (var testForm1 = new ZGridColourCustomiseFormForTest(scheme1, strip, filterControl))
				{
					// Rename test1 to 'testX'
					testForm1.Show();

					var colourStripBO1 = new GridColourStripBusinessObject(strip, scheme1, null);
					colourStripBO1.FilterStrips.AddNew();
					colourStripBO1.RuleName = "rule1";
					scheme1.ColourStrips.Add(colourStripBO1);

					testForm1.SetFilterName("testX");

					// At the same time, test2 is also renamed to 'testX' and saved first on another instance
					var factoryOfAnotherInstance = new BusinessObjectFactory();
					factoryOfAnotherInstance.RefreshEnabled = false;
					var scheme2Reloaded = factoryOfAnotherInstance.Load<GridColourScheme>(scheme2.PK);
					scheme2Reloaded.S9_FilterName = "testX";
					factoryOfAnotherInstance.Save();

					AssertNoExceptionThrown(() => testForm1.FireSaveButton());
					var expectedHandledMessage = "A layout or scheme with this name 'testX' already exists in database. Please use a different name.";
					AssertEquals(expectedHandledMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestShowErrorMessageWhenConcurrencyIssueOccur()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);

			var scheme = Factory.NewWithValidTestData<GridColourScheme>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var scheme2 = newFactory.Load<GridColourScheme>(scheme.PK);

			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();
			var colorStrip = new GridColourStripBusinessObject(strip, scheme, null);
			colorStrip.FilterStrips.AddNew();
			colorStrip.RuleName = "rule1";
			scheme.ColourStrips.Add(colorStrip);
			Factory.Save();

			var strip2 = new GridColourSchemeTest.FilterStripBusinessObjectForTest(newFactory);
			using (var filterControl = new FilterStripControlForTest(null, strip2))
			using (var testForm = new ZGridColourCustomiseFormForTest(scheme2, strip2, filterControl))
			{
				testForm.Show();

				var colorStrip2 = new GridColourStripBusinessObject(strip2, scheme2, null);
				colorStrip2.FilterStrips.AddNew();
				colorStrip2.RuleName = "rule2";
				scheme2.ColourStrips.Add(colorStrip2);
				scheme2.S9_FilterName = "scheme2";
				AssertNoExceptionThrown(() => testForm.FireSaveButton());

				AssertEndsWith("Last message should end with:", "Your changes cannot be saved because they may conflict with the other user's changes.\r\nPlease close and open this form to try again.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Error message", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		public void TestSecurity_PublishedForAllCompanies()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();
			using (var filterControl = new FilterStripControlForTest(null, strip))
			{
				var scheme = Factory.New<GridColourScheme>();
				scheme.S9_ModuleID = "test_module";

				var colourStripBO = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO.FilterStrips.AddNew();
				colourStripBO.RuleName = "rule1";
				scheme.ColourStrips.Add(colourStripBO);

				scheme.S9_IsPublished = true;
				scheme.PublishAcrossAllCompanies = true;
				scheme.S9_RelatedEntityID = staff1.PK;

				Factory.Save();

				// Login as other user
				using (Env.SetTemporaryUserContext(new UserContext(staff2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					// Allow to publish and edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, true, true);

					// Allow to publish and edit, but deny edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, true, false);

					// Allow to publish but deny edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, false, false);

					// Allow to publish but deny publish and edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, false, false, false);

					// Deny publish and edit
					AssertSecurity(scheme, strip, filterControl, false, false, false, false);
				}

				// Login as owner
				using (Env.SetTemporaryUserContext(new UserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					// Allow to publish and edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, true, true);

					// Allow to publish and edit, but deny edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, true, false);

					// Allow to publish but deny edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, false, false);

					// Allow to publish but deny publish and edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, false, false, false);

					// Deny publish and edit
					AssertSecurity(scheme, strip, filterControl, false, false, false, false);
				}
			}
		}

		public void TestSecurity_Published()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();
			using (var filterControl = new FilterStripControlForTest(null, strip))
			{
				var scheme = Factory.New<GridColourScheme>();
				scheme.S9_ModuleID = "test_module";

				var colourStripBO = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO.FilterStrips.AddNew();
				colourStripBO.RuleName = "rule1";
				scheme.ColourStrips.Add(colourStripBO);

				scheme.S9_IsPublished = true;
				scheme.PublishAcrossAllCompanies = false;
				scheme.S9_RelatedEntityID = staff1.PK;

				Factory.Save();

				// Login as other user
				using (Env.SetTemporaryUserContext(new UserContext(staff2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					// Allow to publish and edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, true, true);

					// Allow to publish and edit, but deny edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, true, false);

					// Allow to publish but deny edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, false, false);

					// Allow to publish but deny publish and edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, false, false, false);

					// Deny publish and edit
					AssertSecurity(scheme, strip, filterControl, false, false, false, false);
				}

				// Login as owner
				using (Env.SetTemporaryUserContext(new UserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					// Allow to publish and edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, true, true);

					// Allow to publish and edit, but deny edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, true, false);

					// Allow to publish but deny edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, false, false);

					// Allow to publish but deny publish and edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, false, false, false);

					// Deny publish and edit
					AssertSecurity(scheme, strip, filterControl, false, false, false, false);
				}
			}
		}

		public void TestSecurity_NonPublished()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();
			using (var filterControl = new FilterStripControlForTest(null, strip))
			{
				var scheme = Factory.New<GridColourScheme>();
				scheme.S9_ModuleID = "test_module";

				var colourStripBO = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO.FilterStrips.AddNew();
				colourStripBO.RuleName = "rule1";
				scheme.ColourStrips.Add(colourStripBO);

				scheme.S9_IsPublished = false;
				scheme.PublishAcrossAllCompanies = false;
				scheme.S9_RelatedEntityID = staff1.PK;

				Factory.Save();

				// Login as other user
				using (Env.SetTemporaryUserContext(new UserContext(staff2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					// Allow to publish and edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, true, true);

					// Allow to publish and edit, but deny edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, true, false);

					// Allow to publish but deny edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, false, false);

					// Allow to publish but deny publish and edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, false, false, false);

					// Deny publish and edit
					AssertSecurity(scheme, strip, filterControl, false, false, false, false);
				}

				// Login as owner
				using (Env.SetTemporaryUserContext(new UserContext(staff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					// Allow to publish and edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, true, true);

					// Allow to publish and edit, but deny edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, true, false);

					// Allow to publish but deny edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, true, false, false);

					// Allow to publish but deny publish and edit for all companies
					AssertSecurity(scheme, strip, filterControl, true, false, false, false);

					// Deny publish and edit
					AssertSecurity(scheme, strip, filterControl, false, false, false, false);
				}
			}
		}

		public void TestSecurity_IsSystem()
		{
			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();
			using (var filterControl = new FilterStripControlForTest(null, strip))
			{
				var scheme = Factory.New<GridColourScheme>();
				scheme.S9_ModuleID = "test_module";

				var colourStripBO = new GridColourStripBusinessObject(strip, scheme, null);
				colourStripBO.FilterStrips.AddNew();
				colourStripBO.RuleName = "rule1";
				scheme.ColourStrips.Add(colourStripBO);

				scheme.S9_IsPublished = true;
				scheme.PublishAcrossAllCompanies = true;
				scheme.S9_IsSystem = true;

				Factory.Save();

				// Allow to publish and edit for all companies
				AssertSecurity(scheme, strip, filterControl, true, true, true, true);

				// Allow to publish and edit, but deny edit for all companies
				AssertSecurity(scheme, strip, filterControl, true, true, true, false);

				// Allow to publish but deny edit for all companies
				AssertSecurity(scheme, strip, filterControl, true, true, false, false);

				// Allow to publish but deny publish and edit for all companies
				AssertSecurity(scheme, strip, filterControl, true, false, false, false);

				// Deny publish and edit
				AssertSecurity(scheme, strip, filterControl, false, false, false, false);
			}
		}

		void AssertSecurity(
			GridColourScheme scheme,
			GridColourSchemeTest.FilterStripBusinessObjectForTest strip,
			FilterStripControlForTest filterControl,
			bool allowPublish,
			bool allowPublishAllBranches,
			bool allowEditOthers,
			bool allowEditOthersAllBranches
		)
		{
			var expectedMessage = string.Empty;
			var expectedCanPublish = true;
			var expectedCanPublishForAll = true;
			var expectedCanEdit = true;

			if (scheme.S9_IsSystem)
			{
				expectedMessage = "System defined schemes cannot be modified.";
				expectedCanPublish = false;
				expectedCanPublishForAll = false;
				expectedCanEdit = false;
			}
			else
			{
				if (!allowPublishAllBranches)
				{
					expectedMessage = "You don't have permission to publish schemes across all companies.";
					expectedCanPublishForAll = false;
				}

				if (!allowPublish)
				{
					expectedMessage = "You don't have permission to publish schemes.";
					expectedCanPublish = false;
				}

				if (scheme.S9_RelatedEntityID != EnvProxy.Instance.CurrentUser.PK)
				{
					if (scheme.PublishAcrossAllCompanies && !allowEditOthersAllBranches)
					{
						expectedMessage = "You don't have permission to edit other users' schemes published across all companies.";
						expectedCanEdit = false;
						expectedCanPublishForAll = false;
						expectedCanPublish = false;
					}
					else if (!allowEditOthers)
					{
						expectedMessage = "You don't have permission to edit other users' published schemes.";
						expectedCanEdit = false;
						expectedCanPublishForAll = false;
						expectedCanPublish = false;
					}
				}
			}

			Env.Security.PublishGlobalGridColorSchemes.IsAllowed = allowPublish;
			Env.Security.PublishGlobalGridColorSchemes.IsAllowedForAllBranches = allowPublishAllBranches;
			Env.Security.EditAllGlobalColourSchemes.IsAllowed = allowEditOthers;
			Env.Security.EditAllGlobalColourSchemes.IsAllowedForAllBranches = allowEditOthersAllBranches;

			using (var testForm = new ZGridColourCustomiseFormForTest(scheme, strip, filterControl))
			{
				AssertEquals("Status bar message", expectedMessage, testForm.StatusBarTextForTesting);
				AssertEquals("Can publish", expectedCanPublish, testForm.CanPublished());
				AssertEquals("Can publish for all companies", expectedCanPublishForAll, testForm.CanPublishedAllCompanies());
				AssertEquals("Can edit", expectedCanEdit, testForm.CanEditRules());
			}
		}

		protected override Form GetFormToBashCore()
		{
			var scheme = Factory.New<GridColourScheme>();
			var strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();
			using (var stripControl = new FilterStripControlForTest(null, strip))
			{
				scheme.S9_ModuleID = "test_module";
				return new ZGridColourCustomiseForm(scheme, strip, stripControl, typeof(BusinessObject));
			}
		}

		#region Implementation

		internal class ZGridColourCustomiseFormForTest : ZGridColourCustomiseForm, IZGridColourCustomiseFormForTest
		{
			public ZGridColourCustomiseFormForTest(GridColourScheme scheme, FilterStripBusinessObject filterStripBusinessObject, ZFilterStripCommonControl stripControl, Type businessEntityType)
				: base(scheme, filterStripBusinessObject, stripControl, businessEntityType)
			{
			}

			public ZGridColourCustomiseFormForTest(GridColourScheme scheme, FilterStripBusinessObject filterStripBusinessObject, ZFilterStripCommonControl stripControl)
				: base(scheme, filterStripBusinessObject, stripControl, typeof(BusinessObject))
			{
			}

			public new ZTabControl RulesTabControl
			{
				get { return base.RulesTabControl; }
			}

			ZTabControl IZGridColourCustomiseFormForTest.RulesTabControl => RulesTabControl;

			public void ClickRemoveRuleButton()
			{
				RemoveSelectedRule();
			}

			public void ClickRenameRuleButton()
			{
				RenameRule();
			}

			void IZGridColourCustomiseFormForTest.ClickRemoveRuleButton() => ClickRemoveRuleButton();

			public void ClickRemoveSchemeButton()
			{
				RemoveScheme();
			}

			public ContinueWithSave ShowPreSaveDialogs_Exposed()
			{
				return ShowPreSaveDialogs();
			}

			internal void SwapTabPages(ZTabControl tabControl, TabPage tabPage1, TabPage tabPage2)
			{
				base.SwapTabPages(tabControl, tabPage1, tabPage2);
			}

			public void UncheckPublished()
			{
				currentColourScheme.S9_IsPublished = false;
			}

			public void UncheckPublishedForAllCompanies()
			{
				currentColourScheme.PublishAcrossAllCompanies = false;
			}

			public new ContinueWithSave FireSaveButton(object sender = null)
			{
				SaveButton.PerformClick();

				return ContinueWithSave.Yes;
			}

			public void ClickSaveSchemeButton()
			{
				SaveScheme();
			}

			public bool CanPublished() => base.PublishCheckBox.Enabled;

			public bool CanPublishedAllCompanies() => base.checkBoxIsPublishedAcrossAllCompanies.Enabled;

			public void SetFilterName(string newName)
			{
				currentColourScheme.S9_FilterName = newName;
			}

			public bool CanEditRules()
			{
				Assert("No rule in the form", RulesTabControl.AllTabPages.Any(a => a.Name != "LogsTabPage"));

				var result = true;

				foreach (ZTabPage tabPage in RulesTabControl.TabPages)
				{
					if (tabPage.Name == "LogsTabPage")
					{
						continue;
					}

					foreach (Control tabControl in tabPage.Controls)
					{
						foreach (Control control in tabControl.Controls)
						{
							result = result && control.Enabled;
						}
						result = result && tabControl.Enabled;
					}
				}
				return result;
			}
		}

		#endregion
	}
}
