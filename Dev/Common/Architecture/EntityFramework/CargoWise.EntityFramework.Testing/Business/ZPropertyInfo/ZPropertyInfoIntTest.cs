namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoIntTest : TestCaseWithDummy
	{
		public void TestSetValueFromString()
		{
			Dummy.Z0_Number = 0;

			AssertEquals(true, Dummy.Z0_NumberInfo.SetValueFromString("-584"));
			AssertEquals(-584, Dummy.Z0_Number);

			AssertEquals(false, Dummy.Z0_NumberInfo.SetValueFromString("-584.25"));
			AssertEquals(-584, Dummy.Z0_Number);
		}
	}
}
