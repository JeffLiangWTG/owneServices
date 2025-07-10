using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(Box44ImportSendMessageWrapper))]
	class Box44ImportSendMessageWrapperTest : ImportCommonSendMessageWrapperAbstractTest<Box44ImportSendMessageWrapper>
	{
		public void TestConstructorBox44()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				AssertExceptionThrown<ArgumentOutOfRangeException>("No MergerLines", () => new Box44ImportSendMessageWrapper(entryHeader, Certificate));
			});
		}

		public void TestLines()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 0 Lines when no supporting documents are declared", 0, wrapper.Lines.Count);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";
				var supdoc1 = invoiceLine2.SupportingDocuments.AddNew();
				supdoc1.CSI_Code = "9003";

				var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "2203001012";
				var supdoc2 = invoiceLine3.SupportingDocuments.AddNew();
				supdoc2.CSI_Code = "9003";

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals("Expected 3 EntryLines", 3, entryHeader.MergedLines.Count);

				wrapper = new Box44ImportSendMessageWrapper(entryHeader, Certificate);

				var lines = wrapper.Lines;

				AssertEquals("Expected 2 Lines (only the ones with supporting documents to send)", 2, lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});
		}

		protected override Box44ImportSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new Box44ImportSendMessageWrapper(cusEntryHeader, certificateData);
	}
}
