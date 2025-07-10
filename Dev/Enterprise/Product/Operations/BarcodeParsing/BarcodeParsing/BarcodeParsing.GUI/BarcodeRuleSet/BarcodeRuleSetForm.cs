using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BarcodeParsing.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BarcodeParsing.GUI
{
	public partial class BarcodeRuleSetForm : ZTemplateForm
	{
		public BarcodeRuleSetForm(BarcodeRuleSet ruleSet)
			: base(ruleSet)
		{
			InitializeComponent();
			SetMaxLengthOnTerminatorColumn();

			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("D52993D6-DDD1-4604-99B1-CEF200AA0694", "Run Barcode Parsing Diagnostics Tool"), OnRunParsingDiagnosticsTool);
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("007EC14E-4DDC-40E6-B286-CF28125F56FE", "Run Barcode Validation Diagnostics Tool"), OnRunValidationDiagnosticsTool);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Prevent change to base class as we would lose context of the actual schema class")]
		void SetMaxLengthOnTerminatorColumn()
		{
			var columnStyleInfo = ParsingRulesGrid.ColumnStyles.OfType<ZTextBoxColumnStyleWithMaxLengthInfo>().FirstOrDefault(s => s.ColumnName == BarcodeRuleSchema.Constants.BRU_Terminator);
			if (columnStyleInfo != null)
			{
				columnStyleInfo.MaxLength = BarcodeRule.Schema.BRU_TerminatorMaxLength - 2;
			}
		}

		BarcodeRuleSet RuleSet
		{
			get { return (BarcodeRuleSet)DataSource; }
		}

		#region OnRunDiagnosticsTool

		void OnRunParsingDiagnosticsTool(object sender, EventArgs e) => RunDiagnosticsTool(DiagnosticsTypes.Codes.Parsing);

		void OnRunValidationDiagnosticsTool(object sender, EventArgs e) => RunDiagnosticsTool(DiagnosticsTypes.Codes.Validation);

		void RunDiagnosticsTool(ZString diagnosticsType )
		{
			if (RuleSet.HasChanges)
			{
				Globals.Message.ShowWarning(Res.GetString("6A348F43-DACD-4720-AF2E-85D51ADEA54E", "Please save your changes before accessing barcode parsing diagnostics form."),
					Res.GetString("4C94252D-873F-417B-88B0-BEF140B9E3D9", "Save required."));
			}
			else
			{
				var diag = new BarcodeParsingDiagnostics(new BusinessObjectFactory());
				diag.DiagnosticsType = diagnosticsType;
				diag.ModuleCode = RuleSet.BRS_Module;
				diag.BuyerPK = RuleSet.BRS_OH_Buyer;
				diag.SupplierPK = RuleSet.BRS_OH_Supplier;
				diag.RelatedEntityPK = RuleSet.BRS_RelatedEntityId;

				var diagForm = new BarcodeParsingDiagnosticsForm(diag);
				diagForm.Show();
#if DEBUG
				LastFormShown = diagForm;
#endif
			}
		}

#if DEBUG
		public ZForm LastFormShown;
#endif
		#endregion

		#region OnVisibleChanged

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
			{
				OnVisible();
			}
		}

		void OnVisible()
		{
			SetupForm();
			SetupValidationRulesTabVisibility();
		}

		#region SetupForm

		void SetupForm()
		{
			SetCaptions();
			SetupRelatedEntityFindBox();
			SetupBuyerFindBox();
			SetupSupplierFindBox();
		}

		void SetCaptions()
		{
			SetCaption(BuyerGuidFindBox, () => RuleSet.BuyerCaption);
			SetCaption(SupplierGuidFindBox, () => RuleSet.SupplierCaption);
			SetCaption(RelatedEntityGuidFindBox, () => RuleSet.RelatedEntityCaption);
		}

		static void SetCaption(Control controlWithCaption, Func<string> newCaption)
		{
			controlWithCaption.GetExtension<ILabelCaptionRenderer>().Caption = newCaption();
		}

		void SetupRelatedEntityFindBox()
		{
			const int GuidFindBoxHeight = 26;
			bool isEntityFindBoxVisible = RelatedEntityGuidFindBox.Visible;
			bool isRelatedEntityAvailable = RuleSet.IsRelatedEntityAvailable;

			if (isEntityFindBoxVisible != isRelatedEntityAvailable)
			{
				RelatedEntityGuidFindBox.Visible = isRelatedEntityAvailable;

				int adjustedHeight = isRelatedEntityAvailable ?
					HeaderGroupBox.Size.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(GuidFindBoxHeight)
					: HeaderGroupBox.Size.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(GuidFindBoxHeight);
				ControlDpiScalingHelper.SetHeight(ref HeaderGroupBox, adjustedHeight, false);
			}
		}

		void SetupBuyerFindBox()
		{
			const int GuidFindBoxHeight = 26;
			bool isBuyerFindBoxVisible = BuyerGuidFindBox.Visible;
			bool isBuyerAvailable = RuleSet.IsBuyerAvailable;

			if (isBuyerFindBoxVisible != isBuyerAvailable)
			{
				BuyerGuidFindBox.Visible = isBuyerAvailable;

				var supplierLocation = SupplierGuidFindBox.Location;
				var relatedEntityLocation = RelatedEntityGuidFindBox.Location;
				int adjustedSupplierY = isBuyerAvailable ?
					SupplierGuidFindBox.Location.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(GuidFindBoxHeight)
					: SupplierGuidFindBox.Location.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(GuidFindBoxHeight);
				int adjustedRelatedEntityY = isBuyerAvailable ?
					RelatedEntityGuidFindBox.Location.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(GuidFindBoxHeight)
					: RelatedEntityGuidFindBox.Location.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(GuidFindBoxHeight);
				ControlDpiScalingHelper.SetY(ref supplierLocation, adjustedSupplierY, false);
				ControlDpiScalingHelper.SetY(ref relatedEntityLocation, adjustedRelatedEntityY, false);
				SupplierGuidFindBox.Location = supplierLocation;
				RelatedEntityGuidFindBox.Location = relatedEntityLocation;

				int adjustedHeight = isBuyerAvailable ?
					HeaderGroupBox.Size.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(GuidFindBoxHeight)
					: HeaderGroupBox.Size.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(GuidFindBoxHeight);
				ControlDpiScalingHelper.SetHeight(ref HeaderGroupBox, adjustedHeight, false);
			}
		}

		void SetupSupplierFindBox()
		{
			const int GuidFindBoxHeight = 26;
			bool isSupplierFindBoxVisible = SupplierGuidFindBox.Visible;
			bool isSupplierAvailable = RuleSet.IsSupplierAvailable;

			if (isSupplierFindBoxVisible != isSupplierAvailable)
			{
				SupplierGuidFindBox.Visible = isSupplierAvailable;

				var relatedEntityLocation = RelatedEntityGuidFindBox.Location;
				int adjustedRelatedEntityY = isSupplierAvailable ?
					RelatedEntityGuidFindBox.Location.Y + ControlDpiScalingHelper.ScaleToCurrentDpiY(GuidFindBoxHeight)
					: RelatedEntityGuidFindBox.Location.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(GuidFindBoxHeight);
				ControlDpiScalingHelper.SetY(ref relatedEntityLocation, adjustedRelatedEntityY, false);
				RelatedEntityGuidFindBox.Location = relatedEntityLocation;

				int adjustedHeight = isSupplierAvailable ?
					HeaderGroupBox.Size.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(GuidFindBoxHeight)
					: HeaderGroupBox.Size.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(GuidFindBoxHeight);
				ControlDpiScalingHelper.SetHeight(ref HeaderGroupBox, adjustedHeight, false);
			}
		}

		#endregion

		#endregion

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			UnhookEvents();

			base.SetDataBinding(dataSource, dataMember);

			HookEvents();
		}

		#endregion

		#region Hook / Unhook Events

		void HookEvents()
		{
			var ruleSet = RuleSet;
			if (ruleSet != null)
			{
				ruleSet.BRS_ModuleInfo.ValueChanged += BRS_ModuleInfo_ValueChanged;
			}
		}

		void UnhookEvents()
		{
			var ruleSet = RuleSet;
			if (ruleSet != null)
			{
				ruleSet.BRS_ModuleInfo.ValueChanged -= BRS_ModuleInfo_ValueChanged;
			}
		}

		#endregion

		#region Actions

		#region Module Changed

		void BRS_ModuleInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupForm();
			SetupValidationRulesTabVisibility();
		}

		#endregion

		#region SetupValidationRulesTabVisibility

		void SetupValidationRulesTabVisibility()
		{
			var consumer = RuleSet.Factory.GetBarcodeParsingConsumerFromModuleCode(RuleSet.BRS_Module);
			ValidationRulesTabPage.TabVisible = consumer is IBarcodeValidationRulesConsumer;
		}

		#endregion

		#endregion

		#region FormCaption

		public override string FormCaption
		{
			get { return BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption; }
		}

		#endregion

		#region SelectRule

		public void SelectRule(BarcodeRule rule) => SelectRule(rule, rule?.BRU_BRS_RuleSet ?? ZGuid.Empty, ParsingRulesGrid);

		public void SelectRule(BarcodeValidationRule rule) => SelectRule(rule, rule?.BVR_BRS_RuleSet ?? ZGuid.Empty, ValidationRulesGrid);

		void SelectRule(BusinessObject rule, ZGuid ruleSetPK, ZGrid rulesGrid)
		{
			if (rule != null)
			{
				if (ruleSetPK != RuleSet.PK)
				{
					throw new ArgumentException("You must not attempt to select a Rule from another Rule Set. Parameter name: " + nameof(rule));
				}

				rulesGrid.SelectSingleElement(rule);
			}
		}

		#endregion

		#region SelectValidationRulesTabPage

		public void SelectValidationRulesTabPage()
		{
			RulesTabControl.SelectTab(ValidationRulesTabPage);
		}

		#endregion

		#region SupportsEDocs

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				UnhookEvents();

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
