using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoUnderbondAndPackingUserControlForCMR : SeaCargoUnderbondAndPackingUserControl
	{
		public SeaCargoUnderbondAndPackingUserControlForCMR()
		{
			InitializeComponent();
			detailsTabControl.TabPages.Remove(underbondMovementTabPage);
			underbondMovementTabPage.Dispose();
			CV_FlammableBoundCheckBox.Visible = false;
		}

		private void SACCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if (SACCheckBox.Focused && SACCheckBox.Checked &&
				SACCheckBox.DataBindings["Checked"].BindingManagerBase.Position >= 0)
			{
				var sCAPivot = SACCheckBox.DataBindings["Checked"].BindingManagerBase.GetCurrent() as CusSCAPivot;

				if (sCAPivot != null)
				{
#if DEBUG
					if (Globals.IsTest && !ShowSACForm)
					{
						return;
					}
#endif
					ZFormModaliser.ShowDialogAndDispose(new CMRSACDialogBox(new SACDialogBizo(sCAPivot)));
				}
			}
		}

#if DEBUG
		public bool ShowSACForm;
#endif

	}
}
