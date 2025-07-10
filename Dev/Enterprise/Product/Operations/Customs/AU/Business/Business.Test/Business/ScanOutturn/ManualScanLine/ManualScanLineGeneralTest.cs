using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ManualScanLineGeneralTest : TestCaseWithFactory
	{
		public void TestMaxLengthInManualScanLineIsTheSameAsInCusHAWB()
		{
			var line = new ManualScanLine();
			AssertEquals(Math.Max(CusHAWB.Schema.CS_HAWBMaxLength, CusSCAHouse.Schema.CA_HouseBillMaxLength), line.BarcodeInfo.MaxLength);
		}
	}
}
