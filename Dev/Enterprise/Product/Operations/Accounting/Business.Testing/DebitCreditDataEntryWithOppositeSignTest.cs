using CargoWise.Types;

namespace Enterprise.Accounting.Business.Testing
{
	public class DebitCreditDataEntryWithOppositeSignTest : DebitCreditDataEntryTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			DataEntry = new DebitCreditDataEntry(() => Bizo.Z0_Decimal, x => Bizo.Z0_Decimal = x, false);
		}

		protected override void AssertDebitCreditSign(ZString expected)
		{
			base.AssertDebitCreditSign(expected == DebitCreditDataEntry.DR ? DebitCreditDataEntry.CR : expected == DebitCreditDataEntry.CR ? DebitCreditDataEntry.DR : expected);
		}

		protected override void AssertUnderlyingAmount(ZDecimal expected)
		{
			base.AssertUnderlyingAmount(expected * -1);
		}
	}
}
