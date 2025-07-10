using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineTaxCollection))]
	class JobComInvoiceLineTaxCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTypedSingleParameterAddNew()
		{
			var collection = (JobComInvoiceLineTaxCollection)GetCollectionToTest();
			var collectionType = collection.GetType();
			var method = collectionType.GetMethod("AddNew", new Type[] { typeof(Type) });
			var bizO = (BusinessObject)method.Invoke(collection, new object[] { typeof(JobComInvoiceLineTax) });
			AssertNotNull(bizO);
			AssertType<JobComInvoiceLineTax>(bizO);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			return invoiceLine.Taxes;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<JobComInvoiceLineTax>();
	}
}
