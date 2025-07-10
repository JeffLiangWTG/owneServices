using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EUInvoiceLineUserControl : DeclarationInvoiceLineUserControl
	{
		public EUInvoiceLineUserControl()
		{
			InitializeComponent();
			InitializeComponentExtend();
			InitializeTariffFindBox();
			InitializeSupplementaryCodeControl();
			JiggleCalculationBoxesSoTheyAllShowWithNoGaps();
			ShowHideVatGstDutyCalcuationsAndMoveEverythingElseUpIfNeeded();
			AddEntryInstructionsDropDown();
			SetGridLineColumnCharacterCasing();
			InvoiceLinePaymentTabPage.RunWhenBindingOrFirstShown((s, args) => InitInvoiceLinePaymentUserControl());
			SupplementaryCode1DropEdit.AllowOverlap(zLabel4);
			SupplementaryCode2DropEdit.AllowOverlap(zLabel5);
		}

		public override Customs.Business.ICommonInvoiceDataProvider JobDeclaration
		{
			get => base.JobDeclaration;
			set
			{
				var oldValue = JobDeclaration as ICommonInvoiceDataProvider;
				base.JobDeclaration = value;
				var newValue = JobDeclaration as ICommonInvoiceDataProvider;
				if (oldValue != newValue)
				{
					SetAdditionalInfosTabPageCaptionResourceString(newValue);
					SetSupportingDocumentsTabPageCaptionResourceString(newValue);
					SetPreviousDocumentsTabPageCaptionResourceString(newValue);
					SetPackagesTabPageCaptionResourceString(newValue);
					SetLineChargesTabPageCaptionResourceString(newValue);
					SetSupplyChainActorTabPageCaptionResourceString(newValue);
					SetFiscalReferencesTabPageCaptionResourceString(newValue);
					SetValueIndicatorsTabPageCaption(newValue);
				}
			}
		}

		void SetAdditionalInfosTabPageCaptionResourceString(ICommonInvoiceDataProvider declaration)
		{
			this.AdditionalInfosTabPage.CaptionResourceString = GetAdditionalInfosTabPageCaption(declaration);
		}

		protected virtual ResourceStringData GetAdditionalInfosTabPageCaption(ICommonInvoiceDataProvider declaration) => declaration?.IsUCC6 ?? false ? CaptionProvider.AdditionalDocuments : CaptionProvider.AdditionalInfo_44;

		void SetSupportingDocumentsTabPageCaptionResourceString(ICommonInvoiceDataProvider declaration)
		{
			this.SupportingDocumentsTabPage.CaptionResourceString = GetSupportingDocumentsTabPageCaption(declaration);
		}

		protected virtual ResourceStringData GetSupportingDocumentsTabPageCaption(ICommonInvoiceDataProvider declaration) => CaptionProvider.SupportingDocuments_44;

		void SetPreviousDocumentsTabPageCaptionResourceString(ICommonInvoiceDataProvider declaration)
		{
			PreviousDocumentsTabPage.CaptionResourceString = GetPreviousDocumentsTabPageCaption(declaration);
		}

		protected virtual ResourceStringData GetPreviousDocumentsTabPageCaption(ICommonInvoiceDataProvider declaration) => CaptionProvider.PreviousDocuments_40;

		void SetPackagesTabPageCaptionResourceString(ICommonInvoiceDataProvider declaration)
		{
			this.PackagesPivotTabPage.CaptionResourceString = GetPackagesTabPageCaption(declaration);
		}

		protected virtual ResourceStringData GetPackagesTabPageCaption(ICommonInvoiceDataProvider declaration) => CaptionProvider.Packages_31;

		void SetLineChargesTabPageCaptionResourceString(ICommonInvoiceDataProvider declaration)
		{
			this.LineChargesTabPage.CaptionResourceString = GetLineChargesTabPageCaption(declaration);
		}

		protected virtual ResourceStringData GetLineChargesTabPageCaption(ICommonInvoiceDataProvider declaration) => CaptionProvider.LineCharges;

		void SetSupplyChainActorTabPageCaptionResourceString(ICommonInvoiceDataProvider declaration)
		{
			SupplyChainActorTabPage.CaptionResourceString = GetSupplyChainActorTabPageCaption(declaration);
		}
		protected virtual ResourceStringData GetSupplyChainActorTabPageCaption(ICommonInvoiceDataProvider declaration) => CaptionProvider.SupplyChainActor;

		void SetFiscalReferencesTabPageCaptionResourceString(ICommonInvoiceDataProvider declaration)
		{
			FiscalReferencesTabPage.CaptionResourceString = GetFiscalReferencesTabPageCaption(declaration);
		}

		protected virtual ResourceStringData GetFiscalReferencesTabPageCaption(ICommonInvoiceDataProvider declaration) => CaptionProvider.FiscalReferences;

		void SetValueIndicatorsTabPageCaption(ICommonInvoiceDataProvider declaration)
		{
			this.ValueIndicatorsTabPage.CaptionResourceString = GetValueIndicatorsTabPageCaption(declaration);
		}
		protected virtual ResourceStringData GetValueIndicatorsTabPageCaption(ICommonInvoiceDataProvider declaration) => CaptionProvider.ValueIndicators;

		protected void InitializeComponentExtend()
		{
			StatisticalValueCalcEdit.Visible = IsStatValueAndManualOverrideVisible_NbThisIsNotTheFieldIntheCalculationsAreaButTheOneLabeled46;
			StatValueManualOverrideCheckBox.Visible = IsStatValueAndManualOverrideVisible_NbThisIsNotTheFieldIntheCalculationsAreaButTheOneLabeled46;
			CustomsValueLocalCurrencyControl.Visible = IsCustomsValueCalulationVisible;
			ValueForGstVatLocalCurrencyControl.Visible = IsValueForVatGstCalculationVisible;
			StatisticalValueLocalCurrencyControl.Visible = IsStatisticalValueCalculationVisible;
			JI_Calc_CIFConvertToLocalCurrencyControl.Visible = IsCifCalculationVisible;
			JI_Calc_FOBConvertToLocalCurrencyControl.Visible = IsFobCalculationVisible;
			CommercialReferenceBox.Visible = IsZG_CommercialReferenceVisible;
			TransactionNatureDropEdit.Visible = IsZG_TransactionNatureVisible;

			if (!DesignModeFinder.IsDesigning)
			{
				this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("A619C68F-2178-4A49-86D9-9DA68FC56B73", "Def. {0}", GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription);
			}
		}

		protected virtual void ShowHideVatGstDutyCalcuationsAndMoveEverythingElseUpIfNeeded()
		{
			JI_Calc_GSTConvertToLocalCurrencyControl.Visible = true;
			JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.Visible = true;
			JI_Calc_DutyConvertToLocalCurrencyControl.Visible = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1017", Justification = "We're setting property values from other properties. Those would already be scaled. So go away, stupid analyser.")]
		void JiggleCalculationBoxesSoTheyAllShowWithNoGaps()
		{
			var rootControlInCorrectPosition = GetFirstCalculationControlToWorkOutJiggledPOsitionOfOthers();
			var rootControlInCorrectPositionLocation = rootControlInCorrectPosition.Location;
			var rootControlInCorrectPositionSize = rootControlInCorrectPosition.Size;
			var otherControlsToJiggle = new[]
			{
				CustomsValueLocalCurrencyControl,
				ValueForGstVatLocalCurrencyControl,
				StatisticalValueLocalCurrencyControl,
				JI_Calc_FOBConvertToLocalCurrencyControl,  // will be excluded due to invisibility but keep it in this list for future reference
				JI_Calc_CIFConvertToLocalCurrencyControl
			}.Where(c => c.Visible).ToArray();

			foreach (var c in otherControlsToJiggle)
			{
				c.Size = rootControlInCorrectPositionSize;
			}
			var gapBewtweenControlsInPixels = 3;
			for (int i = 0; i < otherControlsToJiggle.Length; i++)
			{
				var newLocation = new System.Drawing.Point(rootControlInCorrectPositionLocation.X,  // Not scaled, 'cos that would be double scaling
															rootControlInCorrectPositionLocation.Y + (i * rootControlInCorrectPositionSize.Height) + (i * gapBewtweenControlsInPixels));
				otherControlsToJiggle[i].Location = newLocation;
				otherControlsToJiggle[i].TabStop = false;
			}
		}

		protected virtual Control GetFirstCalculationControlToWorkOutJiggledPOsitionOfOthers()
		{
			return JI_Calc_FOBConvertToLocalCurrencyControl;
		}

		public void SetIsStandaloneInvoiceForm(bool isStandalone)
		{
			PackagesPivotTabPage.TabVisible = !isStandalone;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			FiscalReferencesUserControl.UserControlType = GetFiscalReferencesUserControlType();
			SupportingDocumentsUserControl.UserControlType = GetSupportingDocumentsUserControlType();
			PreviousDocumentsUserControl.UserControlType = GetPreviousDocumentsUserControlType();
			additionalInfosUserControl1.UserControlType = GetAdditionalInfosUserControlType();
			VehicleUserControl.UserControlType = GetInvoiceLineVehicleUserControlType();
			AuthorisationsUserControl.UserControlType = GetInvoiceLineAuthorisationsUserControlType();
			organizationsUserControl1.UserControlType = GetOrganizationsUserControlType();
			invoiceLineValuationIndicatorsUserControl.UserControlType = GetValuationIndicatorsUserControlType();

			InitTabsVisibility();
			InitializeSupplementaryCodeControl();
			AddCopyDocumentsOfInvoiceLineMenuToGrid();
		}

		MenuItem copyDocumentsOfInvoiceLineMenuItem;

		void AddCopyDocumentsOfInvoiceLineMenuToGrid()
		{
			copyDocumentsOfInvoiceLineMenuItem =
				new ZMenuItem(ResString.GetMultilingualString("a591a1c8-5a13-4349-bca1-aa437946ff86", "Copy Documents of Invoice Line"),
					CopyDocumentsOfInvoiceLine)
				{
					Name = nameof(copyDocumentsOfInvoiceLineMenuItem),
				};
			CustomsInvoiceLinesBoundGrid.ContextMenu.MenuItems.Add(copyDocumentsOfInvoiceLineMenuItem);
		}

		void CopyDocumentsOfInvoiceLine(object sender, EventArgs e)
		{
			if (CurrentInvoiceLine is { } invoiceLine)
			{
				var copyDocuments = new CopyDocumentsSelectionHeader(invoiceLine);
				using var form = new InvoiceLineCopyDocumentsForm(copyDocuments);
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		void InitTabsVisibility()
		{
			var jobDeclaration = CurrentDataItem as JobDeclaration;
			var configuration = jobDeclaration?.Configuration.InvoiceLineConfiguration;
			if (configuration != null)
			{
				FiscalReferencesTabPage.TabVisible = configuration.FiscalReferencesSupport(jobDeclaration);
				SupportingDocumentsTabPage.TabVisible = configuration.SupportingDocumentsSupport(jobDeclaration);
				AdditionalInfosTabPage.TabVisible = configuration.AdditionalInfosSupport(jobDeclaration);
				PreviousDocumentsTabPage.TabVisible = configuration.PreviousDocumentsSupport(jobDeclaration);
				TaxTabPage.TabVisible = configuration.TaxSupport(jobDeclaration);
				VehicleTabPage.TabVisible = configuration.VehicleSupport(jobDeclaration);
				AuthorisationsTabPage.TabVisible = configuration.AuthorisationsSupportForInvoiceLine(jobDeclaration);
				OrganizationsTabPage.TabVisible = configuration.OrganizationsSupport(jobDeclaration);
				InvoiceLinePaymentTabPage.TabVisible = configuration.InvoiceLinePaymentSupport(jobDeclaration);
				ValueIndicatorsTabPage.TabVisible = configuration.ValueIndicatorsSupport(jobDeclaration);
				SupplyChainActorTabPage.TabVisible = configuration.AdditionalSupplyChainActorSupport(jobDeclaration);
			}
		}

		protected override void CustomsInvoiceLinesBoundGrid_ListManager_CurrentChanged(object sender, EventArgs e)
		{
			base.CustomsInvoiceLinesBoundGrid_ListManager_CurrentChanged(sender, e);
			SetAdditionalProcedureCodesVisibles();
		}

		void SetAdditionalProcedureCodesVisibles()
		{
			var isAdditionalProcedureCodesApplicable = CurrentInvoiceLine?.IsAdditionalProcedureCodesApplicable ?? ZBool.False;
			zButtonMoreAdditionalProcedureCode.Visible = isAdditionalProcedureCodesApplicable;
			zTextBoxAddtionalProcedureCodeAsString.Visible = isAdditionalProcedureCodesApplicable;
		}

		protected override void CustomsInvoiceLinesBoundGrid_ListManager_CurrentItemChanged(object sender, EventArgs e)
		{
			base.CustomsInvoiceLinesBoundGrid_ListManager_CurrentItemChanged(sender, e);
			SetAdditionalProcedureCodesVisibles();
		}

		protected JobComInvoiceLine currentInvoiceLine;

		protected virtual Type GetFiscalReferencesUserControlType()
		{
			return typeof(InvoiceLineFiscalReferencesUserControl);
		}

		protected virtual Type GetSupportingDocumentsUserControlType()
		{
			return typeof(SupportingDocumentsUserControl);
		}

		protected virtual Type GetPreviousDocumentsUserControlType()
		{
			return typeof(PreviousDocumentsUserControl);
		}

		protected virtual Type GetAdditionalInfosUserControlType()
		{
			return typeof(AdditionalInfosUserControl);
		}

		protected virtual Type GetInvoiceLineVehicleUserControlType()
		{
			return typeof(InvoiceLineVehicleUserControl);
		}

		protected virtual Type GetInvoiceLineAuthorisationsUserControlType()
		{
			return typeof(InvoiceLineAuthorisationsUserControl);
		}

		protected virtual Type GetOrganizationsUserControlType()
		{
			return typeof(InvoiceLineOrganizationsUserControl);
		}

		protected virtual Type GetValuationIndicatorsUserControlType()
		{
			return typeof(InvoiceLineValuationIndicatorCheckboxesUserControl);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null)
			{
				// The use of SupplementaryCodeProvider.GetByCountryCode here should match the use in JobComInvoiceLine. Please update in both places if needed.
				var supplementaryCodeProvider = SupplementaryCodeProvider.GetByCountryCode(JobComInvoiceLine.GetCountryCodeForSupplementaryCodeHelper(CurrentDataItem as JobDeclaration, CurrentDataItem as JobComInvoiceHeader));
				SetAdditionalSupplementaryCodeVisibility(supplementaryCodeProvider.NumberOfCodes > 0);
				SetEntryInstructionsVisibility();
				ChangeGridColumnsVisibility();
			}
		}

		protected virtual void SetAdditionalSupplementaryCodeVisibility(bool hasAdditionalCodes)
		{
			SetControlVisibility(hasAdditionalCodes, zLabel5, JI_AdditionalSupplementsTextBox, AdditionalSupplementaryCodesEditButton);
		}

		protected virtual void SetEntryInstructionsVisibility()
		{
			SetControlVisibility(ShowEntryInstructions, JI_CEIGuidDropEdit);
		}

		void SetControlVisibility(bool condition, params Control[] controls)
		{
			foreach (var control in controls)
			{
				control.Visible = condition;
			}
		}

		protected void AdditionalSupplementaryCodesEditButton_Click(object sender, EventArgs e)
		{
			JobComInvoiceLine invoiceLine = null;
			var listManager = CustomsInvoiceLinesBoundGrid.ListManager;
			if (listManager != null)
			{
				invoiceLine = (JobComInvoiceLine)listManager.GetCurrent();
				if (invoiceLine != null && invoiceLine.IsDeleted)
				{
					invoiceLine = null;
				}
			}

			if (invoiceLine != null)
			{
				AdditionalSupplementaryCodesForm.ShowDialog(invoiceLine);
			}
		}

		public List<JobComInvoiceLine> SelectedInvoiceLines => CustomsInvoiceLinesBoundGrid.SelectedElements.OfType<JobComInvoiceLine>().ToList();

		#region GDMLink

		public void ShowGuidedDecisionMakingForm()
		{
			var invoiceLine = CurrentInvoiceLine;
			var invoiceLines = SelectedInvoiceLines;
			var isSelectedMultiple = invoiceLines.Count > 1;

			if (invoiceLine != null)
			{
				if (isSelectedMultiple)
				{
					if (invoiceLines.All(l => l.JI_Tariff == invoiceLines.FirstOrDefault().JI_Tariff))
					{
						ZFormModaliser.ShowDialogAndDispose(GetGuidedDecisionMakingForm(invoiceLine, invoiceLines));
						return;
					}
					else
					{
						Globals.Message.Show(Res.GetString("E3D8B087-9F93-43BE-84F7-B3EE9AFD8DF0", "The selected Invoice Lines have different Commodity Codes. Only the current Invoice Line will be updated."), Res.GetString("72343db3-8d52-45ed-bbe3-f4d0c0fab601", "Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
				}
				ZFormModaliser.ShowDialogAndDispose(GetGuidedDecisionMakingForm(invoiceLine));
			}
		}

		void GDMLink_Clicked(object sender, EventArgs e)
		{
			ShowGuidedDecisionMakingForm();
		}

		protected GuidedDecisionMakingForm GetGuidedDecisionMakingForm(JobComInvoiceLine invoiceLine)
		{
			var gDMBasic = invoiceLine.GetGuidedDecisionMakingBasic();
			var target = invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineTarget();
			return GetGuidedDecisionMakingFormCore(gDMBasic, target);
		}

		protected GuidedDecisionMakingForm GetGuidedDecisionMakingForm(JobComInvoiceLine invoiceLine, List<JobComInvoiceLine> invoiceLines)
		{
			var gDMBasic = invoiceLine.GetGuidedDecisionMakingBasicForMultiLine();
			var target = invoiceLine.GetGuidedDecisionMakingMultiInvoiceLinesTarget(invoiceLines);
			return GetGuidedDecisionMakingFormCore(gDMBasic, target);
		}

		protected virtual GuidedDecisionMakingForm GetGuidedDecisionMakingFormCore(GuidedDecisionMakingBasic gDMBasic, IGuidedDecisionMakingTarget target) => new GuidedDecisionMakingForm(gDMBasic, target);

		#endregion

		ZBool HasCPCCode(JobComInvoiceLine invoiceLine) => !invoiceLine.JI_FormattedProcedure.IsEmpty;

		public new JobComInvoiceLine CurrentInvoiceLine => (JobComInvoiceLine)base.CurrentInvoiceLine;

		void ZButtonMoreAdditionalProcedureCode_Click(object sender, EventArgs e)
		{
			var invoiceLine = CurrentInvoiceLine;
			if (invoiceLine != null)
			{
				if (HasCPCCode(invoiceLine))
				{
					ZFormModaliser.ShowDialogAndDispose(new AdditionalProcedureCodeForm(invoiceLine));
				}
				else
				{
					using (var notification = new ZMessageBox(invoiceLine.ProcedureMustBeEnteredForAdditionalProceduresSelectionErrorMessage,
																	  Res.GetString("EEA991DD-478A-4C3C-B9BB-245810EA9B9D", "Error"),
																	  MessageBoxButtons.OK, MessageBoxIcon.Warning))
					{
						notification.ShowDialog();
					}
				}
			}
		}

		protected override Customs.GUI.InvoiceLineChargesUserControl GetInvoiceLineChargesUserControl() => new InvoiceLineChargesUserControl();

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			AddColumnsToGrid();

			ReorderAndChangeInvoiceLinesGridVisibility();

			CustomsInvoiceLinesBoundGrid.SetColumnGroupName(JobComInvoiceLine.Schema.JI_CustomsSecondQuantity, Res.GetData("B2C7721D-9462-406A-9C0D-9F60ACCC53EA", "Supplementary Qty/UQ"));
			CustomsInvoiceLinesBoundGrid.SetColumnGroupName(JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty, Res.GetData("B2C7721D-9462-406A-9C0D-9F60ACCC53EA", "Supplementary Qty/UQ"));
			CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.JI_CustomsQuantity, 140);
		}

		void ReorderAndChangeInvoiceLinesGridVisibility()
		{
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.SetAllColumnsVisible(false);
				var defaultColumns = GetDefaultColumnsForGrid();
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, defaultColumns);
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(defaultColumns);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(ShowEntryInstructions, JobComInvoiceLineSchema.Constants.JI_CEI);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(ShowEntryInstructions, JobComInvoiceLine.Schema.EntryInstructionDescription);
			}
		}

		protected virtual string[] GetDefaultColumnsForGrid()
		{
			var result = new List<string>()
			{
				JobComInvoiceLine.Schema.JI_LineNo,
				JobComInvoiceLine.Schema.JI_Calc_Invoice,
				JobComInvoiceLine.Schema.JI_CEI,
				JobComInvoiceLine.Schema.EntryInstructionDescription,
				JobComInvoiceLine.Schema.JI_PartNo,
				JobComInvoiceLine.Schema.JI_FormattedTariff,
				JobComInvoiceLine.Schema.JI_Description,
			};

			result.Add(JobComInvoiceLine.Schema.JI_FormattedProcedure);

			result.AddRange(new[] {
				JobComInvoiceLine.Schema.JI_InvoiceQuantity,
				JobComInvoiceLine.Schema.JI_CountryOfOrigin,
				JobComInvoiceLine.Schema.JI_CustomsQuantity,
				JobComInvoiceLine.Schema.JI_LinePrice,
				JobComInvoiceLine.Schema.JI_SupplementaryCode1,
				JobComInvoiceLine.Schema.JI_SupplementaryCode2,
				JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
				JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
				JobComInvoiceLine.Schema.EntryReferenceNumber,
				JobComInvoiceLine.Schema.MergedLineNumber
			});

			if (IsZG_CommercialReferenceVisible)
			{
				result.Add(JobComInvoiceLine.Schema.ZG_CommercialReference);
			}

			return result.ToArray();
		}

		// These are in the "Calculations" section in the right-hand side:
		protected virtual bool IsCifCalculationVisible => false;
		protected virtual bool IsFobCalculationVisible => false;  // Keep this as false because FOB in base is faulty it doesn't actually give you the FOB value at all, it gives you the value for duty.
		protected virtual bool IsCustomsValueCalulationVisible => false;
		protected virtual bool IsValueForVatGstCalculationVisible => false;
		protected virtual bool IsStatisticalValueCalculationVisible => true;

		protected virtual bool IsStatValueAndManualOverrideVisible_NbThisIsNotTheFieldIntheCalculationsAreaButTheOneLabeled46 => false;

		protected virtual bool IsZG_CommercialReferenceVisible => false;

		protected virtual bool IsZG_TransactionNatureVisible => false;

		bool IsRequestedProcedureEnable => JobDeclaration is JobDeclaration declaration && declaration.IsRequestedProcedureEnable;

		protected virtual void AddColumnsToGrid()
		{
			var entryReferenceNumberInfo = new ZArchitecture.ZTextBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("64EA217C-E71B-4BFA-99CB-3AACF861FB06", "Entry Type.", "Entry Ref. Num.", "Entry Reference Number", "Reference number of this invoice line's entry header. If the invoice line is not yet merged into an entry, or the entry header is not yet saved, this may show a blank or a placeholder value. The reference number is generated internally by CargoWise."),
				ColumnName = JobComInvoiceLine.Schema.EntryReferenceNumber,
				IsMandatory = false,
			};

			var mergedLineNumberInfo = new ZArchitecture.ZTextBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("b7f1c602-e921-4850-be40-b17b051f67b8", "Merged Ln. #"),
				ColumnName = JobComInvoiceLine.Schema.MergedLineNumber,
				IsMandatory = false,
			};

			var cPCColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo()
			{
				ColumnName = JobComInvoiceLine.Schema.JI_FormattedProcedure,
				IsMandatory = !IsRequestedProcedureEnable,
				ModuleID = ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusProcedure,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(51),
				IsVisible = !IsRequestedProcedureEnable,
				IsUnavailable = IsRequestedProcedureEnable
			};

			var supplementaryCode1 = CreateSupplementaryCodeColumnStyleInfo(JobComInvoiceLine.Schema.JI_SupplementaryCode1);
			var supplementaryCode2 = CreateSupplementaryCodeColumnStyleInfo(JobComInvoiceLine.Schema.JI_SupplementaryCode2);

			var supplementaryQtyCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo()
			{
				BindToDecimalPlaces = null,
				ColumnName = JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
			};
			var supplementaryUQDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceLinesUserControl|286ED967-AE9D-4F5B-8D3E-5F26A606ECCC", "Sup. UQ", "Supplementary UQ", "Supplementary Unit Quantity"),
				ColumnName = JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
			};

			CustomsInvoiceLinesBoundGrid.ColumnStyles.InsertRange(4, new object[] { entryReferenceNumberInfo, mergedLineNumberInfo, cPCColumnStyleInfo, supplementaryCode1, supplementaryCode2, supplementaryQtyCalcEditColumnStyleInfo, supplementaryUQDropEditColumnStyleInfo });

			var entryInstructionDescriptionInfo = new ZArchitecture.ZTextBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("F219921E-F2AA-47A2-8BA6-1E5CA375EE1A", "Entry Inst. Desc.", "Entry Instruction Desc.", "Entry Instruction Description"),
				ColumnName = JobComInvoiceLine.Schema.EntryInstructionDescription,
				IsMandatory = false,
			};

			var zGuidDropEditColumnStyleInfo1 = new ZGuidDropEditColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("InvoiceLinesUserControl|ED95690E-7500-4858-97F2-033683D7B351", "Entry Ins.", "Entry Instruction", "Entry Instruction Customs Procedure Code"),
				ColumnName = "JI_CEI",
				GroupName = Res.GetData("InvoiceLinesUserControl|D9A6DD9D-7871-4B6A-A3E1-D616B369123B", "Customs Procedure"),
				ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(104),
			};

			CustomsInvoiceLinesBoundGrid.ColumnStyles.InsertRange(2, new object[] { zGuidDropEditColumnStyleInfo1, entryInstructionDescriptionInfo });

			var countryOfOriginColumnStyle = CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin);
			var countryOfOriginColumnIndex = CustomsInvoiceLinesBoundGrid.ColumnStyles.IndexOf(countryOfOriginColumnStyle);
			var newCountryOfOriginColumnStyle = new ZDropEditColumnStyleInfo()
			{
				ColumnName = JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin,
				ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(76),
			};
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(countryOfOriginColumnStyle);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Insert(countryOfOriginColumnIndex, newCountryOfOriginColumnStyle);

			var commercialReferenceInfo = new ZArchitecture.ZTextBoxColumnStyleInfo()
			{
				ColumnName = JobComInvoiceLine.Schema.ZG_CommercialReference,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				IsVisible = IsZG_CommercialReferenceVisible,
				IsUnavailable = !IsZG_CommercialReferenceVisible
			};
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(commercialReferenceInfo);

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_Calc_RequestedProcedure,
				IsMandatory = IsRequestedProcedureEnable,
				IsVisible = IsRequestedProcedureEnable,
				IsUnavailable = !IsRequestedProcedureEnable
			});
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_PreviousProcedure,
				IsMandatory = IsRequestedProcedureEnable,
				IsVisible = IsRequestedProcedureEnable,
				IsUnavailable = !IsRequestedProcedureEnable
			});
		}

		ZMultiControlColumnStyleInfo CreateSupplementaryCodeColumnStyleInfo(string columnName)
		{
			return new ZMultiControlColumnStyleInfo
			{
				ColumnName = columnName,
				IsMandatory = false,
				FieldTypeColumnName = "SupplementaryCodesFieldType",
			};
		}

		void InitializeSupplementaryCodeControl()
		{
			SupplementaryCode2DropEdit.Visible = SupplementaryCode1DropEdit.Visible = UseUniversalTariff;
			SupplementaryCode2TextBox.Visible = SupplementaryCode1TextBox.Visible = !UseUniversalTariff;
		}

		void InitializeTariffFindBox()
		{
			CreateUniversalTariffFindBox();
			SetTariffFindBoxProperty();
		}

		public void CreateUniversalTariffFindBox()
		{
			var unTariffFindBox = new Universal.GUI.TariffFindBox();
			unTariffFindBox.GetTariffType = GetUniversalTariffType;
			unTariffFindBox.GetDataGrouping = GetDataGroupingForUniversalTariff;
			unTariffFindBox.GetEffectiveDate = GetEffectiveAssessmentDateForUniversalTariff;
			tariffFindBox = unTariffFindBox;
		}

		public void SetTariffFindBoxProperty()
		{
			tariffFindBox.Name = "TariffFindBox";
			tariffFindBox.PreBoundMaxLength = 15;
			tariffFindBox.ShowDescriptionBox = false;
			tariffFindBox.Location = ControlDpiScalingHelper.NewScaledPoint(128, 16, true);
			tariffFindBox.Size = ControlDpiScalingHelper.NewScaledSize(148, 18, true);
			tariffFindBox.TabIndex = 1;
			BindingSource.SetBindingMember(tariffFindBox, "FilteredInvoiceLines.JI_FormattedTariff");
			ClassificationDetailsGroupBox.Controls.Add(tariffFindBox);
		}

		protected override ZString TariffColumnNameCore => JobComInvoiceLine.Schema.JI_FormattedTariff;

		protected virtual bool ShowEntryInstructions => (JobDeclaration as JobDeclaration)?.AreMultipleEntryInstructionsAllowed ?? false;

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();

			if (CustomsInvoiceLinesBoundGrid.ColumnStyles.OfType<ZDropEditColumnStyleInfo>().ToArray().Any(x => x.ColumnName.Equals(JobComInvoiceLineSchema.Constants.JI_CEI)))
			{
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(ShowEntryInstructions, JobComInvoiceLineSchema.Constants.JI_CEI);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(ShowEntryInstructions, JobComInvoiceLine.Schema.EntryInstructionDescription);
			}
		}

		protected virtual void AddEntryInstructionsDropDown()
		{
			JI_CEIGuidDropEdit = new ZGuidDropEdit();
			this.JI_CEIGuidDropEdit.SuspendLayout();
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_CEIGuidDropEdit);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CEIGuidDropEdit, 0);

			//
			// JI_CEIGuidDropEdit
			//
			this.JI_CEIGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_CEIGuidDropEdit, "FilteredInvoiceLines.JI_CEI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobDeclaration)(null)).FilteredInvoiceLines.SyncRoot)).JI_CEI);
			this.JI_CEIGuidDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("6182fc0e-3a81-4815-9603-b3fec2c5b65b", "Entry Instruction");
			this.JI_CEIGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 89, true);
			this.JI_CEIGuidDropEdit.Name = "JI_CEIGuidDropEdit";
			this.JI_CEIGuidDropEdit.PreBoundMaxLength = 3;
			this.JI_CEIGuidDropEdit.ShowDescriptionBox = true;
			this.JI_CEIGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			this.JI_CEIGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 18, true);
			this.JI_CEIGuidDropEdit.TabIndex = 17;

			this.JI_CEIGuidDropEdit.ResumeLayout(true);
			this.JI_CEIGuidDropEdit.PerformLayout();
		}

		protected virtual void SetGridLineColumnCharacterCasing()
		{
		}

		void InitInvoiceLinePaymentUserControl()
		{
			invoiceLinePaymentUserControl1.UserControlType = typeof(InvoiceLinePaymentUserControl);
		}
	}
}
