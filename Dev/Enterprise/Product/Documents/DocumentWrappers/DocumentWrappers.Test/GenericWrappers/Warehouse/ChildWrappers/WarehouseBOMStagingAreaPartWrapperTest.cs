using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseBOMStagingLocationPartWrapper))]
	sealed class WarehouseBOMStagingAreaPartWrapperTest : WarehouseGenericLineWrapperTest
	{
		#region Properties

		public void TestUnitsUQ()
		{
			AssertEquals("Precondition UnitsUQ empty.", ZString.Empty, DocWrapper.UnitsUQ);

			var workOrder = Helper.CreateWhsWorkOrderWithLine(Data.Org1, Data.Whs1, Data.BOM.BikeWheel, 1m);
			Helper.CreatePickNew(workOrder);

			var childComponentLine = workOrder.Lines[0].BOM.ChildComponentLines.ElementAt(0);
			WarehouseBOMStagingLocationPart.AddWorkOrderLine(childComponentLine);

			AssertEquals("Correct UnitsUQ.", childComponentLine.SupplierPart.OP_StockKeepingUnit, DocWrapper.UnitsUQ);
		}

		public void TestProductCode()
		{
			AssertEquals("Precondition PartNum empty.", ZString.Empty, DocWrapper.ProductCode);
			var workOrder = Helper.CreateWhsWorkOrderWithLine(Data.Org1, Data.Whs1, Data.BOM.BikeWheel, 1m);
			Helper.CreatePickNew(workOrder);

			var childComponentLine = workOrder.Lines[0].BOM.ChildComponentLines.ElementAt(0);
			WarehouseBOMStagingLocationPart.AddWorkOrderLine(childComponentLine);
			AssertEquals("Correct PartNum.", childComponentLine.SupplierPart.OP_PartNum, DocWrapper.ProductCode);
		}

		public void TestUnits()
		{
			AssertEquals("Precondition Units empty.", 0m, DocWrapper.Units);
			var workOrder = Helper.CreateWhsWorkOrderWithLine(Data.Org1, Data.Whs1, Data.BOM.BikeWheel, 1m);
			Helper.CreatePickNew(workOrder);

			var childComponentLine = workOrder.Lines[0].BOM.ChildComponentLines.ElementAt(0);
			WarehouseBOMStagingLocationPart.AddWorkOrderLine(childComponentLine);
			AssertEquals("Correct Units.", childComponentLine.PickLineQuantity, DocWrapper.Units);
		}

		#region TestNoOfAttribsAndAttributePrintSizeFactor

		protected override bool CanSetProductAttributesInWrapperCore => false;

		#endregion

		#endregion

		#region Abstract members Implementation

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

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WarehouseBOMStagingLocationPart();
		}

		protected override WarehouseGenericLineWrapper GetNewWarehouseLineWrapper(BusinessObject bizO)
		{
			WarehouseBOMStagingLocationPart objectToWrap = bizO as WarehouseBOMStagingLocationPart;
			return new WarehouseBOMStagingLocationPartWrapper(objectToWrap, Factory);
		}

		#endregion

		#region Implementation

		TestDataForBOM Data
		{
			get
			{
				if (data == null)
				{
					data = new TestDataForBOM(Factory);
					data.CreateBOMSubComponentsInInventory();
					data.BOM.SetPartStagingLocation(data.BOM.BikeWheelProduct, data.Whs1.Rows[0].Locations[0]);
					Factory.Save();
				}
				return data;
			}
		}

		WarehouseBOMStagingLocationPart WarehouseBOMStagingLocationPart
		{
			get { return bomStagingLocationPart ?? (bomStagingLocationPart = new WarehouseBOMStagingLocationPart()); }
		}

		WarehouseBOMStagingLocationPartWrapper DocWrapper
		{
			get { return docWrapper ?? (docWrapper = GetNewDocWrapper(WarehouseBOMStagingLocationPart)); }
			set { docWrapper = value; }
		}

		WarehouseBOMStagingLocationPartWrapper GetNewDocWrapper(WarehouseBOMStagingLocationPart objectToWrap)
		{
			return WarehouseBOMStagingLocationPartWrapper.New(objectToWrap, Factory);
		}

		TestDataForBOM data;
		WarehouseBOMStagingLocationPart bomStagingLocationPart;
		WarehouseBOMStagingLocationPartWrapper docWrapper;

		#endregion
	}
}
