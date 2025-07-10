using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Test.ValueProviders.Macros
{
	[TestedType(typeof(ConvertVolume))]
	class ConvertVolumeTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider() => new ConvertVolume();

		public void TestConvertVolume()
		{
			Assert("<ConvertVolume('1', 'M3', 'M3')>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume('1', 'M3', 'M3')>", Passes.FirstPass));
			AssertEquals("1.000", ValueProviderToTest.GetReplacement("<ConvertVolume('1', 'M3', 'M3')>", Report));

			Assert("<ConvertVolume('123.45', 'M3', 'CC', 0)>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume('123.45', 'M3', 'CC', 0)>", Passes.FirstPass));
			AssertEquals("123450000", ValueProviderToTest.GetReplacement("<ConvertVolume('123.45', 'M3', 'CC', 0)>", Report));

			Assert("<ConvertVolume('123.45', 'cc', 'm3', 6)>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume('123.45', 'cc', 'M3', 6)>", Passes.FirstPass));
			AssertEquals("0.000123", ValueProviderToTest.GetReplacement("<ConvertVolume('123.45', 'cc', 'm3', 6)>", Report));
		}

		public void TestConvertVolume_Rounding()
		{
			Assert("<ConvertVolume('1.123', 'CC', 'CC', 1)>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume('1.123', 'CC', 'CC', 1)>", Passes.FirstPass));
			AssertEquals("1.1", ValueProviderToTest.GetReplacement("<ConvertVolume('1.123', 'CC', 'CC', 1)>", Report));

			Assert("<ConvertVolume('1.789', 'CC', 'CC', 1)>", ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume('1.789', 'CC', 'CC', 1)>", Passes.FirstPass));
			AssertEquals("1.8", ValueProviderToTest.GetReplacement("<ConvertVolume('1.789', 'CC', 'CC', 1)>", Report));
		}

		public void TestConvertVolume_InvalidValue()
		{
			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertVolume('1a', 'CC', 'M3')>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertVolume Macro: Couldn't parse 1a to Decimal.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestConvertVolume_InvalidDecimalPlaces()
		{
			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertVolume('1', 'CC', 'M3', 2147483648)>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertVolume Macro: DecimalPlaces should be an integer within the range [0, 10] but is 2147483648.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();

			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertVolume('1', 'CC', 'M3', 11)>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertVolume Macro: DecimalPlaces should be an integer within the range [0, 10] but is 11.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestConvertVolume_InvalidUnit()
		{
			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertVolume('1', 'KM', 'M3')>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertVolume Macro: KM is not a volume unit.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();

			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<ConvertVolume('1', 'M3', 'KG')>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in ConvertVolume Macro: KG is not a volume unit.]", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume('1', 'M3')>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume('1', 'M3', 'M3',)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume('1', 'M3', 'M3', 1 2)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume('1', 'M3', 'M3', -1)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume('1', 'M3', 'M3', '1')>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume('1', 'M3', 'M3', 1,)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume('1', 'M3', 'M3', 1, some)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<Convert Volume('1', 'M3', 'M3')>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("ConvertVolume('1', 'M3', 'M3')>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume '1', 'M3', 'M3'>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<ConvertVolume('1', 'M3', 'M3')> some", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   ConvertVolume  \t  (     '1',  'M3',   'M3'  )   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   ConvertVolume   (     '1',  'M3',   'M3' ,  23 )   >", Passes.FirstPass));
		}
	}
}
