using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.GUI.Plugin;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class ExportInvoiceLineUserControl : EU.GUI.EUExportInvoiceLineUserControl
	{
		protected new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public ExportInvoiceLineUserControl()
		{
			InitializeComponent();
			ControlVisiblityOfLineChargesTabPage(false);

			MoveSplitterUp();
			InvoiceLineFormHelper.SwitchOutCPCFindBoxToBeFormattedProcedureCodeFindBox(this, false);
			SupplementaryCode1DropEdit.AllowOverlap(zLabel2);
			SupplementaryCode1DropEdit.AllowOverlap(zLabel4);
			SupplementaryCode2DropEdit.AllowOverlap(zLabel5);
		}

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);

			if (JobDeclaration.Invoices.Any())
			{
				BindingSource.SetBindingMember(this.SupervisingOfficeAddressControl, "FilteredInvoiceLines.SupervisingOfficeDocAddress");
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			const string isVisibleForBinding = "IsVisibleForBinding";

			PreviousEntryNumberTextBox.DataBindings.RemoveBinding(isVisibleForBinding);
			PreviousEntryLineNumberCalcEdit.DataBindings.RemoveBinding(isVisibleForBinding);
			BondedWhsQuantityCalcDropEdit.DataBindings.RemoveBinding(isVisibleForBinding);

			if (DataSource != null)
			{
				PreviousEntryNumberTextBox.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, "FilteredInvoiceLines.IsPreviousEntryNumberVisible"));
				PreviousEntryLineNumberCalcEdit.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, "FilteredInvoiceLines.IsPreviousEntryNumberVisible"));
				BondedWhsQuantityCalcDropEdit.DataBindings.Add(new KBinding(isVisibleForBinding, DataSource, "FilteredInvoiceLines.IsBondedWhsQuantityVisible"));
			}
		}

		protected override bool IsStatValueAndManualOverrideVisible_NbThisIsNotTheFieldIntheCalculationsAreaButTheOneLabeled46 => true;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			this.SetTariffFindBox(UseUniversalTariff);
			SetControlsProperty();
		}

		protected override Type GetSupportingDocumentsUserControlType()
		{
			return typeof(GBSupportingDocumentsUserControl);
		}

		protected override Type GetPreviousDocumentsUserControlType()
		{
			return typeof(GBPreviousDocumentsUserControl);
		}

		protected override Type GetAdditionalInfosUserControlType()
		{
			return typeof(GBAdditionalInfosUserControl);
		}

		void MoveSplitterUp()
		{
			Splitter.Location = ControlDpiScalingHelper.NewScaledPoint(0, 212, true); //-30px in y-axis
			BottomPanel.Location = ControlDpiScalingHelper.NewScaledPoint(0, 230, true); //-30px in y-axis
			BottomPanel.Size = ControlDpiScalingHelper.NewScaledSize(968, 350, true); //+30px height
		}

		void SetControlsProperty()
		{
			var jobDeclaration = JobDeclaration;

			if (jobDeclaration != null)
			{
				goodsOriginDropEdit.ShowDescriptionBox = false;
				SupervisingOfficeAddressControl.Visible = false;
				PackagesPivotTabPage.Text = jobDeclaration.InvoiceLinePackagesPivotTabCaption;
				TaxTabPage.Text = jobDeclaration.InvoiceLineTaxTabCaption;
				zLabel2.Text = jobDeclaration.InvoiceLineAdditionalCodesCaption;
				zLabel4.Text = jobDeclaration.InvoiceLineBlankCaption;
				zLabel5.Text = jobDeclaration.InvoiceLineBlankCaption;
				SupportingDocumentsTabPage.Text = jobDeclaration.SupportingDocumentsCaption;
				PreviousDocumentsTabPage.Text = jobDeclaration.PreviousDocumentsCaption;
				AdditionalInfosTabPage.Text = jobDeclaration.AdditionalInfoCaption;

				JI_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 16, true);
				JI_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 21, true);
				JI_LinePriceBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 40, true);
				JI_LinePriceBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 20, true);
				goodsOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 40, true);
				goodsOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 17, true);
				goodsOriginDropEdit.ShowDescriptionBox = false;

				JI_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 65, true);
				JI_WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 17, true);
				CPCFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 146, true);
				CPCFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
				MethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 39, true);
				MethodOfPaymentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 20, true);

				tariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 16, true);
				zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(266, 19, true);
				SupplementaryCode1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 16, true);
				SupplementaryCode1DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 16, true);
				zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 19, true);
				SupplementaryCode2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(518, 16, true);
				SupplementaryCode2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(518, 16, true);
				zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(573, 19, true);
				JI_AdditionalSupplementsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 16, true);
				AdditionalSupplementaryCodesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(687, 14, true);
				GDMLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(736, 19, true);

				BondedWHSOrderNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 68, true);
				BondedWHSOrderLineNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 68, true);
				BondedWhsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 172, true);

				SetEntryInstructionsVisibility();
				ChangeGridColumnsVisibility();
				Controls.Find("SpoffFromDeclarantButton", true).Single().Visible = false;
			}
		}

		protected override bool ShowEntryInstructions => true;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var declaration = CurrentDataItem as JobDeclaration;
			if (declaration != null)
			{
				var supplementaryCodeProvider = SupplementaryCodeProvider.GetByJobDeclaration(declaration);
				SetAdditionalSupplementaryCodeVisibility(supplementaryCodeProvider.NumberOfCodes > 0);
			}
		}

		void SpoffFromDeclarantButton_Click(object sender, EventArgs e)
		{
			SetSpoffOnLine(CustomsInvoiceLinesBoundGrid);
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			InvoiceLineFormHelper.SwitchOutCPCColumnToBeFormattedProcedureColumn(this);
			this.SetTariffColumnStyleInfo(UseUniversalTariff);
		}
		protected override bool UseUniversalTariff => true;

		protected override string[] GetDefaultColumnsForGrid()
		{
			var result = new List<string>(base.GetDefaultColumnsForGrid());
			result.Add(JobComInvoiceLine.Schema.JI_Calc_InstructionDisplaySequence);
			return result.ToArray();
		}
	}
}
