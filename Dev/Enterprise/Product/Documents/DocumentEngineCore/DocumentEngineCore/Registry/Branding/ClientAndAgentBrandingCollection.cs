using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public abstract class ClientAndAgentBrandingCollection : RegistryBusinessObjectCollection
	{
		public ClientAndAgentBrandingCollection()
		{
		}

		public ClientAndAgentBrandingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ClientAndAgentBrandingBusinessObject this[int i]
		{
			get { return (ClientAndAgentBrandingBusinessObject)Elements[i]; }
		}

		public new ClientAndAgentBrandingBusinessObject FindByCode(string code)
		{
			return (ClientAndAgentBrandingBusinessObject)base.FindByCode(code);
		}
	}
}
