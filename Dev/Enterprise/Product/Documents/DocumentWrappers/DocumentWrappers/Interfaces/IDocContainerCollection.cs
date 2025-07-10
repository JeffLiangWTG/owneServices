using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class IDocContainerCollection : DocumentWrapperCollection
	{
		public IDocContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new IDocContainer this[int index]
		{
			get
			{
				return (IDocContainer)base[index];
			}
		}
	}
}
