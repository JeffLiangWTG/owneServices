using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EntryHeaderSupportingDocumentCollection))]
	class EntrySupportingDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EntryHeaderSupportingDocumentCollection>
	{
		protected override EntryHeaderSupportingDocumentCollection GetCollectionToTest()
		{
			return new EntryHeaderSupportingDocumentCollection(Factory.New<CusEntryHeader>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EntryHeaderSupportingDocument(new[] { Factory.New<CusSupportingDocument>() });
		}

		public void TestAllows()
		{
			var testCollection = GetCollectionToTest();
			Assert("Should not allow new", !testCollection.AllowNew);
			Assert("Should not allow remove", !testCollection.AllowRemove);
		}

		public void TestLoad()
		{
			SetUpCNDOCRefs();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var declaration = testItems.JobDeclaration;
			declaration.JE_MessageType = "IMP";
			var invoiceLine1 = testItems.InvoiceLine;
			invoiceLine1.JI_PrimaryPreference = "FTA";
			invoiceLine1.TradeAgreementCode = "T";
			var invoiceLine2 = testItems.InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testItems.EntryInstruction.PK;
			invoiceLine2.JI_CL = testItems.EntryLine.PK;
			invoiceLine1.CusSupportingDocuments.AddNew("01", "01001").CSI_LineNo = 2;
			invoiceLine1.CusSupportingDocuments.AddNew("0y", "0y001").CSI_LineNo = 2;
			invoiceLine1.CusSupportingDocuments.AddNew("1A", "1A001").CSI_LineNo = 2;
			invoiceLine2.CusSupportingDocuments.AddNew("01", "01002").CSI_LineNo = 1;
			invoiceLine2.CusSupportingDocuments.AddNew("0y", "01001").CSI_LineNo = 1;
			invoiceLine2.CusSupportingDocuments.AddNew("01", "01001").CSI_LineNo = 1;
			invoiceLine2.CusSupportingDocuments.AddNew("1Z", "1Z001").CSI_LineNo = 1;

			var certificateOfOrigin = invoiceLine1.CusSupportingDocuments.AddNew("1Y", "1Y001");
			certificateOfOrigin.CSI_LineNo = 2;
			certificateOfOrigin.CSI_SubType = "C";

			var testCollection = testItems.EntryHeader.SupportingDocuments.OfType<EntryHeaderSupportingDocument>();
			AssertEquals("8 docs in total, of which 1A(EXP) and 1Z(IsLicense) should be abandoned and 1 should be merged", 5, testCollection.Count());
			Assert("01, 01001, 1,2", testCollection.Any(doc => doc.DocumentType == "1" && doc.DocumentNumber == "01001" && doc.ItemNumbers.JoinAsString() == "1,2"));
			Assert("1Y, <T>C1Y001, 2", testCollection.Any(doc => doc.DocumentType == "Y" && doc.DocumentNumber == "<T>C1Y001" && doc.ItemNumbers.JoinAsString() == "2"));
			Assert("0y, 0y001, 2", testCollection.Any(doc => doc.DocumentType == "y" && doc.DocumentNumber == "0y001" && doc.ItemNumbers.JoinAsString() == "2"));
			Assert("01, 01002, 1", testCollection.Any(doc => doc.DocumentType == "1" && doc.DocumentNumber == "01002" && doc.ItemNumbers.JoinAsString() == "1"));
			Assert("0y, 01001, 1", testCollection.Any(doc => doc.DocumentType == "y" && doc.DocumentNumber == "01001" && doc.ItemNumbers.JoinAsString() == "1"));
		}

		public void TestRefresh()
		{
			SetUpCNDOCRefs();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceline = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			invoiceline.CusSupportingDocuments.AddNew("01", "01001").CSI_LineNo = 2;
			invoiceline.CusSupportingDocuments.AddNew("1Y", "1Y001").CSI_LineNo = 2;
			invoiceline.CusSupportingDocuments.AddNew("1A", "1A001").CSI_LineNo = 2;
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();
			AssertEquals(2, entryHeader.SupportingDocuments.Count);

			invoiceline.CusSupportingDocuments.AddNew("0y", "0y001").CSI_LineNo = 2;
			AssertEquals(2, entryHeader.SupportingDocuments.Count);
			declaration.DoMerge();
			AssertEquals(3, entryHeader.SupportingDocuments.Count);
		}

		void SetUpCNDOCRefs()
		{
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypes.Codes.CNRequiredDocuments, "01", "1");
			var code2 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypes.Codes.CNRequiredDocuments, "0y", "y");
			var code3 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypes.Codes.CNRequiredDocuments, "1A", "A");
			var code4 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypes.Codes.CNRequiredDocuments, "1Y", "Y");
			var code5 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, RefCusCodeListTypes.Codes.CNRequiredDocuments, "1Z", "Z");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DisplayCode", "Desc.", RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsLicense", "Desc.", RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Import", "Desc.", RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Export", "Desc.", RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China);
			Factory.Save();
			code1.Attributes.AddNew("DisplayCode", "1");
			code1.Attributes.AddNew("Import", "");
			code2.Attributes.AddNew("DisplayCode", "y");
			code2.Attributes.AddNew("Import", "");
			code3.Attributes.AddNew("DisplayCode", "A");
			code3.Attributes.AddNew("Export", "");
			code4.Attributes.AddNew("DisplayCode", "Y");
			code4.Attributes.AddNew("Import", "");
			code5.Attributes.AddNew("DisplayCode", "Z");
			code5.Attributes.AddNew("IsLicense", "");
			Factory.Save();
		}
	}
}
