using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class OrganisationWrapperCollection : GenericWrapperCollection<OrganisationWrapper>
	{
		public OrganisationWrapperCollection(BusinessObjectFactory factory)
			: base(factory) { }
	}
}
