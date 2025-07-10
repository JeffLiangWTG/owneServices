using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using CoreConstants = Enterprise.Core.Constants;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusSupportingDocument))]
	class CusSupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<CusSupportingDocument>
	{
		public void TestSupportsNotes()
		{
			var document = (CusSupportingDocument)GetNewBusinessObject();
			AssertEquals("SupportsNotes", false, document.SupportsNotes);
		}

		[TestDate(2019, 2, 1)]
		public void TestEffectiveDate()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			testItems.EntryInstruction.CEI_DateForDuty = new ZDateTime(2019, 2, 25);
			var cusSupDoc = testItems.InvoiceLine.CusSupportingDocuments.AddNew();
			AssertEquals("Should return JobComInvoiceLine.EffectiveAssessmentDate", new ZDateTime(2019, 2, 25), cusSupDoc.EffectiveDate);
		}

		public void TestCSI_LineNoEditableAndCleared()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("RequiresLineNumber", "Desc.", CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			code1.Attributes.AddNew("RequiresLineNumber", "Mandatory");
			var code2 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD2", "Code 2");
			code2.Attributes.AddNew("RequiresLineNumber", "Optional");
			CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD3", "Code 3");
			Factory.Save();
			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)testDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			var testItem = invoiceLine.CusSupportingDocuments.AddNew();
			testItem.CSI_LineNo = 1;
			testItem.CSI_Code = "CD1";
			Assert("Editable when attribute Mandotary", !testItem.CSI_LineNoInfo.ReadOnly);
			Assert("Cleared when readonly", !testItem.CSI_LineNo.IsEmpty);
			var testItem2 = invoiceLine.CusSupportingDocuments.AddNew();
			testItem2.CSI_LineNo = 2;
			testItem2.CSI_Code = "CD2";
			Assert("Editable when attribute Optional", !testItem2.CSI_LineNoInfo.ReadOnly);
			Assert("Cleared when readonly", !testItem2.CSI_LineNo.IsEmpty);
			var testItem3 = invoiceLine.CusSupportingDocuments.AddNew();
			testItem3.CSI_LineNo = 3;
			testItem3.CSI_Code = "CD3";
			Assert("Readonly when attribute empty", testItem3.CSI_LineNoInfo.ReadOnly);
			Assert("Cleared when readonly", testItem3.CSI_LineNo.IsEmpty);
		}

		public void TestDisplayCode()
		{
			PrepareRefCusCodeLists();

			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = (JobComInvoiceLine)testDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			var testItem = invoiceLine.CusSupportingDocuments.AddNew();
			testItem.CSI_Code = "CD0";
			AssertEquals("DisplayCode", "0", testItem.DisplayCode);
		}

		public void TestDescription()
		{
			PrepareRefCusCodeLists();

			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
			code1.Attributes.AddNew("Import", ZString.Empty);
			code1.Attributes.AddNew("DisplayCode", "x");
			Factory.Save();
			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = testDeclaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var testItem = invoiceLine.CusSupportingDocuments.AddNew();
			testItem.DocumentType = "DESC";
			AssertEquals("DocumentType should be empty", ZString.Empty, testItem.DocumentType);
			AssertEquals("CSI_Codeshould be empty", ZString.Empty, testItem.CSI_Code);
			testItem.DocumentType = "x.Code 1";
			AssertEquals("x.Code 1", testItem.DocumentType);
			AssertEquals("CD1", testItem.CSI_Code);
			testItem.DocumentType = "You will not find me";
			AssertEquals("DocumentType should be empty for invalid selection", ZString.Empty, testItem.DocumentType);
			AssertEquals("CSI_Codeshould be empty for invalid selection", ZString.Empty, testItem.CSI_Code);
		}

		public void TestIsLicense()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Import", "Desc.", CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsLicense", "Desc.", CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			code1.Attributes.AddNew("Import", ZString.Empty);
			code1.Attributes.AddNew("IsLicense", ZString.Empty);
			var code2 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD2", "Code 2");
			code2.Attributes.AddNew("Import", ZString.Empty);
			Factory.Save();
			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = testDeclaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var testItem = invoiceLine.CusSupportingDocuments.AddNew();
			testItem.CSI_Code = "CD1";
			Assert("CD1, should be license.", testItem.IsLicense);
			testItem.CSI_Code = "CD2";
			Assert("CD2, should NOT be license.", !testItem.IsLicense);
		}

		public void TestMaxLengths()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Import", "Desc.", CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsLicense", "Desc.", CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			code1.Attributes.AddNew("Import", ZString.Empty);
			code1.Attributes.AddNew("IsLicense", ZString.Empty);
			var code2 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD2", "Code 2");
			code2.Attributes.AddNew("Import", ZString.Empty);
			Factory.Save();
			var testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = testDeclaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var testItem = invoiceLine.CusSupportingDocuments.AddNew();
			testItem.CSI_Code = "CD1";
			AssertEquals("IsLicense, max length should be 20", 20, testItem.CSI_ReferenceNumberInfo.MaxLength);
			testItem.CSI_Code = "CD2";
			AssertEquals("Not IsLicense, max length should be 32", 32, testItem.CSI_ReferenceNumberInfo.MaxLength);
		}

		public void TestIsCertificateOfOrigin()
		{
			var document = (CusSupportingDocument)GetNewBusinessObject();
			document.CSI_Code = Constants.DocumentCodes.CertificateOfOrigin;
			Assert("IsCertificateOfOrigin", document.IsCertificateOfOrigin);
			document.CSI_Code = ZString.Empty;
			Assert("IsCertificateOfOrigin", !document.IsCertificateOfOrigin);
		}

		public void TestIsTheSameDocument()
		{
			var document1 = (CusSupportingDocument)GetNewBusinessObject();
			var document2 = (CusSupportingDocument)GetNewBusinessObject();
			document1.CSI_Code = "01";
			document1.CSI_ReferenceNumber = "11";
			document2.CSI_Code = "01";
			document2.CSI_ReferenceNumber = "11";
			Assert("IsTheSameDocument", document1.IsTheSameDocument(document2));
			document2.CSI_ReferenceNumber = "22";
			Assert("IsTheSameDocument", !document1.IsTheSameDocument(document2));
		}

		public void TestIsTheSameTypeWithDifferentNumber()
		{
			var document1 = (CusSupportingDocument)GetNewBusinessObject();
			var document2 = (CusSupportingDocument)GetNewBusinessObject();
			document1.CSI_Code = "01";
			document1.CSI_ReferenceNumber = "11";
			document2.CSI_Code = "01";
			document2.CSI_ReferenceNumber = "11";
			Assert("IsDifferentDocumentWithTheSameType", !document1.IsDifferentDocumentWithTheSameType(document2));
			document2.CSI_ReferenceNumber = "22";
			Assert("IsDifferentDocumentWithTheSameType", document1.IsDifferentDocumentWithTheSameType(document2));
			document2.CSI_ReferenceNumber = "11";
			document2.CSI_SubType = "C";
			Assert("IsDifferentDocumentWithTheSameType", document1.IsDifferentDocumentWithTheSameType(document2));
			document1.CSI_SubType = "C";
			Assert("IsDifferentDocumentWithTheSameType", !document1.IsDifferentDocumentWithTheSameType(document2));
		}

		public void TestCSI_ReferenceNumber()
		{
			var document = (CusSupportingDocument)GetNewBusinessObject();
			document.CSI_Code = "01";
			document.CSI_ReferenceNumber = "11111";
			AssertEquals("CSI_ReferenceNumber", "11111", document.CSI_ReferenceNumber);
			document.CSI_Code = "1Y";
			document.CSI_SubType = "C";
			document.CSI_ReferenceNumber = "11111";
			AssertEquals("CSI_ReferenceNumber", "11111", document.CSI_ReferenceNumber);
			document.CSI_SubType = "X";
			AssertEquals("CSI_ReferenceNumber", "XJE00000", document.CSI_ReferenceNumber);
		}

		void PrepareRefCusCodeLists()
		{
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD0", "Code 0");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Import", "Desc.", CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Export", "Desc.", CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DisplayCode", "Desc.", CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, CoreConstants.Customs.Universal.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			code1.Attributes.AddNew("Import", ZString.Empty);
			code1.Attributes.AddNew("Export", ZString.Empty);
			code1.Attributes.AddNew("DisplayCode", "0");
			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var invoiceHeader = testDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			return invoiceLine.CusSupportingDocuments.AddNew();
		}

		protected override void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
			base.SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(info);
			if (info.Name == CusSupportingDocument.Schema.DocumentType)
			{
				info.Value = new ZString("0.Code 0");
			}
		}

		protected override IEnumerable<CusSupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var invoiceHeader = testDeclaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var result = invoiceLine.CusSupportingDocuments.AddNew();
			Factory.Save();
			yield return result;
		}
	}
}
