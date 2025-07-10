using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(DUACompleteImportSendMessageWrapper))]
	class DUACompleteImportSendMessageWrapperTest : ImportCommonSendMessageWrapperAbstractTest<DUACompleteImportSendMessageWrapper>
	{
		public void TestConstructorDUAComplete()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertExceptionThrown<ArgumentOutOfRangeException>("No MergerLines", () => GetWrapper(entryHeader, Certificate));
		}

		public void TestProcedureDate()
		{
			ZDateTime.TryParseExact(HeaderData.DateOfRecap, out var procedureDate, CustomsDateTimeExtension.DateFormat);
			declaration.ZG_LCPDepart = procedureDate;
			AssertEquals("Expected filled ProcedureDate", procedureDate, wrapper.ProcedureDate);
		}

		public void TestHeader()
		{
			CombineAssertions(() =>
			{
				var header = wrapper.Header;

				AssertNotNull("Expected not null Header", header);
				AssertSame("Cached Header", header, wrapper.Header);
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

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader, Certificate);

				var lines = wrapper.Lines;

				AssertEquals("Expected 3 Lines", 3, lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});
		}

		protected override DUACompleteImportSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new DUACompleteImportSendMessageWrapper(cusEntryHeader, certificateData);
	}
}
