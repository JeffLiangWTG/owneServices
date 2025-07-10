using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ReceptionAmendmentSendMessageWrapperTest : ReceptionSendMessageWrapperTest
	{
		public void TestReceptionAmendmentSendMessageWrapperConstructor()
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

			wrapper = (ReceptionAmendmentSendMessageWrapper)GetWrapper(entryHeader, Certificate);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		CusEntryHeader entryHeader;
		ReceptionAmendmentSendMessageWrapper wrapper;

		protected override ReceptionSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new ReceptionAmendmentSendMessageWrapper(cusEntryHeader, certificateData);

		protected override ReceptionSendMessageWrapper GetProvider() => wrapper;
	}
}
