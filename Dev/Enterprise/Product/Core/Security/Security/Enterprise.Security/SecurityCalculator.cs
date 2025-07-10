using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Security
{
	public abstract class SecurityCalculator
	{
		protected internal SecurityState GetSecurityStateForOneLevel(GlbSecurityCollection securities, SecurityCheckpoint checkPoint, bool groupOwner, ZGuid ownerPk, ZGuid departmentPk, ZGuid branchPk, ZGuid companyPk)
		{
			SecurityState result = SecurityState.Implicit;
			GlbSecurity[] securityRights = securities.Find(GlbSecurityRightsLookupKey.ForLookup(checkPoint, ownerPk, departmentPk, branchPk, companyPk));

			if (securityRights.Length == 1)
			{
				result = securityRights[0].GU_SecurityItemIsAllowed ? SecurityState.Granted : SecurityState.Denied;
			}
			else if (securityRights.Length > 1)
			{
				bool atLeastOneAllowed = false;

				foreach (GlbSecurity securityItem in securityRights)
				{
					if (securityItem.GU_SecurityItemIsAllowed)
					{
						result = SecurityState.Granted;
						atLeastOneAllowed = true;
						break;
					}
				}

				if (!atLeastOneAllowed)
				{
					result = SecurityState.Denied;
				}
			}

			return result;
		}
	}
}
