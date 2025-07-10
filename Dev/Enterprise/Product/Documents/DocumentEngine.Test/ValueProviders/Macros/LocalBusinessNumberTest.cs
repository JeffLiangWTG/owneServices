using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(LocalBusinessNumber))]
	sealed class LocalBusinessNumberTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<localbusinessnumber>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   local business number   >", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   localbusinessnumber somefield   >", Passes.FirstPass));
			Assert("should match <LocalBusinessNumber(AField)>", ValueProviderToTest.IsResponsibleForReplacing("<LocalBusinessNumber(AField)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   LocalBusinessNumber(AField, other)   >", Passes.SecondPass));
		}

		public void TestReplacement()
		{
			var factory = new BusinessObjectFactory();
			var testHeader = factory.New<OrgHeader>();
			testHeader.OH_Code = "XXXXXX";
			testHeader.MainAddress.OA_Address1 = "123 Test Way";
			testHeader.PrimaryRegistrationNumber.Number = "123 456 789 0";
			factory.Save();

			AssertEquals("123 456 789 0", ValueProviderToTest.GetReplacement(String.Format("<LocalBusinessNumber({0})>", testHeader.PK), Report));
		}

		public void TestReplacingInvalidMacro()
		{
			Report.ErrorManager.ClearErrors();
			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<LocalBusinessNumber({ThisIsntAPK})>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in LocalBusinessNumber Macro: Unable to obtain Local Business Number. Please check {ThisIsntAPK} is a valid orgheaderpk.]",
									Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
		}

		public void TestReplacement_WhenParamIsNotAnOrgPk()
		{
			var guid = ZGuid.NewZGuid();
			Report.ErrorManager.ClearErrors();
			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement(string.Format("<LocalBusinessNumber({0})>", guid), Report));
			AssertEquals("report.ErrorManager.ToString()", $@"Severity: [Warning (without error report)] Message: [Error in LocalBusinessNumber Macro: Unable to obtain Local Business Number. No organization matching the orgheaderpk '{guid}' could be found.]",
									Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
		}

		protected override ValueProvider GetNewValueProvider() => new LocalBusinessNumber();

		protected override void PrepareDataForExamplesEvaluate()
		{
			var factory = new BusinessObjectFactory();
			var testHeader = factory.New<OrgHeader>();
			testHeader.OH_Code = "XXXXXX";
			testHeader.MainAddress.OA_Address1 = "123 Test Way";
			testHeader.PrimaryRegistrationNumber.Number = "123 456 789 0";
			factory.Save();
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("OH_PK", testHeader.PK));
		}
	}
}
