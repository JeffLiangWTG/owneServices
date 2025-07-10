using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class LocalTransportOrganisationWrapper : OrganisationWrapper
	{
		public LocalTransportOrganisationWrapper(OrganisationUsageType usageType, ZString localTransportCompanyLabel, BusinessObjectFactory factory)
			: base(usageType, ZString.Empty, factory)
		{
			OrganisationBO.OH_Code = localTransportCompanyLabel;
		}
	}
}
