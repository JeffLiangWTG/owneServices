using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.BarcodeParsingEngine.Warehouse;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.GUI.Testing
{
	[TestedType(typeof(BarcodeParsingDiagnosticsForm))]
	class BarcodeParsingDiagnosticsFormTest : ZFormBasherTest
	{
		#region TestDiagnosticsType

		public void TestDiagnosticsType_WhenSelectParsing()
		{
			using (var form = new TestBarcodeParsingDiagnosticsForm())
			{
				var diagnostic = (BarcodeParsingDiagnostics)form.BusinessEntity;
				diagnostic.DiagnosticsType = DiagnosticsTypes.Codes.Parsing;
				diagnostic.ModuleCode = BarcodeModuleTypes.Codes.Warehouse;
				form.Show();

				AssertEquals(true, form.BuyerGuidFindBox.Visible);
				AssertEquals(true, form.SupplierGuidFindBox.Visible);
				AssertEquals(true, form.RelatedEntityGuidFindBox.Visible);
				AssertEquals(true, form.IsGS1CheckBox.Visible);
				AssertEquals(false, form.TargetFieldDropEdit.Visible);
				AssertEquals("Run Barcode Parsing Rules", form.RunRulesButton.Text);
			}
		}

		public void TestDiagnosticsType_WhenSelectValidation()
		{
			using (var form = new TestBarcodeParsingDiagnosticsForm())
			{
				var diagnostic = (BarcodeParsingDiagnostics)form.BusinessEntity;
				diagnostic.DiagnosticsType = DiagnosticsTypes.Codes.Validation;
				diagnostic.ModuleCode = BarcodeModuleTypes.Codes.Warehouse;
				form.Show();

				AssertEquals(true, form.BuyerGuidFindBox.Visible);
				AssertEquals(true, form.SupplierGuidFindBox.Visible);
				AssertEquals(true, form.RelatedEntityGuidFindBox.Visible);
				AssertEquals(false, form.IsGS1CheckBox.Visible);
				AssertEquals(true, form.TargetFieldDropEdit.Visible);
				AssertEquals("Run Barcode Validation Rules", form.RunRulesButton.Text);
			}
		}

		#endregion

		#region TestCaptions

		[RequiresSTA]
		public void TestCaptions()
		{
			using (var form = new TestBarcodeParsingDiagnosticsForm())
			{
				var diagnostic = (BarcodeParsingDiagnostics)form.BusinessEntity;
				diagnostic.ModuleCode = DummyBarcodeParsingConsumer.Module;
				var factory = diagnostic.Factory;

				using (BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(factory))
				{
					form.Show();
					AssertEquals("Dummy Buyer", form.BuyerGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Dummy Supplier", form.SupplierGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Dummy Entity", form.RelatedEntityGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);

					form.ModuleCodeDropEdit.ResetText();
					form.BuyerGuidFindBox.Visible = true;
					form.BuyerGuidFindBox.Focus();
					AssertEquals("Buyer", form.BuyerGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Supplier", form.SupplierGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("Related Entity", form.RelatedEntityGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				}
			}
		}

		#endregion

		#region TestSetBuyerAndSupplierFindBoxesVisibility

		[RequiresSTA]
		public void TestSetBuyerAndSupplierFindBoxesVisibility()
		{
			using (var form = new TestBarcodeParsingDiagnosticsForm())
			{
				var diagnostic = (BarcodeParsingDiagnostics)form.BusinessEntity;
				diagnostic.ModuleCode = BarcodeModuleTypes.Codes.Warehouse;

				form.Show();

				AssertEquals(true, form.BuyerGuidFindBox.Visible);
				AssertEquals(true, form.SupplierGuidFindBox.Visible);
			}
		}

		#endregion

		#region TestFormVerb

		public void TestFormVerb()
		{
			using (var form = new BarcodeParsingDiagnosticsForm())
			{
				AssertEquals("", form.FormVerb);
			}
		}

		#endregion

		#region TestKeyPressEvent

		public void TestKeyPressEvent()
		{
			using (var form = new TestBarcodeParsingDiagnosticsForm())
			{
				form.Show();
				var barcodeTextBox = form.BarcodeTextBox;
				KeySender.SendKeyPress(barcodeTextBox, barcodeTextBox.Handle, '\u001d'); // GS1 Terminator
				AssertEquals(BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator, barcodeTextBox.Text);
				AssertEquals(1, barcodeTextBox.SelectionStart);

				KeySender.SendKeyPress(barcodeTextBox, barcodeTextBox.Handle, '1');
				AssertEquals(BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator + "1", barcodeTextBox.Text);
				AssertEquals(2, barcodeTextBox.SelectionStart);

				KeySender.SendKeyPress(barcodeTextBox, barcodeTextBox.Handle, '2');
				AssertEquals(BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator + "12", barcodeTextBox.Text);
				AssertEquals(3, barcodeTextBox.SelectionStart);

				KeySender.SendKeyPress(barcodeTextBox, barcodeTextBox.Handle, '\b'); // backspace
				AssertEquals(BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator + "1", barcodeTextBox.Text);
				AssertEquals(2, barcodeTextBox.SelectionStart);
			}
		}

		#endregion

		#region TestClearButton

		public void TestClearButton()
		{
			using (var form = new TestBarcodeParsingDiagnosticsForm())
			{
				form.Show();
				var barcodeTextBox = form.BarcodeTextBox;
				var clearButton = form.ClearButton;
				barcodeTextBox.Text = "ABCD";
				clearButton.Focus();
				Assert("Precondition: Clear Button is focused.", clearButton.Focused);

				clearButton.PerformClick();
				AssertEquals("Clear Button should clear out Barcode Text.", "", barcodeTextBox.Text);
				Assert("Clear Button should focus Barcode TextBox", barcodeTextBox.Focused);
			}
		}

		#endregion

		#region TestLinkLabel

		public void TestLinkLabel_Parsing()
		{
			using (var form = new TestBarcodeParsingDiagnosticsForm())
			{
				form.Show();

				// there is always a system warehouse rule for GS1, starting with 30
				form.ModuleCodeDropEdit.Text = BarcodeModuleTypes.Codes.Warehouse;
				form.BarcodeTextBox.Text = "30123";
				form.IsGS1CheckBox.Checked = true;

				AssertNull(form.BarcodeParsingController_ForTesting);
				form.RunRulesButton.PerformClick();

				form.MatchedRulesLinkLabel.OnLinkClicked_Exposed(new LinkLabelLinkClickedEventArgs(form.MatchedRulesLinkLabel.Links[0]));
				AssertNotNull(form.BarcodeParsingController_ForTesting.LastShownForm);
				AssertEquals(ControllerIDs.BarcodeParsing, form.BarcodeParsingController_ForTesting.LastShownForm.ControllerID);
				((ZForm)form.BarcodeParsingController_ForTesting.LastShownForm).Close();
			}
		}

		[RequiresSTA]
		public void TestLinkLabel_Validation()
		{
			var rule = Helper.CreateValidationRule(nameof(WarehouseTargetField.PRC));
			rule.BVR_Format = GS1DataFormatTypes.Codes.AlphaNumericWithSymbols;
			Factory.Save();

			using (var form = new TestBarcodeParsingDiagnosticsForm())
			{
				var diagnostic = (BarcodeParsingDiagnostics)form.BusinessEntity;
				diagnostic.DiagnosticsType = DiagnosticsTypes.Codes.Validation;
				diagnostic.ModuleCode = BarcodeModuleTypes.Codes.Warehouse;
				diagnostic.TargetField = nameof(WarehouseTargetField.PRC);
				diagnostic.Barcode = "12345";

				form.Show();
				AssertNull(form.BarcodeParsingController_ForTesting);
				form.RunRulesButton.PerformClick();

				form.MatchedRulesLinkLabel.OnLinkClicked_Exposed(new LinkLabelLinkClickedEventArgs(form.MatchedRulesLinkLabel.Links[0]));
				AssertNotNull(form.BarcodeParsingController_ForTesting.LastShownForm);
				AssertEquals(ControllerIDs.BarcodeValidation, form.BarcodeParsingController_ForTesting.LastShownForm.ControllerID);
				((ZForm)form.BarcodeParsingController_ForTesting.LastShownForm).Close();
			}
		}

		#endregion

		#region TestPasteGS1Char

#if !WINZOR

		[DeveloperOnlyTest]
		public void TestPasteGS1Char()
		{
			using (var form = new TestBarcodeParsingDiagnosticsForm())
			{
				form.Show();

				const string textWithGS1 = "AA" + BarcodeRule.GS1Terminator + "BB";
				SafeClipboard.SetText(textWithGS1);

				AssertEquals("Precondition:", "", form.BarcodeTextBox.Text);
				AssertEquals("Precondition:", textWithGS1, SafeClipboard.GetText(TextDataFormat.UnicodeText));
				AssertEquals("Precondition:", false, form.IsGS1CheckBox.Checked);

				form.BarcodeTextBox.Paste();

				AssertEquals("AA" + BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator + "BB", form.BarcodeTextBox.Text);
				AssertEquals(true, form.IsGS1CheckBox.Checked);

				SafeClipboard.Clear();
			}
		}

#endif

		#endregion

		#region Implmentation

		class TestBarcodeParsingDiagnosticsForm : BarcodeParsingDiagnosticsForm
		{
			public new ZDropEdit DiagnosticsTypeDropEdit => base.DiagnosticsTypeDropEdit;

			public new ZGuidFindBox BuyerGuidFindBox => base.BuyerGuidFindBox;

			public new ZDropEdit ModuleCodeDropEdit => base.ModuleCodeDropEdit;

			public new ZGuidFindBox RelatedEntityGuidFindBox => base.RelatedEntityGuidFindBox;

			public new ZGuidFindBox SupplierGuidFindBox => base.SupplierGuidFindBox;

			public new ZDropEdit TargetFieldDropEdit => base.TargetFieldDropEdit;

			public new ZTextBox BarcodeTextBox => base.BarcodeTextBox;

			public new ZButton ClearButton => base.ClearButton;

			public new ZButton RunRulesButton => base.RunRulesButton;

			public new ZCheckBox IsGS1CheckBox => base.IsGS1CheckBox;

			public new ZLinkLabel MatchedRulesLinkLabel => base.MatchedRulesLinkLabel;
		}

		protected override Form GetFormToBashCore()
		{
			return new BarcodeParsingDiagnosticsForm();
		}

		#region Helper

		BarcodeParsingTestHelper Helper
		{
			get { return helper ?? (helper = new BarcodeParsingTestHelper(Factory)); }
		}

		BarcodeParsingTestHelper helper;

		#endregion

		#endregion
	}
}
