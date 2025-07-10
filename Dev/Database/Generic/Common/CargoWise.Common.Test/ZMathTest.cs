namespace CargoWise.Common.Testing
{
	using NUnit.Framework;

	class TestShorthandMethods : TestCase
	{
		public void TestClampInt()
		{
			AssertEquals("Should get val back", 5, ZMath.Clamp(5, 0, 10));
			AssertEquals("Should get min back", 0, ZMath.Clamp(-1, 0, 10));
			AssertEquals("Should get max back", 10, ZMath.Clamp(11, 0, 10));
		}

		public void TestClampFloat()
		{
			AssertEquals("Should get val back", 5.9f, ZMath.Clamp(5.9f, 0.1f, 10.7f));
			AssertEquals("Should get min back", -5.3f, ZMath.Clamp(-91.6f, -5.3f, 11.2f));
			AssertEquals("Should get max back", 10.3f, ZMath.Clamp(121.4431f, 0.01f, 10.3f));
		}

		public void TestClampDouble()
		{
			AssertEquals("Should get val back", 5.01, ZMath.Clamp(5.01, 0.14, 10.98));
			AssertEquals("Should get min back", -905.84, ZMath.Clamp(-1923.6, -905.84, 11.21));
			AssertEquals("Should get max back", 10.2, ZMath.Clamp(126.4311, -0.01, 10.2));
		}
	}
}
