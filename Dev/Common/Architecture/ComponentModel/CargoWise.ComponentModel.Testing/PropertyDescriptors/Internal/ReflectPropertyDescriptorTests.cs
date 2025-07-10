#if DEBUG
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing.PropertyDescriptors.Internal
{
	[TestClass]
	sealed class ReflectPropertyDescriptorTests : TestCase
	{
		#region NUnitCore test methods - can be removed when NUnit replaces NUnitCore
		public void TestBitDefaultValueQueriedEquals1()
		{
			var field = "BitDefaultValueQueried";
			var expected = 1;

			TestFieldEqualsExpected(field, expected);
		}

		public void TestBitGetQueriedEquals2()
		{
			var field = "BitGetQueried";
			var expected = 2;

			TestFieldEqualsExpected(field, expected);
		}

		public void TestBitSetQueriedEquals4()
		{
			var field = "BitSetQueried";
			var expected = 4;

			TestFieldEqualsExpected(field, expected);
		}

		public void TestBitShouldSerializeQueried8()
		{
			var field = "BitShouldSerializeQueried";
			var expected = 8;

			TestFieldEqualsExpected(field, expected);
		}

		public void TestBitResetQueried16()
		{
			var field = "BitResetQueried";
			var expected = 16;

			TestFieldEqualsExpected(field, expected);
		}

		public void TestBitChangedQueried32()
		{
			var field = "BitChangedQueried";
			var expected = 32;

			TestFieldEqualsExpected(field, expected);
		}

		public void TestBitIPropChangedQueried64()
		{
			var field = "BitIPropChangedQueried";
			var expected = 64;

			TestFieldEqualsExpected(field, expected);
		}

		public void TestBitReadOnlyChecked()
		{
			var field = "BitReadOnlyChecked";
			var expected = 128;

			TestFieldEqualsExpected(field, expected);
		}

		public void TestBitAmbientValueQueried256()
		{
			var field = "BitAmbientValueQueried";
			var expected = 256;

			TestFieldEqualsExpected(field, expected);
		}

		public void TestBitSetOnDemand512()
		{
			var field = "BitSetOnDemand";
			var expected = 512;

			TestFieldEqualsExpected(field, expected);
		}
		#endregion

		// This should be changed to TestCases when NUnit replaces NUnitCore
		//[TestCase("BitDefaultValueQueried", 1)]
		//[TestCase("BitGetQueried", 2)]
		//[TestCase("BitSetQueried", 4)]
		//[TestCase("BitShouldSerializeQueried", 8)]
		//[TestCase("BitResetQueried", 16)]
		//[TestCase("BitChangedQueried", 32)]
		//[TestCase("BitIPropChangedQueried", 64)]
		//[TestCase("BitReadOnlyChecked", 128)]
		//[TestCase("BitAmbientValueQueried", 256)]
		//[TestCase("BitSetOnDemand", 512)]
		public void TestFieldEqualsExpected(string field, int expected)
		{
			var type = typeof(ReflectPropertyDescriptor);
			var fieldInfo = type.GetField(field, BindingFlags.Static | BindingFlags.NonPublic);

			var value = fieldInfo.GetValue(null);

			AssertEquals(expected, value);
		}
	}
}
#endif
