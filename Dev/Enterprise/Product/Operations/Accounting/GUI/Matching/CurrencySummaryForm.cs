using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class CurrencySummaryForm : ZChildForm
	{
		public CurrencySummaryForm(CurrencySummary summary) : base(summary)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton, null);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}

