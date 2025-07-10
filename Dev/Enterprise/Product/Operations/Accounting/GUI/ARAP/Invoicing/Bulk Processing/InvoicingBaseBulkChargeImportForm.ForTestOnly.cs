#if DEBUG

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class InvoicingBaseBulkChargeImportForm
	{
		public ZArchitecture.GUI.ZButton DeselectAllButton_ForTestOnly
		{
			get { return DeselectAllButton; }
			set { DeselectAllButton = value; }
		}

		public ZArchitecture.GUI.ZButton SelectAllButton_ForTestOnly
		{
			get { return SelectAllButton; }
			set { SelectAllButton = value; }
		}

		public ZArchitecture.ZGrid ChargesGrid_ForTestOnly
		{
			get { return ChargesGrid; }
			set { ChargesGrid = value; }
		}

		public ZArchitecture.ZGrid JobsGrid_ForTestOnly
		{
			get { return JobsGrid; }
			set { JobsGrid = value; }
		}
	}
}

#endif
