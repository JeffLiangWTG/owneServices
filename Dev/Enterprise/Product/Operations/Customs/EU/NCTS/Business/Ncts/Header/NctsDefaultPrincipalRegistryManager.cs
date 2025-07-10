using CargoWise.Common;
using Enterprise.Customs.EU.Registry;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsDefaultPrincipalRegistryManager
	{
		bool IsRegistryEnabled();

		void ApplyDefaultingIfEnabled();
	}

	public class NctsDefaultPrincipalRegistryManager : INctsDefaultPrincipalRegistryManager
	{
		public NctsDefaultPrincipalRegistryManager(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		readonly NctsHeader nctsHeader;

		public bool IsRegistryEnabled()
		{
			var nctsDefaultPrincipal = EUCustomsDataRegistry.Instance.NCTSDefaultPrincipal.Value;
			return nctsDefaultPrincipal.LeaveBlank || !nctsDefaultPrincipal.Principal.IsEmpty;
		}

		public void ApplyDefaultingIfEnabled()
		{
			if (!IsRegistryEnabled())
			{
				return;
			}
			nctsHeader.Principal.OrganisationPK = EUCustomsDataRegistry.Instance.NCTSDefaultPrincipal.Value.Principal;
		}
	}
}
