using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class CusBRForeignOperatorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBFR_OH_Owner()
		{
			var foreignOperator = Factory.New<CusBRForeignOperator>();

			var owner = Factory.New<OrgHeader>();
			owner.OH_IsConsignor = false;
			owner.OH_IsConsignee = false;
			owner.OH_Code = "OWN";
			owner.OH_FullName = "TEST COMPANY1";
			owner.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "75.400.331/0001-15", Core.Constants.CountryCodes.Brazil);
			foreignOperator.BFR_OH_Owner = owner.PK;
			AssertHasMessageErrorContaining(foreignOperator.BFR_OH_OwnerInfo, "Please enter a Root CNPJ in this organization to continue.");
			AssertHasMessageErrorContaining(foreignOperator.BFR_OH_OwnerInfo, "An Owner should be designated as either a Consignor or a Consignee.");

			owner.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, "75400331", Core.Constants.CountryCodes.Brazil);
			owner.OH_IsConsignee = true;
			foreignOperator.BFR_OH_Owner = owner.PK;
			AssertNoMessageErrorContaining(foreignOperator.BFR_OH_OwnerInfo, "Please enter a Root CNPJ in this organization to continue.");
			AssertNoMessageErrorContaining(foreignOperator.BFR_OH_OwnerInfo, "An Owner should be designated as either a Consignor or a Consignee.");

			owner.OH_IsConsignee = false;
			owner.OH_IsConsignor = true;
			foreignOperator.BFR_OH_Owner = owner.PK;
			AssertNoMessageErrorContaining(foreignOperator.BFR_OH_OwnerInfo, "An Owner should be designated as either a Consignor or a Consignee.");
		}

		public void TestCheckBFR_OH_ForeignOperator()
		{
			var foreignOperator = Factory.New<CusBRForeignOperator>();
			var owner = Factory.New<OrgHeader>();
			owner.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Brazil;
			foreignOperator.BFR_OH_ForeignOperator = owner.PK;
			AssertHasMessageErrorContaining(foreignOperator.BFR_OH_ForeignOperatorInfo, "A Foreign Operator should not have its Country set to BR.");

			owner.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Australia;
			foreignOperator.BFR_OH_ForeignOperator = owner.PK;
			AssertNoMessageErrorContaining(foreignOperator.BFR_OH_ForeignOperatorInfo, "A Foreign Operator should not have its Country set to BR.");
			AssertNoErrorContaining(foreignOperator.BFR_OH_ForeignOperatorInfo, MandatoryValidation.MustBeEnteredMessage(foreignOperator.BFR_OH_ForeignOperatorInfo.HumanReadableName));

			foreignOperator.BFR_OH_ForeignOperator = ZGuid.Empty;
			AssertNoMessageErrorContaining(foreignOperator.BFR_OH_ForeignOperatorInfo, "A Foreign Operator should not have its Country set to BR.");
			AssertHasErrorContaining(foreignOperator.BFR_OH_ForeignOperatorInfo, MandatoryValidation.MustBeEnteredMessage(foreignOperator.BFR_OH_ForeignOperatorInfo.HumanReadableName));
		}
	}
}
