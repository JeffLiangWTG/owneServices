using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.GUI.Testing
{
	public static class NctsTestDataProvider
	{
		public static (CusGuaranteeHeader cusGuaranteeHeader, NctsGuarantee nctsGuarantee) CreateNctsGuaranteeWithCusGuaranteeHeader(BusinessObjectFactory factory)
		{
			EU.NCTS.Business.Testing.NCTSTestHelper.SetupC0009ForEuAndCtCountries(factory);
			var cusGuaranteeHeader = factory.NewWithValidTestData<CusGuaranteeHeader>();
			cusGuaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			cusGuaranteeHeader.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			cusGuaranteeHeader.CPH_Number = "ABC123";
			cusGuaranteeHeader.CPH_SubType = "1";
			cusGuaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			cusGuaranteeHeader.CPH_EndDate = ZDate.Today.AddDays(1);

			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.Principal.E2_OA_Address = cusGuaranteeHeader.PermitHolder.MainAddress.PK;
			var nctsGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
			nctsGuarantee.PW_BondType = "1";
			nctsGuarantee.PW_BondNumber = "ABC123";
			nctsGuarantee.PW_Password = "DEF";

			return (cusGuaranteeHeader, nctsGuarantee);
		}
	}
}
