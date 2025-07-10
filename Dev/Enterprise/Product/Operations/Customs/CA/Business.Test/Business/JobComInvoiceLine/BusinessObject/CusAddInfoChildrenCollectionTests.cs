using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusAddInfoChildrenCollectionTests : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var ex = AssertExceptionThrown<ArgumentNullException>(() => new CusAddInfoChildrenCollection(null));
			AssertStartsWith("exception message", "Parent of CusAddInfoChildrenCollection cannot be null.", ex.Message);
		}

		public void TestOperationsWithUnregisteredTypes()
		{
			JobComInvoiceLine parent = CreateInvoiceLine();

			// Registering GACPGAHeader within another collection, to make sure that registered types ane not leaked between instances.
			CusAddInfoChildrenCollection unrelatedCollection = new CusAddInfoChildrenCollection(parent);
			unrelatedCollection.Register<GACPGAHeader>();

			CusAddInfoChildrenCollection collection = new CusAddInfoChildrenCollection(parent);
			collection.Register<HCPGAHeader>();
			collection.Register<PHACPGAHeader>();

			InvalidOperationException ex;

			ex = AssertExceptionThrown<InvalidOperationException>(() => collection.Load<GACPGAHeader>());
			AssertEquals($"Type code '{CusAddInfoTypeAttribute.Codes.CAGACPGAHeader}' is not registered for this collection.", ex.Message);

			ex = AssertExceptionThrown<InvalidOperationException>(() => collection.Delete<GACPGAHeader>());
			AssertEquals($"Type code '{CusAddInfoTypeAttribute.Codes.CAGACPGAHeader}' is not registered for this collection.", ex.Message);
		}

		public void TestLoadAndDelete()
		{
			JobComInvoiceLine parent = CreateInvoiceLine();

			CusAddInfoChildrenCollection collection = new CusAddInfoChildrenCollection(parent);
			collection.Register<HCPGAHeader>();
			collection.Register<PHACPGAHeader>();

			var hcPGAHeader = collection.Load<HCPGAHeader>();
			var phacPGAHeader = collection.Load<PHACPGAHeader>();

			AssertNotNull(hcPGAHeader);
			AssertNotNull(phacPGAHeader);

			collection.Delete<HCPGAHeader>();

			AssertEquals(true, hcPGAHeader.IsDeleted);
			AssertEquals(false, phacPGAHeader.IsDeleted);

			var hcPGAHeader2 = collection.Load<HCPGAHeader>();
			var phacPGAHeader2 = collection.Load<PHACPGAHeader>();

			AssertNotEquals(hcPGAHeader.PK, hcPGAHeader2.PK);
			AssertEquals(phacPGAHeader.PK, phacPGAHeader2.PK);

			AssertEquals(false, hcPGAHeader2.IsDeleted);
			AssertEquals(false, phacPGAHeader2.IsDeleted);
		}

		public void TestDeleteByAnotherInstance()
		{
			JobComInvoiceLine parent = CreateInvoiceLine();

			CusAddInfoChildrenCollection collection1 = new CusAddInfoChildrenCollection(parent);
			collection1.Register<HCPGAHeader>();

			CusAddInfoChildrenCollection collection2 = new CusAddInfoChildrenCollection(parent);
			collection2.Register<HCPGAHeader>();

			var hcPGAHeader1 = collection1.Load<HCPGAHeader>();
			var hcPGAHeader2 = collection2.Load<HCPGAHeader>();

			collection1.Delete<HCPGAHeader>();

			AssertEquals(true, hcPGAHeader1.IsDeleted);
			AssertEquals(true, hcPGAHeader2.IsDeleted);

			hcPGAHeader1 = collection1.Load<HCPGAHeader>();
			hcPGAHeader2 = collection2.Load<HCPGAHeader>();

			AssertEquals(false, hcPGAHeader1.IsDeleted);
			AssertEquals(false, hcPGAHeader2.IsDeleted);

			AssertSame("should reuse non-deleted instance from same factory", hcPGAHeader1, hcPGAHeader2);
		}

		JobComInvoiceLine CreateInvoiceLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			return (JobComInvoiceLine)header.InvoiceLines.AddNew();
		}
	}
}
