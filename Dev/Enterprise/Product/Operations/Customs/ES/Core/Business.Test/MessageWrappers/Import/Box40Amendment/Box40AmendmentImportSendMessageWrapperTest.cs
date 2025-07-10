using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(Box40AmendmentImportSendMessageWrapper))]
	class Box40AmendmentImportSendMessageWrapperTest : ImportCommonSendMessageWrapperAbstractTest<Box40AmendmentImportSendMessageWrapper>
	{
		public void TestConstructorBox40Amendment()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				AssertExceptionThrown<ArgumentOutOfRangeException>("No MergerLines", () => new Box40AmendmentImportSendMessageWrapper(entryHeader, Certificate));
			});
		}

		public void TestLines()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapper.Lines.Count);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";
				var previousDoc2 = invoiceLine2.PreviousDocuments.AddNew();

				var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "2203001012";
				var previousDoc3 = invoiceLine3.PreviousDocuments.AddNew();

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = new Box40AmendmentImportSendMessageWrapper(entryHeader, Certificate);

				var lines = wrapper.Lines;

				AssertEquals("Expected 3 Lines", 3, lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			invoiceLine.PreviousDocuments.AddNew();
		}

		protected override Box40AmendmentImportSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new Box40AmendmentImportSendMessageWrapper(cusEntryHeader, certificateData);
	}
}
