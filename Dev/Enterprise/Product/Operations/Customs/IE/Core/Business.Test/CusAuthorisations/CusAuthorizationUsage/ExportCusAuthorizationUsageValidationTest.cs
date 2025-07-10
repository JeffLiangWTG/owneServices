using Enterprise.Customs.Common.IE;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(ExportCusAuthorizationUsageValidation))]
	sealed class ExportCusAuthorizationUsageValidationTest : CusAuthorizationUsageValidationTest
	{
		public void TestCheckAGC_Code_UsesValidationStrategy()
		{
			const string message = "Bad wolf";
			const string codeWithError = "FAIL";
			const string codeWithoutError = "PASS";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			var validationStrategy = new Mock<IExportCusAuthorizationUsageValidationStrategy>();
			validationStrategy.Setup(x => x.GetRulesApplicableToCode(codeWithError)).Returns(new[] { message });
			var validation = new ExportCusAuthorizationUsageValidation(authorizationUsage, validationStrategy.Object);

			CombineAssertions(() =>
			{
				authorizationUsage.AGC_Code = codeWithError;
				validation.ValidateAGC_Code();
				AssertHasMessageError("With applicable error", authorizationUsage.AGC_CodeInfo, message);

				authorizationUsage.AGC_Code = codeWithoutError;
				validation.ValidateAGC_Code();
				AssertNoMessageError("Without applicable error", authorizationUsage.AGC_CodeInfo, message);
			});
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.Export;
	}
}
