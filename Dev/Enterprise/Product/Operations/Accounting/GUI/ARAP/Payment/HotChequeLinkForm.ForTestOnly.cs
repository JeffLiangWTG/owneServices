#if DEBUG

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class HotChequeLinkForm
	{
		public ZArchitecture.GUI.ZDisplayGrid HotChequesGrid_ForTestOnly
		{
			get { return HotChequesGrid; }
			set { HotChequesGrid = value; }
		}

		public void HandleDoubleClick_ForTestOnly(object sender, System.EventArgs e)
		{
			HandleDoubleClick(sender, e);
		}

		public void SelectButton_Click_ForTestOnly(object sender, System.EventArgs e)
		{
			SelectButton_Click(sender, e);
		}

		public Business.ARAP.HotCheque.AccHotCheque FSelectedHotCheque_ForTestOnly
		{
			get { return fSelectedHotCheque; }
			set { fSelectedHotCheque = value; }
		}

		public Core.Forms.ZPostOrCancelButton SelectButton_ForTestOnly
		{
			get { return SelectButton; }
			set { SelectButton = value; }
		}
	}
}

#endif
