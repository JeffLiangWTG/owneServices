using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(ImportCusAuthorizationUsageValidation))]
	sealed class ImportCusAuthorizationUsageValidationTest : CusAuthorizationUsageValidationTest
	{
		public void TestValidateRuleBR2039()
		{
			var message = "[BR2039] If [12 12 002 000] Authorization type is 'REX', then [12 12 001 000] Reference Number must conform to the following Format: (A2)(REX)(AN1)(AN..29) where a) A2 represents 2 digit country code (capital letters), b) REX is the constant text, c) after the 'REX' constant there must be minimum 1 to maximum 30 characters. These characters can only be numeric 0..9 and/or alphas A..Z in capital letters.";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_JE = declaration.PK;
			var authorization = instruction.CusAuthorizationUsages.AddNew();

			var info = authorization.AGC_NumberInfo;
			CombineAssertions(() =>
			{
				authorization.AGC_Code = "XXX";
				authorization.AGC_Number = "1REX2";
				authorization.Validation.ValidateAGC_Number();
				AssertNoMessageErrorContaining(info, message);

				authorization.AGC_Code = "REX";
				authorization.Validation.ValidateAGC_Number();
				AssertHasMessageErrorContaining(info, message);

				authorization.AGC_Number = "IEREX";
				authorization.Validation.ValidateAGC_Number();
				AssertHasMessageErrorContaining(info, message);

				authorization.AGC_Number = "IERET123";
				authorization.Validation.ValidateAGC_Number();
				AssertHasMessageErrorContaining(info, message);

				authorization.AGC_Number = "IEREX12REX34ABC";
				authorization.Validation.ValidateAGC_Number();
				AssertNoMessageErrorContaining(info, message);
			});
		}

		public void TestCheckAGC_OH_Owner_Mandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "V1";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_JE = declaration.PK;
			instruction.CEI_Style = "I1";
			var authorization = instruction.CusAuthorizationUsages.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(authorization.AGC_OH_OwnerInfo, "[BR0339] You have not entered");

			declaration.JE_ApplicationCode = "V2";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(authorization.AGC_OH_OwnerInfo, "[BR0339] You have not entered");
		}

		public void TestCheckAGC_OH_Owner_UsesValidationStrategy()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			var orgHeader2 = Factory.New<OrgHeader>();
			declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader1.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var authorizationUsage = invoiceLine.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_OH_Owner = orgHeader2.PK;

			CombineAssertions(() =>
			{
				authorizationUsage.AGC_OH_Owner = orgHeader1.PK;
				AssertNoMessageErrorContaining("Same organization and applicable error", authorizationUsage.AGC_OH_OwnerInfo, "BR1031");
				AssertNoMessageErrorContaining("Same organization and applicable error", authorizationUsage.AGC_OH_OwnerInfo, "BR1130");

				authorizationUsage.AGC_OH_Owner = orgHeader2.PK;
				AssertNoMessageErrorContaining("Different organization empty requested procedure, no message.", authorizationUsage.AGC_OH_OwnerInfo, "BR1031");
				AssertNoMessageErrorContaining($"Different organization and requested procedure 44, should have message.", authorizationUsage.AGC_OH_OwnerInfo, "BR1130");

				invoiceLine.JI_Procedure = "440000";
				authorizationUsage.Validation.ValidateAGC_OH_Owner();
				AssertHasMessageErrorContaining($"Different organization and requested procedure 44, should have message.", authorizationUsage.AGC_OH_OwnerInfo, "BR1031");
				AssertHasMessageErrorContaining($"Different organization and requested procedure 44, should have message.", authorizationUsage.AGC_OH_OwnerInfo, "BR1130");
			});
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.Import;
	}
}
