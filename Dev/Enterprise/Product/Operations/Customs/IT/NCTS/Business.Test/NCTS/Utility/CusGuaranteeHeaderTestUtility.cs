using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

static class CusGuaranteeHeaderTestUtility
{
	public static CusGuaranteeHeader SetupGuarantee(BusinessObjectFactory factory, OrgHeader guaranteeHolder, string valueFrom = "", string bondNumber = "")
	{
		var guaranteeHeader = factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		guaranteeHeader.CPH_OH_PermitHolder = guaranteeHolder.PK;
		guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		guaranteeHeader.CPH_Number = bondNumber;
		guaranteeHeader.CPH_StartDate = ZDate.Today.AddYears(-20);
		guaranteeHeader.CPH_EndDate = ZDate.Today.AddYears(20);
		guaranteeHeader.CPH_QtyValIndicator = Customs.Business.PermitQtyValIndicatorList.Codes.QTY;
		guaranteeHeader.CPH_UnitOfMeasure = "KGM";
		guaranteeHeader.CPH_Type = "TRA";
		guaranteeHeader.CPH_SubType = "3";
		var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
		rule.CPR_RuleCode = "ADD";
		rule.CPR_ValueFrom = valueFrom;
		return guaranteeHeader;
	}
}
