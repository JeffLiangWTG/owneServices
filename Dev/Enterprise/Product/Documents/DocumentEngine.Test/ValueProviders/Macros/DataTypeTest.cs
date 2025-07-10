using System;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(DataType))]
	sealed class DataTypeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<DataType(\"System.String\")>", Passes.SecondPass));
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<datatype(\"<blah>\")>", Passes.SecondPass));

			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<Data type(<blah>)>", Passes.SecondPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<DataType>", Passes.SecondPass));
		}

		public void TestCreateDataTypeMacro()
		{
			string typeMacroForDecimal = DataType.CreateDataTypeMacro(100m);
			string typeMacroForBoolean = DataType.CreateDataTypeMacro(true);
			string typeMacroForDateTime = DataType.CreateDataTypeMacro(new DateTime(2017, 1, 1));
			string typeMacroForString = DataType.CreateDataTypeMacro("Test");

			string typeMacroForZDecimal = DataType.CreateDataTypeMacro(new ZDecimal(100m));
			string typeMacroForZBoolean = DataType.CreateDataTypeMacro(new ZBool(true));
			string typeMacroForZDateTime = DataType.CreateDataTypeMacro(new ZDateTime(2017, 1, 1));
			ZString typeMacroForZString = DataType.CreateDataTypeMacro(new ZString("Test"));

			AssertEquals("Should create correct macro for Decimal type", "<DataType(\"System.Decimal\")>", typeMacroForDecimal);
			AssertEquals("Should create correct macro for Boolean type", "<DataType(\"System.Boolean\")>", typeMacroForBoolean);
			AssertEquals("Should create correct macro for DateTime type", "<DataType(\"System.DateTime\")>", typeMacroForDateTime);
			AssertEquals("Should not create macro for string type", string.Empty, typeMacroForString);

			AssertEquals("Should create correct macro for ZDecimal type", "<DataType(\"CargoWise.Types.ZDecimal\")>", typeMacroForZDecimal);
			AssertEquals("Should create correct macro for ZBoolean type", "<DataType(\"CargoWise.Types.ZBool\")>", typeMacroForZBoolean);
			AssertEquals("Should create correct macro for ZDateTime type", "<DataType(\"CargoWise.Types.ZDateTime\")>", typeMacroForZDateTime);
			AssertEquals("Should not create macro for ZString type", string.Empty, typeMacroForZString);
		}

		public void TestConvertCellValueType()
		{
			AssertTestNonStringValuesGetReplacedByTheirNativeType("<InvalidMacro>", "100", "100", typeof(string));

			AssertTestNonStringValuesGetReplacedByTheirNativeType("<DataType(\"System.Decimal\")>", "100", 100m, typeof(decimal));
			AssertTestNonStringValuesGetReplacedByTheirNativeType("<DataType(\"System.Boolean\")>", "true", true, typeof(bool));
			AssertTestNonStringValuesGetReplacedByTheirNativeType("<DataType(\"System.DateTime\")>", "01/01/2017", new DateTime(2017, 1, 1), typeof(DateTime));

			AssertTestNonStringValuesGetReplacedByTheirNativeType("<DataType(\"CargoWise.Types.ZDecimal\")>", "100", new ZDecimal(100m), typeof(ZDecimal));
			AssertTestNonStringValuesGetReplacedByTheirNativeType("<DataType(\"CargoWise.Types.ZBool\")>", "true", new ZBool(true), typeof(ZBool));
			AssertTestNonStringValuesGetReplacedByTheirNativeType("<DataType(\"CargoWise.Types.ZDateTime\")>", "01/01/2017", new ZDateTime(2017, 1, 1), typeof(ZDateTime));
			AssertTestNonStringValuesGetReplacedByTheirNativeType("<DataType(\"CargoWise.Types.ZDateTime\")>", "InvalidValue", "InvalidValue", typeof(string));

			AssertTestNonStringValuesGetReplacedByTheirNativeType("<DataType(\"CargoWise.Types.ZDate\")>", "01/01/2017", new ZDate(2017, 1, 1), typeof(ZDate));
			AssertTestNonStringValuesGetReplacedByTheirNativeType("<DataType(\"CargoWise.Types.ZDate\")>", "InvalidValue", "InvalidValue", typeof(string));
		}

		public void TestIsINonVisualisableValueProvider()
		{
			var provider = GetNewValueProvider() as INonVisualisableValueProvider;

			AssertNotNull(provider);
			AssertEquals(DataType.RegexToFindMacroAnyWhereInString, provider.RegexToReplaceMacro);
		}

		public void TestConvertZDecimalTypeInFrenchLanguage()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.French))
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Core.SharedConstants.Languages.French)))
			{
				AssertTestNonStringValuesGetReplacedByTheirNativeType("<DataType(\"CargoWise.Types.ZDecimal\")>", "100,20", new ZDecimal(100.20m), typeof(ZDecimal));
			}
		}

		void AssertTestNonStringValuesGetReplacedByTheirNativeType(string macro, string replacedCellContent, object expectedValue, Type expectedType)
		{
			var result = DataType.ConvertCellValueType(macro, replacedCellContent);
			AssertEquals("cellReplacer.Content", expectedValue, result);
			AssertEquals("cellReplacer.Content Type", expectedType, result.GetType());
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new DataType();
		}
	}
}
