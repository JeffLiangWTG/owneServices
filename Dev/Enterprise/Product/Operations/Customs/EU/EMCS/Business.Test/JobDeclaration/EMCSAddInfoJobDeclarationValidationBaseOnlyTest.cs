namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class EMCSAddInfoJobDeclarationValidationBaseOnlyTest : EUEMCSAddInfoValidationTest
	{
		public void TestCheckZG_GuarantorType_GuarantorNotRequired()
		{
			const string errorMessage = "Guarantor(s) must be 0 when Destination Type is 1.";
			CombineAssertions(() =>
			{
				var declaration = Factory.New<EMCSJobDeclaration>();
				declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.Consignor;
				declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;
				declaration.AddInfoValidation.ValidateZG_GuarantorType();
				AssertHasMessageError("Validation requirements met", declaration.ZG_GuarantorTypeInfo, errorMessage);
				declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingSameMemberStateMovements;
				AssertNoMessageError("Guarantor Type not required", declaration.ZG_GuarantorTypeInfo, errorMessage);
				declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
				declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.Consignor;
				AssertNoMessageError("Message Subtype not required", declaration.ZG_GuarantorTypeInfo, errorMessage);
			});
		}
	}
}
