namespace CargoWise.Common.Testing
{
	using System;
	using NUnit.Framework;
	class TestFactoryServiceInstantiation
	{
		public string A;
		public int B;
		public bool C;

		public TestFactoryServiceInstantiation(string a, int b, bool c)
		{
			A = a;
			B = b;
			C = c;
		}
	}

	interface IFactoryForThing
	{
		TestFactoryServiceInstantiation CreateThing(string a, int b, bool c);
	}

	class FactoryForThing : IFactoryForThing
	{
		public TestFactoryServiceInstantiation CreateThing(string a, int b, bool c)
		{
			return new TestFactoryServiceInstantiation(a, b, c);
		}
	}

	class FactoryServiceTests : TestCase
	{
		public void TestBasicTest()
		{
			var factory = new FactoryService();
			factory.RegisterFactory<IFactoryForThing>(new FactoryForThing());
			var result = factory.GetFactory<IFactoryForThing>().CreateThing("hi", 3, false);
			AssertEquals("hi", result.A);
			AssertEquals(3, result.B);
			AssertEquals(false, result.C);

			factory.RegisterFactory<Func<string, int, bool, TestFactoryServiceInstantiation>>((a, b, c) => new TestFactoryServiceInstantiation(a, b, c));
			result = factory.GetFactory<Func<string, int, bool, TestFactoryServiceInstantiation>>().Invoke("hi", 3, false);
			AssertEquals("hi", result.A);
			AssertEquals(3, result.B);
			AssertEquals(false, result.C);
		}
	}
}
