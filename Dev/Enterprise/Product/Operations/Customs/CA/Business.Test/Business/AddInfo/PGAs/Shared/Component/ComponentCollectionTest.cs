using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ComponentCollection))]
	sealed class ComponentCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		public void TestTypedSingleParameterAddNew()
		{
			var collection = (ComponentCollection)GetCollectionToTest();

			var collectionType = collection.GetType();
			var method = collectionType.GetMethod("AddNew", new Type[] { typeof(Type) });
			var bizO = (BusinessObject)method.Invoke(collection, new object[] { typeof(Component) });

			AssertNotNull("AddNew of Type " + method.ReturnType.FullName + " not null", bizO);
		}

		public override void TestTypedget_Item()
		{
			Assert(true);
		}

		public void TestCopyPersistentValuesFrom()
		{
			var bo1 = Factory.New<JobDeclaration>();
			var bo2 = Factory.New<JobDeclaration>();
			var collection1 = new ComponentCollection(bo1);
			var collection2 = new ComponentCollection(bo2);

			Factory.Save();

			AssertEquals(0, collection1.Count);
			AssertEquals(0, collection2.Count);

			collection2.CopyPersistentValuesFrom(collection1);

			AssertEquals(0, collection1.Count);
			AssertEquals(0, collection2.Count);

			// checking that updates in original collection does not propagate automatically to the copied collection
			var component1 = collection1.AddNew();
			component1.CA_Name = "COMP1";
			component1.CA_Concentration = 1m;

			Factory.Save();

			AssertEquals(1, collection1.Count);
			AssertEquals(0, collection2.Count);

			// checking that CopyPersistentValuesFrom copied component from the first collection
			collection2.CopyPersistentValuesFrom(collection1);

			AssertEquals(1, collection1.Count);
			AssertEquals(1, collection2.Count);

			var component1Copy = collection2.OfType<Component>().Single(c => c.CA_Name == "COMP1");
			AssertEquals(1m, component1Copy.CA_Concentration);

			// checking that updates in the original component does not propagate automatically to the copied component
			component1.CA_Name = "COMP1 (UPD)";

			Factory.Save();

			AssertEquals("COMP1", component1Copy.CA_Name);
			AssertEquals(1m, component1Copy.CA_Concentration);

			// checking that CopyPersistentValuesFrom removes existing components and adds new components
			var component2 = collection1.AddNew();
			component2.CA_Name = "COMP2";
			component2.CA_Concentration = 2m;

			Factory.Save();

			collection2.CopyPersistentValuesFrom(collection1);

			AssertEquals(2, collection1.Count);
			AssertEquals(2, collection2.Count);

			AssertEquals(true, component1Copy.IsDeleted);

			component1Copy = collection2.OfType<Component>().Single(c => c.CA_Name == "COMP1 (UPD)");
			AssertEquals(1m, component1Copy.CA_Concentration);
			AssertEquals(false, component1Copy.IsDeleted);

			var component2Copy = collection2.OfType<Component>().Single(c => c.CA_Name == "COMP2");
			AssertEquals(2m, component2Copy.CA_Concentration);
			AssertEquals(false, component1Copy.IsDeleted);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ComponentCollection(CNSCHeader);
		}

		CNSCPGAHeader CNSCHeader
		{
			get
			{
				if (cnscHeader == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
					JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

					invoiceLine.CA_CNSCInd = "Y";
					cnscHeader = invoiceLine.CNSCPGAHeader;
				}
				return cnscHeader;
			}
		}

		CNSCPGAHeader cnscHeader;

		#endregion
	}
}
