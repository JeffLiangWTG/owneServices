using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Test.ValueProviders.Macros
{
	[TestedType(typeof(ConvertDimension))]
	class ConvertDimensionTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider() => new ConvertDimension();

		public void TestConvertDimension()
		{
			Assert("<ConvertDimension('1', 'M', 'M')>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension('1', 'M', 'M')>", Passes.FirstPass));
			AssertEquals("1.000", ValueProviderToTest.GetReplacement("<ConvertDimension('1', 'M', 'M')>", Report));

			Assert("<ConvertDimension('123.45', 'M', 'CM', 0)>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension('123.45', 'M', 'CM', 0)>", Passes.FirstPass));
			AssertEquals("12345", ValueProviderToTest.GetReplacement("<ConvertDimension('123.45', 'M', 'CM', 0)>", Report));

			Assert("<ConvertDimension('123.45', 'cm', 'm', 6)>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension('123.45', 'cm', 'm', 6)>", Passes.FirstPass));
			AssertEquals("1.234500", ValueProviderToTest.GetReplacement("<ConvertDimension('123.45', 'cm', 'm', 6)>", Report));
		}

		public void TestConvertDimension_Rounding()
		{
			Assert("<ConvertDimension('1.123', 'M', 'M', 1)>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension('1.123', 'M', 'M', 1)>", Passes.FirstPass));
			AssertEquals("1.1", ValueProviderToTest.GetReplacement("<ConvertDimension('1.123', 'M', 'M', 1)>", Report));

			Assert("<ConvertDimension('1.789', 'M', 'M', 1)>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension('1.789', 'M', 'M', 1)>", Passes.FirstPass));
			AssertEquals("1.8", ValueProviderToTest.GetReplacement("<ConvertDimension('1.789', 'M', 'M', 1)>", Report));
		}

		public void TestConvertDimension_InvalidValue()
		{
			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertDimension('1a', 'MM', 'M')>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertDimension Macro: Couldn't parse 1a to Decimal.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestConvertDimension_InvalidDecimalPlaces()
		{
			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertDimension('1', 'MM', 'M', 2147483648)>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertDimension Macro: DecimalPlaces should be an integer within the range [0, 10] but is 2147483648.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();

			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertDimension('1', 'MM', 'M', 11)>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertDimension Macro: DecimalPlaces should be an integer within the range [0, 10] but is 11.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestConvertDimension_InvalidUnit()
		{
			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertDimension('1', 'ML', 'M')>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertDimension Macro: ML is not a dimension unit.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();

			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertDimension('1', 'MM', 'KG')>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertDimension Macro: KG is not a dimension unit.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension('1', 'M3')>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension('1', 'M3', 'M3',)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension('1', 'M3', 'M3', 1 2)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension('1', 'M3', 'M3', -1)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension('1', 'M3', 'M3', '1')>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension('1', 'M3', 'M3', 1,)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension('1', 'M3', 'M3', 1, some)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<Convert Dimension('1', 'M3', 'M3')>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("ConvertDimension('1', 'M3', 'M3')>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension '1', 'M3', 'M3'>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertDimension('1', 'M3', 'M3')> some", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   ConvertDimension  \t  (     '1',  'M3',   'M3'  )   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   ConvertDimension   (     '1',  'M3',   'M3' ,  23 )   >", Passes.FirstPass));
		}
	}
}
