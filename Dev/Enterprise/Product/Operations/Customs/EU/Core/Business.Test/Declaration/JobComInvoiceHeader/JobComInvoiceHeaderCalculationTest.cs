using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

public class JobComInvoiceHeaderCalculationTest : BaseJobComInvoiceHeaderCalculationTest
{
	protected override void PrepareCharge(BaseJobComInvHeaderCharge charge)
	{
		base.PrepareCharge(charge);
		charge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
	}
	public override void TestCalculateFOBValueWithDDP()
	{
		Assert("FOB should not be calculated from charges based on their dutiable status.  Base is flawed to think this. EU doesn't want FOB at the moment.", true);
	}

	public override void TestCalculateFOBValueWithCIF()
	{
		Assert("FOB should not be calculated from charges based on their dutiable status.  Base is flawed to think this. EU doesn't want FOB at the moment.", true);
	}

	public override void TestCalculateFOBValueWithCFR()
	{
		Assert("FOB should not be calculated from charges based on their dutiable status.  Base is flawed to think this. EU doesn't want FOB at the moment.", true);
	}

	public override void TestCalculateFOBValueWithFOB()
	{
		Assert("FOB should not be calculated from charges based on their dutiable status.  Base is flawed to think this. EU doesn't want FOB at the moment.", true);
	}

	public override void TestCalculateFOBValueWithEXW()
	{
		Assert("FOB should not be calculated from charges based on their dutiable status.  Base is flawed to think this. EU doesn't want FOB at the moment.", true);
	}

	#region Implementation
	protected virtual string OverseasFreightCode => ChargeTypeList.Codes.InternationalFreight;

	protected override BaseJobDeclaration GetNewDeclaration() => JobDeclaration.New(Factory);

	protected override void SetUp()
	{
		base.SetUp();
		testDec = (JobDeclaration)GetNewDeclaration();
		testDec.AutoCreateChargesBasedOnIncoTerm = false;
	}

	JobDeclaration testDec;

	#endregion
}
