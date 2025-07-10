using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public sealed class ChargeToSearchInfoTest : TestCase
	{
		public void TestConstructor()
		{
			var chargePK = ZGuid.NewZGuid();
			var jobHeaderPK = ZGuid.NewZGuid();
			var chargeCodePK = ZGuid.NewZGuid();
			var branchPK = ZGuid.NewZGuid();
			var departmentPK = ZGuid.NewZGuid();
			var chargeToSearchInfo = new ChargeToSearchInfo(chargePK, jobHeaderPK, chargeCodePK, branchPK, departmentPK);

			AssertEquals(chargePK, chargeToSearchInfo.JR_PK);
			AssertEquals(jobHeaderPK, chargeToSearchInfo.JR_JH);
			AssertEquals(chargeCodePK, chargeToSearchInfo.JR_AC);
			AssertEquals(branchPK, chargeToSearchInfo.JR_GB);
			AssertEquals(departmentPK, chargeToSearchInfo.JR_GE);
		}
	}
}
