using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using ResString = Enterprise.Customs.AU.Declaration.GUI.ResString;

namespace Enterprise.Customs.AU.GUI
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

		public override string FormCaption => ResString.GetMultilingualString("EBDDD830-CE1F-4859-B173-4D5BD4800868", "Customs Calculation Rule");
	}
}
