//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoComplianceCommodityDetailLookups
//
//    This class should be used for overriding collections in AutoComplianceCommodityDetailLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceCommodityDetailLookups : AutoComplianceCommodityDetailLookups
	{
		public ComplianceCommodityDetailLookups(AutoComplianceCommodityDetail parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CommodityRiskStatusCodeList
		{
			get
			{
				return (string)((ComplianceCommodityDetail)Parent).CCD_RiskStatus switch
				{
					ComplianceRiskStatusCodeList.Codes.Clear => Factory.GetCachedValue("CommodityRiskStatusCodeList|Clear", () => ComplianceRiskStatusCodeList.GetCommodityClearRiskStatusList()),
					ComplianceRiskStatusCodeList.Codes.PotentialRisk => Factory.GetCachedValue("CommodityRiskStatusCodeList|PotentialRisk", () => ComplianceRiskStatusCodeList.GetCommodityPotentialRiskStatusList()),
					ComplianceRiskStatusCodeList.Codes.NotChecked => Factory.GetCachedValue((NoResString)"CommodityRiskStatusCodeList|Not Checked", () => ComplianceRiskStatusCodeList.GetCommodityNotCheckedStatusList()),
					ComplianceRiskStatusCodeList.Codes.PossibleRisk => Factory.GetCachedValue("CommodityRiskStatusCodeList|PossibleRisk", () => ComplianceRiskStatusCodeList.GetCommodityPossibleRiskStatusList()),
					ComplianceRiskStatusCodeList.Codes.HighRisk => Factory.GetCachedValue("CommodityRiskStatusCodeList|HighRisk", () => ComplianceRiskStatusCodeList.GetCommodityHighRiskStatusList()),
					_ => Factory.GetCachedValue("CommodityRiskStatusCodeList|Other", () => ComplianceRiskStatusCodeList.GetCommodityBlockedReleasedStatusList()),
				};
			}
		}

		public CodeDescriptionPairList CommodityImportAlertForExportCodeList
		{
			get
			{
				return Factory.GetCachedValue("CommodityImportAlertForExportCodeList", () => ComplianceRiskStatusCodeList.GetImportAlertForExportJobStatusList());
			}
		}
	}
}
