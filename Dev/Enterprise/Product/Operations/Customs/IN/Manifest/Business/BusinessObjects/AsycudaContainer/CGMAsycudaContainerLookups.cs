using CargoWise.Integration;
using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Manifest.Business;

public class CGMAsycudaContainerLookups : ASYCUDA.Business.AsycudaContainerLookups
{
	public CGMAsycudaContainerLookups(CGMAsycudaContainer parent)
		: base(parent)
	{
	}

	public OrganisationsFindBoxCollection ContainerAgentCodeOrganisations => new OrganisationsFindBoxCollection(Factory);

	public ICodeDescriptionPairList ISOCodeList => Factory.GetCachedValue<ISOContainerCodeList>();
}
