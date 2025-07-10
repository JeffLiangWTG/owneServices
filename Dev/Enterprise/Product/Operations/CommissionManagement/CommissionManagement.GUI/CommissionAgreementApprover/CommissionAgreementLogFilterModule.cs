using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public class CommissionAgreementLogFilterModule : ZStmALogModule
	{
		protected override IFilterControl GetNewFilterControl()
		{
			return new CommissionAgreementLogFilterControl(master, GridCollection, FilterBusinessObject, this);
		}
	}
}
