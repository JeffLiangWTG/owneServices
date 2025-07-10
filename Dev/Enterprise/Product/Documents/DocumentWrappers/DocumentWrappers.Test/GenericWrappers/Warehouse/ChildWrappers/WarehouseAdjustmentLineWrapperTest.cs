using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseAdjustmentLineWrapper))]
	sealed class WarehouseAdjustmentLineWrapperTest : WarehouseDocketLineWrapperTest
	{
		#region TestReasonCode

		public void TestReasonCode()
		{
			var adjustmentLine = Factory.New<WhsAdjustmentLine>();
			adjustmentLine.WE_ReasonCode = "SHR";
			var wrapper = new WarehouseAdjustmentLineWrapper(adjustmentLine, Factory);
			AssertEquals("SHR", wrapper.ReasonCode);
			wrapper = new WarehouseAdjustmentLineWrapper(null, Factory);
			AssertEquals("", wrapper.ReasonCode);
		}

		#endregion

		#region TestReasonDescription

		public void TestReasonDescription()
		{
			var adjustmentLine = Factory.New<WhsAdjustmentLine>();
			adjustmentLine.WE_ReasonCode = "SHR";
			var wrapper = new WarehouseAdjustmentLineWrapper(adjustmentLine, Factory);
			AssertEquals("Shrinkage", wrapper.ReasonDescription);
			wrapper = new WarehouseAdjustmentLineWrapper(null, Factory);
			AssertEquals("", wrapper.ReasonDescription);
		}

		#endregion

		#region Implementation

		#region Business Objects For Testing

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

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsAdjustment>();
		}

		protected override WarehouseGenericLineWrapper GetNewWarehouseLineWrapper(BusinessObject bizO)
		{
			return new WarehouseAdjustmentLineWrapper((WhsAdjustmentLine)bizO, Factory);
		}

		#endregion

		#endregion
	}
}
