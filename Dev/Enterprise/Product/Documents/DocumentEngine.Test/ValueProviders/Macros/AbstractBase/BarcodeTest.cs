using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	abstract class BarcodeTest : ValueProviderTest
	{
		#region Replace

		public virtual void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert($"should not match <{CodeType}>", !ValueProviderToTest.IsResponsibleForReplacing($"<{CodeType}>", Passes.FirstPass));
			Assert($"should not match <{CodeType}(abc,1,2)>", !ValueProviderToTest.IsResponsibleForReplacing($"<{CodeType}>", Passes.FirstPass));
			Assert($"should match < {CodeType}(\"abc\",1,2)       >", ValueProviderToTest.IsResponsibleForReplacing($"< {CodeType}(\"abc\",1,2)       >", Passes.FirstPass));
			Assert($"should not match <{CodeType}(\"abc\",1,2,-15)>", !ValueProviderToTest.IsResponsibleForReplacing($"<{CodeType}(\"abc\",1,2,-15)>", Passes.FirstPass));
			Assert($"should not match <{CodeType}(\"abc\",3,4,15)>", !ValueProviderToTest.IsResponsibleForReplacing($"<{CodeType}(\"abc\",3,4,15)>", Passes.FirstPass));
			Assert($"should not match <{CodeType}(\"abc\",1,2,15,M)>", !ValueProviderToTest.IsResponsibleForReplacing($"<{CodeType}(\"abc\",1,2,15,M)>", Passes.FirstPass));
			Assert($"should not match <{CodeType}(\"abc\",1,2,15,\"R\")>", !ValueProviderToTest.IsResponsibleForReplacing($"<{CodeType}(\"abc\",1,2,15,\"R\")>", Passes.FirstPass));
			Assert($"should not match <{CodeType}(\"abc\",3,4,15,\"M\")>", !ValueProviderToTest.IsResponsibleForReplacing($"<{CodeType}(\"abc\",3,4,15,\"M\")>", Passes.FirstPass));
			Assert($"should not match <{CodeType}(\"abc\",1,2,15,\"R\",UTF-8)>", !ValueProviderToTest.IsResponsibleForReplacing($"<{CodeType}(\"abc\",1,2,15,\"R\",UTF-8)>", Passes.FirstPass));
			Assert($"should not match <{CodeType}(\"abc\",3,4,15,\"M\",\"UTF-8\")>", !ValueProviderToTest.IsResponsibleForReplacing($"<{CodeType}(\"abc\",3,4,15,\"M\",\"UTF-8\")>", Passes.FirstPass));
		}

		public virtual void TestReplace()
		{
			PrepareRenderer();
			Report.Renderer.SaveOriginalColumnWidths();
			var barcodeMacro = (Barcode)ValueProviderToTest;
			var image = (ExcelImage)ValueProviderToTest.GetReplacement($"< {CodeType} ( \"{barcodeMacro.ExampleContentForDocumentation}\" , 2  , 4 )  >", Report);
			var content = ((Barcode)ValueProviderToTest).BarcodeProcessor.ParseCode((Bitmap)image.Image);

			AssertEquals($"{barcodeMacro.ExampleContentForDocumentation}", content.Text);
		}

		public virtual void TestReplaceWithErrors()
		{
			PrepareRenderer();
			Report.Renderer.SaveOriginalColumnWidths();

			var result = (ExcelImage)ValueProviderToTest.GetReplacement(MacroWithUnExpectedContent, Report);

			AssertNull(result);
			AssertContains($"Error generating {CodeType} code by macro", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false), true);
		}

		public void TestErrorReportedWithoutTemplate()
		{
			AssertExceptionThrown<ArgumentException>(() =>
			{
				ValueProviderToTest.GetReplacement("Whatever", Report.NewForTesting(DocumentPack.EmptyPack));
			});
		}

		#endregion

		#region Implementations

		protected abstract string MacroWithUnExpectedContent { get; }

		protected abstract string CodeType { get; }

		protected override List<FieldInfo> FieldCollection => new List<FieldInfo>
		{
			GetNewValueProvider().GetType().GetField("<BarcodeProcessor>k__BackingField", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
			GetNewValueProvider().GetType().GetField("<CodeType>k__BackingField", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.Renderer.SaveOriginalColumnWidths();
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("BarCodeText", ((Barcode)ValueProviderToTest).ExampleContentForDocumentation));
		}

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			var actualResult = Report.MacroTranslator.GetValue(example, PassToReplaceExample);
			AssertType(typeof(ExcelImage), actualResult);
			var content = ((Barcode)ValueProviderToTest).BarcodeProcessor.ParseCode((Bitmap)((ExcelImage)actualResult).Image);
			AssertEquals(expectedResult, content.Text);
		}

		#endregion
	}
}
