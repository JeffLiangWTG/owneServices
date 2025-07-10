using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DataTransfer.Business
{
	public enum IfUnmatched
	{
		ReturnUnmatchedOrganisation,
		ReturnNull,
		TakeBehaviourFromOverallSetting,
		TakeBehaviourFromUXMLModuleSepcifiedSetting,
	}

	public interface IOrgDefaultValueMatcher
	{
		OrgHeader Match();
	}

	public class NullDefaultValueMatcher : IOrgDefaultValueMatcher
	{
		public OrgHeader Match()
		{
			return null;
		}
	}

	public class OrgDefaultValueMatcher : BaseOrgDefaultValueMatcher
	{
		public OrgDefaultValueMatcher(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override OrgHeader Match()
		{
			OrgHeader result = null;

			if (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.IsEnabled)
			{
				result = base.Match();
			}
			return result;
		}
	}

	public class BaseOrgDefaultValueMatcher : IOrgDefaultValueMatcher
	{
		public BaseOrgDefaultValueMatcher(BusinessObjectFactory factory)
		{
			Factory = factory;
		}
		protected readonly BusinessObjectFactory Factory;

		public virtual OrgHeader Match()
		{
			return OrgHeader.UnmatchOrg(Factory);
		}
	}
}
