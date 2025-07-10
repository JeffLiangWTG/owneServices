using System.Drawing;
using CargoWise.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class DocumentConditionHeaderControl : ZUserControl
	{
		public DocumentConditionHeaderControl(GuidedDecisionMakingCondition condition)
		{
			InitializeComponent();
			if (!this.IsDesignMode())
			{
				SetDataBinding(condition, "");
			}

			IsSatisfiedResultLabel.TextChanged += (s, e) =>
			{
				IsSatisfiedResultLabel.ForeColor = condition.IsInformationCondition ? Color.Black : condition.IsSatisfied ? Color.Green : Color.Red;
			};
		}
	}
}
