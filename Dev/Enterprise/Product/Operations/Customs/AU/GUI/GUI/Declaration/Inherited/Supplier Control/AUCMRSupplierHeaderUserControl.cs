using System.Collections.Generic;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUCMRSupplierHeaderUserControl : AUSupplierHeaderUserControl
	{
		public AUCMRSupplierHeaderUserControl()
		{
			InitializeComponent();
			AddExtraGridColumn();
			UpdateComponentProperty();
		}

		void UpdateComponentProperty()
		{
			var premisesIdColumn = this.aQISPremisesIdProcessingTypeGrid.GetColumnStyle(nameof(AQISPremisesIdAndProcessingType.PremisesId)) as ZCodeFindBoxColumnStyleInfo;
			premisesIdColumn.ModuleID = CMRReferenceDataHelper.UseReferenceData ? Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList : Enterprise.ZArchitecture.Modules.ModuleIDs.Premises;

			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutgo14qunWIFHdj00dMR9xAw==";

			ApportionmentPendingLabel.AllowOverlap(overrideFOBCheckBox);
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			zA_GSTECodeFindBox.Visible = JobDeclaration.IsImport;
			ZA_VALB_HiddenDropEdit.Visible = !JobDeclaration.IsExWarehouse && JobDeclaration.IsImport;
			zA_HeaderREL_HiddenDropEdit.Visible = !JobDeclaration.IsExWarehouse && JobDeclaration.IsImport;
			AQISTabHiddenLabel.Visible = JobDeclaration.IsExWarehouse;
			aQISDetailsPanel.Visible = !JobDeclaration.IsExWarehouse;
			overrideFOBCheckBox.Visible = !JobDeclaration.IsExWarehouse && JobDeclaration.IsImportCMR;
			addInfoPermitNumberBoundTextBox.Visible = !JobDeclaration.IsExWarehouse;
		}

		#region Column Ordering

		protected override string[] GetInvoiceHeadersGridColumnOrder()
		{
			List<string> result = new List<string>();

			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_InvoiceNumber);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_OH_Supplier);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_OA_SupplierAddress);
			result.Add(JobComInvoiceHeader.Schema.SupplierName);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_IncoTermPlace);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency);
			result.Add(JobComInvoiceHeader.Schema.InvoiceLineTotal);
			result.Add(JobComInvoiceHeader.Schema.JZ_Calc_BalanceString);
			result.Add(JobComInvoiceHeader.Schema.JZ_Nature10PackCount);
			result.Add(JobComInvoiceHeader.Schema.ZA_ORG);
			result.Add("AddInfo+ZA_POC");
			result.Add("AddInfo+ZA_PST");
			result.Add("AddInfo+ZA_PRT");
			result.Add("AddInfo+ZA_VALB_Hidden");
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRate);
			result.Add(JobComInvoiceHeader.Schema.JZ_Calc_FOBAmount);
			result.Add(JobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency);
			result.Add(JobComInvoiceHeader.Schema.JZ_Calc_CIFAmount);
			result.Add(JobComInvoiceHeader.Schema.JZ_Calc_CIFCurrency);
			result.Add(JobComInvoiceHeader.Schema.ZA_GSTE);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_PaymentNo);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_PaymentAmount);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_PaymentDate);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_PaymentExRate);
			result.Add("AddInfo+ZA_PermitNumbers_Hidden");
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_Volume);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_VolumeUQ);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_Weight);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_WeightUQ);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_NetWeight);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_NetWeightUQ);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_CU_RelatedHouseBill);
			result.Add(JobComInvoiceHeaderSchema.Constants.JZ_ValuationDateOverride);

			return result.ToArray();
		}

		#endregion

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnCaption(JobComInvoiceHeader.Schema.JZ_Nature10PackCount, "No. of Packages");
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();

			using (JobComInvoiceHeadersBoundGrid.InnerGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				bool isExWarehouse = JobDeclaration.IsExWarehouse;
				bool isSAC = JobDeclaration.IsSAC;

				JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns(JobComInvoiceHeaderSchema.Constants.JZ_OH_Buyer);
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(!isExWarehouse,
					[
						JobComInvoiceHeaderSchema.Constants.JZ_InvoiceNumber,
						JobComInvoiceHeaderSchema.Constants.JZ_OH_Supplier,
						JobComInvoiceHeaderSchema.Constants.JZ_OA_SupplierAddress,
						JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm,
						JobComInvoiceHeaderSchema.Constants.JZ_IncoTermPlace,
						JobComInvoiceHeader.Schema.InvoiceLineTotal,
						"AddInfo+ZA_POC",
						"AddInfo+ZA_PST",
						"AddInfo+ZA_PRT",
						"AddInfo+ZA_VALB_Hidden",
						JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRate,
						JobComInvoiceHeader.Schema.JZ_Calc_FOBAmount,
						JobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency,
						JobComInvoiceHeader.Schema.JZ_Calc_CIFAmount,
						JobComInvoiceHeader.Schema.JZ_Calc_CIFCurrency,
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
						JobComInvoiceHeaderSchema.Constants.JZ_NetWeightUQ
					]);

				JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(!isSAC, JobComInvoiceHeader.Schema.JZ_Calc_BalanceString);

				JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(ShouldShowNature10PackCount, JobComInvoiceHeader.Schema.JZ_Nature10PackCount);
			}
		}

		void AddExtraGridColumn()
		{
			// 
			// JobComInvoiceHeadersBoundGrid
			//
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			zCodeFindBoxColumnStyleInfo2.BindToList = "AddInfo+Lookups+ZA_POC_List";
			zCodeFindBoxColumnStyleInfo2.Caption = "Pref. Origin";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "AddInfo+ZA_POC";
			zCodeFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|1bb9d076-289c-4c69-8eba-df7ab80f2f55", "Preference");
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.BindToList = "AddInfo+Lookups+ZA_PST_List";
			zDropEditColumnStyleInfo3.Caption = "Pref. Scheme";
			zDropEditColumnStyleInfo3.ColumnName = "AddInfo+ZA_PST";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|1bb9d076-289c-4c69-8eba-df7ab80f2f55", "Preference");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.BindToList = "AddInfo+Lookups+ZA_PRT_List";
			zDropEditColumnStyleInfo4.Caption = "Pref. Rule";
			zDropEditColumnStyleInfo4.ColumnName = "AddInfo+ZA_PRT";
			zDropEditColumnStyleInfo4.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRSupplierHeaderUserControl|1bb9d076-289c-4c69-8eba-df7ab80f2f55", "Preference");
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.BindToList = "AddInfo+Lookups+ValuationBasisListForCMR";
			zDropEditColumnStyleInfo5.Caption = "Valuation Basis";
			zDropEditColumnStyleInfo5.ColumnName = "AddInfo+ZA_VALB_Hidden";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
		}

		bool ShouldShowNature10PackCount
		{
			get { return JobDeclaration != null && JobDeclaration.IsTransportModeOther; }
		}

		new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;
	}
}
