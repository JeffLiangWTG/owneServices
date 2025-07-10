using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderContractCollection))]
	class JobComInvoiceHeaderContractCollectionTest : ActiveBusinessObjectCollectionTestCase<JobComInvoiceHeaderContractCollection>
	{
		public void TestContractNumbersChangedEventsSuspender()
		{
			var contractNumbersChangedEventHits = 0;
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.ContractNumbers.ContractNumbersChanged += (object sender, EventArgs e) =>
			{
				contractNumbersChangedEventHits++;
			};
			var ctr1 = invoice.ContractNumbers.AddNew();
			AssertEquals(1, contractNumbersChangedEventHits);
			ctr1.J2_ReferenceNumber = "0001";
			AssertEquals(2, contractNumbersChangedEventHits);
			invoice.ContractNumbers.Delete(ctr1);
			AssertEquals(3, contractNumbersChangedEventHits);
			invoice.ContractNumbersAsString = "abc,de f, ,, GHI0123546789012345678901234567890";
			AssertEquals(4, contractNumbersChangedEventHits);
		}

		public void TestContractNumberssAsString()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.ContractNumbersAsString = "abc,de f, ,, GHI0123546789012345678901234567890";
			AssertEquals(3, invoice.ContractNumbers.Count);
			AssertEquals("abc", invoice.ContractNumbers[0].J2_ReferenceNumber);
			AssertEquals("de f", invoice.ContractNumbers[1].J2_ReferenceNumber);
			AssertEquals("GHI01235467890123456789012345678", invoice.ContractNumbers[2].J2_ReferenceNumber);
		}

		public void TestCollectionLoadedCorrectly()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var ref1 = Factory.New<JobComInvoiceHeaderContract>();
			ref1.J2_JZ = invoice.PK;
			ref1.J2_ReferenceType = "CCN";
			var ref2 = Factory.New<JobComInvoiceHeaderContract>();
			ref2.J2_JZ = invoice.PK;
			ref2.J2_ReferenceType = "CTR";
			AssertEquals(1, invoice.ContractNumbers.Count);
			AssertEquals(ref2, invoice.ContractNumbers[0]);
		}

		protected override JobComInvoiceHeaderContractCollection GetCollectionToTest() => new JobComInvoiceHeaderContractCollection(Factory.New<JobComInvoiceHeader>());

		protected override void SetUp()
		{
			base.SetUp();
			var baseDec = Factory.NewWithValidTestData<JobDeclaration>();
			baseDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			baseDec.JE_DeclarationReference = "B00000001";
			header = baseDec.Invoices.AddNew();
			header.JZ_InvoiceNumber = "INVNO";
		}

		JobComInvoiceHeader header;
	}
}
