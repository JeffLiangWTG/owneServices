using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	public class CusGoodsLocationValidationTest : TestCaseWithFactory
	{
		public void TestCheckUnlocode_RuleC0061_UCC5_UCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			foreach (var appCode in new[] { ImportDeclarationApplicationCodeList.Codes.V1, ImportDeclarationApplicationCodeList.Codes.V2 })
			{
				declaration.JE_ApplicationCode = appCode;

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var goodsLocation = instruction.GoodsLocation;

				var validation = new CusGoodsLocationValidation(goodsLocation);

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				validation.ValidateCGL_CustomsOffice();

				AssertHasMessageErrorContaining(
					"Should trigger C0061 on UNLOCODE field",
					goodsLocation.CGL_CustomsOfficeInfo,
					"[C0061]"
				);

				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.PostcodeAddress;
				validation.ValidateCGL_CustomsOffice();

				AssertNoMessageErrorContaining(
					"Should not trigger C0061 on UNLOCODE field if qualifier isn't U",
					goodsLocation.CGL_CustomsOfficeInfo,
					"[C0061]"
				);
			}
		}

		public void TestNoRuleOnAdditionalIdentifierWithQualifierU()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			foreach (var appCode in new[] { ImportDeclarationApplicationCodeList.Codes.V1, ImportDeclarationApplicationCodeList.Codes.V2 })
			{
				declaration.JE_ApplicationCode = appCode;

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var goodsLocation = instruction.GoodsLocation;
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;

				goodsLocation.Validation.ValidateCGL_AdditionalIdentifier();
				AssertNoMessageErrors("Should have no error on Additional Identifier", goodsLocation.CGL_AdditionalIdentifierInfo);
			}
		}
	}
}
