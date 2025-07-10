using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	sealed class DocBaseCusContainerTestClass : DocBaseCusContainer
	{
		DocBaseCusContainerTestClass(BaseCusContainer cusContainer, BusinessObjectFactory factoryToWrap)
			: base(cusContainer, factoryToWrap)
		{
		}

		public static DocBaseCusContainerTestClass New(BaseCusContainer cusContainer, BusinessObjectFactory factoryToWrap)
		{
			if (cusContainer == null)
			{
				return null;
			}
			else
			{ return new DocBaseCusContainerTestClass(cusContainer, factoryToWrap); }
		}
	}
}
