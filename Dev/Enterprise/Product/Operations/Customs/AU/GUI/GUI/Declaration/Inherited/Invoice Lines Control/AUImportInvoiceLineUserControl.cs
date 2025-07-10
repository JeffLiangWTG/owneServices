using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUImportInvoiceLineUserControl : AUInvoiceLineUserControl
	{
		public AUImportInvoiceLineUserControl()
		{
			InitializeComponent();
			InitialiseTariffFindBox();
			SetDynamicControlStates();
		}

		protected override bool UseUniversalTariff => AUCClassWrapper.UseCustomsReferenceData;
		public bool UseCMRTariffTestData => AUCClassWrapper.UseCMRTariffTestData;
		protected override ZString UniversalTariffType => Universal.Constants.TariffTypes.Import;
		protected override ModuleIdentifier ClassificationModuleID => ModuleIDs.ImportClassification;
		protected override string GetCustomsCountryCode() => UseCMRTariffTestData ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;
		protected override ZString GetDataGroupingForUniversalTariff() => UseCMRTariffTestData ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				SetDynamicControlStates();
			}
		}

		public const string AddInfoPrefix = JobComInvoiceLine.Schema.AddInfo + "+";

		string[] CustomsInvoiceLinesBoundGridColumnNamesInSortOrder
		{
			get { return customsInvoiceLinesBoundGridColumnNamesInSortOrder ?? (customsInvoiceLinesBoundGridColumnNamesInSortOrder = GetInvoiceLinesGridColumnOrder()); }
		}
		string[] customsInvoiceLinesBoundGridColumnNamesInSortOrder;

		protected virtual string[] GetInvoiceLinesGridColumnOrder()
		{
			List<string> result = new List<string>();
			result.AddRange(DefaultColumnsForGrid);
			result.Add(JobComInvoiceLineSchema.Constants.JI_Weight);
			result.Add(JobComInvoiceLineSchema.Constants.JI_WeightUQ);
			result.Add(JobComInvoiceLineSchema.Constants.JI_Volume);
			result.Add(JobComInvoiceLineSchema.Constants.JI_VolumeUQ);
			result.Add(JobComInvoiceLineSchema.Constants.JI_OrderNumber);
			result.Add(JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine);
			result.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib1);
			result.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib2);
			result.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib3);
			result.Add(JobComInvoiceLineSchema.Constants.JI_SerialNumber);
			result.Add(JobComInvoiceLine.Schema.UnitPrice);
			result.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib1);
			result.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib2);
			result.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib3);
			result.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib4);
			result.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib5);
			result.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib6);
			result.Add(JobComInvoiceLineSchema.Constants.JI_CustomTextBlob1);
			result.Add(AddInfoPrefix + AUAddInfo.Schema.ZA_ValuationBasis_Hidden);
			result.Add(AddInfoPrefix + AUAddInfo.Schema.ZA_TreatmentCode_Hidden);
			result.Add(JobComInvoiceLine.Schema.MergedLineNumber);
			result.Add(AddInfoPrefix + AUAddInfo.Schema.AdjustmentAmount_Hidden);
			result.Add(AddInfoPrefix + AUAddInfo.Schema.AdjustmentDollarPercentage_Hidden);
			result.Add(AddInfoPrefix + AUAddInfo.Schema.AdjustmentCurrency_Hidden);
			AddToListIfNotExists(result, AddInfoPrefix + AUAddInfo.Schema.ZA_WRN);
			AddToListIfNotExists(result, AddInfoPrefix + AUAddInfo.Schema.ZA_WRL);
			result.Add(JobComInvoiceLineSchema.Constants.JI_RH_NKCommodity_Code);
			return result.ToArray();
		}

		protected void AddToListIfNotExists(List<string> list, string newElement)
		{
			if (!list.Contains(newElement))
			{
				list.Add(newElement);
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			ReorderAndChangeInvoiceLinesGridVisibility();
		}

		void ReorderAndChangeInvoiceLinesGridVisibility()
		{
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(CustomsInvoiceLinesBoundGridColumnNamesInSortOrder);
				CustomsInvoiceLinesBoundGrid.SetAllColumnsVisible(false);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, DefaultColumnsForGrid.ToArray());
			}
		}

		protected List<string> DefaultColumnsForGrid
		{
			get
			{
				if (defaultColumnsForGrid == null)
				{
					defaultColumnsForGrid = new List<string>();

					defaultColumnsForGrid.Add(JobComInvoiceLineSchema.Constants.JI_LineNo);
					defaultColumnsForGrid.Add(BaseJobComInvoiceLine.Schema.JI_Calc_Invoice);
					defaultColumnsForGrid.Add(JobComInvoiceLineSchema.Constants.JI_PartNo);
					defaultColumnsForGrid.Add(JobComInvoiceLineSchema.Constants.JI_CC);
					defaultColumnsForGrid.Add(JobComInvoiceLineSchema.Constants.JI_Tariff);
					defaultColumnsForGrid.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity);
					defaultColumnsForGrid.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ);
					defaultColumnsForGrid.Add(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity);
					defaultColumnsForGrid.Add(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty);
					defaultColumnsForGrid.Add(JobComInvoiceLineSchema.Constants.JI_LinePrice);
					defaultColumnsForGrid.Add(JobComInvoiceLineSchema.Constants.JI_Description);
					defaultColumnsForGrid.Add(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin);
					defaultColumnsForGrid.Add(JobComInvoiceLine.Schema.JI_IsPackToBondForLine);

					defaultColumnsForGrid.AddRange(GetDefaultColumnsForGrid());
				}
				return defaultColumnsForGrid;
			}
		}
		List<string> defaultColumnsForGrid;

		protected virtual string[] GetDefaultColumnsForGrid()
		{
			return System.Array.Empty<string>();
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			CustomsInvoiceLinesBoundGrid.SetAvailability(!JobDeclaration.IsExWarehouse, JobComInvoiceLine.Schema.JI_IsPackToBondForLine);
		}

		#region Implementation

		void InitialiseTariffFindBox()
		{
			if (UseUniversalTariff)
			{
				tariffFindBox.GetEffectiveDate = GetEffectiveAssessmentDateForUniversalTariff;
				tariffFindBox.Visible = true;
				tariffFindBox.GetCountryCode = GetCustomsCountryCode;
				tariffFindBox.GetDataGrouping = GetDataGroupingForUniversalTariff;
			}
			else
			{
				tariffFindBoxAUCClass.Visible = true;

				if (!DesignMode)
				{
					var tariffColumnStyle = new AUCClassColumnStyleInfo()
					{
						ColumnName = JobComInvoiceLineSchema.Constants.JI_Tariff,
						Caption = "Tariff",
						ToolTip = "Tariff",
					};

					CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(tariffColumnStyle);
				}
			}
		}

		void AdjustmentDollarPercentageBoundDropEdit_Leave(object sender, System.EventArgs e)
		{
			SetDynamicControlStates();
		}

		protected virtual void SetDynamicControlStates()
		{
			currencyAmountVisible = !string.IsNullOrEmpty(AdjustmentDollarPercentageBoundDropEdit.Text);
			currencyVisible = AdjustmentDollarPercentageBoundDropEdit.Text == "$";
			AdjustmentCurrencyBoundFindBox.Visible = currencyAmountVisible && currencyVisible;
			AdjustmentAmountBoundCalcEdit.Visible = currencyAmountVisible;
		}

		protected bool currencyAmountVisible;
		protected bool currencyVisible;

		#endregion
	}
}
