using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AirOutturnLine))]
	sealed class AirOutturnLineTest : OutturnLineTest
	{
		public void TestMaxLengthInOutturnLineIsTheSameAsInCusHAWB()
		{
			var line = new AirOutturnLine();
			AssertEquals(CusHAWB.Schema.CS_HAWBMaxLength, line.ConsignmentRefInfo.MaxLength);
		}

		protected override IScanHouseBillProvider GetNewHouseBill() => Factory.New<CusHAWB>();
	}
}
