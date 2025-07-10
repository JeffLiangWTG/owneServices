using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class IDocSimpleContainerCollection : DocumentWrapperCollection
	{
		public IDocSimpleContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new IDocSimpleContainer this[int index]
		{
			get
			{
				return (IDocSimpleContainer)base[index];
			}
		}
	}
}
