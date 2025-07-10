using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseEmptyJobLineWrapper))]
	sealed class WarehouseEmptyJobLineWrapperTest : WarehouseGenericLineWrapperTest
	{
		#region TestNoOfAttribsAndAttributePrintSizeFactor

		protected override bool CanSetProductAttributesInWrapperCore => false;

		#endregion

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CrossDockConsigneeAddress : 
DangerousGoodsSubstance :  is null
ExtendedLinePrice :  is null
ManufacturerAddress : 
Product : (No Default Field Value Available on Product)
RecommendedUnitPrice : 
Registry : (No Default Field Value Available on Registry)
UnitDiscountAmount : 
UnitDiscountPercent : 
UnitPriceAfterDiscount : 
UnitsMet : 
UnitsOrdered : 
UnitsPicked : 
UnitsShort :
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new WarehouseEmptyJobLineWrapper(null, Factory);
		}

		protected override WarehouseGenericLineWrapper GetNewWarehouseLineWrapper(BusinessObject biz)
		{
			return (WarehouseGenericLineWrapper)GetNewDocumentWrapper();
		}

		//WarehouseEmptyJobLineWrapper WarehouseEmptyJobLineWrapper
		//{
		//    get { return warehouseEmptyJobLineWrapper ?? (warehouseEmptyJobLineWrapper = (WarehouseEmptyJobLineWrapper)GetNewDocumentWrapper());  }
		//}
		//WarehouseEmptyJobLineWrapper warehouseEmptyJobLineWrapper;

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new WarehouseEmptyJobLineWrapper(null, Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return null;
		}
	}
}
