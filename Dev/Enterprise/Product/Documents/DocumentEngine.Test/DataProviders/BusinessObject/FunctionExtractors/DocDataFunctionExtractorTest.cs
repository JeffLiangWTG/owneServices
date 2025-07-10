using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Testing
{
	sealed class DocDataFunctionExtractorTest : Core.Testing.BaseFunctionExtractorTest
	{
		public override void TestGetMethodInfoChainLink()
		{
			MyWrapper wrapper = new MyWrapper();
			DocDataFunctionExtractor docDataExtractor = DocDataFunctionExtractor.ParseAndExtract("DocDataValue(\"Identifier\", \"{Twelve}\")");
			MethodInfoChainLink methodInfoChainLink = docDataExtractor.GetMethodInfoChainLink(typeof(MyWrapper));
			AssertNotNull("Got to have a methodInfoChainLink before you can evaluate it. It's null.", methodInfoChainLink);
			AssertEquals("chainLink.ReflectOutObject(0, collection, collection).ToString())", "12", methodInfoChainLink.ReflectOutObject(wrapper, wrapper).ToString());

			MyBusinessObject businessObject = Factory.New<MyBusinessObject>();
			methodInfoChainLink = docDataExtractor.GetMethodInfoChainLink(typeof(MyBusinessObject));
			AssertNotNull("Got to have a methodInfoChainLink before you can evaluate it. It's null.", methodInfoChainLink);
		}

		class MyWrapper : DocumentWrapper
		{
			public ZInt Twelve
			{
				get { return 12; }
			}
		}

		class MyBusinessObject : DummyBaseBusinessObject
		{
			public MyBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		public void TestConstructorWithOneStringAndOneParameters()
		{
			DocDataFunctionExtractor docDataExtractor = DocDataFunctionExtractor.ParseAndExtract("<DocDataValue(\"Rotor Size\")>");
			AssertEquals("Rotor Size", docDataExtractor.PropertyLabel);
			AssertEquals("Rotor Size", docDataExtractor.DocDataIdentifier);
			AssertEquals("", docDataExtractor.FormatStringForFallbackValue);
		}

		public void TestConstructorWithOneStringAndTwoParameters()
		{
			DocDataFunctionExtractor docDataExtractor = DocDataFunctionExtractor.ParseAndExtract("<DocDataValue(\"Rotor Size\", \"{Twelve}\")>");
			AssertEquals("Rotor Size", docDataExtractor.PropertyLabel);
			AssertEquals("Rotor Size", docDataExtractor.DocDataIdentifier);
			AssertEquals("{Twelve}", docDataExtractor.FormatStringForFallbackValue);
		}

		public void TestDocDataValueWorkWithQuotesInFallbackValue()
		{
			AssertFallbackValues("\"Test\", Test2", "\"Test\", Test2");
			AssertFallbackValues("   \"Test\", Test2   ", "   \"Test\", Test2   ");
			AssertFallbackValues("\\<", "<");
			AssertFallbackValues("\\>", ">");
		}

		void AssertFallbackValues(string fallbackValue, string expectedValue)
		{
			var docDataExtractor = DocDataFunctionExtractor.ParseAndExtract($"<DocDataValue(\"Rotor Size\", \"{fallbackValue}\")>");
			AssertEquals(expectedValue, docDataExtractor.FormatStringForFallbackValue);
		}

		public void TestConstructorWithOneEmptyString()
		{
			DocDataFunctionExtractor docDataExtractor = DocDataFunctionExtractor.ParseAndExtract(ZString.Empty);
			AssertNull(docDataExtractor);
		}
	}
}
