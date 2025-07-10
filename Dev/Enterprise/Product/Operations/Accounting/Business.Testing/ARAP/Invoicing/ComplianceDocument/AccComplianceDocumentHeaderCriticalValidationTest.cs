using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class AccComplianceDocumentHeaderCriticalValidationTest : CriticalValidationTest<AccComplianceDocumentHeader>
	{
		protected override List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases()
		{
			var result = new List<TestCaseDefinitionWithDelegate_Obsolete>();

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("Address not related to organization", factory =>
			{
				var address = TestObjectCreator.CreateAddress(TestObjectCreator.AALSHI);

				var header = Factory.NewWithValidTestData<APComplianceDocumentHeader>();
				header.ADH_Ledger = LedgerTypes.AccountsPayable;
				header.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
				header.ADH_OH_Organisation = TestObjectCreator.ABIGAS.PK;
				header.ADH_OA_AddressOverride = address.PK;
				return header;
			}, true, CriticalValidationErrorType.AccComplianceDocumentHeaderWithAddressNotRelatedToHeader_1, "The organization header address of the compliance document record does not belong to the transaction's organization.", "Compliance Document Header: PK = "));

			return result;
		}

		protected override ISupportCriticalValidation GetCriticalValidationParent() => Factory.NewWithValidTestData<APComplianceDocumentHeader>();

		public void TestAddressNotRelatedToOrganization_CriticalValidation()
		{
			var address = TestObjectCreator.CreateAddress(TestObjectCreator.AALSHI);

			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.TWD, 1m, TestObjectCreator.ABIGAS);
			arInv.AH_OA_InvoiceAddressOverride = address.PK;
			var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;

			var invoiceDocumentHeader = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "TX00010001", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine);

			CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(invoiceDocumentHeader.PK, CriticalValidationInfoCollectorServiceKeyType.AccComplianceDocumentHeaderWithAddressNotRelatedToHeaderCallStack);

			invoiceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.ABIGAS.PK;
			invoiceDocumentHeader.ADH_OA_AddressOverride = address.PK;

			var expectedMessageMainError = @$"Compliance Document Header: PK = {invoiceDocumentHeader.PK}, Organization = ABIGAS, Organization for Address = AALSHI, Address = {address.OA_Code}, Address Of Organization CustomeCode = , Address Of Parent Transactiton = {address.OA_Code}, Sending Documents Address = {invoiceDocumentHeader.Organisation.AddressForSendingARDocuments.OA_Code}, Is In DB = No, Has Changes = Yes.
";
			var expectMessageStackTrace = @"Compliance Document Organization = ABIGAS, Compliance Document Organization for Address = AALSHI, Parent Organization = ABIGAS, Parent Organization for Address = AALSHI
   at System.Environment.GetStackTrace";
			var userErrorMessage = "The organization header address of the compliance document record does not belong to the transaction's organization.";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(CriticalValidationMessageTemplate.AccComplianceDocumentHeaderWithAddressNotRelatedToHeaderErrorMessage, true,
				CriticalValidationErrorType.AccComplianceDocumentHeaderWithAddressNotRelatedToHeader_1,
				userErrorMessage,
				expectedMessageMainError, expectMessageStackTrace);
			AssertOnSavingCheck(invoiceDocumentHeader, testCase);

			var address2 = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);
			invoiceDocumentHeader.ADH_OA_AddressOverride = address2.PK;
			address2.Factory.Save();
			invoiceDocumentHeader.ADH_DocumentStatus = "SYN";
			var testCase2 = new TestCaseDefinition_ForSeparateTestsMethods(CriticalValidationMessageTemplate.AccComplianceDocumentHeaderWithAddressNotRelatedToHeaderErrorMessage);
			AssertOnSavingCheck(invoiceDocumentHeader, testCase2);

			invoiceDocumentHeader.ADH_OA_AddressOverride = address.PK;
			expectedMessageMainError = @$"Compliance Document Header: PK = {invoiceDocumentHeader.PK}, Organization = ABIGAS, Organization for Address = AALSHI, Address = {address.OA_Code}, Address Of Organization CustomeCode = , Address Of Parent Transactiton = {address.OA_Code}, Sending Documents Address = {invoiceDocumentHeader.Organisation.AddressForSendingARDocuments.OA_Code}, Is In DB = Yes, Has Changes = Yes.
