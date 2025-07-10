using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;

namespace Enterprise.Customs.FR.DocumentWrappers.Transit.Testing;

sealed class T2LDocWrapperTest : DocBaseWrapperTest
{
	public void TestT2LDocLines()
	{
		AssertType<T2LDocWrapper>("Wrapper should be of type T2LDocWrapper", Wrapper);
		AssertEquals("Wrapper.Lines should populate elements for the T2LApplicableEntryLines in entry header.", 3, Wrapper.T2LDocLines.Count);
	}

	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = Enterprise.Customs.EU.Business.MessageTypeList.Codes.Export;
		declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

		var invoice = declaration.Invoices.AddNew();
		invoice.FillWithValidTestData();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.FillWithValidTestData();
		invoiceLine1.JI_Tariff = "1111";
		invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
		invoiceLine1.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LDocument;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.FillWithValidTestData();
		invoiceLine2.JI_Tariff = "2222";
		invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Spain;
		var invoiceLine3 = invoice.InvoiceLines.AddNew();
		invoiceLine3.FillWithValidTestData();
		invoiceLine3.JI_Tariff = "3333";
		invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
		invoiceLine3.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LDocument;
		var invoiceLine4 = invoice.InvoiceLines.AddNew();
		invoiceLine4.FillWithValidTestData();
		invoiceLine4.JI_Tariff = "4444";
		invoiceLine4.JI_CountryOfOrigin = ZString.Empty;
		invoiceLine4.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.RefCusCodeList.SupportingDocumentsCodes.T2LDocument;
		var merger = new LineMerger(declaration);
		merger.DoMerge();

		var entryHeader = declaration.CustomsEntryHeaders[0];

		return T2LDocWrapper.New(entryHeader, Factory);
	}

	new T2LDocWrapper Wrapper => (T2LDocWrapper)base.Wrapper;
}
