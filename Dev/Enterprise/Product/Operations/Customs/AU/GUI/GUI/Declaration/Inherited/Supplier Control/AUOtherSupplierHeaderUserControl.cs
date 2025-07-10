using System.Collections.Generic;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUOtherSupplierHeaderUserControl : AUSupplierHeaderUserControl
	{
		public AUOtherSupplierHeaderUserControl()
		{
			InitializeComponent();
			AddExtraGridColumn();
			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(DeclarationType.EdificeImport);
			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutZqATrs1wYGJY91xTMxFaKQ==";
		}

		#region Column Ordering

		protected override string[] GetInvoiceHeadersGridColumnOrder()
		{
			var columns = new List<string>()
				{
					JobComInvoiceHeaderSchema.Constants.JZ_InvoiceNumber,
					JobComInvoiceHeaderSchema.Constants.JZ_OH_Supplier,
					JobComInvoiceHeaderSchema.Constants.JZ_OA_SupplierAddress,
					JobComInvoiceHeader.Schema.SupplierName,
					JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm,
					JobComInvoiceHeaderSchema.Constants.JZ_IncoTermPlace,
					JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount,
					JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency,
					JobComInvoiceHeader.Schema.InvoiceLineTotal,
					JobComInvoiceHeader.Schema.JZ_Calc_BalanceString,
					JobComInvoiceHeader.Schema.JZ_PiecesForRelease,
					JobComInvoiceHeader.Schema.JZ_Nature10PackCount,
					JobComInvoiceHeader.Schema.JZ_PiecesToBond,
					JobComInvoiceHeader.Schema.JZ_BondPackCount,
					JobComInvoiceHeader.Schema.ZA_ORG,
					JobComInvoiceHeader.Schema.ZA_PRF,
					JobComInvoiceHeaderSchema.Constants.JZ_OH_Buyer,
					JobComInvoiceHeader.Schema.JZ_ValuationBasis,
					JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRate,
					JobComInvoiceHeader.Schema.JZ_Calc_FOBAmount,
					JobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency,
					JobComInvoiceHeader.Schema.JZ_Calc_CIFAmount,
					JobComInvoiceHeader.Schema.JZ_Calc_CIFCurrency,
					JobComInvoiceHeader.Schema.ZA_GSTE,
					JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate,
					JobComInvoiceHeaderSchema.Constants.JZ_PaymentNo,
					JobComInvoiceHeaderSchema.Constants.JZ_PaymentAmount,
					JobComInvoiceHeaderSchema.Constants.JZ_PaymentDate,
					JobComInvoiceHeaderSchema.Constants.JZ_PaymentExRate,
					"AddInfo+ZA_PermitNumbers_Hidden",
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

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			using (JobComInvoiceHeadersBoundGrid.InnerGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				bool isImport = JobDeclaration.IsImport;
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(isImport,
					[
						JobComInvoiceHeader.Schema.JZ_Nature10PackCount,
						JobComInvoiceHeader.Schema.JZ_BondPackCount,
						JobComInvoiceHeader.Schema.JZ_PiecesForRelease,
						JobComInvoiceHeader.Schema.JZ_PiecesToBond
					]);

				bool isExWarehouse = JobDeclaration.IsExWarehouse;
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(!isExWarehouse,
					[
						JobComInvoiceHeader.Schema.JZ_BondPackCount,
						JobComInvoiceHeader.Schema.JZ_PiecesToBond,
						JobComInvoiceHeader.Schema.JZ_PiecesForRelease
					]);

				bool isExport = JobDeclaration.IsExport;
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(!isExport,
					[
						JobComInvoiceHeader.Schema.JZ_OH_Supplier,
						JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress
					]);
			}
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			gSTEDropEdit.Visible = JobDeclaration.IsImport;
			jZ_PreferenceBoundDropDownEdit.Visible = JobDeclaration.IsImport;
			JZ_ValuationBasisBoundDropDownEdit.Visible = !JobDeclaration.IsExWarehouse && JobDeclaration.IsImport;
			addInfoPermitNumberBoundTextBox.Visible = !JobDeclaration.IsExWarehouse;
		}

		void AddExtraGridColumn()
		{
			// 
			// JobComInvoiceHeadersBoundGrid
			//
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo3.BindToList = "ZA_PRF_List";
			zDropEditColumnStyleInfo3.Caption = "PRF";
			zDropEditColumnStyleInfo3.ColumnName = "ZA_PRF";
			zDropEditColumnStyleInfo3.ToolTip = "Preference";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Units For Release";
			zCalcEditColumnStyleInfo4.ColumnName = "JZ_PiecesForRelease";
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.Caption = "Units To Bond";
			zCalcEditColumnStyleInfo5.ColumnName = "JZ_PiecesToBond";
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.Caption = "Packs to bond";
			zCalcEditColumnStyleInfo6.ColumnName = "JZ_BondPackCount";
			zDropEditColumnStyleInfo4.BindToList = "JZ_ValuationBasis_List_ForEDIFICE";
			zDropEditColumnStyleInfo4.Caption = "Valuation Basis";
			zDropEditColumnStyleInfo4.ColumnName = "JZ_ValuationBasis";
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
		}
	}
}
