using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ProductStyleClassificationWrapper : GenericWrapper
	{
		public ProductStyleClassificationWrapper(WhsProductStyleClassification styleClassificationBO, BusinessObjectFactory factory)
			: base(styleClassificationBO ?? factory.GetNull<WhsProductStyleClassification>(), factory)
		{
		}

		#region Properties

		public ZString Code
		{
			get { return ClassificationBO.WSS_Code; }
		}

		public ZString Description
		{
			get { return ClassificationBO.WSS_Description; }
		}

		#endregion

		#region Related

		public ProductStyleWrapper Style
		{
			get { return style ?? (style = new ProductStyleWrapper(ClassificationBO.ProductStyle, Factory)); }
		}
		ProductStyleWrapper style;

		#endregion

		#region ClassificationBO

		WhsProductStyleClassification ClassificationBO
		{
			get { return (WhsProductStyleClassification)WrappedBO; }
		}

		#endregion
	}
}
