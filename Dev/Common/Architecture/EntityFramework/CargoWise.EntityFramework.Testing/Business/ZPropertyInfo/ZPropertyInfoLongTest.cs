namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoLongTest : TestCaseWithDummy
	{
		public void TestSetValueFromString()
		{
			Dummy.Z0_Long = 0;

			CombineAssertions(() =>
			{
				AssertEquals("-123 parsable?", true, Dummy.Z0_LongInfo.SetValueFromString("-123"));
				AssertEquals("-123 parsed result", -123L, Dummy.Z0_Long);

				AssertEquals("-456.78 parsable?", false, Dummy.Z0_LongInfo.SetValueFromString("-456.78"));
				AssertEquals("value stays the same", -123L, Dummy.Z0_Long);
			});
		}
	}
}
