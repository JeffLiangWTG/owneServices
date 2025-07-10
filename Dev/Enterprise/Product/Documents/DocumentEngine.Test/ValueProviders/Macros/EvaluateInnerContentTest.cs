using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(EvaluateInnerContent))]
	sealed class EvaluateInnerContentTest : ValueProviderTest
	{
		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals("Must be TextEdit to allow any EvaluateInnerContent macro to be modifiable in the visualiser", VisualiserComponentTypes.TextEdit, ValueProviderToTest.ComponentType);
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<EvaluateInnerContent(\"blah\")>", Passes.FirstPass));
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<evaluateinnercontent(\"<blah>\")>", Passes.FirstPass));
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("< EvaluateInnerContent (\"<blah>\") >", Passes.FirstPass));
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<EvaluateInnerContent(\"blah\r\nhello\")>", Passes.FirstPass));
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<EvaluateInnerContent(\"\")>", Passes.FirstPass));

			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<EvaluateInne rcontent(<blah>)>", Passes.FirstPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<EvaluateInnerContent>", Passes.FirstPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<EvaluateInnerContent(blah)>", Passes.FirstPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<evaluateinnercontent(<blah>)>", Passes.FirstPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("< EvaluateInnerContent (<blah>) >", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals("", ValueProviderToTest.GetReplacement("<EvaluateInnerContent(\"\")>", Report));
			AssertEquals("blah", ValueProviderToTest.GetReplacement("<EvaluateInnerContent(\"blah\")>", Report));

			Report.Renderer.CurrentPass = Passes.FirstPass;
			GlbStaff.CurrentUser.GS_Title = "HELLO SAILOR";
			GlbStaff.CurrentUser.GS_WorkPhone = "NIKE SAYS:";

			AssertEquals("Evaluated inner macro 1", "HELLO SAILOR LIS\r\nPH", ValueProviderToTest.GetReplacement("<EvaluateInnerContent(\"<LoginTitle> LIS\r\nPH\")>", Report));
			AssertEquals("Evaluated inner macro 2", "LIS\r\nPH HELLO SAILOR", ValueProviderToTest.GetReplacement("<EvaluateInnerContent(\"LIS\r\nPH <LoginTitle>\")>", Report));
		}

		public void TestReplacementMultiLevel()
		{
			Report.Renderer.CurrentPass = Passes.FirstPass;
			GlbStaff.CurrentUser.GS_Title = "HELLO SAILOR";

			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			var code = org.CustomsCodes.AddNew();
			code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			code.OK_CodeType = "XYZ";
			code.OK_CustomsRegNo = "ABC123";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;
			factory.Save();

			AssertEquals("Evaluated inner macro 3", "HELLO SAILOR ", ValueProviderToTest.GetReplacement("<EvaluateInnerContent(\"<LoginTitle> <CustomsCode(<BranchProxy>, AU, XYZ)>\")>", Report));
			AssertEquals("Evaluated inner macro 3", "HELLO SAILOR ", ValueProviderToTest.GetReplacement("<EvaluateInnerContent(\"<LoginTitle> <CustomsCode(<BranchProxy>, NZ, ABC)>\")>", Report));
			AssertEquals("Evaluated inner macro 3", "HELLO SAILOR ABC123", ValueProviderToTest.GetReplacement("<EvaluateInnerContent(\"<LoginTitle> <CustomsCode(<BranchProxy>, NZ, XYZ)>\")>", Report));
		}

		public void TestReplacementDoesntBarfWhenTheFieldIsNotFound()
		{
			Report.Renderer.CurrentPass = Passes.FirstPass;
			AssertEquals("Evaluated inner macro 3", "", ValueProviderToTest.GetReplacement("<EvaluateInnerContent(\"<OtherStuff>\")> ", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new EvaluateInnerContent();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.Renderer.CurrentPass = Passes.FirstPass;
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Z0_NVarchar", "ABC <Z0_Decimal>"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Z0_Decimal", "123"));

			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgHeader>();
			var code = org.CustomsCodes.AddNew();
			code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			code.OK_CodeType = "XYZ";
			code.OK_CustomsRegNo = "ABC123";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;
			factory.Save();
		}
	}
}
