using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	sealed class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
	{
		public void TestRoundChangeAmount()
		{
			var entryLineFee = Factory.New<CusEntryLineFee>();

			entryLineFee.CF_ChargeAmount = 1.3542m;
			AssertEquals("ChargeAmount", 1.35m, entryLineFee.CF_ChargeAmount);

			entryLineFee.CF_ChargeAmount = 1.3555;
			AssertEquals("ChargeAmount", 1.36m, entryLineFee.CF_ChargeAmount);
		}
	}
}
