using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EntryHeaderSupportingDocument))]
	class EntrySupportingDocumentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EntryHeaderSupportingDocument(new[] { Factory.New<CusSupportingDocument>() });
		}

		public void TestProperties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.China);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, Core.Constants.CountryCodes.China);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DisplayCode", "Desc.", RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsLicense", "Desc.", RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Import", "Desc.", RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Export", "Desc.", RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China);
			Factory.Save();
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CNPTA", "AU1", "CN-AU free trade agreement 1", ("ApplicableCountry", "AU"));
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypes.Codes.CNRequiredDocuments, "01", "Import License");
			var code2 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypes.Codes.CNRequiredDocuments, "1Y", "Certificate of Origin");
			code1.Attributes.AddNew("DisplayCode", "1");
			code2.Attributes.AddNew("DisplayCode", "Y");
			Factory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var declaration = testItems.JobDeclaration;
			declaration.JE_MessageType = "IMP";
			var invoiceLine1 = testItems.InvoiceLine;
			invoiceLine1.JI_PrimaryPreference = "FTA";
			invoiceLine1.TradeAgreementCode = "AU1";
			invoiceLine1.CertificateOfOriginCountry = "AU";
			invoiceLine1.CertificateOfOriginType = "D";
			invoiceLine1.CertificateOfOrigin = "01001";
			invoiceLine1.ItemNoOnCertOfOrigin = 1;
			var invoiceLine2 = testItems.InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testItems.EntryInstruction.PK;
			invoiceLine2.JI_CL = testItems.EntryLine.PK;
			invoiceLine2.CusSupportingDocuments.AddNew("01", "01002").CSI_LineNo = 1;
			invoiceLine2.CusSupportingDocuments.AddNew("01", "01002").CSI_LineNo = 2;
			invoiceLine2.CusSupportingDocuments.AddNew("01", "01002").CSI_LineNo = 3;

			var entryHeaderSupportingDocument1 = new EntryHeaderSupportingDocument(invoiceLine1.CusSupportingDocuments.Cast<CusSupportingDocument>());
			var entryHeaderSupportingDocument2 = new EntryHeaderSupportingDocument(invoiceLine2.CusSupportingDocuments.Cast<CusSupportingDocument>());
			AssertEquals("DocumentType", "Y", entryHeaderSupportingDocument1.DocumentType);
			AssertEquals("DocumentType", "Certificate of Origin", entryHeaderSupportingDocument1.DocumentTypeDesc);
			AssertEquals("DocumentNumber", "<AU1>D01001", entryHeaderSupportingDocument1.DocumentNumber);
			AssertEquals("ItemNumbers", "1", entryHeaderSupportingDocument1.ItemNumbers.JoinAsString());
			AssertEquals("DocumentType", "1", entryHeaderSupportingDocument2.DocumentType);
			AssertEquals("DocumentType", "Import License", entryHeaderSupportingDocument2.DocumentTypeDesc);
			AssertEquals("DocumentNumber", "01002", entryHeaderSupportingDocument2.DocumentNumber);
			AssertEquals("ItemNumbers", "1,2,3", entryHeaderSupportingDocument2.ItemNumbers.JoinAsString());

			invoiceLine1.CertificateOfOriginType = "X";
			AssertEquals("DocumentNumber", "<AU1>XJE00000", entryHeaderSupportingDocument1.DocumentNumber);
		}
	}
}
