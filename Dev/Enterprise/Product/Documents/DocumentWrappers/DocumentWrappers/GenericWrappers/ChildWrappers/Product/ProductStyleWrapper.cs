using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ProductStyleWrapper : GenericWrapper
	{
		public ProductStyleWrapper(WhsProductStyle styleBO, BusinessObjectFactory factory)
			: base(styleBO ?? factory.GetNull<WhsProductStyle>(), factory)
		{
		}

		#region Properties

		public ZString Code
		{
			get { return StyleBO.WST_Code; }
		}

		public ZString Description
		{
			get { return StyleBO.WST_Description; }
		}

		#endregion

		#region Related

		public ProductStyleSizeCollection Sizes
		{
			get { return sizes ?? (sizes = new ProductStyleSizeCollection(StyleBO, Factory)); }
		}
		ProductStyleSizeCollection sizes;

		#endregion

		#region StyleBO

		WhsProductStyle StyleBO
		{
			get { return (WhsProductStyle)WrappedBO; }
		}

		#endregion

	}
}
