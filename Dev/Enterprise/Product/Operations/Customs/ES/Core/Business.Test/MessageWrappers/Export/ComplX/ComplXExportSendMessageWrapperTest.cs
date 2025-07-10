using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.ExportMessageConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ComplXExportSendMessageWrapper))]
	class ComplXExportSendMessageWrapperTest : ExportSendMessageCommonWrapperAbstractTest<ComplXExportSendMessageWrapper>
	{
		protected override ZString ExpectedLocalReferenceNumber => "Reference_C";

		public void TestMessageType()
		{
			AssertEquals("MessageType is 36", ExportDeclarationWrapperMessageTypeCodeList.ExportComplementaryXDeclaration, wrapper.MessageType);
		}

		public void TestCustomsProcedureCategory5()
		{
			entryHeader.MovementReferenceNumber = MovementReferenceNumber;
			AssertEquals("Expected filled CustomsProcedureCategory5", MovementReferenceNumber, wrapper.CustomsProcedureCategory5);
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
				AssertEquals("Merge", true, mergeResult);

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader, Certificate);

				var lines = wrapper.Lines;

				AssertEquals("Expected 3 Lines", 3, wrapper.Lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});
		}

		protected override ComplXExportSendMessageWrapper GetProvider() => wrapper;

		protected override ComplXExportSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new ComplXExportSendMessageWrapper(cusEntryHeader, certificateData);
	}
}
