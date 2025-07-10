using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class YardUnitWrapperCollection : GenericWrapperCollection<YardUnitWrapper>
	{
		public YardUnitWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
