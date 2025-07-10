namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoStringTest : TestCaseWithDummy
	{
		public void TestTypedValue()
		{
			ZPropertyInfoString info = (ZPropertyInfoString)Dummy.Z0_DescriptionInfo;
			info.Value = "HELLO";
			AssertEquals("HELLO", info.Value);
			info.Value = "GOODBYE";
			AssertEquals("GOODBYE", info.Value);
		}

		public void TestSetValueFromString()
		{
			Dummy.Z0_Description = "";

			AssertEquals(true, Dummy.Z0_DescriptionInfo.SetValueFromString("Shorty"));
			AssertEquals("Shorty", Dummy.Z0_Description);

			AssertEquals(false, Dummy.Z0_DescriptionInfo.SetValueFromString("A".PadRight(Dummy.Z0_DescriptionInfo.MaxLength + 1, 'A')));
			AssertEquals("Shorty", Dummy.Z0_Description);
		}
	}
}
