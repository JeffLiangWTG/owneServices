using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAProductForm : ZChildForm
	{
		public CPQAProductForm(CusClassPartPivot pivot) : base(pivot)
		{
			this.pivot = pivot;
		}
		readonly CusClassPartPivot pivot;

		public override string FormCaption
		{
			get
			{
				return "CMR Default CP Dec Questions for Classification-Part Pivot";
			}
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void CMRRefreshCPDecQuestionsButton_Click(object sender, EventArgs e)
		{
			if (pivot != null)
			{
				pivot.RefreshQuestions();
				Refresh();
			}
		}
	}
}
