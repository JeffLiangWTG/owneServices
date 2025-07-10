using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ExpeditionAmendmentSendMessageWrapperTest : GenericExpeditionSendMessageWrapperTest<ExpeditionAmendmentSendMessageWrapper>
	{
		public void TestExpeditionAmendmentSendMessageWrapperConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null CusEntryHeader", () => GetWrapper(null, Certificate));
				AssertExceptionThrown<ArgumentNullException>("Null Certificate", () => GetWrapper(entryHeader, null));
				var declaration = Factory.New<JobDeclaration>();
				var newEntryHeader = declaration.CustomsEntryHeaders.AddNew();
				AssertExceptionThrown<ArgumentOutOfRangeException>(() => GetWrapper(newEntryHeader, Certificate));
			});
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

			wrapper = GetWrapper(entryHeader, Certificate);
		}

		protected override ExpeditionAmendmentSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new ExpeditionAmendmentSendMessageWrapper(cusEntryHeader, certificateData);

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		CusEntryHeader entryHeader;
		ExpeditionAmendmentSendMessageWrapper wrapper;
	}
}
