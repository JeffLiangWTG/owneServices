using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseCollection))]
	public class InvoicingBaseCollectionTest : BusinessObjectCollectionTestCase
	{
		protected bool shouldRaiseNoConcreteTypeExceptionWhenAddingNew = true;

		public new void TestAddNew()
		{
			if (shouldRaiseNoConcreteTypeExceptionWhenAddingNew)
			{
				bool excpetionThrown = false;
				try
				{
					TestCollection.AddNew();
				}
				catch (NoConcreteTypeException)
				{
					excpetionThrown = true;
				}
				Assert("NoConcreteTypeException was thrown", excpetionThrown);
			}
			else
			{
				base.TestAddNew();
			}
		}

		public void TestIndexer()
		{
			BusinessObject obj1 = Factory.New(typeof(APInvoice));
			TestCollection.Add(obj1);
			AssertEquals("Index 0", obj1, TestCollection[0]);

			BusinessObject obj2 = Factory.New(typeof(ARInvoice));
			TestCollection.Add(obj2);
			AssertEquals("Index 1", obj2, TestCollection[1]);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			base.TestRemoveFromRelationship();
		}

		public void TestAllowNew()
		{
			Assert("Allow New by default", TestCollection.AllowNew);
			bool defaultValue = TestCollection.AllowNew;

			TestCollection.SetAllowNew(!defaultValue);
			AssertEquals("Allow New should be changed", !defaultValue, TestCollection.AllowNew);
		}

		#region Implementation

		protected InvoicingBaseCollection TestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = (InvoicingBaseCollection)GetCollectionToTest();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoicingBaseCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(APInvoice));
		}

		protected void Base_TestAddAndCancelOfElementAsThoughBinding()
		{
			base.TestAddAndCancelOfElementAsThoughBinding();
		}

		protected virtual bool AllowNewExpectedValue
		{
			get { return true; }
		}

		#endregion
	}
}
