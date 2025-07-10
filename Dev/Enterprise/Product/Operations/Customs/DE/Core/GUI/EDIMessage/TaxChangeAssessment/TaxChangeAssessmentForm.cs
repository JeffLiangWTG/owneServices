using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class TaxChangeAssessmentForm : ZTemplateForm
	{
		public TaxChangeAssessmentForm(TaxChangeAssessment taxChangeAssessment) : base(taxChangeAssessment)
		{
			InitializeComponent();
			LogsTabPage.TabVisible = false;
		}

		public override string FormCaption => Res.GetString("85BA2CCC-B0E7-4E53-9698-43BA20D8DF65", "Tax Change Assessment");

		protected override bool AllowNew => false;

		protected override bool ShowNotesTab => false;
	}
}
