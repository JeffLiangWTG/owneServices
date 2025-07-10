using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsWorkOrder))]
	sealed class DocWhsWorkOrderTest : DocWhsPickableDocketTest<WhsWorkOrder, DocWhsWorkOrder>
	{
		#region Properties

		public override void TestIsCustomsTransaction()
		{
			Assert("property not used by work orders", true);
		}

		protected override bool SupportsCarrierServiceLevel(WhsWorkOrder docket) => false;

		#region ZString Fields

		protected override void TestDockDoorLocationCore()
		{
			Assert("DDL is not used by Work Orders.", true);
		}

		public void TestWorkOrderLevelHeirarchyTree()
		{
			CombineAssertions(() =>
				{
					AssertNotNull("Precondition", MasterWorkOrder);
					AssertNotNull("Precondition", DocMasterWorkOrder);
					AssertNotNull("Precondition", DocChildWorkOrder);

					// Show Master Work Order levels
					for (int idx = 1; idx < 11; idx++)
					{
						AssertEquals("Master Work Order levels", true, GetWorkOrderLevel(DocMasterWorkOrder, idx).Contains(ZString.Format("Level {0}", idx)));
					}

					for (int idx = 1; idx < 11; idx++)
					{
						AssertEquals("1 level down Child Work Order levels", true, GetWorkOrderLevel(DocChildWorkOrder, idx).Contains(ZString.Format("Level {0}", idx + 1)));
					}
				});
		}

		#endregion

		#endregion

		#region Collections

		#region TestWorkOrderLines

		public void TestWorkOrderLines()
		{
			CombineAssertions(() =>
			   {
				   AssertNotNull("Precondition", MasterWorkOrder);
				   AssertNotNull("Precondition", DocMasterWorkOrder);
				   AssertNotNull("Precondition", DocChildWorkOrder);

				   AssertEquals("Master Collection populated", 10, DocMasterWorkOrder.WorkOrderLines.Count);
			   });
		}

		#endregion

		#region TestPackingLines

		protected override void TestPackingLines_SortCore()
		{
			Assert("Need to decide whatever sorting of PackingLines required for WorkOrder. If it is -- implement test.", true);
		}

		protected override void TestPackingLines_RollingUpCore()
		{
			Assert("Need to decide whatever Roll Up of PackingLines required for WorkOrder. If it is -- implement test.", true);
		}

		#endregion

		#endregion

		#region Implementation

		ZString GetWorkOrderLevel(DocWhsWorkOrder workOrderDocketWrapper, int level)
		{
			return workOrderDocketWrapper.WorkOrderLevelDescription(level);
		}

		protected override DocWhsWorkOrder CreateWhsDocketWrapper(WhsDocketLabelControl docketLabel)
		{
			return DocWhsWorkOrder.New(docketLabel, Factory);
		}

		protected override DocWhsWorkOrder CreateWhsDocketWrapper(WhsWorkOrder docket)
		{
			return DocWhsWorkOrder.New(docket, Factory);
		}

		#region Use the following properties but beware they are lazy loaded.

		WhsWorkOrder MasterWorkOrder
		{
			get
			{
				if (masterWorkOrder == null)
				{
					TestDataForBOM.CreateBOMProducts();
					TestDataForBOM.BOM.BulkLoadDeepLevelMasterChildWorkOrders(Docket, 15);
					masterWorkOrder = Docket;
				}
				return masterWorkOrder;
			}
		}
		WhsWorkOrder masterWorkOrder;

		DocWhsWorkOrder DocMasterWorkOrder
		{
			get { return docMasterWorkOrder ?? (docMasterWorkOrder = DocketWrapper); }
		}
		DocWhsWorkOrder docMasterWorkOrder;

		DocWhsWorkOrder DocChildWorkOrder
		{
			get { return docChildWorkOrder ?? (docChildWorkOrder = DocWhsWorkOrder.New(Factory.Load<WhsWorkOrder>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, MasterWorkOrder.PK))[0], Factory)); }
		}
		DocWhsWorkOrder docChildWorkOrder;

		TestDataForBOM TestDataForBOM
		{
			get { return testDataForBOM ?? (testDataForBOM = new TestDataForBOM(Factory)); }
		}
		TestDataForBOM testDataForBOM;

		#endregion

		#endregion
	}
}
