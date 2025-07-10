using System.Collections;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusAuthorizationUsageLookups(AutoCusAuthorizationUsage parent) : EU.Business.CusAuthorizationUsageLookups(parent)
{
	public override ICollection CodeList => Parent.Parent switch
		{
			CusExitReport report => GetAuthorizationUsageCodesForExitReport(report),
			CusExitReportItem => GetAuthorizationUsageCodesForExitReportItem(),
			_ => base.CodeList,
		};

	CodeDescriptionPairList GetAuthorizationUsageCodesForExitReport(CusExitReport report)
	{
		var countryCode = report.Header.Company?.GC_RN_NKCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		return Factory.GetCachedValue($"CusAuthorizationUsageLookups_ExitReport_{countryCode}", () =>
		{
			return Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(countryCode).GetAuthorisationTypeList(Factory);
		});
	}

	CodeDescriptionPairList GetAuthorizationUsageCodesForExitReportItem() => Factory.GetCachedValue("CusAuthorizationUsageLookups_ExitReportItem", () =>
	{
		var result = new CodeDescriptionPairList();

		var data = new CusAuthorizationUsageInvoiceLineCodeList();
		foreach (CodeDescriptionPair item in data)
		{
			result.Add(CusAuthorizationUsageLookupsHelper.GetAuthorizationUsageCodePairByCustomsCode(Factory, item));
		}

		result.SortByDescription();
		return result;
	});
}
