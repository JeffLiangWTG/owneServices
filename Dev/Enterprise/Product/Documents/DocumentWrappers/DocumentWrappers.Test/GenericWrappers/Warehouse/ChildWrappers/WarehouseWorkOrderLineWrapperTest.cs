using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse.ChildWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseWorkOrderLineWrapper))]
	sealed class WarehouseWorkOrderLineWrapperTest : WarehouseDocketLineWrapperTest
	{
		#region Properties

		#region TestStagingAreaName

		public void TestStagingAreaName()
		{
			var data = new TestDataForBOM(Factory);
			data.BOM.CreateBOMProducts();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.BikeWheel, 2m);
			var productParams = workOrderLine.Product.ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_WL_StagingLocationBOM = data.Whs1.DefaultLocation.PK;

			var wrapper = new WarehouseWorkOrderLineWrapper(workOrderLine, Factory);
			AssertEquals(data.Whs1.DefaultLocation.WLV_LocationString, wrapper.StagingAreaName);
		}

		#endregion

		public void TestBomLevel()
		{
			AssertEquals("BOM level first level", 0, BikeLineWrapper.BOMLevel);
			AssertEquals("BOM level second level", 1, BikeWheelLineWrapper.BOMLevel);
		}

		public void TestIsTopLevelOrOddIndex()
		{
			AssertEquals("Is Top Level Or Odd Index first level", "Y", BikeLineWrapper.IsTopLevelOrOddIndex);
			AssertEquals("Is Top Level Or Odd Index second level", "N", BikeWheelLineWrapper.IsTopLevelOrOddIndex);
		}

		public void TestIsTopLevelOrEvenIndex()
		{
			AssertEquals("Is Top Level Or Even Index first level", "Y", BikeLineWrapper.IsTopLevelOrEvenIndex);
			AssertEquals("Is Top Level Or Even Index second level", "Y", BikeWheelLineWrapper.IsTopLevelOrEvenIndex);
		}

		public void TestAttributes()
		{
			CombineAssertions(() =>
			{
				AssertEquals("DocWrapper Attributes shoule be empty", ZString.Empty, WorkOrderLineWrapper.Attributes);

				OrgHeader orgHeader = Factory.New<OrgHeader>();
				WorkOrderLine.Docket.WD_OH_Client = orgHeader.PK;
				WorkOrderLine.WE_PartAttrib1 = ZString.Empty;
				AssertEquals("DocWrapper Attributes shoule be empty", ZString.Empty, WorkOrderLineWrapper.Attributes);

				WorkOrderLine.WE_PartAttrib1 = "TEST";
				WorkOrderLine.Docket.Client.MiscServ.OM_IMPartAttrib1Name = ZString.Empty;
				AssertEquals("DocWrapper Attributes is correct", "Attribute 1: TEST", WorkOrderLineWrapper.Attributes);

				WorkOrderLine.WE_PartAttrib1 = string.Empty;
				WorkOrderLine.WE_SerialNumber = "SERN";
				AssertEquals("DocWrapper Attributes is correct", "Tracked Serial Number: SERN", WorkOrderLineWrapper.Attributes);

				WorkOrderLine.WE_PartAttrib1 = "TEST";
				WorkOrderLine.Docket.Client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
				AssertEquals("DocWrapper Attributes is correct", "LABEL: TEST,   Tracked Serial Number: SERN", WorkOrderLineWrapper.Attributes);

				WorkOrderLine.WE_PartAttrib2 = "TEST2";
				WorkOrderLine.Docket.Client.MiscServ.OM_IMPartAttrib2Name = "LABEL2";
				AssertEquals("DocWrapper Attributes is correct", "LABEL: TEST,   LABEL2: TEST2,   Tracked Serial Number: SERN", WorkOrderLineWrapper.Attributes);

				WorkOrderLine.WE_PartAttrib3 = "TEST3";
				WorkOrderLine.Docket.Client.MiscServ.OM_IMPartAttrib3Name = "LABEL3";
				AssertEquals("DocWrapper Attributes is correct", "LABEL: TEST,   LABEL2: TEST2,   LABEL3: TEST3,   Tracked Serial Number: SERN", WorkOrderLineWrapper.Attributes);

				WorkOrderLine.WE_PartAttrib1 = ZString.Empty;
				WorkOrderLine.Docket.Client.MiscServ.OM_IMPartAttrib1Name = ZString.Empty;
				AssertEquals("DocWrapper Attributes is correct", "LABEL2: TEST2,   LABEL3: TEST3,   Tracked Serial Number: SERN", WorkOrderLineWrapper.Attributes);

				WorkOrderLine.WE_PartAttrib2 = ZString.Empty;
				WorkOrderLine.WE_SerialNumber = ZString.Empty;
				WorkOrderLine.Docket.Client.MiscServ.OM_IMPartAttrib2Name = ZString.Empty;
				AssertEquals("DocWrapper Attributes is correct", "LABEL3: TEST3", WorkOrderLineWrapper.Attributes);
			});
		}

		#endregion

		#region Implementation

		WhsWorkOrderLine WorkOrderLine
		{
			get { return workOrderLine ?? (workOrderLine = WorkOrder.Lines.AddNew()); }
		}
		WhsWorkOrderLine workOrderLine;

		WarehouseWorkOrderLineWrapper WorkOrderLineWrapper
		{
			get { return workOrderLineWrapper ?? (workOrderLineWrapper = new WarehouseWorkOrderLineWrapper(WorkOrderLine, Factory)); }
		}
		WarehouseWorkOrderLineWrapper workOrderLineWrapper;

		WarehouseWorkOrderLineWrapper BikeWheelLineWrapper
		{
			get
			{
				if (bikeWheelLineWrapper == null)
				{
					bikeWheelLineWrapper = new WarehouseWorkOrderLineWrapper(Data.BOM.Lines.BikeWheel(WorkOrder), Factory);
					bikeWheelLineWrapper.Index = 2;
				}
				return bikeWheelLineWrapper;
			}
		}
		WarehouseWorkOrderLineWrapper bikeWheelLineWrapper;

		WarehouseWorkOrderLineWrapper BikeLineWrapper
		{
			get
			{
				if (bikeLineWrapper == null)
				{
					bikeLineWrapper = new WarehouseWorkOrderLineWrapper(Helper.CreateWhsWorkOrderLine(WorkOrder, data.BOM.Bike, 2m), Factory);
					bikeLineWrapper.Index = 1;
				}
				return bikeLineWrapper;
			}
		}
		WarehouseWorkOrderLineWrapper bikeLineWrapper;

		WhsWorkOrder WorkOrder
		{
			get { return workOrder ?? (workOrder = Helper.CreateWhsWorkOrder(Data.Org1, Data.Whs1)); }
		}
		WhsWorkOrder workOrder;

		TestDataForBOM Data
		{
			get
			{
				if (data == null)
				{
					data = new TestDataForBOM(Factory);
					data.CreateBOMProductsInInventory();
				}
				return data;
			}
		}
		TestDataForBOM data;

		protected override WarehouseGenericLineWrapper GetNewWarehouseLineWrapper(BusinessObject whsLineBO)
		{
			return new WarehouseWorkOrderLineWrapper((WhsWorkOrderLine)whsLineBO, Factory);
		}

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
			return GetAndSetWarehouseWorkOrderLineWrapper();
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return GetAndSetWarehouseWorkOrderLineWrapper();
		}

		WarehouseWorkOrderLineWrapper GetAndSetWarehouseWorkOrderLineWrapper()
		{
			var workOrder = Factory.New<WhsWorkOrder>();
			var workOrderLine = workOrder.Lines.AddNew();
			var workOrderLineWrapper = new WarehouseWorkOrderLineWrapper(workOrderLine, Factory);
			return workOrderLineWrapper;
		}

		#endregion

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsWorkOrder>();
		}
	}
}
