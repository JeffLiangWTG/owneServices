using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class TemporaryValueSetterTest : TestCase
	{
		public void TestSet()
		{
			var actual = false;
			AssertEquals("value", false, actual);

			using (var temp = new TemporaryValueSetter<bool>(delegate(bool value)
			{ actual = value; }, actual))
			{
				AssertEquals("value", false, actual);

				temp.Set(true);
				AssertEquals("value", true, actual);

				temp.Set(false);
				AssertEquals("value", false, actual);
			}

			AssertEquals("value", false, actual);
		}

		public void TestTemporaryValueSetter()
		{
			var actual = false;
			AssertEquals("value", false, actual);

			using (new TemporaryValueSetter<bool>(delegate(bool value)
			{ actual = value; }, actual, true))
			{
				AssertEquals("value", true, actual);
			}

			AssertEquals("value", false, actual);
		}
	}
}
