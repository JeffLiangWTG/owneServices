using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CustomsCode))]
	sealed class CustomsCodeTest : ValueProviderWithLoadControlFactoryTest<CustomsCode>
	{
		public override void TestIsResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<CustomsCode>");
			AssertNotResponsibleForReplacing("<CustomsCode({875C4949-60AD-4f6c-9CB0-6F6AD4C96348}, AU)>");
			AssertNotResponsibleForReplacing("<CustomsCode({875C4949-60AD-4f6c-9CB0-6F6AD4C96348}, AU, GST, XX)>");
			AssertIsResponsibleForReplacing("<CustomsCode({875C4949-60AD-4f6c-9CB0-6F6AD4C96348}, AU, GST)>");
			AssertIsResponsibleForReplacing("<CustomsCode(<BranchCode>, AU, GST)>");
		}

		public override void TestReplacement()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.New<OrgHeader>();
			header.OH_Code = "Blaticus";

			var code1 = header.CustomsCodes.AddNew();
			code1.OK_RN_NKCodeCountry = "AU";
			code1.OK_CodeType = "GST";
			code1.OK_CustomsRegNo = "Blaticus";

			var code2 = header.CustomsCodes.AddNew();
			code2.OK_RN_NKCodeCountry = "IS";
			code2.OK_CodeType = "KEN";
			code2.OK_CustomsRegNo = "Majapahit";

			factory.Save();

			AssertIsReplacedWith("Majapahit", string.Format("<CustomsCode({0}, IS, KEN)>", header.PK));
			AssertIsReplacedWith("Blaticus", string.Format("<CustomsCode({0}, AU, GST)>", header.PK));
			AssertIsReplacedWith("", string.Format("<CustomsCode({0}, XX, GST)>", header.PK));
			AssertIsReplacedWith("", string.Format("<CustomsCode({0}, AU, XXX)>", header.PK));
			AssertIsReplacedWith("", "<CustomsCode({875C4949-60AD-4f6c-9CB0-6F6AD4C96348}, AU, GST)>");
			AssertIsReplacedWith("", "<CustomsCode({orgheader}, {countrycode}, {typecode})>");
		}

		#region Implementation

		protected override void PrepareDataForExamplesEvaluate()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.New<OrgHeader>();
			header.OH_Code = "Blaticus";
			var code = header.CustomsCodes.AddNew();
			code.OK_RN_NKCodeCountry = "AU";
			code.OK_CodeType = "GST";
			code.OK_CustomsRegNo = "Blaticus";
			factory.Save();
			Report.MacroTranslator.RegisterValueProvider(new Enterprise.DocumentEngine.ValueReplacers.FixedValueProvider("OrgPK", header.PK));
		}

		#endregion
	}
}
