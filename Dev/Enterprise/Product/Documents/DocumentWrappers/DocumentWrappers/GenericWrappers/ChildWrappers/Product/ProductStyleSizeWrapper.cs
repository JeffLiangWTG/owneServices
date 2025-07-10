using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ProductStyleSizeWrapper : GenericWrapper
	{
		public ProductStyleSizeWrapper(WhsProductStyleSize styleSizeBO, BusinessObjectFactory factory)
			: base(styleSizeBO ?? factory.GetNull<WhsProductStyleSize>(), factory)
		{
		}

		#region Properties

		public ZByte Sequence
		{
			get { return SizeBO.WSZ_Sequence; }
		}

		public ZString Size
		{
			get { return SizeBO.WSZ_Size; }
		}

		#endregion

		#region Related

		public ProductStyleWrapper Style
		{
			get { return style ?? (style = new ProductStyleWrapper(SizeBO.ProductStyle, Factory)); }
		}
		ProductStyleWrapper style;

		#endregion

		#region SizeBO

		WhsProductStyleSize SizeBO
		{
			get { return (WhsProductStyleSize)WrappedBO; }
		}

		#endregion
	}
}
