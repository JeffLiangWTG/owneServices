using System;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public partial class CashBookFilterControl : ZFilterStripControl
	{
		public CashBookFilterControl()
		{
			InitializeComponent();
		}

		public CashBookFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			RemovePlaceOfSupplyColumn();

			for (int i = 0; i < FilteredGrid.ColumnStyles.Count; i++)
			{
				ZGridColumnInfo column = (ZGridColumnInfo)FilteredGrid.ColumnStyles[i];
				if (column.ColumnName == AccTransactionHeaderSchema.AH_ExchangeRate.Name)
				{
					ZCalcEditColumnStyleInfo exRateColumn = (ZCalcEditColumnStyleInfo)column;
					exRateColumn.Decimals = GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			FilteredGrid.SetAvailability(AccountingMasterFilesUtils.IsTaxBranchApplicable, AccTransactionHeaderSchema.Constants.AH_GB_TaxBranch);
		}

		void RemovePlaceOfSupplyColumn()
		{
			if (!PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
			{
				var column = FilteredGrid.GetColumnStyle(AccTransactionHeaderSchema.AH_PlaceOfSupply.Name);
				if (column != null)
				{
					grid.ColumnStyles.Remove(column);
				}
			}
		}
	}
}
