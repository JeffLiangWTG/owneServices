using System;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.Testing
{
	public class EInvoicingDataValidatorForVietnamTest : BaseEInvoicingDataValidatorTest
	{
		public override BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest()
		{
			return new EInvoicingDataValidatorForVietnam(GlbCompany.CurrentCompany);
		}

		public void TestValidateRunCoreOnlyOnSameCompanyBatch()
		{
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345");

			var pivot = CreateNonCurrentCompanyInvoice();
			var orgProxy = Factory.Load<OrgHeader>(NonCurrentCompanyInvoice.Company.GC_OH_OrgProxy);
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "987654321", Core.Constants.CountryCodes.VietNam);

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("There should be no error discription because noncurrent company batch will not run the validation.", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.Batched, pivot.AIP_Status);

			pivot = CreateInvoice();
			var batchID = pivot.AIP_AIB;
			orgProxy = Factory.Load<OrgHeader>(Invoice.Company.GC_OH_OrgProxy);
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "0100233488", Core.Constants.CountryCodes.VietNam);
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Vietnam E-Invoice Service Partner Connection Username and Password should be set in registry.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			AssertEquals(batchID, pivot.AIP_AIB);
		}

		public void TestValidateDebtorTaxCodeCountryAndType()
		{
			var pivot = CreateInvoice();
			var batchID = pivot.AIP_AIB;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345");
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "67890");

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("Branch proxy or company proxy must have VN VAT number.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			AssertEquals(batchID, pivot.AIP_AIB);

			Invoice.Branch.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
			var branchOrgProxy = Factory.Load<OrgHeader>(Invoice.Branch.GB_OH_OrgProxy);
			branchOrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "6666666", Core.Constants.CountryCodes.VietNam);
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			pivot.AIP_ErrorDescription = string.Empty;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("The VN VAT number '6666666' is invalid. It should be in format 'NNNNNNNNNN' or 'NNNNNNNNNN-NNN' and complies with check digit validation.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			AssertEquals(batchID, pivot.AIP_AIB);

			var cusCode = branchOrgProxy.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam)[0];
			cusCode.OK_CustomsRegNo = "0100233488";
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			pivot.AIP_ErrorDescription = string.Empty;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("Validation should succeed", ZString.Empty, pivot.AIP_ErrorDescription);

			cusCode.OK_CustomsRegNo = "0100233488-123";
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			pivot.AIP_ErrorDescription = string.Empty;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("Validation should succeed", ZString.Empty, pivot.AIP_ErrorDescription);

			branchOrgProxy.CustomsCodes.RemoveAndDeleteAll();
			var companyOrgProxy = Factory.Load<OrgHeader>(Invoice.Company.GC_OH_OrgProxy);
			companyOrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "6666666", Core.Constants.CountryCodes.VietNam);
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			pivot.AIP_ErrorDescription = string.Empty;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("The VN VAT number '6666666' is invalid. It should be in format 'NNNNNNNNNN' or 'NNNNNNNNNN-NNN' and complies with check digit validation.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			AssertEquals(batchID, pivot.AIP_AIB);

			cusCode = companyOrgProxy.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam)[0];
			cusCode.OK_CustomsRegNo = "0100233488";
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			pivot.AIP_ErrorDescription = string.Empty;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("Validation should succeed", ZString.Empty, pivot.AIP_ErrorDescription);

			cusCode.OK_CustomsRegNo = "0100233488-001";
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Batched;
			pivot.AIP_ErrorDescription = string.Empty;
			Factory.Save();

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("Validation should succeed", ZString.Empty, pivot.AIP_ErrorDescription);
		}

		public void TestMultipleDebtorOrganizationContactDEFLengthOfEmailExceed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.ExportMultipleDebtorOrganizationContactEmail.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ExportMultipleDebtorOrganizationContactEmailCodes.DEF))
			{
				var createTransaction = () => TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);

				TestObjectCreator.ABIGAS.Contacts.RemoveAndDeleteAll();
				var contact = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C1", "Email1_890123456789001234567890123456789012345678900123456789012345678901234567890@contact.com;Email1_890123456789001234567890123456789012345678900123456789012345678901234567890@contact.com;Email1_890123456789001234567890123456789012345671@contact.com", "11111111", ContactType.Receivables.Code, Constants.ContactNotifyModes.Email);
				var pivot = CreateInvoiceWithContactInfo(createTransaction, ZGuid.Empty, 100, contact);

				GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

				AssertEquals("Maximum allowed Emails exceeded, please reduce the number of Organization Contacts marked with 'Group = A/R, Delivery = EML'.", pivot.AIP_ErrorDescription);
				AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			}
		}

		public void TestMultipleDebtorOrganizationContactMARLengthOfEmailExceed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.ExportMultipleDebtorOrganizationContactEmail.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ExportMultipleDebtorOrganizationContactEmailCodes.MAR))
			{
				var createTransaction = () => TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);

				TestObjectCreator.ABIGAS.Contacts.RemoveAndDeleteAll();
				var contact = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C1", "Email1_890123456789001234567890123456789012345678900123456789012345678901234567890@contact.com", "11111111", ContactType.Receivables.Code, Constants.ContactNotifyModes.Email);
				contact = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C2", "Email2_890123456789001234567890123456789012345678900123456789012345678901234567890@contact.com", "22222222", ContactType.Receivables.Code, Constants.ContactNotifyModes.Email);
				contact = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C3", "Email3_890123456789001234567890123456789012345678900123456789012345678901234567890@contact.com", "33333333", ContactType.Receivables.Code, Constants.ContactNotifyModes.Email);
				var pivot = CreateInvoiceWithContactInfo(createTransaction, ZGuid.Empty, 100, contact);

				GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

				AssertEquals("Maximum allowed Emails exceeded, please reduce the number of Organization Contacts marked with 'Group = A/R, Delivery = EML'.", pivot.AIP_ErrorDescription);
				AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			}
		}

		public void TestMultipleDebtorOrganizationContactMARLengthOfEmailValid()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.ExportMultipleDebtorOrganizationContactEmail.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, ExportMultipleDebtorOrganizationContactEmailCodes.MAR))
			{
				var createTransaction = () => TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);

				TestObjectCreator.ABIGAS.Contacts.RemoveAndDeleteAll();
				var contact = CreateContactWithDocumentDetails(TestObjectCreator.ABIGAS, "C1", "Email1_890123456789001234567890123456789012345678900123456789012345678901234567890@contact.com", "11111111", ContactType.Receivables.Code, Constants.ContactNotifyModes.Email);
				var pivot = CreateInvoiceWithContactInfo(createTransaction, ZGuid.Empty, 100, contact);

				GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

				AssertEquals(string.Empty, pivot.AIP_ErrorDescription);
			}
		}

		OrgContact CreateContactWithDocumentDetails(OrgHeader orgHeader, ZString name, ZString email, ZString phone, ZString documentGroup, ZString deliveryBy)
		{
			var contact = TestObjectCreator.CreateContact(orgHeader, name, email);

			var contactDocument = contact.Documents.AddNew();
			contactDocument.OD_DocumentGroup = documentGroup;
			contactDocument.OD_DeliverBy = deliveryBy;

			return contact;
		}

		AccEInvoicingTransactionPivot CreateInvoiceWithContactInfo(Func<InvoicingBase> createTransaction, ZGuid invoiceContactOverride, ZInt batchNumber, OrgContact expectedOrgContact)
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var invoice = createTransaction();
				invoice.AH_OC_InvoiceContactOverride = invoiceContactOverride;
				invoice.Branch.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;

				var batch = TestObjectCreator.CreateEInvoicingBatch(batchNumber, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "admin");
				AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345");
				var branchOrgProxy = Factory.Load<OrgHeader>(invoice.Branch.GB_OH_OrgProxy);
				branchOrgProxy.CustomsCodes.RemoveAndDeleteAll();
				branchOrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "0100233488", Core.Constants.CountryCodes.VietNam);
				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForVietnam();
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);

				return pivot;
			}
		}

		public void TestValidateEInvoiceUsername()
		{
			var pivot = CreateInvoice();
			var batchID = pivot.AIP_AIB;
			var orgProxy = Factory.Load<OrgHeader>(Invoice.Company.GC_OH_OrgProxy);
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "0100233488", Core.Constants.CountryCodes.VietNam);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "67890");

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("Vietnam E-Invoice Service Partner Connection Username and Password should be set in registry.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			AssertEquals(batchID, pivot.AIP_AIB);
		}

		public void TestValidateEInvoicePassword()
		{
			var pivot = CreateInvoice();
			var batchID = pivot.AIP_AIB;
			var orgProxy = Factory.Load<OrgHeader>(Invoice.Company.GC_OH_OrgProxy);
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "0100233488", Core.Constants.CountryCodes.VietNam);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345");

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("Vietnam E-Invoice Service Partner Connection Username and Password should be set in registry.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			AssertEquals(batchID, pivot.AIP_AIB);
		}

		public void TestValidationExchangeRate()
		{
			var pivot = CreateInvoice();
			pivot.ParentTransactionHeader.AH_ExchangeRate = 100000m;
			Factory.Save();

			var batchID = pivot.AIP_AIB;
			var orgProxy = Factory.Load<OrgHeader>(Invoice.Company.GC_OH_OrgProxy);
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "0100233488", Core.Constants.CountryCodes.VietNam);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345");
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "67890");

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

			AssertEquals("The exchange rate value '100000' has exceeded the maximum allowable length. It should have a maximum length of 7 digits inclusive of 2 decimals.", pivot.AIP_ErrorDescription);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			AssertEquals(batchID, pivot.AIP_AIB);
		}

		public void TestTruncateErrorMessageLengthWhenExceedMaxLength()
		{
			var pivot = CreateInvoice();
			var batchID = pivot.AIP_AIB;
			Invoice.Branch.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
			var branchOrgProxy = Factory.Load<OrgHeader>(Invoice.Branch.GB_OH_OrgProxy);
			branchOrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "12345678", Core.Constants.CountryCodes.VietNam);
			pivot.ParentTransactionHeader.AH_ExchangeRate = 100000m;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

			AssertNoExceptionThrown(() => GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest()));
			AssertEquals(AccEInvoicingTransactionPivotSchema.AIP_ErrorDescription.MaxLength, pivot.AIP_ErrorDescription.Length);
			AssertEquals(@"The VN VAT number '12345678' is invalid. It should be in format 'NNNNNNNNNN' or 'NNNNNNNNNN-NNN' and complies with check digit validation.
Vietnam E-Invoice Service Partner Connection Username and Password should be set in registry.
The exchange rate value '100000' has exceeded the maximum allo...", pivot.AIP_ErrorDescription);
		}

		public void TestValidate_Success()
		{
			var pivot = CreateInvoice();
			var orgProxy = Factory.Load<OrgHeader>(Invoice.Company.GC_OH_OrgProxy);
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "0100233488", Core.Constants.CountryCodes.VietNam);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345");
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "67890");

			GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());
			AssertEquals("Validation should succeed", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Status is not changed after data validation", Core.Constants.EInvoicingPivotState.Batched, pivot.AIP_Status);
		}

		public void TestSendValidationErrorMessage()
		{
			var company = Helper.CreateCompanyAndBranch("VN1", "BR1", Core.Constants.CountryCodes.VietNam, true);
			Factory.Save();
			var notificationGroupPK = Helper.CreateNotificationGroup("Test User 2", "company2user@abc.com");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				CreateInvoice();
				Factory.Save();

				AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345");
				AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "67890");

				GetEInvoicingDataValidatorForTest().Run(new DetailedLoggerForTest());

				AssertEquals("Email should be created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		ARInvoice Invoice;
		ARInvoice NonCurrentCompanyInvoice;
		ARInvoiceLine ARInvoiceLine;
		ARInvoiceLine NonCurrentCompanyARInvoiceLine;

		AccEInvoicingTransactionPivot CreateInvoice()
		{
			Invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1m, TestObjectCreator.Debtor);
			ARInvoiceLine = TestObjectCreator.CreateARInvoiceLine(Invoice, null, TestObjectCreator.FRT, TestObjectCreator.VND, 1m, "desc", 100m);
			ARInvoiceLine.AL_AT = TestObjectCreator.GST1.PK;

			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, Invoice, Core.Constants.EInvoicingPivotState.Batched);

			return pivot;
		}

		protected EInvoicingTestHelper Helper
		{
			get { return helper ?? (helper = new EInvoicingTestHelper(TestObjectCreator)); }
		}
		EInvoicingTestHelper helper;

		AccEInvoicingTransactionPivot CreateNonCurrentCompanyInvoice()
		{
			var differentCompany = GlbCompany.GetDemoCompany(Factory);
			var differentCompanyBranch = TestObjectCreator.CreateBranch("BR1", differentCompany);

			NonCurrentCompanyInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.VND, 1m, TestObjectCreator.Debtor);
			NonCurrentCompanyInvoice.AH_GC = differentCompany.PK;
			NonCurrentCompanyInvoice.AH_GB = differentCompanyBranch.PK;
			NonCurrentCompanyARInvoiceLine = TestObjectCreator.CreateARInvoiceLine(NonCurrentCompanyInvoice, null, TestObjectCreator.FRT, TestObjectCreator.VND, 1m, "desc2", 100m);
			NonCurrentCompanyARInvoiceLine.AL_AT = TestObjectCreator.GST1.PK;
			NonCurrentCompanyARInvoiceLine.AL_GC = differentCompany.PK;
			NonCurrentCompanyARInvoiceLine.AL_GB = differentCompanyBranch.PK;
			Factory.Save();

			var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, differentCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, NonCurrentCompanyInvoice, Core.Constants.EInvoicingPivotState.Batched);

			return pivot;
		}
	}
}
