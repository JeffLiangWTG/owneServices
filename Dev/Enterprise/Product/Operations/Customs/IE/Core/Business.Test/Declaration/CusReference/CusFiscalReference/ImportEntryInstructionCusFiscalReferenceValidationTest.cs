using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportEntryInstructionCusFiscalReferenceValidation))]
	class ImportEntryInstructionCusFiscalReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCEI_SubStyle_BR8063_NoAmend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AISEntryStatusList.Codes.Accepted;
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			var fiscalReference = instruction.FiscalReferences.AddNew();
			fiscalReference.CFR_Reference = "A";
			Factory.Save();

			fiscalReference = NewFactory().Load<CusFiscalReference>(fiscalReference.PK);
			fiscalReference.CFR_Reference = "B";
			AssertHasMessageErrorContaining("Amend check.", fiscalReference.CFR_ReferenceInfo, CommonResStrings.ShouldNotAmendThisValue);
			fiscalReference.Instruction.ValidationModes = EU.Business.Declaration.ValidationModes.None;
			fiscalReference.Validation.ValidateCFR_Reference();
			AssertNoMessageErrorContaining("Skip Amend check when ValidationModes is NONE.", fiscalReference.CFR_ReferenceInfo, CommonResStrings.ShouldNotAmendThisValue);
			instruction.ValidationModes = EU.Business.Declaration.ValidationModes.Amendment;

			fiscalReference.CFR_Reference = "A";
			AssertNoMessageErrorContaining("Amend check(changed back to same as in the ouggoing message, validation passes).", fiscalReference.CFR_ReferenceInfo, CommonResStrings.ShouldNotAmendThisValue);
		}
	}
}
