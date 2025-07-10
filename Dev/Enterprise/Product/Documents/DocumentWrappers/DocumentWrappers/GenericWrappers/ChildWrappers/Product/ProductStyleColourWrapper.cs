using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ProductStyleColourWrapper : GenericWrapper
	{
		public ProductStyleColourWrapper(WhsProductStyleColour styleColourBO, BusinessObjectFactory factory)
			: base(styleColourBO ?? factory.GetNull<WhsProductStyleColour>(), factory)
		{
		}

		#region Properties

		public ZString Code
		{
			get { return ColourBO.WSC_Code; }
		}

		public ZString Description
		{
			get { return ColourBO.WSC_Description; }
		}

		#endregion

		#region Related

		public ProductStyleWrapper Style
		{
			get { return style ?? (style = new ProductStyleWrapper(ColourBO.ProductStyle, Factory)); }
		}
		ProductStyleWrapper style;

		#endregion

		#region ColourBO

		WhsProductStyleColour ColourBO
		{
			get { return (WhsProductStyleColour)WrappedBO; }
		}

		#endregion
	}
}
