using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRuleLookups : AutoComplianceRuleLookups
	{
		public ComplianceRuleLookups(AutoComplianceRule parent) : base(parent)
		{
		}

		#region RefCountry_List

		RefCountryCollection fRefCountry_List;
		public RefCountryCollection RefCountry_List
		{
			get
			{
				if (fRefCountry_List == null)
				{
					fRefCountry_List = new RefCountryCollection(Factory);
				}
				return fRefCountry_List;
			}
		}

		#endregion

		#region ComplianceRuleRiskStatusCodeList

		public CodeDescriptionPairList ComplianceRuleRiskStatusCodeList
		{
			get
			{
				return Factory.GetCachedValue("ComplianceRuleRiskStatusCodeList", () => new CodeDescriptionPairList() { new CodeDescriptionPair(ComplianceRiskStatusCodeList.Codes.Blocked, ComplianceRiskStatusCodeList.Descriptions.Blocked) });
			}
		}

		#endregion
	}
}
