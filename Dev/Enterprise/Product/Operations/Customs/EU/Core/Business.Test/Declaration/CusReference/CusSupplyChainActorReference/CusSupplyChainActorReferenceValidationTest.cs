using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class CusSupplyChainActorReferenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCFR_Reference_Mandatory_OwnerWithNoCusCodes()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABCZXY";
			const string errorMessage = "The selected Owner (ABCZXY) does not contain an EOR or TCU Customs Code";
			CombineAssertions(() =>
			{
				cusSupplyChainActorReference.CFR_OA_Owner = orgHeader.MainAddress.PK;
				cusSupplyChainActorReference.Validation.ValidateCFR_Reference();
				AssertHasErrorContaining("Has error message", cusSupplyChainActorReference.CFR_ReferenceInfo, errorMessage);
				cusSupplyChainActorReference.CFR_Reference = "EOR1";
				AssertNoErrorContaining("No error message2", cusSupplyChainActorReference.CFR_ReferenceInfo, errorMessage);
			});
		}

		public void TestCheckCFR_Reference_Length()
		{
			const string message = "The value entered must not exceed 17 characters";
			CombineAssertions(() =>
			{
				cusSupplyChainActorReference.CFR_Reference = ZString.Empty.PadLeft(18, 'A');
				AssertHasMessageErrorContaining("Has message error", cusSupplyChainActorReference.CFR_ReferenceInfo, message);
				cusSupplyChainActorReference.CFR_Reference = ZString.Empty.PadLeft(17, 'A');
				AssertNoMessageErrorContaining("No message error", cusSupplyChainActorReference.CFR_ReferenceInfo, message);
			});
		}

		public void TestCheckOwnerOrgPK_AEORequirement()
		{
			const string messageError = "Organization is missing a Registration Number of type 'AEO'.";
			var orgHeader = Factory.New<OrgHeader>();
			var propertyInfo = cusSupplyChainActorReference.OwnerOrgPKInfo;
			CombineAssertions(() =>
			{
				cusSupplyChainActorReference.OwnerOrgPK = orgHeader.PK;
				AssertHasMessageError("Missing AEO customs code", propertyInfo, messageError);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "regnum");
				cusSupplyChainActorReference.Validation.ValidateOwnerOrgPK();
				AssertNoMessageError("AEO customs code exists", propertyInfo, messageError);
			});
		}

		public void TestCodeMandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(cusSupplyChainActorReference.CFR_CodeInfo);
			cusSupplyChainActorReference.CFR_Code = "#@";
			AssertHasErrorContaining(cusSupplyChainActorReference.CFR_CodeInfo, ListValidation.InvalidCodeError);
			foreach (ICodeDescription data in new SupplyChainActorRoleList())
			{
				cusSupplyChainActorReference.CFR_Code = data.Code;
				AssertNoErrorContaining(cusSupplyChainActorReference.CFR_CodeInfo, ListValidation.InvalidCodeError);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusSupplyChainActorReference = Factory.New<CusSupplyChainActorReference>();
		}
		CusSupplyChainActorReference cusSupplyChainActorReference;
	}
}