";
			testCase = new TestCaseDefinition_ForSeparateTestsMethods(CriticalValidationMessageTemplate.AccComplianceDocumentHeaderWithAddressNotRelatedToHeaderErrorMessage, true,
				CriticalValidationErrorType.AccComplianceDocumentHeaderWithAddressNotRelatedToHeader_1,
				userErrorMessage,
				expectedMessageMainError, expectMessageStackTrace);
			AssertOnSavingCheck(invoiceDocumentHeader, testCase);

			invoiceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.AALSHI.PK;
			invoiceDocumentHeader.ADH_OA_AddressOverride = address2.PK;
			expectedMessageMainError = @$"Compliance Document Header: PK = {invoiceDocumentHeader.PK}, Organization = AALSHI, Organization for Address = ABIGAS, Address = {address2.OA_Code}, Address Of Organization CustomeCode = , Address Of Parent Transactiton = {address.OA_Code}, Sending Documents Address = {invoiceDocumentHeader.Organisation.AddressForSendingARDocuments.OA_Code}, Is In DB = Yes, Has Changes = Yes.
";
			expectMessageStackTrace = @"Compliance Document Organization = AALSHI, Compliance Document Organization for Address = ABIGAS, Parent Organization = ABIGAS, Parent Organization for Address = AALSHI
   at System.Environment.GetStackTrace";
			testCase = new TestCaseDefinition_ForSeparateTestsMethods(CriticalValidationMessageTemplate.AccComplianceDocumentHeaderWithAddressNotRelatedToHeaderErrorMessage, true,
				CriticalValidationErrorType.AccComplianceDocumentHeaderWithAddressNotRelatedToHeader_1,
				userErrorMessage,
				expectedMessageMainError, expectMessageStackTrace);
			AssertOnSavingCheck(invoiceDocumentHeader, testCase);
		}

		public void TestAccComplianceDocumentNumberAlreadyInUseErrorMessage_CriticalValidation()
		{
			var complianceSequence = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, "TX", 2, 100, 25);

			var address = TestObjectCreator.CreateAddress(TestObjectCreator.ABIGAS);

			var arInv1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.TWD, 1m, TestObjectCreator.ABIGAS);
			arInv1.AH_OA_InvoiceAddressOverride = address.PK;
			var arInvLine1 = (ARInvoiceLine)arInv1.Lines.AddNew();
			arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;

			var invoiceDocumentHeader1 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "TX00010001", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine1, complianceSequence: complianceSequence);

			var arInv2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.TWD, 1m, TestObjectCreator.ABIGAS);
			arInv2.AH_OA_InvoiceAddressOverride = address.PK;
			var arInvLine2 = (ARInvoiceLine)arInv2.Lines.AddNew();
			arInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;

			var invoiceDocumentHeader2 = TestObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine2, complianceSequence: complianceSequence);

			invoiceDocumentHeader2.ADH_OH_Organisation = TestObjectCreator.ABIGAS.PK;
			invoiceDocumentHeader2.ADH_OA_AddressOverride = address.PK;
			TestObjectCreator.Factory.Save();

			invoiceDocumentHeader2.ADH_DocumentNumber = "TX00010001";

			AssertEquals("PreCondition: ADH_DocumentNumberInfo has changes.", true, invoiceDocumentHeader2.ADH_DocumentNumberInfo.HasChanges);

			var userErrorMessage = "This Compliance Document Number is already in use. Please try to allocate a number to it again.";
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(CriticalValidationMessageTemplate.AccComplianceDocumentNumberAlreadyInUseErrorMessage, true, CriticalValidationErrorType.AccComplianceDocumentNumberAlreadyInUse, userErrorMessage);

			AssertOnSavingCheck(invoiceDocumentHeader2, testCase);
		}

		public TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
