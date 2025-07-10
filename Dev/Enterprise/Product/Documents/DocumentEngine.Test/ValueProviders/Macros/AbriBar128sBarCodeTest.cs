using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(AbriBar128sBarCode))]
	sealed class AbriBar128sBarCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<AbriBar128sBarCode(AField)>", Passes.SecondPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<AbriBar128sBarCode>", Passes.SecondPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   bar code  >", Passes.SecondPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   AbriBar128sBarCode somefield   >", Passes.SecondPass));
			Assert("should not match as it's first pass", !ValueProviderToTest.IsResponsibleForReplacing("<   AbriBar128sBarCode  \t  (     ' fld ' )   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   AbriBar128sBarCode  \t  (     ' fld ' )   >", Passes.SecondPass));
			Assert("should not match as it's first pass", !ValueProviderToTest.IsResponsibleForReplacing("<   AbriBar128sBarCode('<'AField'> ', UseOptimisedEncoding)   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   AbriBar128sBarCode('<'AField'> ', UseOptimisedEncoding)   >", Passes.SecondPass));
			Assert("should not match as it's first pass", !ValueProviderToTest.IsResponsibleForReplacing("<   AbriBar128sBarCode('<'AField'> ', UseOptimisedEncoding,   IsGS1128Barcode)   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   AbriBar128sBarCode('<'AField'> ', UseOptimisedEncoding,   IsGS1128Barcode)   >", Passes.SecondPass));
			Assert("should not match as it's first pass", !ValueProviderToTest.IsResponsibleForReplacing("<   AbriBar128sBarCode('<'AField'> ', IsGS1128Barcode)   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   AbriBar128sBarCode('<'AField'> ', IsGS1128Barcode)   >", Passes.SecondPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			TextBarcode textBarcode = new TextBarcode("Hello World");
			AssertEquals(textBarcode.TextAs128sFontString, ValueProviderToTest.GetReplacement("<AbriBar128sBarCode('Hello World')>", Report));

			textBarcode = new TextBarcode(" Hello 'World' ", true);
			AssertEquals(textBarcode.TextAs128sFontString, ValueProviderToTest.GetReplacement("<AbriBar128sBarCode(' Hello 'World' ',  UseOptimisedEncoding )>", Report));

			textBarcode = new TextBarcode(" Hello 'World' ", true, true);
			AssertEquals(textBarcode.TextAs128sFontString, ValueProviderToTest.GetReplacement("<AbriBar128sBarCode(' Hello 'World' ',  UseOptimisedEncoding,  IsGS1128Barcode)>", Report));

			textBarcode = new TextBarcode(" Hello 'World' ", false, true);
			AssertEquals(textBarcode.TextAs128sFontString, ValueProviderToTest.GetReplacement("<AbriBar128sBarCode(' Hello 'World' ',  IsGS1128Barcode)>", Report));
		}

		public void TestSeparatorsInBarcodeForMultipleAIFields()
		{
			PrepareRenderer();
			TextBarcode textBarcode = new TextBarcode(new ZString[] { "421 21000", "21 12345" }, isGS1128Barcode: true);
			AssertEquals(textBarcode.TextAs128sFontString, ValueProviderToTest.GetReplacement("<AbriBar128sBarCode('421 21000,21 12345', IsGS1128Barcode)>", Report));

			textBarcode = new TextBarcode("421 21000, 21 12345");
			AssertEquals(textBarcode.TextAs128sFontString, ValueProviderToTest.GetReplacement("<AbriBar128sBarCode('421 21000, 21 12345')>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new AbriBar128sBarCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ShipmentNumber", "S00001234"));
		}
	}
}
