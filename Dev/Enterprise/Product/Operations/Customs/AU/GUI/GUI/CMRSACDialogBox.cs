using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CMRSACDialogBox : ZChildForm
	{
		public CMRSACDialogBox(SACDialogBizo sACBizo) : base(sACBizo)
		{
		}

		public override string FormHeading
		{
			get { return "Self Assessed Clearance"; }
		}

		private void OKButton_Click(object sender, System.EventArgs e)
		{
			((SACDialogBizo)BusinessEntity).SetSACFlag(true);
			Close();
		}

		private void SACCancelButton_Click(object sender, System.EventArgs e)
		{
			((SACDialogBizo)BusinessEntity).SetSACFlag(false);
			Close();
		}
	}
}
