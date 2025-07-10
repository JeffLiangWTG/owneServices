using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GetEventReferenceValue))]
	sealed class GetEventReferenceValueTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new GetEventReferenceValue();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("GetEventReferenceValue", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<GetEventReferenceValue meh>", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<GetEventReferenceValue(\"|ACT=123\")>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("< GetEventReferenceValue (\"\",\"DEF\", )>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("< GetEventReferenceValue (\"EventReference\",\"DEF\", )>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetEventReferenceValue(\"|ACT=123\",\"DEF\")>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetEventReferenceValue(\"<Upper(\"|act=123\")>\",\"DEF\")>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetEventReferenceValue(\"<SL_Reference>\", \"OLD\")>", Passes.FirstPass));
		}

		public void TestReplacementFindParameter()
		{
			var eventReference1 = "|ACT=123";
			var eventReference2 = "FreeText|ACT=123|MST=Bobby|TYP=0|OLD=BKD";

			var replacement1 = ValueProviderToTest.GetReplacement($"<GetEventReferenceValue(\"{eventReference1}\",\"ACT\")>", Report);
			var replacement2 = ValueProviderToTest.GetReplacement($"<GetEventReferenceValue(\"{eventReference2}\",\"ACT\")>", Report);
			var replacement2_2 = ValueProviderToTest.GetReplacement($"<GetEventReferenceValue(\"{eventReference2}\",\"OLD\")>", Report);

			CombineAssertions(() =>
			{
				AssertEquals("Return the value if Event Reference has that parameter", "123", replacement1);
				AssertEquals("Return the value if Event Reference has that parameter", "123", replacement2);
				AssertEquals("Return the value if Event Reference has that parameter", "BKD", replacement2_2);
			});
		}

		public void TestReplacementCannotFindParameter()
		{
			var eventReference1 = "";
			var eventReference2 = "	FREETEXT @#$%^&*()|";
			var eventReference3 = "FreeText|MST=Bobby|TYP=0|OLD=BKD";
			var eventReference4 = "FreeText | ACT =123 | MST = Bobby|TYP=0|OLD=BKD";
			var eventReference5 = "|ACT=123";
			var eventReference6 = "|ACT=123";

			var replacement1 = ValueProviderToTest.GetReplacement($"<GetEventReferenceValue(\"{eventReference1}\",\"ACT\")>", Report);
			var replacement2 = ValueProviderToTest.GetReplacement($"<GetEventReferenceValue(\"{eventReference2}\",\"ACT\")>", Report);
			var replacement3 = ValueProviderToTest.GetReplacement($"<GetEventReferenceValue(\"{eventReference3}\",\"ACT\")>", Report);
			var replacement4 = ValueProviderToTest.GetReplacement($"<GetEventReferenceValue(\"{eventReference4}\",\"ACT\")>", Report);
			var replacement5 = ValueProviderToTest.GetReplacement($"<GetEventReferenceValue(\"{eventReference5}\",\"ACT \")>", Report);
			var replacement6 = ValueProviderToTest.GetReplacement($"<GetEventReferenceValue(\"{eventReference6}\",\"\")>", Report);

			CombineAssertions(() =>
			{
				AssertEquals("Return empty if EventReference is empty", "", replacement1);
				AssertEquals("Return empty if EventReference is free text", "", replacement2);
				AssertEquals("Return empty if Event Reference does not have that parameter", "", replacement3);
				AssertEquals("Return empty if Event Reference format is incorrect", "", replacement4);
				AssertEquals("Return empty if parameter format is incorrect", "", replacement5);
				AssertEquals("Return empty if parameter name is empty", "", replacement6);
			});
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("SL_Reference", "|ACT=123|OLD=BKD"));
		}
	}
}
