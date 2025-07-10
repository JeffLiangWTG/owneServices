using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Res = Enterprise.UniversalDataBuss.DataObjects.Res;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	public class OrganizationAddressMatchPool
	{
		public OrganizationAddressMatchPool()
		{
			this.matchCache = new Dictionary<OrganizationAddress, OrganizationMatchResult>(new OrganizationAddressMatchingComparator());
		}

		readonly Dictionary<OrganizationAddress, OrganizationMatchResult> matchCache;

		public OrganizationMatchResult GetMatch(OrganizationAddress organisationData, BusinessObjectFactory factory)
		{
			OrganizationMatchResult result;
			if (!matchCache.TryGetValue(organisationData, out result))
			{
				var matcher = ObjectFactory.New<IOrganizationAddressMatcher>();
				var matchedOrgHeader = matcher.GetMatchingOrgHeader(organisationData, factory);

				var orgDescription = GetDescription(organisationData);

				result = new OrganizationMatchResult();
				if (matchedOrgHeader != null)
				{
					result.SetMatch(matchedOrgHeader);
					result.Log(LogType.Information, Res.GetString("a765a14a-dead-41f9-b1c5-e82a689dd77f", "Incoming Organization [{0}] matched to {1} Organization Code [{2}].", orgDescription, Core.Constants.ProductName, matchedOrgHeader.OH_Code));
				}
				else
				{
					result.Log(LogType.Information, Res.GetString("eb926701-ead9-449f-a98a-3d983e18355d", "No Match Found for incoming Organization [{0}].", orgDescription));
				}

				matchCache.Add(organisationData, result);
			}

			return result;
		}

		static string GetDescription(OrganizationAddress organisationData)
		{
			ZString orgCode = organisationData.OrganizationCode.GetValueOrDefault();
			var orgName = organisationData.CompanyName.GetValueOrDefault();

			if (!orgName.IsEmpty)
			{
				if (!orgCode.IsEmpty)
				{
					return orgCode + "-" + orgName;
				}
				else
				{
					return orgName;
				}
			}
			else if (!orgCode.IsEmpty)
			{
				return orgCode;
			}

			return Res.GetString("9dfd40c4-cdf3-407c-8636-3d0b1a18f631", "(no name specified)");
		}
	}
}
