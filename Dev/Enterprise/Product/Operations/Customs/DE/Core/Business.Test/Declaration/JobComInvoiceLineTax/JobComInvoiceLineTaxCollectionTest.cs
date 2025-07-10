using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineTaxCollection))]
	public class JobComInvoiceLineTaxCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestElementType()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var deTax = invLine.Taxes.AddNew();
			AssertType<JobComInvoiceLineTax>(deTax);
			AssertType<JobComInvoiceLineTax>(invLine.Taxes[0]);
		}

		public new void TestReintroducedAddNewRemovedForGenericCollection()
		{
			Assert(true);
		}

		public new void TestReintroducedIndexerRemovedForGenericCollection()
		{
			Assert(true);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			return invLine.Taxes;
		}

		public void TestTypedSingleParameterAddNew()
		{
			var collection = (JobComInvoiceLineTaxCollection)GetCollectionToTest();
			var collectionType = collection.GetType();
			var method = collectionType.GetMethod("AddNew", new Type[] { typeof(Type) });
			var bizO = (BusinessObject)method.Invoke(collection, new object[] { typeof(JobComInvoiceLineTax) });
			AssertNotNull(bizO);
			AssertType<JobComInvoiceLineTax>(bizO);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<JobComInvoiceLineTax>();
	}
}
