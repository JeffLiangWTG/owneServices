using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Registry.Business.OrganisationsDataRegistry;

namespace Enterprise.DataTransfer.Business
{
	public class OrgModuleValueMatcher : BaseOrgDefaultValueMatcher
	{
		readonly DataContextType moduleType;

		public OrgModuleValueMatcher(BusinessObjectFactory factory, DataContextType moduleType)
			: base(factory)
		{
			this.moduleType = moduleType;
		}

		public override OrgHeader Match()
		{
			OrgHeader result = null;

			if (IsJobTypeSetToUseUnmatchedOrgForMatching)
			{
				result = base.Match();
			}

			return result;
		}

		bool IsJobTypeSetToUseUnmatchedOrgForMatching
		{
			get
			{
				var result = false;

				if (moduleType == DataContextType.OrderManagerOrder)
				{
					result = Instance.UnmatchedOrganisationConfiguration.Value.GetBoolFromCode(JobTypeCodes.Order);
				}

				return result;
			}
		}
	}
}
