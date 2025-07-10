using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class APEnquiryFilterControl : EnquiryFilterControl
	{
		protected APEnquiryFilterControl()
		{
			InitializeComponent();
		}

		public APEnquiryFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject filterBizO)
		: base(collection, filterBizO)
		{
			InitializeComponent();
			this.FilteredGrid.ColumnLayoutContext = nameof(FilteredGridColumnLayoutContext.AP);
			RemoveWHTColumns();

			AverageDaysToFullyPayGroupBox.AllowOutsideOfParent();
		}

		void RemoveWHTColumns()
		{
			if (!GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled())
			{
				RemoveColumnFromGrid(FilteredGrid, FilteredGrid.GetColumnStyle(nameof(TransactionHeader.AH_NotionalWHTTax)));
				RemoveColumnFromGrid(FilteredGrid, FilteredGrid.GetColumnStyle(nameof(TransactionHeader.AH_RealizedWHTTax)));
			}
		}

		void RemoveColumnFromGrid(ZDisplayGrid grid, ZGridColumnInfo columnInfo)
		{
			if (columnInfo != null)
			{
				grid.ColumnStyles.Remove(columnInfo);
			}
		}
	}
}
