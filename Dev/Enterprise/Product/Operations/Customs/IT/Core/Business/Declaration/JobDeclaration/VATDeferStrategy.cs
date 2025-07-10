using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class VATDeferStrategy : EU.Business.Declaration.VATDeferStrategy
{
	public VATDeferStrategy(JobDeclaration declaration) : base(declaration)
	{
	}

	new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override void DefaultDefermentAccountNumber()
	{
		Declaration.JE_DefermentAccountNumber = ZString.Empty;

		var lookups = Declaration.Lookups;
		var paymentMethod = Declaration.JE_PaymentMethod;

		if (lookups.PaymentPartyList.ContainsCode(paymentMethod))
		{
			var defermentApprovalNumberList = lookups.DefermentApprovalNumberList;
			if (defermentApprovalNumberList != null && defermentApprovalNumberList.Count == 1)
			{
				Declaration.JE_DefermentAccountNumber = defermentApprovalNumberList[0].Code;
			}
		}
	}
}
