using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.GUI.Testing
{
	[TestedType(typeof(BarcodeRuleSetForm))]
	class BarcodeRuleSetFormTest : ZFormBasherTest
	{
		#region TestCaptions

		[RequiresSTA]
		public void TestCaptions()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;

			using (var form = new TestBarcodeRuleSetForm(ruleSet))
			{
				form.Show();

				AssertEquals("Dummy Buyer", form.BuyerGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Dummy Supplier", form.SupplierGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Dummy Entity", form.RelatedEntityGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);

				ruleSet.BRS_Module = "";
				AssertEquals("Buyer", form.BuyerGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Supplier", form.SupplierGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Related Entity", form.RelatedEntityGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		#endregion

		#region TestActionsMenuItem

		public void TestActionsMenuItem()
		{
			// this test is to check the existance of menu items only. To test behaviour of each menuitem create a separate test below.
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;

			using (var form = new TestBarcodeRuleSetForm(ruleSet))
			{
				AssertNotNull(form.ActionsMenuItem.MenuItems.FindByText("Run Barcode Parsing Diagnostics Tool"));
				AssertNotNull(form.ActionsMenuItem.MenuItems.FindByText("Run Barcode Validation Diagnostics Tool"));
			}
		}

		#region TestRunDiagnosticsTool

		[RequiresSTA]
		public void TestRunDiagnosticsTool_Parsing()
		{
			TestRunDiagnosticsToolCore((TestBarcodeRuleSetForm form) => form.ActionsMenuItem.MenuItems.FindByText("Run Barcode Parsing Diagnostics Tool"), DiagnosticsTypes.Codes.Parsing);
		}

		[RequiresSTA]
		public void TestRunDiagnosticsTool_Validation()
		{
			TestRunDiagnosticsToolCore((TestBarcodeRuleSetForm form) => form.ActionsMenuItem.MenuItems.FindByText("Run Barcode Validation Diagnostics Tool"), DiagnosticsTypes.Codes.Validation);
		}

		void TestRunDiagnosticsToolCore(Func<TestBarcodeRuleSetForm, MenuItem> findMenu, string diagnosticsType)
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;
			ruleSet.BRS_OH_Buyer = Helper.CreateOrg("AA").PK;
			ruleSet.BRS_OH_Supplier = Helper.CreateOrg("BB").PK;
			ruleSet.BRS_RelatedEntityId = ZGuid.NewZGuid();
			ruleSet.BRS_RelatedEntityTableCode = "OP";

			using (var form = new TestBarcodeRuleSetForm(ruleSet))
			{
				var runDiagnosticsMenuItem = findMenu(form);
				AssertNotNull(runDiagnosticsMenuItem);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				runDiagnosticsMenuItem.PerformClick();
				AssertEquals("Please save your changes before accessing barcode parsing diagnostics form.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				Factory.Save();
				AssertNull(form.LastFormShown);

				runDiagnosticsMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(form.LastFormShown);
				AssertType<BarcodeParsingDiagnosticsForm>(form.LastFormShown);

				var diagnosticsBizO = (BarcodeParsingDiagnostics)((BarcodeParsingDiagnosticsForm)form.LastFormShown).BusinessEntity;
				AssertEquals(diagnosticsType, diagnosticsBizO.DiagnosticsType);
				AssertEquals(ruleSet.BRS_Module, diagnosticsBizO.ModuleCode);
				AssertEquals(ruleSet.BRS_OH_Buyer, diagnosticsBizO.BuyerPK);
				AssertEquals(ruleSet.BRS_OH_Supplier, diagnosticsBizO.SupplierPK);
				AssertEquals(ruleSet.BRS_RelatedEntityId, diagnosticsBizO.RelatedEntityPK);

				form.LastFormShown.Close(); // to avoid test cleanup failure
			}
		}

		#endregion

		#endregion

		#region TestFormCaption

		[RequiresSTA]
		public void TestFormCaption()
		{
			using (var form = (BarcodeRuleSetForm)GetFormToBash())
			{
				AssertEquals("Barcode Rule Set", form.FormCaption);
			}
		}

		#endregion

		#region TestMaxLengthIsSetForTerminatorColumn

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Prevent change to base class as we would lose context of the actual schema class")]
		[RequiresSTA]
		public void TestMaxLengthIsSetForTerminatorColumn()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.Rules.AddNew();

			using (var form = new TestBarcodeRuleSetForm(ruleSet))
			{
				form.Show();
				var column = form.ParsingRulesGrid.Columns[BarcodeRule.Schema.BRU_Terminator];
				var columnStyle = (ZTextBoxColumnStyleWithMaxLength)column.ColumnStyle;
				var columnIndex = form.ParsingRulesGrid.Columns.ToList().FindIndex(c => c == column);
				form.ParsingRulesGrid.Focus();
				form.ParsingRulesGrid.CurrentCell = new DataGridCell(1, columnIndex);

				KeySender.SendKeyPress(columnStyle.TextBox, Keys.N); // Send key to trigger 'Edit' on column Style, needs to be currently focused cell
				AssertEquals(8, columnStyle.TextBox.MaxLength);
			}
		}

		#endregion

		#region TestRelatedEntityFindBoxIsVisibleOnlyIfModuleSupportsIt

		[RequiresSTA]
		public void TestRelatedEntityFindBoxIsVisibleOnlyIfModuleSupportsIt()
		{
			var ruleSet = Helper.CreateRuleSet();
			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.IsRelatedEntityAvailable = true;

			using (var form = new TestBarcodeRuleSetForm(ruleSet))
			{
				form.Show();

				var originalHeightOfGroupBox = form.HeaderGroupBox.Height;
				AssertEquals("Related Entity should be visible for if module supports related entity.", true, form.RelatedEntityGuidFindBox.Visible);

				ruleSet.BRS_Module = "";
				AssertEquals("Related Entity should *not* be visible for Modules that don't use Related Entity.", false, form.RelatedEntityGuidFindBox.Visible);
				Assert("If Related Entity is not visible, HeaderGroupBox should reduce in size.", form.HeaderGroupBox.Height < originalHeightOfGroupBox);

				ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;
				AssertEquals("Changing module back to Warehouse should make Related Entity Visible.", true, form.RelatedEntityGuidFindBox.Visible);
				AssertEquals("Changing module back to Warehouse should make HeaderGroupBox increase in size.", originalHeightOfGroupBox, form.HeaderGroupBox.Height);
			}
		}

		#endregion

		#region TestSelectRule

		[RequiresSTA]
		public void TestSelectParsingRule()
		{
			var ruleSet = Helper.CreateRuleSet();
			var rule = Helper.CreateRule(ruleSet);
			Factory.Save();

			using (var form = new TestBarcodeRuleSetForm(ruleSet))
			{
				form.Show();

				AssertEquals("Precondition", 0, form.ParsingRulesGrid.SelectedElements.Length);

				form.SelectRule((BarcodeRule)null);
				AssertEquals("Invoking SelectRule() with null should not select anything.", 0, form.ParsingRulesGrid.SelectedElements.Length);
				AssertExceptionThrown(typeof(ArgumentException),
					"You must not attempt to select a Rule from another Rule Set. Parameter name: rule", () => form.SelectRule(Factory.New<BarcodeRule>()));

				form.SelectRule(rule);
				AssertContainsExactElementsInAnyOrder("Invoking SelectRule() with a rule should select it on the Rules Grid.", new[] { rule }, form.ParsingRulesGrid.SelectedElements);
			}
		}

		[RequiresSTA]
		public void TestSelectValidationRule()
		{
			var ruleSet = Helper.CreateRuleSet();
			var rule = Helper.CreateValidationRule(ruleSet, DummyTargetFields.Codes.TargetField1);
			ruleSet.BRS_Module = BarcodeModuleTypes.Codes.Warehouse;
			Factory.Save();

			using (var form = new TestBarcodeRuleSetForm(ruleSet))
			{
				form.Show();
				form.RulesTabControl.SelectTab(form.ValidationRulesTabPage);
				AssertEquals("Precondition", 0, form.ValidationRulesGrid.SelectedElements.Length);

				form.SelectRule((BarcodeValidationRule)null);
				AssertEquals("Invoking SelectRule() with null should not select anything.", 0, form.ValidationRulesGrid.SelectedElements.Length);
				AssertExceptionThrown(typeof(ArgumentException),
					"You must not attempt to select a Rule from another Rule Set. Parameter name: rule", () => form.SelectRule(Factory.New<BarcodeValidationRule>()));

				form.SelectRule(rule);
				AssertContainsExactElementsInAnyOrder("Invoking SelectRule() with a rule should select it on the Rules Grid.", new[] { rule }, form.ValidationRulesGrid.SelectedElements);
			}
		}

		#endregion

		#region TestSupportsEDocs

		[RequiresSTA]
		public void TestSupportsEDocs()
		{
			using (var form = (BarcodeRuleSetForm)GetFormToBash())
			{
				AssertNull(form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			}
		}

		#endregion

		#region TestGetSampleBarcodeNoAOORExceptionInSecondComponent

		[RequiresSTA]
		public void TestGetSampleBarcodeNoAOORExceptionInSecondComponent()
		{
			var ruleSet = Helper.CreateRuleSet();
			var rule = Helper.CreateRule(ruleSet);
			var component1 = rule.Components.AddNew();
			component1.BRC_ApplicationID = "TestApp";
			component1.BRC_MinLength = 1;
			component1.BRC_MaxLength = 9;
			component1.BRC_Format = GS1DataFormatTypes.Codes.AlphaNumericWithSymbols;
			component1.BRC_TargetField = DummyTargetFields.Codes.TargetField1;
			Factory.Save();

			using (var form = new TestBarcodeRuleSetForm(ruleSet))
			{
				form.Show();
				form.SelectRule(rule);

				var component2 = rule.Components.AddNew();
				component2.BRC_Format = GS1DataFormatTypes.Codes.AlphaNumericWithSymbols;
				Application.DoEvents();

				Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
			}
		}

		#endregion

		#region TestValidationTabVisibility

		[RequiresSTA]
		public void TestValidationTabVisibility()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;

			using (var form = new TestBarcodeRuleSetForm(ruleSet))
			{
				form.Show();
				AssertEquals(false, form.ValidationRulesTabPage.TabVisible);

				ruleSet.BRS_Module = BarcodeModuleTypes.Codes.Warehouse;
				AssertEquals(true, form.ValidationRulesTabPage.TabVisible);

				ruleSet.BRS_Module = BarcodeModuleTypes.Codes.ETail;
				AssertEquals(false, form.ValidationRulesTabPage.TabVisible);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = BarcodeModuleTypes.Codes.Warehouse; // Dummy doesn't use multilingual descriptions
			Factory.Save();

			// load in new factory to remove 'Dummy' Module
			return new BarcodeRuleSetForm(new BusinessObjectFactory().Load<BarcodeRuleSet>(ruleSet.PK));
		}

		class TestBarcodeRuleSetForm : BarcodeRuleSetForm
		{
			public TestBarcodeRuleSetForm(BarcodeRuleSet ruleSet)
				: base(ruleSet)
			{
			}

			public new ZTemplateTabControl MainTabControl => base.MainTabControl;

			public new ZGroupBox HeaderGroupBox => base.HeaderGroupBox;

			public new ZGrid ParsingRulesGrid => base.ParsingRulesGrid;

			public new ZGuidFindBox BuyerGuidFindBox => base.BuyerGuidFindBox;

			public new ZGuidFindBox SupplierGuidFindBox => base.SupplierGuidFindBox;

			public new ZGuidFindBox RelatedEntityGuidFindBox => base.RelatedEntityGuidFindBox;

			public new MenuItem ActionsMenuItem => base.ActionsMenuItem;

			public new ZTemplateTabControl RulesTabControl => base.RulesTabControl;

			public new ZTabPage ValidationRulesTabPage => base.ValidationRulesTabPage;

			public new ZGrid ValidationRulesGrid => base.ValidationRulesGrid;
		}

		protected override void SetUp()
		{
			base.SetUp();
			dummyBarcodeEnableDisposable = BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			dummyBarcodeEnableDisposable?.Dispose();
		}

		IDisposable dummyBarcodeEnableDisposable;

		BarcodeParsingTestHelper Helper
		{
			get { return helper ?? (helper = new BarcodeParsingTestHelper(Factory)); }
		}

		BarcodeParsingTestHelper helper;

		#endregion
	}
}
