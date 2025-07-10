using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BarcodeParsing.Business;
using Enterprise.Integration.BarcodeParsing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BarcodeParsing.GUI
{
	public partial class BarcodeParsingDiagnosticsForm : ZChildForm, IBarcodeParsingDiagnosticsForm
	{
		public BarcodeParsingDiagnosticsForm()
			: this(new BarcodeParsingDiagnostics(new BusinessObjectFactory()))
		{
		}

		internal BarcodeParsingDiagnosticsForm(BarcodeParsingDiagnostics diag)
			: base(diag)
		{
			InitializeComponent();
			HookEvents();
		}

		#region Load

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupForm();
		}

		#endregion

		#region ClearButton_Click

		void ClearButton_Click(object sender, EventArgs e)
		{
			BarcodeTextBoxWithGS1Support.Text = "";
			BarcodeTextBoxWithGS1Support.Focus();
		}

		#endregion

		#region FormVerb

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region RunRulesButton_Click

		void RunRulesButton_Click(object sender, EventArgs e) => Diagnostics?.Run();

		#endregion

		#region MatchedRulesLinkLabel_LinkClicked

		void MatchedRulesLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (Diagnostics.LastMatchedRule != null)
			{
				if (Diagnostics.IsDiagnosticsTypeParsing)
				{
					CreateController(ControllerIDs.BarcodeParsing).ShowEditForm((BarcodeRule)Diagnostics.LastMatchedRule);
				}
				else if (Diagnostics.IsDiagnosticsTypeValidation)
				{
					CreateController(ControllerIDs.BarcodeValidation).ShowEditForm((BarcodeValidationRule)Diagnostics.LastMatchedRule);
				}
			}
		}

		ZController CreateController(ControllerID id)
		{
			var controller = ZControllerFactory.Create(id);
#if DEBUG
			BarcodeParsingController_ForTesting = controller;
#endif
			return controller;
		}

		#endregion

		#region Setup Form Controls / Captions

		void SetupForm()
		{
			SetCaptions();
			SetRelatedEntityVisibility();
			SetBuyerAndSupplierFindBoxesVisibility();
			SetIsGS1Visibility();
			SetTargetFieldVisibility();
			SetRunRulesButtonText();
		}

		void SetCaptions()
		{
			SetCaption(BuyerGuidFindBox, () => Diagnostics.BuyerCaption);
			SetCaption(SupplierGuidFindBox, () => Diagnostics.SupplierCaption);
			SetCaption(RelatedEntityGuidFindBox, () => Diagnostics.RelatedEntityCaption);
		}

		static void SetCaption(Control controlWithCaption, Func<string> newCaption)
		{
			controlWithCaption.GetExtension<ILabelCaptionRenderer>().Caption = newCaption();
		}

		void SetBuyerAndSupplierFindBoxesVisibility()
		{
			bool isBuyerVisiable = BuyerGuidFindBox.Visible;
			bool isSupplierVisiable = SupplierGuidFindBox.Visible;
			bool isBuyerAvailable = Diagnostics.IsBuyerAvailable;
			bool isSupplierAvailable = Diagnostics.IsSupplierAvailable;

			if (isBuyerVisiable != isBuyerAvailable)
			{
				BuyerGuidFindBox.Visible = isBuyerAvailable;
			}

			if (isSupplierVisiable != isSupplierAvailable)
			{
				SupplierGuidFindBox.Visible = isSupplierAvailable;
			}
		}

		void SetRelatedEntityVisibility()
		{
			bool isEntityFindBoxVisible = RelatedEntityGuidFindBox.Visible;
			bool isRelatedEntityAvailable = Diagnostics.IsRelatedEntityAvailable;

			if (isEntityFindBoxVisible != isRelatedEntityAvailable)
			{
				RelatedEntityGuidFindBox.Visible = isRelatedEntityAvailable;
			}
		}

		void SetIsGS1Visibility() => IsGS1CheckBox.Visible = Diagnostics.DiagnosticsType != DiagnosticsTypes.Codes.Validation;

		void SetTargetFieldVisibility() => TargetFieldDropEdit.Visible = Diagnostics.DiagnosticsType == DiagnosticsTypes.Codes.Validation;

		void SetRunRulesButtonText()
		{
			if (Diagnostics.IsDiagnosticsTypeParsing)
			{
				RunRulesButton.Text = Res.GetData("84775e4c-63ba-44e8-b69c-fe561244145c", "Run Barcode Parsing Rules").Caption;
			}
			else if (Diagnostics.IsDiagnosticsTypeValidation)
			{
				RunRulesButton.Text = Res.GetData("76af8d3e-d6a5-45b5-b25a-c2cbbfb09328", "Run Barcode Validation Rules").Caption;
			}
		}

		#endregion

		#region Actions

		#region DiagnosticsType Changed

		void DiagnosticsTypeInfo_ValueChanged(object sender, EventArgs e) => SetupForm();

		#endregion

		#region Module Changed

		void ModuleInfo_ValueChanged(object sender, EventArgs e) => SetupForm();

		#endregion

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Hook / Unhook Events

		void HookEvents()
		{
			var diagnostics = Diagnostics;
			if (diagnostics != null)
			{
				diagnostics.DiagnosticsTypeInfo.ValueChanged += DiagnosticsTypeInfo_ValueChanged;
				diagnostics.ModuleCodeInfo.ValueChanged += ModuleInfo_ValueChanged;
			}
		}

		void UnhookEvents()
		{
			var diagnostics = Diagnostics;
			if (diagnostics != null)
			{
				diagnostics.ModuleCodeInfo.ValueChanged -= ModuleInfo_ValueChanged;
				diagnostics.DiagnosticsTypeInfo.ValueChanged -= DiagnosticsTypeInfo_ValueChanged;
			}
		}

		#endregion

		#region Diagnostics

		BarcodeParsingDiagnostics Diagnostics
		{
			get { return ((BarcodeParsingDiagnostics)BusinessEntity); }
		}

		#endregion

		#region BarcodeTextBox KeyPress & TextChanged

		void BarcodeTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == BarcodeRule.GS1Terminator[0])
			{
				e.KeyChar = BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator[0];
			}
		}

		void BarcodeTextBox_TextChanged(object sender, EventArgs eventArgs)
		{
			if (BarcodeTextBoxWithGS1Support.Text.Contains(BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator)
				&& !IsGS1CheckBox.Checked)
			{
				IsGS1CheckBox.Checked = true;
			}
		}

		#endregion

#if DEBUG
		protected ZTextBox BarcodeTextBox => BarcodeTextBoxWithGS1Support;

		public ZController BarcodeParsingController_ForTesting;
#endif
	}

	#region ZTextBoxWithGS1Support

	/// <summary>
	/// Thit textbox will replace GS1Terminator with a ReplacementCharacterForGS1Terminator
	/// when string is pasted from Clipboard
	/// </summary>
	class ZTextBoxWithGS1Support : ZTextBox
	{
#if !WINZOR
		const int WM_PASTE = 0x0302; //Paste Event code

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_PASTE)
			{
				var clipboardText = SafeClipboard.GetText();
				var textToPaste = clipboardText.Replace(BarcodeRule.GS1Terminator, BarcodeParsingDiagnostics.ReplacementCharacterForGS1Terminator);
				var currentStart = SelectionStart;
				Text = Text.Insert(currentStart, textToPaste);
				SelectionStart = currentStart + textToPaste.Length;
			}
			else
			{
				base.WndProc(ref m);
			}
		}
#endif
	}

	#endregion
}
