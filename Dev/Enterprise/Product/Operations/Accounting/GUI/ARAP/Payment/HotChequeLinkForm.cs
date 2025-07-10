using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class HotChequeLinkForm : ZChildForm, IDoDisplayModeBrowseOverride
	{
		public HotChequeLinkForm(HotChequeLink hotChequeLinkBizO) : base(hotChequeLinkBizO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, SelectButton, CloseButton);
		}

		public HotChequeLink HotChequeLink
		{
			get { return BusinessEntity as HotChequeLink; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return Res.GetString("HotChequeLinkForm|DC9E8432-B678-4370-861C-5FC374356CFA", "Select Hot Check"); }
		}

		public AccHotCheque SelectedHotCheque
		{
			get { return fSelectedHotCheque; }
		}

		protected override void ShowNewForm()
		{
			// do nothing
		}

		#region Implementation

		void IDoDisplayModeBrowseOverride.DoDisplayModeBrowse()
		{
			ZFormStrategy.DoDisplayModeBrowse(this);
			SelectButton.Enabled = true;
			SelectButton.ReadOnly = false;
			SelectButton.Text = Res.GetString("HotChequeLinkForm|8a6e00a1-72d8-4579-932a-2eb55fc7082a", "&Select");
		}

		protected void HandleDoubleClick(object sender, System.EventArgs e)
		{
			SetSelectedHotChequeAndClose();
		}

		void SelectButton_Click(object sender, System.EventArgs e)
		{
			SetSelectedHotChequeAndClose();
		}

		void HotChequeLinkForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				SetSelectedHotChequeAndClose();
			}
		}

		void SetSelectedHotChequeAndClose()
		{
			if (HotChequesGrid != null && HotChequesGrid.ListManager != null)
			{
				fSelectedHotCheque = HotChequesGrid.ListManager.GetCurrent() as AccHotCheque;
			}
			if (fSelectedHotCheque != null)
			{
				Close();
			}
		}

		AccHotCheque fSelectedHotCheque;

		#endregion
	}
}

