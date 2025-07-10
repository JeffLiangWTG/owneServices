#if DEBUG

using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class PeriodicInvoiceControl
	{
		public void JobsFilterControl_PerformSearch_ForTestOnly(object sender, EventArgs e)
		{
			JobsFilterControl_PerformSearch(sender, e);
		}

		public ZArchitecture.ZGrid JobsGrid_ForTestOnly
		{
			get { return JobsGrid; }
			set { JobsGrid = value; }
		}

		public ZArchitecture.ZLabel JobsInfoLabel_ForTestOnly
		{
			get { return JobsInfoLabel; }
			set { JobsInfoLabel = value; }
		}

		public void JobsFilterControl_ClearButtonClicked_ForTestOnly(object sender, EventArgs e)
		{
			JobsFilterControl_ClearButtonClicked(sender, e);
		}

		public void MiscInvoicesFilterControl_PerformSearch_ForTestOnly(object sender, EventArgs e)
		{
			MiscInvoicesFilterControl_PerformSearch(sender, e);
		}

		public ZArchitecture.ZLabel MiscInvoicesInfoLabel_ForTestOnly
		{
			get { return MiscInvoicesInfoLabel; }
			set { MiscInvoicesInfoLabel = value; }
		}

		public void MiscInvoicesFilterControl_ClearButtonClicked_ForTestOnly(object sender, EventArgs e)
		{
			MiscInvoicesFilterControl_ClearButtonClicked(sender, e);
		}

		public ZCalcFindBox LocalExTaxAmountCalcFindBox_ForTestOnly
		{
			get { return LocalExTaxAmountCalcFindBox; }
			set { LocalExTaxAmountCalcFindBox = value; }
		}

		public ZCalcFindBox LocalTaxAmountCalcFindBox_ForTestOnly
		{
			get { return LocalTaxAmountCalcFindBox; }
			set { LocalTaxAmountCalcFindBox = value; }
		}

		public ZCalcFindBox LocalTotalAmountCalcFindBox_ForTestOnly
		{
			get { return LocalTotalAmountCalcFindBox; }
			set { LocalTotalAmountCalcFindBox = value; }
		}

		public ZCalcFindBox LocalExtraTaxAmount_ForTestOnly
		{
			get { return LocalExtraTaxAmount; }
			set { LocalExtraTaxAmount = value; }
		}

		public ZCalcFindBox OSExTaxAmountCalcFindBox_ForTestOnly
		{
			get { return OSExTaxAmountCalcFindBox; }
			set { OSExTaxAmountCalcFindBox = value; }
		}

		public ZCalcFindBox OSExtraTaxAmount_ForTestOnly
		{
			get { return OSExtraTaxAmount; }
			set { OSExtraTaxAmount = value; }
		}

		public ZCalcFindBox OSTotalAmountCalcFindBox_ForTestOnly
		{
			get { return OSTotalAmountCalcFindBox; }
			set { OSTotalAmountCalcFindBox = value; }
		}

		public ZCalcFindBox OSTaxAmountCalcFindBox_ForTestOnly
		{
			get { return OSTaxAmountCalcFindBox; }
			set { OSTaxAmountCalcFindBox = value; }
		}
	}
}

#endif
