using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DataTransfer.Business
{
	public interface IOrgSimilarMatcher
	{
		OrgHeader GetMatchingOrganisation(IOrgHeaderForMatching temporaryOrganisation, bool shouldIncludeUnmatched = true, ISimpleLogger logger = null);
		OrgAddress GetMatchingAddress(IOrgHeaderForMatching temporaryOrganisation, bool shouldIncludeUnmatched = true, ISimpleLogger logger = null);
	}

	public class OrgSimilarMatcher : IOrgSimilarMatcher
	{
		public OrgSimilarMatcher(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public OrgHeader GetMatchingOrganisation(IOrgHeaderForMatching temporaryOrganisation, bool shouldIncludeUnmatched = true, ISimpleLogger logger = null)
		{
			var bestMatch = GetBestMatch(temporaryOrganisation, shouldIncludeUnmatched, logger ?? new Enterprise.UniversalDataBuss.Integration.DummyLogger());

			if (bestMatch != null)
			{
				return factory.Load<OrgHeader>(bestMatch.OS_OH);
			}

			return null;
		}

		public OrgAddress GetMatchingAddress(IOrgHeaderForMatching temporaryOrganisation, bool shouldIncludeUnmatched = true, ISimpleLogger logger = null)
		{
			Argument.NotNull(logger, "logger");
			var bestMatch = GetBestMatch(temporaryOrganisation, shouldIncludeUnmatched, logger);

			if (bestMatch != null)
			{
				return factory.Load<OrgAddress>(bestMatch.OS_OA);
			}

			return null;
		}

		OrgPatternMatch GetBestMatch(IOrgHeaderForMatching temporaryOrganisation, bool shouldIncludeUnmatched, ISimpleLogger logger)
		{
			if (temporaryOrganisation == null)
			{
				logger.Log(LogType.Information, Res.GetString("D54040DD-CE7C-419E-AF74-E7E6FE9E94F4", "No organization data provided to match."));
				return null;
			}

			var finder = temporaryOrganisation.SimilarOrgFinder;
			finder.FindSimilarOrganisations(shouldIncludeUnmatched);
			if (!finder.LikelyMatchFound)
			{
				logger.Log(LogType.Warning, Res.GetString("44916ED2-7105-4D65-BAA9-28F1092C9E50", "No match found for '[{0}]'.", DisplayMessageOfOrganizationMatchingData(temporaryOrganisation)));
				return null;
			}

			var patternMatch = temporaryOrganisation.SimilarOrgMatches.OfType<OrgPatternMatch>().FirstOrDefault(m => m.OS_Rank == 1);

			if (patternMatch == null)
			{
				return null;
			}

			logger.Log(LogType.Information,
					   patternMatch.OS_OH == OrgHeader.UnmatchedOrganisationPK ?
							Res.GetString("a3f70db7-19ab-4bee-b84b-f153b23d7704", "No match found - Assigned to UNMATCHED organization (Code: {0})", OrgHeader.UnmatchedOrganisationCode) :
							Res.GetString("B80B202B-76A6-411B-A550-77A9BF7D61AC", "Matched to '{0}' address '{1}' with a score of {2}.", patternMatch.OH_Code, patternMatch.Address.OA_Code, patternMatch.OS_Score));

			return patternMatch;
		}

		static string DisplayMessageOfOrganizationMatchingData(IOrgHeaderForMatching matchingSource)
		{
			var result = new StringBuilder();
			result.Append(DisplayOrEmptyString(Res.GetString("c4e32fa0-6516-4b7f-ab80-e44521cdeb01", "Org. Code"), matchingSource.OH_Code));
			result.Append(DisplayOrEmptyString(Res.GetString("cc5ead7e-f1dd-4442-9cab-78beee67685c", "Company Name"), matchingSource.OH_FullName));

			var address = matchingSource.Addresses.FirstOrDefault();
			if (address != null)
			{
				result.Append(DisplayOrEmptyString(Res.GetString("ab69bbfd-d493-4f2d-88bf-d1aa600fef5c", "Address Code"), address.OA_Code));
				result.Append(DisplayOrEmptyString(Res.GetString("3104de7e-a859-49d2-992c-5b11a91f6bf5", "Address 1"), address.OA_Address1));
				result.Append(DisplayOrEmptyString(Res.GetString("bb555356-a7a3-4ed6-9b7e-ca5aacc9931c", "Address 2"), address.OA_Address2));
				result.Append(DisplayOrEmptyString(Res.GetString("ee1829da-7a90-4c38-8c07-abcfea8c97d5", "City"), address.OA_City));
			}

			return result.ToString().TrimEnd(new char[] { ';', ' ' });
		}

		static string DisplayOrEmptyString(string type, string value)
		{
			return string.IsNullOrEmpty(value) ? string.Empty : type + ": " + value + "; ";
		}
	}
}
