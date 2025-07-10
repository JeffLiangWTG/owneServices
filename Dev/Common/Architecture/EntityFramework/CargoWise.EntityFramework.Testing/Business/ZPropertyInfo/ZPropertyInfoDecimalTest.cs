namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoDecimalTest : TestCaseWithDummy
	{
		public void TestTypedValue()
		{
			ZPropertyInfoDecimal info = (ZPropertyInfoDecimal)Dummy.Z0_DecimalInfo;
			info.Value = 1m;
			AssertEquals(1m, info.Value);
			info.Value = 2m;
			AssertEquals(2m, info.Value);
		}

		public void TestSetValueFromString()
		{
			Dummy.Z0_AnotherDecimal = 0m;

			AssertEquals(true, Dummy.Z0_AnotherDecimalInfo.SetValueFromString("-8954712.254"));
			AssertEquals(-8954712.254m, Dummy.Z0_AnotherDecimal);

			AssertEquals(false, Dummy.Z0_AnotherDecimalInfo.SetValueFromString("Invalid"));
			AssertEquals(-8954712.254m, Dummy.Z0_AnotherDecimal);
		}
	}
}
