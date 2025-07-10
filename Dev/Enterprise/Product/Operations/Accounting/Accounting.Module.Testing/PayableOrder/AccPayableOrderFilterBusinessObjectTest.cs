using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccPayableOrderFilterBusinessObject))]
	public class AccPayableOrderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestOrderNumberFilter()
		{
			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Order #"];
			filter.Property = order1.APH_OrderNumber;
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection = new AccPayableOrderHeaderCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(order1));

			filter.Property = order2.APH_OrderNumber;
			collection = new AccPayableOrderHeaderCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(order2));
		}

		[TestDate(2011, 11, 11)]
		public void TestDueDateFilter()
		{
			ModuleDateFilter filter = (ModuleDateFilter)FilterBO["Due Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDate(2015, 06, 01);
			filter.Property2 = new ZDate(2015, 06, 01);
			filter.IsActive = true;
			collection = new AccPayableOrderHeaderCollection(Factory, FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(order1));
			Assert(!collection.Contains(order2));
			Assert(!collection.Contains(order3));

			filter.Property1 = new ZDate(2015, 05, 01);
			filter.Property2 = new ZDate(2015, 07, 01);
			collection = new AccPayableOrderHeaderCollection(Factory, FilterBO.Filter);
			AssertEquals(3, collection.Count);
			Assert(collection.Contains(order1));
			Assert(collection.Contains(order2));
			Assert(collection.Contains(order3));
		}

		AccPayableOrderFilterBusinessObject FilterBO;
		AccPayableOrderHeaderCollection collection;
		AccPayableOrderHeader order1;
		AccPayableOrderHeader order2;
		AccPayableOrderHeader order3;

		protected override void SetUp()
		{
			base.SetUp();
			order1 = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order1.APH_OrderNumber = "PO100001";
			order1.APH_DueDate = new ZDate(2015, 06, 01);

			order2 = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order2.APH_OrderNumber = "PO100002";
			order2.APH_DueDate = new ZDate(2015, 06, 02);

			order3 = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			order3.APH_OrderNumber = "PO100003";
			order3.APH_DueDate = new ZDate(2015, 06, 03);

			Factory.Save();

			FilterBO = (AccPayableOrderFilterBusinessObject)GetNewFilterStripBusinessObject();
			Factory.Save();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccPayableOrderFilterBusinessObject();
		}
	}
}
