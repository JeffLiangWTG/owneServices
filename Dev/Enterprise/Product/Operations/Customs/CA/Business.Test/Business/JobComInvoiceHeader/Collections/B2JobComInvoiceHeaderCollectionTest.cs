using System.ComponentModel;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B2JobComInvoiceHeaderCollection))]
	sealed class B2JobComInvoiceHeaderCollectionTest : Customs.Business.Testing.BaseInvoiceHeaderCollectionTest<B2JobComInvoiceHeaderCollection, JobComInvoiceHeader>
	{
		public void TestSetDefaultsForNewChild()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceGroupHeader = dec.B2AsAccountedForInvoiceGroupHeader;
			var collection = new B2JobComInvoiceHeaderCollection(invoiceGroupHeader);
			collection.AddNew();
			AssertEquals(invoiceGroupHeader.PK, collection[0].JZ_JZ_GroupInvoiceFK);
		}

		public void TestAllowNew()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var collection = new B2JobComInvoiceHeaderCollection(dec.B2AsAccountedForInvoiceGroupHeader);
			Assert(((IBindingList)collection).AllowNew);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert(!((IBindingList)collection).AllowNew);

			dec.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			Assert(((IBindingList)collection).AllowNew);
		}

		protected override B2JobComInvoiceHeaderCollection GetCollectionToTest()
		{
			return Factory.NewWithValidTestData<JobDeclaration>().B2AsAccountedForInvoices;
		}

		public void TestDeleteForAsClaimed()
		{
			var b2Declaration = Factory.New<JobDeclaration>();
			b2Declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var asAccountedInvoice = b2Declaration.B2AsAccountedForInvoices.AddNew();
			asAccountedInvoice.JZ_InvoiceNumber = "INV1";
			var asClaimedInvoice = asAccountedInvoice.CorrespondingAsClaimedForInvoice;
			var asClaimedInvoice1 = b2Declaration.B2AsClaimedForInvoices.AddNew();

			var collection = b2Declaration.B2AsClaimedForInvoices;
			collection.Delete(asClaimedInvoice1);
			AssertEquals(1, collection.Count);
			AssertEquals(1, b2Declaration.B2AsAccountedForInvoices.Count);

			collection.Delete(asClaimedInvoice);
			AssertEquals(0, collection.Count);
			AssertEquals(0, b2Declaration.B2AsAccountedForInvoices.Count);
		}
	}
}
