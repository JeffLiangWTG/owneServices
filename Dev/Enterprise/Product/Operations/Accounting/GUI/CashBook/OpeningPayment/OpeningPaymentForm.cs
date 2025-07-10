using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class OpeningPaymentForm : AccountingZForm
	{
		public OpeningPaymentForm(OpeningPayment openingPay) : base(openingPay)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, oPostingButtonsUserControl1);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			DisplayModeChanged += OpeningPaymentForm_DisplayModeChanged;
		}

		void OpeningPaymentForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			DocManagerReadOnlyOverrideHelper.TrySetReadOnlyOverride(BusinessEntity, e.ToMode);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool ShowAuditTab => true;

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

