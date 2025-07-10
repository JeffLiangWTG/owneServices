using Enterprise.Customs.Common.IE;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(ExportEntryInstructionCusAuthorizationUsageValidationStrategy))]
	sealed class ExportEntryInstructionCusAuthorizationUsageValidationStrategyTest : CusAuthorizationUsageValidationTest
	{
		public void TestCheckAGC_Code()
		{
			var errorMessage = "Code 'CCL' is not allowed when Subtype in B,E";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

			entryInstruction.CEI_SubStyle = "C";
			cusAuthorizationUsage.AGC_Code = "CCL";
			AssertNoMessageError("CEI_SubStyle is not in B,E", cusAuthorizationUsage.AGC_CodeInfo, errorMessage);

			entryInstruction.CEI_SubStyle = "B";
			cusAuthorizationUsage.AGC_Code = "CCL";
			AssertHasMessageError("CCL Authorizations exist", cusAuthorizationUsage.AGC_CodeInfo, errorMessage);

			entryInstruction.CEI_SubStyle = "E";
			cusAuthorizationUsage.Validation.ValidateAGC_Code();
			AssertHasMessageError("CCL Authorizations exist", cusAuthorizationUsage.AGC_CodeInfo, errorMessage);

			cusAuthorizationUsage.AGC_Code = "ACE";
			AssertNoMessageError("CCL Authorizations not exist", cusAuthorizationUsage.AGC_CodeInfo, errorMessage);
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.Export;
	}
}
