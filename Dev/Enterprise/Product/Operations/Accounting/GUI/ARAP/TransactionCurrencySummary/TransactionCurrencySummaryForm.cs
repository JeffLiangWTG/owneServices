using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class TransactionCurrencySummaryForm : ZChildForm
	{
		public TransactionCurrencySummaryForm(TransactionCurrencySummary summary) : base(summary)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton, null);
		}

		public override string FormVerb => string.Empty;

		ZDisplayGrid CurrencySummaryGrid;
		Core.Forms.ZPostOrCancelButton CloseButton;
		ZDateEdit EarliestDueDateEdit;
		ZDateEdit LatestDueDteDateEdit;
		ZCalcFindBox TotalAmountCalcFindBox;
		ZCalcFindBox TotalOutStandingCalcFindBox;
		ZArchitecture.ZCalcEdit TotalCountCalcEdit;
		ZPanel LineSummaryGridPanel;
		ZPanel zPanel1;
		readonly System.ComponentModel.Container components;
	}
}
