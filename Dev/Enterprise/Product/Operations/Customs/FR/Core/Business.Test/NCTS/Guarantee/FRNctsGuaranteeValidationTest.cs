using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class FRNctsGuaranteeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPW_BondNumber()
		{
			NCTSTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.France);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var nctsGuarantee = nctsHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.FlatRateVoucher;
			nctsGuarantee.PW_BondNumber = "123456789012345678901234";
			nctsGuarantee.Validation.ValidatePW_BondNumber();
			AssertNoMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "Guarantee reference must be ");

			nctsGuarantee.PW_BondNumber = "12345678901234567890123";
			AssertHasMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "Guarantee reference must be ");

			nctsGuarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
			nctsGuarantee.Validation.ValidatePW_BondNumber();
			AssertHasMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "Guarantee reference must be ");

			nctsGuarantee.PW_BondNumber = "12345678901234567";
			nctsGuarantee.Validation.ValidatePW_BondNumber();
			AssertNoMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "Guarantee reference must be ");

			nctsGuarantee.PW_BondNumber = "1234567890123456";
			AssertHasMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "Guarantee reference must be ");

			nctsGuarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee;
			nctsGuarantee.Validation.ValidatePW_BondNumber();
			AssertNoMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "Guarantee reference must be ");

			nctsGuarantee.PW_BondType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Type = GuaranteeTypeList.Codes.COD;
			guarantee.CPH_SubType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiver;
			guarantee.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			guarantee.CPH_Number = "1234567890123456";
			guarantee.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			guarantee.CPH_StartDate = ZDate.Today;
			guarantee.CPH_OH_PermitHolder = orgHeader.PK;
			nctsHeader.Principal.E2_OA_Address = orgHeader.MainAddress.PK;

			Factory.Save();

			nctsGuarantee = nctsHeader.Guarantees.Single();
			nctsGuarantee.Validation.ValidatePW_BondNumber();
			AssertNoMessageErrorContaining(nctsGuarantee.PW_BondNumberInfo, "Guarantee reference must be ");
		}
	}
}
