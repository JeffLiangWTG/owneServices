namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoBoolTest : TestCaseWithDummy
	{
		public void TestSetValueFromString()
		{
			Dummy.Z0_Bool = false;

			ZPropertyInfoBool info = (ZPropertyInfoBool)Dummy.Z0_BoolInfo;
			AssertEquals(true, info.SetValueFromString("y"));
			AssertEquals(true, Dummy.Z0_Bool);

			AssertEquals(false, info.SetValueFromString("NO"));
			AssertEquals(true, Dummy.Z0_Bool);

			Dummy.Z0_Bool = true;
			AssertEquals(true, info.SetValueFromString("n"));
			AssertEquals(false, Dummy.Z0_Bool);
		}
	}
}
