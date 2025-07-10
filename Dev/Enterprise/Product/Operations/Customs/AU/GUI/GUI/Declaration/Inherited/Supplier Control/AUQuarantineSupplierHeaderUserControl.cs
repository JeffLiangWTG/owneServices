using System;
using System.Collections.Generic;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUQuarantineSupplierHeaderUserControl : AUSupplierHeaderUserControl
	{
		public AUQuarantineSupplierHeaderUserControl()
		{
			InitializeComponent();
			AddExtraGridColumn();

			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(DeclarationType.Quarantine);
			InvoiceHeadersBoundGrid.AfterBind += InvoiceHeadersBoundGrid_AfterBind;

			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutl4kHxyoLJZjbddFGTuFelw==";
		}

		void InvoiceHeadersBoundGrid_AfterBind(object sender, EventArgs e)
		{
			InvoiceTabControl.TabPages.Remove(CustomFieldsTabPage);

			var manager = new QuarantineControlsManager();
			manager.Initialize(InvoiceTabControl, BindingSource, InvoiceHeadersBoundGrid.GetCurrent() as JobComInvoiceHeader, InvoiceHeadersBoundGrid.ListManager);

			InvoiceTabControl.TabPages.Add(CustomFieldsTabPage);
		}

		void AddExtraGridColumn()
		{
			// 
			// JobComInvoiceHeadersBoundGrid
			//
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo1.BindToList = "ZA_PRF_List";
			zDropEditColumnStyleInfo1.Caption = "PRF";
			zDropEditColumnStyleInfo1.ColumnName = "ZA_PRF";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Units For Release";
			zCalcEditColumnStyleInfo1.ColumnName = "JZ_PiecesForRelease";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Units To Bond";
			zCalcEditColumnStyleInfo2.ColumnName = "JZ_PiecesToBond";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Packs to bond";
			zCalcEditColumnStyleInfo3.ColumnName = "JZ_BondPackCount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.BindToList = "JZ_ValuationBasis_List_ForEDIFICE";
			zDropEditColumnStyleInfo2.Caption = "Valuation Basis";
			zDropEditColumnStyleInfo2.ColumnName = "JZ_ValuationBasis";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (declaration != null)
				{
					declaration.AddInfo.ZA_IsAQISCertificateRequest_HiddenInfo.ValueChanged -= ZA_IsAQISCertificateRequest_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Column Ordering

		protected override string[] GetInvoiceHeadersGridColumnOrder()
		{
			var columns = new List<string>()
				{
					JobComInvoiceHeaderSchema.Constants.JZ_InvoiceNumber,
					JobComInvoiceHeaderSchema.Constants.JZ_OH_Supplier,
					JobComInvoiceHeaderSchema.Constants.JZ_OA_SupplierAddress,
					Customs.Business.BaseJobComInvoiceHeader.Schema.SupplierName,
					JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm,
					JobComInvoiceHeaderSchema.Constants.JZ_IncoTermPlace,
					JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount,
					JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency,
					Customs.Business.BaseJobComInvoiceHeader.Schema.InvoiceLineTotal,
					Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_BalanceString,
					JobComInvoiceHeader.Schema.ZA_ORG,
					JobComInvoiceHeader.Schema.ZA_PRF,
					JobComInvoiceHeaderSchema.Constants.JZ_OH_Buyer,
					JobComInvoiceHeader.Schema.JZ_ValuationBasis,
					JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRate,
					Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_FOBAmount,
					Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency,
					Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_CIFAmount,
					Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_CIFCurrency,
					JobComInvoiceHeader.Schema.ZA_GSTE,
					JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate,
					JobComInvoiceHeaderSchema.Constants.JZ_PaymentNo,
					JobComInvoiceHeaderSchema.Constants.JZ_PaymentAmount,
					JobComInvoiceHeaderSchema.Constants.JZ_PaymentDate,
					JobComInvoiceHeaderSchema.Constants.JZ_PaymentExRate,
					JobComInvoiceHeaderSchema.Constants.JZ_Volume,
					JobComInvoiceHeaderSchema.Constants.JZ_VolumeUQ,
					JobComInvoiceHeaderSchema.Constants.JZ_Weight,
					JobComInvoiceHeaderSchema.Constants.JZ_WeightUQ,
					JobComInvoiceHeaderSchema.Constants.JZ_NetWeight,
					JobComInvoiceHeaderSchema.Constants.JZ_NetWeightUQ,
					JobComInvoiceHeaderSchema.Constants.JZ_CU_RelatedHouseBill,
					JobComInvoiceHeaderSchema.Constants.JZ_ValuationDateOverride
				};
			return columns.ToArray();
		}

		#endregion

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			declaration = ((JobDeclaration)BindingSource.Current);
			declaration.AddInfo.ZA_IsAQISCertificateRequest_HiddenInfo.ValueChanged += ZA_IsAQISCertificateRequest_ValueChanged;
			CertificateRequestViewChanges();
		}

		#region ZA_IsAQISCertificateRequest_ValueChanged

		void ZA_IsAQISCertificateRequest_ValueChanged(object sender, EventArgs e)
		{
			CertificateRequestViewChanges();
		}

		void CertificateRequestViewChanges()
		{
			var tabPage = InvoiceTabControl.TabPages[RFPMessagingTabPageName];

			if (tabPage != null && declaration != null)
			{
				tabPage.Text = declaration.IsAQISCertificateRequest
					? Res.GetString("586F7999-243A-4196-85E9-F708AEAFBAC8", "Messages")
					: Res.GetString("9CAFBB72-E776-4ff8-A5F8-FFAD3B7F547D", "RFP Messaging");
			}
		}

		const string RFPMessagingTabPageName = "RFPMessagingTabPage";

		JobDeclaration declaration;

		#endregion

		protected override void ChangeGridColumnsVisibility()
		{
			using (JobComInvoiceHeadersBoundGrid.InnerGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				base.ChangeGridColumnsVisibility();
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetAllAvailability(false);
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(true, InvoiceHeadersGridColumnNamesInSortOrder);
			}
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			ComInvoiceDetailsTabPage.RunWhenBindingOrFirstShown(
				delegate
				{
					GSTEDropEdit.Visible = false;
					JZ_PreferenceBoundDropDownEdit.Visible = false;
					JZ_ValuationBasisBoundDropDownEdit.Visible = false;
				});
		}
	}
}
