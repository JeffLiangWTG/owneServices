using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class CusCalculationRuleForm : ZForm
	{
		public CusCalculationRuleForm()
		{
			InitializeComponent();
		}

		public CusCalculationRuleForm(CusCalculationRule rule) : base(rule)
		{
			this.rule = rule;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		public CusCalculationRule rule;

		public override string FormCaption => ResString.GetMultilingualString("86A3610B-FF0F-4018-A09E-D4869324CD97", "Customs Calculation Rule");
	}
}
