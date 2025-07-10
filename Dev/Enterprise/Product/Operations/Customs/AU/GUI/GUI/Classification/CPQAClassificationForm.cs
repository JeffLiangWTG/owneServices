using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAClassificationForm : ZChildForm
	{
		public CPQAClassificationForm(Classification classification) : base(classification)
		{
			this.classification = classification;
		}
		readonly Classification classification;

		public override string FormCaption
		{
			get
			{
				return "CMR Default CP Dec Questions for Classification Lookup";
			}
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void CMRRefreshCPDecQuestionsButton_Click(object sender, EventArgs e)
		{
			if (classification != null)
			{
				classification.RefreshQuestions();
				Refresh();
			}
		}
	}
}
