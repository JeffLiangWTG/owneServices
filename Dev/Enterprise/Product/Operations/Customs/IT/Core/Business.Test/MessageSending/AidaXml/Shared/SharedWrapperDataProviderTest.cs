using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SharedWrapperDataProviderTest : TestCaseWithFactory
{
	public void TestGetAdditionalInfosFromInvoiceLinesAndHeaders()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var invoiceHeader1 = declaration.Invoices.AddNew();
		var header1AdditionalInfo1 = invoiceHeader1.AdditionalInfos.AddNew("Code1", "Ref1");
		header1AdditionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		var header1AdditionalInfo2 = invoiceHeader1.AdditionalInfos.AddNew("Code2", "Ref2");
		header1AdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

		var invoiceHeader2 = declaration.Invoices.AddNew();
		var header2AdditionalInfo = invoiceHeader2.AdditionalInfos.AddNew("Code3", "Ref3");
		header2AdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

		var invoiceLine1_1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1_1.JI_CEI = entryInstruction.PK;
		var line1_1AdditionalInfo = invoiceHeader1.AdditionalInfos.AddNew("Code3", "Ref3");
		line1_1AdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

		var invoiceLine1_2 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1_2.JI_CEI = entryInstruction.PK;
		var line1_2AdditionalInfo1 = invoiceLine1_2.AdditionalInfos.AddNew("Code4", "Ref4");
		line1_2AdditionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		var line1_2AdditionalInfo2 = invoiceLine1_2.AdditionalInfos.AddNew("Code5", "Ref5");
		line1_2AdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

		var invoiceLine2_1 = invoiceHeader2.InvoiceLines.AddNew();
		invoiceLine2_1.JI_CEI = entryInstruction.PK;
		var line2_1AdditionalInfo1 = invoiceLine2_1.AdditionalInfos.AddNew("Code6", "Ref6");
		line2_1AdditionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		var line2_1AdditionalInfo2 = invoiceLine2_1.AdditionalInfos.AddNew("Code7", "Ref7");
		line2_1AdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

		var result = SharedWrapperDataProvider.GetAdditionalInfosFromInvoiceLinesAndHeaders<ITransportDocument>
			(entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>(), AdditionalInfoSubTypeList.Codes.TransportDocument, a => new TransportDocumentWrapper(a));

		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("DocumentType", new[] { "Code1", "Code3", "Code3", "Code4", "Code5", "Code6" }, result.Select(a => a.DocumentType));
			AssertContainsExactElementsInAnyOrder("ReferenceNumber", new[] { "Ref1", "Ref3", "Ref3", "Ref4", "Ref5", "Ref6" }, result.Select(a => a.ReferenceNumber));
		});
	}
}
