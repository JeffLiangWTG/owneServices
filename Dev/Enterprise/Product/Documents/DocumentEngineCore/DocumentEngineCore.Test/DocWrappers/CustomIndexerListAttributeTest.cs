using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class CustomIndexerListAttributeTest : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new CustomIndexerListAttribute(null); });
			AssertExceptionThrown(typeof(ArgumentOutOfRangeException), delegate
			{ new CustomIndexerListAttribute(typeof(object)); });
			AssertNoExceptionThrown(delegate
			{ new CustomIndexerListAttribute(typeof(CodeDescriptionPairList)); });
		}

		public void TestGetListForIndexerWithNoList()
		{
			AssertEquals(null, CustomIndexerListAttribute.GetListForIndexer(typeof(TestClassWithNoList)));
		}

		public void TestGetListForIndexerWithValidList()
		{
			AssertEquals(typeof(DummyListForTesting), CustomIndexerListAttribute.GetListForIndexer(typeof(TestClassWithValidList)).GetType());
		}

		public void TestGetListForIndexerWithKnackeredList()
		{
			AssertExceptionThrown(typeof(InvalidCodeDescriptionPairException), delegate
			{ CustomIndexerListAttribute.GetListForIndexer(typeof(TestClassWithKnackeredList)); });
		}

		#region Implementation
		class DummyListForTesting : CodeDescriptionPairList
		{
			public DummyListForTesting()
			{
				AddPair("Dumb", "Dumb it is");
				AddPair("Dumber", "Dumber it is");
			}
		}

		abstract class TestClassWithNoList { }

		[CustomIndexerList(typeof(DummyListForTesting))]
		abstract class TestClassWithValidList { }

		[CustomIndexerList(typeof(CodeDescriptionPairListWithNoParameterlessConstructor))]
		abstract class TestClassWithKnackeredList { }

		class CodeDescriptionPairListWithNoParameterlessConstructor : CodeDescriptionPairList
		{
			public CodeDescriptionPairListWithNoParameterlessConstructor(string me)
			{
				Me = me;
			}
			public readonly string Me;
		}
		#endregion
	}
}
