using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for PremiseIDMatcher.
	/// </summary>
	public static class PremiseIDMatcher
	{
		public static ZString ABNForCompany(GlbCompany company, ZString premiseID)
		{
			ZString result = "";
			OrgHeader proxy = null;

			foreach (GlbBranch branch in company.Branches)
			{
				if (OrgIsDepotWithPremiseID(branch.OrgProxy, premiseID))
				{
					proxy = branch.OrgProxy;
					break;
				}
			}

			if (proxy != null)
			{
				result = proxy.LocalBusinessRegNo;
			}

			return result;
		}

		public static bool CompanyHasDepotWithPremiseID(GlbCompany company, ZString premiseID)
		{
			bool result = false;
			foreach (GlbBranch branch in company.Branches)
			{
				result = OrgIsDepotWithPremiseID(branch.OrgProxy, premiseID);
				if (result)
				{
					break;
				}
			}
			return result;
		}

		public static bool OrgIsDepotWithPremiseID(OrgHeader organisation, ZString premiseID)
		{
			bool result = false;
			if (organisation != null)
			{
				foreach (OrgAddress address in organisation.Addresses)
				{
					if (address.LocalControlledPremisesID == premiseID)
					{
						result = organisation.OH_IsUnpackDepot;
						break;
					}
				}
			}
			return result;
		}
	}
}
