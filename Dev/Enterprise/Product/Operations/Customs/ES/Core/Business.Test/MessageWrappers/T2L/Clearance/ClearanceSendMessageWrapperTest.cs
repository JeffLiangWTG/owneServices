using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ClearanceSendMessageWrapperTest : WrapperHelperTest<ClearanceSendMessageWrapper>
	{
		public void TestConstructor()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertExceptionThrown<ArgumentOutOfRangeException>("No MergedLines", () => new ClearanceSendMessageWrapper(entryHeader, Certificate));
		}

		public void TestHeader()
		{
			CombineAssertions(() =>
			{
				var header = wrapper.Header;

				AssertNotNull("Expected not null Header", header);
				AssertSame("Cached Header", wrapper.Header, header);
			});
		}

		public void TestLines()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapper.Lines.Count);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";

				var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "2203001012";

				var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge failed", true, mergeResult);

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = new ClearanceSendMessageWrapper(entryHeader, Certificate);

				var lines = wrapper.Lines;

				AssertEquals("Expected 3 Lines", 3, lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = new ClearanceSendMessageWrapper(entryHeader, Certificate);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		CusEntryHeader entryHeader;
		ClearanceSendMessageWrapper wrapper;

		protected override ClearanceSendMessageWrapper GetProvider() => wrapper;
	}
}
