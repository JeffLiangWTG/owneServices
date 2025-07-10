namespace Enterprise.Customs.CN.Business.Testing
{
	sealed class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestInvoice()
		{
			var parent = Factory.New<JobComInvoiceHeader>();
			AssertEquals(parent.Lookups.Invoice, parent);
		}

		public void TestJZ_IncoTerm_List()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var parent = Factory.New<JobComInvoiceHeader>();
			jobDeclaration.Invoices.Add(parent);
			AssertContainsExactElementsInExactOrder(new [] { "C&I", "CFR", "CIF", "FOB", "EXW" }, parent.Lookups.JZ_IncoTerm_List.GetAllCodes());
			AssertSame(parent.Lookups.JZ_IncoTerm_List, parent.Lookups.JZ_IncoTerm_List);
		}

		public override void TestMessageTypes()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var list = invoiceHeader.Lookups.MessageTypes;
			AssertEquals(3, list.Count);
			Assert(list.ContainsCode("ASN"));
			Assert(list.ContainsCode("EXP"));
			Assert(list.ContainsCode("IMP"));
		}

		public void TestConfirmationTypeList() => CombineAssertions(() =>
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var list = invoiceHeader.Lookups.ConfirmationTypeList;
			AssertType<ConfirmationTypeList>("Type", list);
			AssertSame("Cached", list, invoiceHeader.Lookups.ConfirmationTypeList);
		});
	}
}
