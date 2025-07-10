using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ValueChangedEventArgsTest : TestCaseWithDummy
	{
		public void TestConstructor()
		{
			ZPropertyInfoString info = new ZPropertyInfoString(Dummy, "Trevor");
			ZString s = new ZString("S");
			ValueChangedEventArgs e = new ValueChangedEventArgs(s, info);
			AssertEquals(s, e.OldValue);
			AssertEquals(info, e.Info);
		}

		public void TestNewValue()
		{
			ZString newValue = "123";
			ZPropertyInfo info = Dummy.Z0_DescriptionInfo;
			info.Value = newValue;

			ValueChangedEventArgs e = new ValueChangedEventArgs(ZString.Empty, info);
			AssertEquals(newValue, e.NewValue);
		}
	}
}
