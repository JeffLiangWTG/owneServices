using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment
{
	public partial class ApportionmentTemplateForm : ZForm
	{
		public ApportionmentTemplateForm(AccApportionmentTemplate businessEntity)
			: base(businessEntity)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
		}
	}
}
