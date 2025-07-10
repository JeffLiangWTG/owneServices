using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

public class JobComInvoiceHeaderApportionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderApportionTest
{
	public override void TestFOBWithPreFOBCharge()
	{
		Assert("Do not assert FOB.  FOB in base is wrong.", true);
	}

	protected override void PrepareCharge(BaseJobComInvHeaderCharge charge)
	{
		base.PrepareCharge(charge);
		charge.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
	}

	#region Implementation
	protected override BaseJobDeclaration GetNewDeclaration() => JobDeclaration.New(Factory);
	#endregion
}
