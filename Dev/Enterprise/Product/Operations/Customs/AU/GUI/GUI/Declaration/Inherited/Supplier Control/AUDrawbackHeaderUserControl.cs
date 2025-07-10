using System;
using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUDrawbackHeaderUserControl : DeclarationInvoiceHeaderUserControl
	{
		public AUDrawbackHeaderUserControl()
		{
			InitializeComponent();
			InitializeJobComInvoiceHeadersBoundGrid();
			InitializeBaseGroupChargesGrid();
			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Drawback);
		}

		#region Modify Column Properties

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			ModifyColumnProperties();
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAllColumnsVisible(true);
			JobComInvoiceHeadersBoundGrid.InnerGrid.ReOrderColumns(CustomsInvoiceLinesBoundGridColumnNamesInSortOrder);
		}
		string[] CustomsInvoiceLinesBoundGridColumnNamesInSortOrder
		{
			get { return customsInvoiceLinesBoundGridColumnNamesInSortOrder ?? (customsInvoiceLinesBoundGridColumnNamesInSortOrder = GetInvoiceLinesGridColumnOrder()); }
		}
		string[] customsInvoiceLinesBoundGridColumnNamesInSortOrder;

		string[] GetInvoiceLinesGridColumnOrder()
		{
			var result = new List<string>();
			result.Add(JobComInvoiceHeader.Schema.JZ_InvoiceNumber);
			result.Add(JobComInvoiceHeader.Schema.JZ_IncoTerm);
			result.Add(JobComInvoiceHeader.Schema.JZ_IncoTermPlace);
			result.Add(JobComInvoiceHeader.Schema.JZ_InvoiceAmount);
			result.Add(JobComInvoiceHeader.Schema.InvoiceLineTotal);
			result.Add(JobComInvoiceHeader.Schema.JZ_Calc_ChargesExcludedFromITOT);
			result.Add(JobComInvoiceHeader.Schema.JZ_Calc_BalanceString);
			result.Add(JobComInvoiceHeader.Schema.JZ_InvoiceDate);
			result.Add(JobComInvoiceHeader.Schema.JZ_InvoiceDisplaySequence);
			result.Add(JobComInvoiceHeader.Schema.JZ_NoOfPacks);
			result.Add(JobComInvoiceHeader.Schema.JZ_OH_Supplier);
			result.Add(JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress);
			result.Add(JobComInvoiceHeader.Schema.SupplierName);
			result.Add(JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate);
			result.Add(JobComInvoiceHeader.Schema.JZ_InvoiceCurrLandedCostExRate);
			result.Add(JobComInvoiceHeader.Schema.JZ_Calc_FOBAmount);
			result.Add(JobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency);
			result.Add(JobComInvoiceHeader.Schema.JZ_Calc_CIFAmount);
			result.Add(JobComInvoiceHeader.Schema.JZ_Calc_CIFCurrency);
			result.Add(JobComInvoiceHeader.Schema.JZ_OH_Buyer);
			result.Add(JobComInvoiceHeader.Schema.JZ_Volume);
			result.Add(JobComInvoiceHeader.Schema.JZ_VolumeUQ);
			result.Add(JobComInvoiceHeader.Schema.JZ_Weight);
			result.Add(JobComInvoiceHeader.Schema.JZ_WeightUQ);
			result.Add(JobComInvoiceHeader.Schema.JZ_NetWeight);
			result.Add(JobComInvoiceHeader.Schema.JZ_NetWeightUQ);
			result.Add(JobComInvoiceHeader.Schema.JZ_PaymentNo);
			result.Add(JobComInvoiceHeader.Schema.JZ_PaymentAmount);
			result.Add(JobComInvoiceHeader.Schema.JZ_PaymentDate);
			result.Add(JobComInvoiceHeader.Schema.JZ_PaymentExRate);
			result.Add(JobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill);
			result.Add(JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice);
			return result.ToArray();
		}

		void ModifyColumnProperties()
		{
			var supplierColumn = this.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OH_Supplier);
			supplierColumn.GroupName = AUSupplierHeaderUserControl.SupplierGroupName;
		}

		void InitializeJobComInvoiceHeadersBoundGrid()
		{
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new ZGuidDropEditColumnStyleInfo();
			zGuidDropEditColumnStyleInfo1.ColumnName = JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress;
			zGuidDropEditColumnStyleInfo1.IsVisible = true;
			zGuidDropEditColumnStyleInfo1.GroupName = AUSupplierHeaderUserControl.SupplierGroupName;
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
		}

		void InitializeBaseGroupChargesGrid()
		{
			using (BaseGroupChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var isCalculatedColumn = new ZCheckBoxColumnStyleInfo
				{
					ColumnName = JobComInvHeaderChargeSchema.Constants.J7_IsCalculated,
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(70)
				};
				BaseGroupChargesGrid.ColumnStyles.Add(isCalculatedColumn);
			}
		}

		#endregion

		#region Change Control Visibility

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			IncoTermTextBox.Visible = false;

			GrossWeightCalcDropEdit.Visible = false;
			NetWeightCalcDropEdit.Visible = false;
			NoOfPacksCalcDropEdit.Visible = false;
			LineTotalBoundConvertToLocalCurrencyControl.Visible = false;
			JZ_FOBAmountBoundCurrencyControl.Visible = false;
			JZ_CIFAmountBoundCurrencyControl.Visible = false;
			JZ_Calc_TNIBoundInvoiceCurrencyControl.Visible = false;
			GroupInvoiceDropEdit.Visible = false;
			JZ_InvoiceCurrLandedCostExRateCalcEdit.Visible = false;
		}

		void ApportionedTabPage_BindingOrFirstShown(object sender, EventArgs e)
		{
			ApportionedChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, "Add to FOB?");
			ApportionedChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsGSTApplicable, "Add to CIF?");
		}

		#endregion
	}
}
