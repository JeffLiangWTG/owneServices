using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.TNT.NZ
{
	class NZOrgMatcher : OrgMatcher
	{
		public NZOrgMatcher(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AdditionalOrgPatternMatch(OrgHeader orgToMatch, OrgPatternMatch match)
		{
			bool result = true;

			if (orgToMatch != null && match != null)
			{
				BusinessObjectFactory tempFactory = PatternMatchFactory;
				OrgPatternMatch tempPatternMatch = tempFactory.New<OrgPatternMatch>();

				try
				{
					tempPatternMatch.EncodeName(new StringWithLanguage(orgToMatch.OH_FullName, orgToMatch.OH_Language), OrgPatternMatchGenerationHelper.Get(orgToMatch.OH_Language, orgToMatch.PortName, orgToMatch.CountryName), true);

					result = tempPatternMatch.OS_CompanyName1 == match.OS_CompanyName1 &&
						tempPatternMatch.OS_CompanyName2 == match.OS_CompanyName2 &&
						tempPatternMatch.OS_CompanyName3 == match.OS_CompanyName3 &&
						tempPatternMatch.OS_CompanyName4 == match.OS_CompanyName4;
				}
				finally
				{
					tempPatternMatch.Delete();
				}
			}

			return result;
		}
	}
}
