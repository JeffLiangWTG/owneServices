using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	public static class NctsTestDataProvider
	{
		public static (CusGuaranteeHeader cusGuaranteeHeader, NctsGuarantee nctsGuarantee) CreateNctsGuaranteeWithCusGuaranteeHeader(BusinessObjectFactory factory, string movementType = NctsMovementType.Codes.Departure)
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(factory);

			var cusGuaranteeHeader = CreateCusGuaranteeHeader(factory, "ABC123", ZGuid.Empty);

			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(movementType);
			nctsHeader.Principal.E2_OA_Address = cusGuaranteeHeader.PermitHolder.MainAddress.PK;

			NctsGuarantee nctsGuarantee;
			nctsGuarantee = nctsHeader.IsPhase5Departure ? nctsHeader.MovementHeader.Guarantees[0] : nctsHeader.Guarantees[0];
			nctsGuarantee.PW_BondType = "1";
			nctsGuarantee.PW_Password = "DEF";
			nctsGuarantee.PW_CPH_Guarantee = cusGuaranteeHeader.PK;
			nctsGuarantee.PW_Override = true;
			nctsGuarantee.PW_BondAmount = 25000m;

			return (cusGuaranteeHeader, nctsGuarantee);
		}

		internal static CusGuaranteeHeader CreateCusGuaranteeHeader(BusinessObjectFactory factory, string number, ZGuid holderPK)
		{
			var cusGuaranteeHeader = factory.NewWithValidTestData<CusGuaranteeHeader>();
			cusGuaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			cusGuaranteeHeader.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			cusGuaranteeHeader.CPH_SubType = "1";
			cusGuaranteeHeader.CPH_Number = number;
			cusGuaranteeHeader.CPH_StartDate = ZDate.Today.AddYears(-1);
			cusGuaranteeHeader.CPH_EndDate = ZDate.Today.AddYears(1);
			cusGuaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			if (holderPK.IsValid)
			{
				cusGuaranteeHeader.CPH_OH_PermitHolder = holderPK;
			}
			cusGuaranteeHeader.AddTransaction("0001", "Opening Balance", "", "", 100000m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL);
			cusGuaranteeHeader.CPH_Balance = 100000m;
			return cusGuaranteeHeader;
		}
	}
}
