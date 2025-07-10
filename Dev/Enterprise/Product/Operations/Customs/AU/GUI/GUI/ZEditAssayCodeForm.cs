using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class ZEditAssayCodeForm : ZChildForm
	{
		public ZEditAssayCodeForm(JobComInvoiceLine bizObj) : base(bizObj)
		{
		}

		public override string FormCaption
		{
			get { return "Assay Element Code"; }
		}

		private void OKButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
