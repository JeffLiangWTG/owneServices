using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Test.ValueProviders.Macros
{
	[TestedType(typeof(ConvertWeight))]
	class ConvertWeightTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider() => new ConvertWeight();

		public void TestConvertWeight()
		{
			Assert("<ConvertWeight('1', 'KG', 'KG')>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight('1', 'KG', 'KG')>", Passes.FirstPass));
			AssertEquals("1.000", ValueProviderToTest.GetReplacement("<ConvertWeight('1', 'KG', 'KG')>", Report));

			Assert("<ConvertWeight('123.45', 'HG', 'KG', 0)>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight('123.45', 'HG', 'KG', 0)>", Passes.FirstPass));
			AssertEquals("12", ValueProviderToTest.GetReplacement("<ConvertWeight('123.45', 'HG', 'KG', 0)>", Report));

			Assert("<ConvertWeight('123.45', 'oz', 'kg', 6)>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight('123.45', 'oz', 'kg', 6)>", Passes.FirstPass));
			AssertEquals("3.499749", ValueProviderToTest.GetReplacement("<ConvertWeight('123.45', 'oz', 'kg', 6)>", Report));
		}

		public void TestConvertWeight_Rounding()
		{
			Assert("<ConvertWeight('1.123', 'KG', 'KG', 1)>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight('1.123', 'KG', 'KG', 1)>", Passes.FirstPass));
			AssertEquals("1.1", ValueProviderToTest.GetReplacement("<ConvertWeight('1.123', 'KG', 'KG', 1)>", Report));

			Assert("<ConvertWeight('1.789', 'KG', 'KG', 1)>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight('1.789', 'KG', 'KG', 1)>", Passes.FirstPass));
			AssertEquals("1.8", ValueProviderToTest.GetReplacement("<ConvertWeight('1.789', 'KG', 'KG', 1)>", Report));
		}

		public void TestConvertWeight_InvalidValue()
		{
			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertWeight('1a', 'KG', 'HG')>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertWeight Macro: Couldn't parse 1a to Decimal.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestConvertWeight_InvalidDecimalPlaces()
		{
			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertWeight('1', 'KG', 'HG', 2147483648)>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertWeight Macro: DecimalPlaces should be an integer within the range [0, 10] but is 2147483648.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();

			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertWeight('1', 'KG', 'HG', 11)>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertWeight Macro: DecimalPlaces should be an integer within the range [0, 10] but is 11.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestConvertWeight_InvalidUnit()
		{
			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertWeight('1', 'ML', 'KG')>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertWeight Macro: ML is not a weight unit.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();

			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertWeight('1', 'OZ', 'KM')>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertWeight Macro: KM is not a weight unit.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight('1', 'M3')>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight('1', 'M3', 'M3',)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight('1', 'M3', 'M3', 1 2)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight('1', 'M3', 'M3', -1)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight('1', 'M3', 'M3', '1')>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight('1', 'M3', 'M3', 1,)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight('1', 'M3', 'M3', 1, some)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<Convert Weight('1', 'M3', 'M3')>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("ConvertWeight('1', 'M3', 'M3')>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight '1', 'M3', 'M3'>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertWeight('1', 'M3', 'M3')> some", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   ConvertWeight  \t  (     '1',  'M3',   'M3'  )   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   ConvertWeight   (     '1',  'M3',   'M3' ,  23 )   >", Passes.FirstPass));
		}
	}
}
