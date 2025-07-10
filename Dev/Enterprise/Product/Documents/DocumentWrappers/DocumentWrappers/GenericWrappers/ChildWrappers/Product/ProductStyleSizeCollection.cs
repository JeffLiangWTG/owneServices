using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ProductStyleSizeCollection : GenericWrapperCollection<ProductStyleSizeWrapper>
	{
		public ProductStyleSizeCollection(WhsProductStyle style, BusinessObjectFactory factory)
			: base(factory)
		{
			if (style != null)
			{
				var sizeWrappers = style.Sizes.OrderBy(s => s.WSZ_Sequence).Select(s => new ProductStyleSizeWrapper(s, factory));
				AddRange(sizeWrappers);
			}
		}
	}
}
