using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.GUI.JobDeclarationForms;
using Enterprise.Customs.GB.GUI.Plugin;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class GBImportSupplierHeaderUserControl : EU.GUI.EUNonLayoutImportSupplierHeaderUserControl
	{
		public GBImportSupplierHeaderUserControl()
		{
			InitializeComponent();
			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutLTt6BSgzSfuOqd/b9PPK5Q==";
			JZ_IncoTermBoundDropDownEdit.AllowOverlap(IncoTermExplainButton);
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
				ValueIndicatorsTabPage.Text = jobDeclaration.ValueIndicatorsCaption;

				JobComInvoiceHeadersBoundGrid.InvoiceInnerGrid.SetColumnWidth(JobComInvoiceHeader.Schema.JZ_OH_Supplier, ControlDpiScalingHelper.ScaleToCurrentDpiX(110));
				this.JZ_InvoiceAmountBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 79, true);
				this.JZ_InvoiceAmountBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
				this.JZ_ValuationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 151, true);
				this.JZ_ValuationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 17, true);
				this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 127, true);
				this.JZ_IncoTermBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 127, true);
				this.JZ_IncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 127, true);

				foreach (JobComInvoiceHeader header in jobDeclaration.Invoices)
				{
					header.ZG_HouseSplitReferenceInfo.RefreshBinding();
				}
			}
		}

		void WireHandlersFromBiz(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				declaration.OnApplicationCodeChanged -= Dec_OnAppCodeChanged;
				declaration.OnApplicationCodeChanged += Dec_OnAppCodeChanged;
				Dec_OnAppCodeChanged(declaration, null);
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			Dec_OnAppCodeChanged(JobDeclaration, null);
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null)
			{
				declaration.ZG_ManualCalcInfo.ValueChanged -= ZG_ManualCalcInfo_ValueChanged;
			}
			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null)
			{
				declaration.ZG_ManualCalcInfo.ValueChanged += ZG_ManualCalcInfo_ValueChanged;
				ControlVisibilityOfChargesGrids();
			}
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ControlVisibilityOfChargesGrids();
		}

		void ZG_ManualCalcInfo_ValueChanged(object sender, EventArgs e)
		{
			ControlVisibilityOfChargesGrids();
		}

		void ControlVisibilityOfChargesGrids()
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null)
			{
				ChargesGroupBox.Visible = declaration.UseStandardValuation;
				BaseGroupChargesGroupBox.Visible = declaration.UseStandardValuation;
			}
		}

		protected override bool ShouldShowCalculateDDPButton => true;

		protected override void GetInvoiceChargesDDPCalculationResults()
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null)
			{
				var results = declaration.GetDDPCalculationResults();
				Enterprise.ZArchitecture.Environment.Globals.Message.ShowInformation(results, "DDP Calculation");
			}
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
