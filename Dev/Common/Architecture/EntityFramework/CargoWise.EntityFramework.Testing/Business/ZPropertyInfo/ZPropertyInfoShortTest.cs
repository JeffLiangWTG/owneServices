namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoShortTest : TestCaseWithDummy
	{
		public void TestSetValueFromString()
		{
			Dummy.Z0_Short = 0;

			AssertEquals(true, Dummy.Z0_ShortInfo.SetValueFromString("-584"));
			AssertEquals((short)-584, Dummy.Z0_Short);

			AssertEquals(false, Dummy.Z0_ShortInfo.SetValueFromString("65536"));
			AssertEquals((short)-584, Dummy.Z0_Short);
		}
	}
}
