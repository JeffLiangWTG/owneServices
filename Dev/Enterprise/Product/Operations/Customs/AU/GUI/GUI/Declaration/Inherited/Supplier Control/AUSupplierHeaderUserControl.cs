using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUSupplierHeaderUserControl : DeclarationInvoiceHeaderUserControl
	{
		public AUSupplierHeaderUserControl()
		{
			InitializeComponent();
			InitializeJobComInvoiceHeadersBoundGrid();
			InitializeBaseGroupChargesGrid();
		}

		#region Modify Column Properties

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			ModifyColumnProperties();
			ReorderAndChangeInvoiceHeadersGridVisibility();
		}

		void ReorderAndChangeInvoiceHeadersGridVisibility()
		{
			using (JobComInvoiceHeadersBoundGrid.InnerGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				JobComInvoiceHeadersBoundGrid.InnerGrid.ReOrderColumns(InvoiceHeadersGridColumnNamesInSortOrder);
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetAllColumnsVisible(true);
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnVisible(false, [JobComInvoiceHeaderSchema.Constants.JZ_PaymentDate, JobComInvoiceHeader.Schema.SupplierName]);
			}
		}

		void ModifyColumnProperties()
		{
			var supplierColumn = this.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OH_Supplier);
			supplierColumn.GroupName = SupplierGroupName;
		}

		protected string[] InvoiceHeadersGridColumnNamesInSortOrder
		{
			get { return invoiceHeadersGridColumnNamesInSortOrder ?? (invoiceHeadersGridColumnNamesInSortOrder = GetInvoiceHeadersGridColumnOrder()); }
		}
		string[] invoiceHeadersGridColumnNamesInSortOrder;

		protected virtual string[] GetInvoiceHeadersGridColumnOrder()
		{
			return Array.Empty<string>();
		}

		#endregion

		#region Change Control Visibility

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			bool isImport = JobDeclaration.IsImport;
			if (isImport)
			{
				InvoiceChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, "Dutiable");
				BaseGroupChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, "Dutiable");
				ApportionedChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, "Dutiable");
				InvoiceChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsGSTApplicable, "GST Apply");
				BaseGroupChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsGSTApplicable, "GST Apply");
				ApportionedChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsGSTApplicable, "GST Apply");
			}
			else
			{
				InvoiceChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, "Add to FOB?");
				BaseGroupChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, "Add to FOB?");
				ApportionedChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, "Add to FOB?");
				InvoiceChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsGSTApplicable, "Add to CIF?");
				BaseGroupChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsGSTApplicable, "Add to CIF?");
				ApportionedChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsGSTApplicable, "Add to CIF?");
			}

			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(isImport || ((JobDeclaration)JobDeclaration).IsQuarantine, [AUAddInfo.Schema.ZA_ORG, AUAddInfo.Schema.ZA_GSTE]);

			bool isExWarehouse = JobDeclaration.IsExWarehouse;
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(!isExWarehouse,
				[
					JobComInvoiceHeader.Schema.JZ_InvoiceNumber,
					JobComInvoiceHeader.Schema.JZ_OH_Supplier,
					JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress,
					JobComInvoiceHeader.Schema.JZ_IncoTerm,
					JobComInvoiceHeader.Schema.JZ_IncoTermPlace,
					JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate,
					JobComInvoiceHeader.Schema.JZ_Calc_FOBAmount,
					JobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency,
					JobComInvoiceHeader.Schema.JZ_OH_Buyer,
					JobComInvoiceHeader.Schema.JZ_Calc_CIFAmount,
					JobComInvoiceHeader.Schema.JZ_Calc_CIFCurrency,
					JobComInvoiceHeader.Schema.JZ_Volume,
					JobComInvoiceHeader.Schema.JZ_VolumeUQ,
					JobComInvoiceHeader.Schema.JZ_Weight,
					JobComInvoiceHeader.Schema.JZ_WeightUQ,
					JobComInvoiceHeader.Schema.JZ_NetWeight,
					JobComInvoiceHeader.Schema.JZ_NetWeightUQ,
					JobComInvoiceHeader.Schema.JZ_Nature10PackCount,
					JobComInvoiceHeader.Schema.JZ_PaymentAmount,
					JobComInvoiceHeader.Schema.JZ_PaymentDate,
					JobComInvoiceHeader.Schema.JZ_PaymentExRate,
					JobComInvoiceHeader.Schema.JZ_PaymentNo,
					JobComInvoiceHeader.Schema.JZ_ValuationBasis,
					JobComInvoiceHeader.Schema.JZ_InvoiceDate,
					JobComInvoiceHeader.Schema.InvoiceLineTotal,
					JobComInvoiceHeader.Schema.ZA_PermitNumbers_Hidden
				]);
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			JZ_AddInfoBoundAddInfoControl.Visible = JobDeclaration.IsImport;
			InvoiceOriginCodeFindBox.Visible = JobDeclaration.IsImport;

			IncoTermTextBox.GetExtension<ILabelCaptionRenderer>().Caption = JobDeclaration.IsImport ? "ITOT Incoterm" : "Invoice Incoterm";
			IncoTermTextBox.Visible = JobDeclaration.IsExport;

			bool exWarehouseEnabled = JobDeclaration.IsExWarehouse;
			JZ_InvoiceAmountBoundCurrencyControl.GetExtension<ILabelCaptionRenderer>().Caption = !exWarehouseEnabled ? "Inv Total Amount" : "Total Customs Value";
			SetControlState(JZ_InvoiceNumberBoundTextBox, exWarehouseEnabled);
			SetControlState(JZ_InvoiceCurrExRateCalcEdit, exWarehouseEnabled);
			SetControlState(JZ_IncoTermBoundDropDownEdit, exWarehouseEnabled);
			SetControlState(IncoTermExplainButton, exWarehouseEnabled);
			SetControlState(JZ_IncoTermPlaceTextBox, exWarehouseEnabled);
			SetControlState(GrossWeightCalcDropEdit, exWarehouseEnabled);
			SetControlState(NetWeightCalcDropEdit, exWarehouseEnabled);
			SetControlState(NoOfPacksCalcDropEdit, exWarehouseEnabled);
			SetControlState(ITOTIncoTermTextBox, exWarehouseEnabled || !JobDeclaration.IsImport);
			SetControlState(LineTotalBoundConvertToLocalCurrencyControl, exWarehouseEnabled);
			SetControlState(JZ_FOBAmountBoundCurrencyControl, exWarehouseEnabled);
			SetControlState(JZ_CIFAmountBoundCurrencyControl, exWarehouseEnabled);
			SetControlState(JZ_Calc_TNIBoundInvoiceCurrencyControl, exWarehouseEnabled);

			SetControlState(ChargesGroupBox, exWarehouseEnabled);
			SetControlState(BaseGroupChargesGroupBox, exWarehouseEnabled);
		}

		void SetControlState(Control control, bool exWarehouseActive)
		{
			control.Visible = !exWarehouseActive;
		}

		void ApportionedTabPage_BindingOrFirstShown(object sender, EventArgs e)
		{
			if (JobDeclaration.IsImport)
			{
				ApportionedChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, "Dutiable");
				ApportionedChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsGSTApplicable, "GST Apply");
			}
			else
			{
				ApportionedChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, "Add to FOB?");
				ApportionedChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsGSTApplicable, "Add to CIF?");
			}
		}

		#endregion

		void InitializeJobComInvoiceHeadersBoundGrid()
		{
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new ZGuidDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new ZCodeFindBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			zGuidDropEditColumnStyleInfo1.ColumnName = JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress;
			zGuidDropEditColumnStyleInfo1.IsVisible = true;
			zGuidDropEditColumnStyleInfo1.GroupName = SupplierGroupName;
			zCalcEditColumnStyleInfo14.Caption = "Packs for release";
			zCalcEditColumnStyleInfo14.ColumnName = "JZ_Nature10PackCount";
			zCalcEditColumnStyleInfo14.ToolTip = "Pack count to be released";
			zCodeFindBoxColumnStyleInfo2.BindToList = "ZA_ORG_List";
			zCodeFindBoxColumnStyleInfo2.Caption = "ORG";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ZA_ORG";
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			zCodeFindBoxColumnStyleInfo2.ToolTip = "Origin Ctry/Rgn.";
			zCodeFindBoxColumnStyleInfo3.Caption = "GSTE";
			zCodeFindBoxColumnStyleInfo3.ColumnName = "ZA_GSTE";
			zCodeFindBoxColumnStyleInfo3.IsVisible = false;
			zCodeFindBoxColumnStyleInfo3.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.CMRCodeLists;
			zCodeFindBoxColumnStyleInfo3.ToolTip = "GST Exemption";
			zDateEditColumnStyleInfo3.Caption = "Valuation Date Override";
			zDateEditColumnStyleInfo3.ColumnName = "JZ_ValuationDateOverride";
			zTextBoxColumnStyleInfo2.Caption = "Permit/Encryption";
			zTextBoxColumnStyleInfo2.ColumnName = "AddInfo+ZA_PermitNumbers_Hidden";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
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

		internal static ResourceStringData SupplierGroupName => Res.GetData("AUSupplierHeaderUserControl|ECD1EE0D-8B83-4126-8753-93440D6C8690", "Supplier");
		internal ZTextBox JZ_InvoiceNumberBoundTextBoxInternal => JZ_InvoiceNumberBoundTextBox;
		internal ZCalcEdit JZ_InvoiceCurrExRateCalcEditInternal => JZ_InvoiceCurrExRateCalcEdit;
		internal ZDropEdit JZ_IncoTermBoundDropDownEditInternal => JZ_IncoTermBoundDropDownEdit;
		internal ZButton IncoTermExplainButtonInternal => IncoTermExplainButton;
		internal ZTextBox IncoTermTextBoxInternal => IncoTermTextBox;
		internal ZTextBox JZ_IncoTermPlaceTextBoxInternal => JZ_IncoTermPlaceTextBox;
		internal ZCalcDropEdit GrossWeightCalcDropEditInternal => GrossWeightCalcDropEdit;
		internal ZCalcDropEdit NetWeightCalcDropEditInternal => NetWeightCalcDropEdit;
		internal ConvertToLocalCurrencyControl LineTotalBoundConvertToLocalCurrencyControlInternal => LineTotalBoundConvertToLocalCurrencyControl;
		internal ConvertToLocalCurrencyControl JZ_FOBAmountBoundCurrencyControlInternal => JZ_FOBAmountBoundCurrencyControl;
		internal ConvertToLocalCurrencyControl JZ_CIFAmountBoundCurrencyControlInternal => JZ_CIFAmountBoundCurrencyControl;
		internal ConvertToLocalCurrencyControl JZ_Calc_TNIBoundInvoiceCurrencyControlInternal => JZ_Calc_TNIBoundInvoiceCurrencyControl;
		internal ZGroupBox BaseGroupChargesGroupBoxInternal => BaseGroupChargesGroupBox;
	}
}
