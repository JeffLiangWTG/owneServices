using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;
using CusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.ES.Business.Testing
{
	public static class SupDocTestHelper
	{
		public static void AddSuportingDocumentsToInvoiceHeaders(JobDeclaration declaration, ZString docType)
		{
			foreach (JobComInvoiceHeader invoice in declaration.Invoices)
			{
				AddSuportingDocumentToInvoiceHeader(invoice, docType);
			}
		}

		public static void AddSuportingDocumentsToEntryInstructions(JobDeclaration declaration, ZString docType)
		{
			foreach (CusEntryInstruction entryInstruction in declaration.CustomsEntryInstructions)
			{
				AddSuportingDocumentToEntryInstruction(entryInstruction, docType);
			}
		}

		public static void AddSuportingDocumentToInvoiceHeader(JobComInvoiceHeader invoiceHeader, ZString docType) => AddDocumentToCollection(invoiceHeader.SupportingDocuments, docType);

		public static void AddSuportingDocumentToEntryInstruction(CusEntryInstruction entryInstruction, ZString docType) => AddDocumentToCollection(entryInstruction.SupportingDocuments, docType);

		public static void AddSuportingDocumentToDeclaration(JobDeclaration declaration, ZString docType) => AddDocumentToCollection(declaration.SupportingDocuments, docType);

		public static void AddSuportingDocumentToInvoiceLine(JobComInvoiceLine invoiceLine, ZString docType) => AddDocumentToCollection(invoiceLine.SupportingDocuments, docType);

		static void AddDocumentToCollection(SupportingDocumentCollection docs, ZString docType)
		{
			var supDoc = docs.AddNew();
			supDoc.CSI_Code = docType;
		}

		public static void EditSuportingDocumentsToEntryInstructions(JobDeclaration declaration)
		{
			foreach (CusEntryInstruction entryInstruction in declaration.CustomsEntryInstructions)
			{
				var supDoc = entryInstruction.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault();
				supDoc.CSI_ReferenceNumber = initialDocRef;
				supDoc.CSI_DateOfIssue = initialDocDate;
			}
		}

		public static CusAuthorisationHeader AddAuthorisationWithHolder(BusinessObjectFactory factory, ZGuid holderPK, ZString authType)
		{
			var authorisation = factory.New<CusAuthorisationHeader>();
			authorisation.CPH_OH_PermitHolder = holderPK;
			authorisation.CPH_Type = authType;
			authorisation.CPH_StartDate = authorisationDate;
			authorisation.CPH_Number = authorisationNumber;
			authorisation.Factory.Save();
			return authorisation;
		}

		public static readonly string docType = "TYPE";
		public static readonly string authorisationNumber = "Number";
		public static readonly ZDate authorisationDate = new ZDate(2021, 11, 03);

		public static readonly string initialDocRef = "AAAA";
		public static readonly ZDate initialDocDate = new ZDate(2020, 01, 01);

		public static void AssertAEODocuments(ZString messagePrefix, CusEntryInstruction entryInstruction, ZString docType, string docRef = "", ZDate? docDate = null)
		{
			docRef = string.IsNullOrWhiteSpace(docRef) ? SupDocTestHelper.authorisationNumber : docRef;
			docDate = docDate == null ? SupDocTestHelper.authorisationDate : docDate;

			NUnit.Framework.Assertion.AssertEquals(messagePrefix + " new document added to the list", 1, SupDocTestHelper.GetAEODocumentCount(entryInstruction, docType));
			var supDoc = entryInstruction.SupportingDocuments.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == docType);
			NUnit.Framework.Assertion.AssertEquals(messagePrefix + " new document Type", docType, supDoc.CSI_Code);
			NUnit.Framework.Assertion.AssertEquals(messagePrefix + " new document Reference", docRef, supDoc.CSI_ReferenceNumber);
			NUnit.Framework.Assertion.AssertEquals(messagePrefix + " new document Issue Date", docDate, supDoc.CSI_DateOfIssue);
		}

		public static int GetAEODocumentCount(CusEntryInstruction entryInstruction, ZString docType) => GetTypeDocumentCount(entryInstruction.SupportingDocuments, docType);
		public static int GetTypeDocumentCount(SupportingDocumentCollection docs, ZString docType) => docs.Cast<SupportingDocument>().Count(x => x.CSI_Code == docType);
		public static int GetTypeGetPreviouslySentSupportingDocumentsCount(CusEntryLine entryLine, ZString docType) => entryLine.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == docType);
		public static int GetTypeGetPreviouslySentSupportingDocumentsCount(CusEntryLine entryLine, ZString docType, ZString docReference, ZString subType) => entryLine.GetPreviouslySentSupportingDocuments().Count(x => x.CSI_Code == docType && x.CSI_ReferenceNumber == docReference && x.CSI_SubType == subType);
	}
}
