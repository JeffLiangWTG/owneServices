using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.GUI.JobDeclarationForms;
using Enterprise.Customs.GB.GUI.Plugin;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class GBExportSupplierHeaderUserControl : EU.GUI.EUNonLayoutExportSupplierHeaderUserControl
	{
		public GBExportSupplierHeaderUserControl()
		{
			InitializeComponent();
			ChargesGroupBox.Visible = false;
			BaseGroupChargesGroupBox.Visible = false;
			TransportChargesMethodOfPaymentDropEdit.CaptionResourceString = null;
			JZ_IncoTermBoundDropDownEdit.AllowOverlap(IncoTermExplainButton);
		}

		protected override Type GetSupportingDocumentsUserControlType() => typeof(GBSupportingDocumentsUserControl);

		protected override Type GetPreviousDocumentsUserControlType() => typeof(GBPreviousDocumentsUserControl);

		protected override Type GetAdditionalInfosUserControlType() => typeof(GBAdditionalInfosUserControl);

		protected override void UpdateControlsVisiblityWhenJobDeclarationChanged()
		{
			base.UpdateControlsVisiblityWhenJobDeclarationChanged();
			WireHandlersFromBiz((JobDeclaration)JobDeclaration);
		}

		void Dec_OnAppCodeChanged(object sender, EventArgs e)
		{
			var jobDeclaration = sender as JobDeclaration;

			if (jobDeclaration != null)
			{
				this.RefreshControlCaptions();
				JobComInvoiceHeadersBoundGrid.InvoiceInnerGrid.RefreshColumnCaptions(typeof(JobComInvoiceHeader), jobDeclaration.MultipleKeysToUse);

				SupportingDocumentsTabPage.GetExtension<ILabelCaptionRenderer>().Caption = jobDeclaration.SupportingDocumentsCaption;
				PreviousDocumentsTabPage.GetExtension<ILabelCaptionRenderer>().Caption = jobDeclaration.PreviousDocumentsCaption;
				AdditionalInfoTabPage.GetExtension<ILabelCaptionRenderer>().Caption = jobDeclaration.AdditionalInfoCaption;

				TransportChargesMethodOfPaymentDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(173, 250, true);
				TransportChargesMethodOfPaymentDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(184, 20, true);
				JZ_InvoiceAmountBoundCurrencyControl.Location = ControlDpiScalingHelper.NewScaledPoint(128, 79, true);
				JZ_InvoiceAmountBoundCurrencyControl.Size = ControlDpiScalingHelper.NewScaledSize(229, 20, true);
				JZ_ValuationCodeDropEdit.Location = ControlDpiScalingHelper.NewScaledPoint(128, 151, true);
				JZ_ValuationCodeDropEdit.Size = ControlDpiScalingHelper.NewScaledSize(229, 17, true);
				IncoTermExplainButton.Location = ControlDpiScalingHelper.NewScaledPoint(180, 127, true);
				JZ_IncoTermBoundDropDownEdit.Location = ControlDpiScalingHelper.NewScaledPoint(120, 127, true);
				JZ_IncoTermPlaceTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(280, 127, true);

				ChargesGroupBox.Visible = true;
				BaseGroupChargesGroupBox.Visible = true;
			}
		}

		void WireHandlersFromBiz(JobDeclaration header)
		{
			if (header != null)
			{
				header.OnApplicationCodeChanged -= Dec_OnAppCodeChanged;
				header.OnApplicationCodeChanged += Dec_OnAppCodeChanged;
				Dec_OnAppCodeChanged(header, null);
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			Dec_OnAppCodeChanged(JobDeclaration, null);
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (JobDeclaration is JobDeclaration declaration)
			{
				declaration.OnApplicationCodeChanged -= Dec_OnAppCodeChanged;
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
