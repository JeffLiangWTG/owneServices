using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CodeAndDesriptionWrapperCollection : GenericWrapperCollection<CodeAndDescriptionWrapper>
	{
		public CodeAndDesriptionWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
