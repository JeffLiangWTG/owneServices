using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class CusSupplyChainActorReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCFR_Reference_ValidEORIorTCUI()
		{
			const string errorMessage = "Additional Supply Chain Actor's Identification should have a valid EORI or TCUI format: A member state or third country code [a2] plus a unique identifier [an..15]";
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var supplyChainActorReference = entryInstruction.CusSupplyChainActorReferences.AddNew();
				supplyChainActorReference.CFR_Reference = "XXInvalid";
				AssertHasMessageErrorContaining("Invalid ref - no country identifier", supplyChainActorReference.CFR_ReferenceInfo, errorMessage);
				supplyChainActorReference.CFR_Reference = "FR1234567891234567";
				AssertHasMessageErrorContaining("Invalid ref - exceeds max length", supplyChainActorReference.CFR_ReferenceInfo, errorMessage);
				supplyChainActorReference.CFR_Reference = "GB123456789123456";
				AssertNoMessageErrorContaining("Valid TCUI", supplyChainActorReference.CFR_ReferenceInfo, errorMessage);
				supplyChainActorReference.CFR_Reference = "FR123456789123456";
				AssertNoMessageErrorContaining("Valid EORI", supplyChainActorReference.CFR_ReferenceInfo, errorMessage);
			});
		}
	}
}
