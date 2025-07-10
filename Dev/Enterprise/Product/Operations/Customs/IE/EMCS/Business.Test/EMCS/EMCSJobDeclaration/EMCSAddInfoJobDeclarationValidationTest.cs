using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSAddInfoJobDeclaration))]
	class EMCSAddInfoJobDeclarationValidationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTypeOfValidation()
		{
			var infoDeclaration = (EMCSAddInfoJobDeclaration)GetNewBusinessObject();
			AssertType<EMCSAddInfoJobDeclarationValidation>(infoDeclaration.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EMCSAddInfoJobDeclaration(declaration);
		}

		public void TestCheckZG_DispatchReference()
		{
			declaration.AddInfoValidation.ValidateZG_DispatchReference();
			AssertNoMessageErrors("Validation for Dispatch Reference is not required for IE - No message errors expected", declaration.ZG_DispatchReferenceInfo);
		}

		public void TestCheckZG_CCTMSA()
		{
			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee;
			declaration.ZG_CCTMSA = "IE";
			declaration.Validation.ValidateAll();
			AssertHasMessageError(declaration.ZG_CCTMSAInfo, "Member State should only be entered when Destination Type = 5 - Destination - Exempted consignee.");

			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExemptedConsignee;
			declaration.ZG_CCTMSA = "IE";
			declaration.Validation.ValidateAll();
			AssertNoMessageErrors(declaration.ZG_CCTMSAInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
	}
}
