//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoComplianceRiskStatusLookups
//
//    This class should be used for overriding collections in AutoComplianceRiskStatusLookups
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
	public class ComplianceRiskStatusLookups : AutoComplianceRiskStatusLookups
	{
		public ComplianceRiskStatusLookups(AutoComplianceRiskStatus parent) : base(parent)
		{
		}

		public CodeDescriptionPairList PartyRiskStatusCodes => Factory.GetCachedValue("ComplianceRiskStatusCodeList|PartyRiskStatus", ComplianceRiskStatusCodeList.GetPartyRiskStatusList);
		public CodeDescriptionPairList LocationRiskStatusCodes => Factory.GetCachedValue("ComplianceRiskStatusCodeList|LocationRiskStatus", ComplianceRiskStatusCodeList.GetLocationRiskStatusList);
		public CodeDescriptionPairList CommodityRiskStatusCodes => Factory.GetCachedValue("ComplianceRiskStatusCodeList|CommodityRiskStatus", ComplianceRiskStatusCodeList.GetComplianceCommodityRiskStatusList);
		public CodeDescriptionPairList OverallRiskStatusCodes => Factory.GetCachedValue("ComplianceRiskStatusCodeList|OverallRiskStatus", ComplianceRiskStatusCodeList.GetOverallRiskStatusList);
		public ComplianceRiskStatusCodeList AllRiskStatusCodes => Factory.GetCachedValue("ComplianceRiskStatusCodeList|AllRiskStatus", () => new ComplianceRiskStatusCodeList());
	}
}
