using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore.Testing
{
	public class TransactionBatchToGEIConverterForTaxCoreTest : TestCaseWithFactory
	{
		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestConversion()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("FJBXL");
			Factory.Save();
			var branch = company.Branches[0];

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				EnsureCorrectFijiUtcOffset(TestDateAttribute.Date);
				var credential = credentialCreator.CreateCertificateCredential();
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				Factory.Save();

				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AP001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
				Factory.Save();

				var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForTaxCore(CountryFactory);
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(invoicingBatch);

				AssertNullOrEmpty("ValidationErrors", validationErrors);
				AssertEquals("MessagingSystem", CountryFactory.InvoicingSystemName, eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
				AssertEquals("MessageType", "REQ", eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
				AssertEquals("BatchNumber", invoicingBatch.AIB_BatchNumber.ToString(), eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
				AssertEquals("FileName", string.Empty, eInvoice.Header.ElectronicInvoiceBatchRequest.FileName);
				AssertEquals("Certificate", credential.GP_Certificate, Convert.FromBase64String(eInvoice.Header.ElectronicInvoiceBatchRequest.Certificate));
				var actualPasswordString = EInvoicingTestHelper.DecodePassword(eInvoice.Header.ElectronicInvoiceBatchRequest.Password);
				AssertEquals("Password", credential.CurrentDecryptedCertificatePassphrase, actualPasswordString);
				var expectedJson = @"{""DateAndTimeOfIssue"":""2019-07-16T15:26:00Z"",""IT"":0,""TT"":0,""PaymentType"":0,""InvoiceNumber"":""00001000"",""Options"":{""OmitQRCodeGen"":""1"",""OmitTextualRepresentation"":""1""},""Hash"":""hLvuKoFVdtkpEAM3+jdvRg=="",""Items"":[{""Name"":""charge1"",""Quantity"":1,""UnitPrice"":10.0000,""TotalAmount"":10.0000}]}";
				AssertEquals("Error", string.Empty, validationErrors.ToString());
				AssertEquals("Payload", expectedJson, eInvoice.Payload.ToUTF8FromBase64());
			}
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestConversionWithBatchWithoutPivot()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("FJBXL");
			Factory.Save();
			var branch = company.Branches[0];

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				EnsureCorrectFijiUtcOffset(TestDateAttribute.Date);
				var credential = credentialCreator.CreateCertificateCredential();
				Factory.Save();

				var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				Factory.Save();

				AssertEquals("PreCond: batch doesn't have any pivot", 0, invoicingBatch.TransactionPivots.Count);
				ErrorReporter.Clear();

				var converter = new TransactionBatchToGEIConverterForTaxCore(CountryFactory);

				AssertExceptionThrown<NotSupportedException>("batch without pivot expected exception",
					"The batch has 0 transactions. There must be only one transaction per batch to generate the Electronic Invoice.",
					() => converter.Convert(invoicingBatch));

				var expectedDevError = $@"Batch Info:
	PK = {invoicingBatch.PK}
	Type = AccEInvoicingBatch
	Types around row = AccEInvoicingBatch
	Factory Instance = {invoicingBatch.Factory._Instance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None

Properties:
 AIB_BatchNumber = 1
 AIB_EHubAllocatedNumber = 
 AIB_GC = {company.PK} ({company.GC_Code})
 AIB_GovernmentAllocatedNumber = 
 AIB_Status = RDY
 AIB_SystemCreateTimeUtc = 16-Jul-19 15:26:32
 AIB_SystemCreateUser = E
 AIB_SystemLastEditTimeUtc = 16-Jul-19 15:26:32
 AIB_SystemLastEditUser = E

Batch has 0 pivot(s):";

				AssertMultilineASCIIEquals(expectedDevError, ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
		}

		[TestDate(2019, 7, 16, 15, 26, 32)]
		public void TestConversionWithBatchWithMultiplePivots()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("FJBXL");
			Factory.Save();
			var branch = company.Branches[0];
			var factoryInstance = Factory._Instance;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				EnsureCorrectFijiUtcOffset(TestDateAttribute.Date);
				var credential = credentialCreator.CreateCertificateCredential();
				var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), Business.TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), Business.TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arInvoice1, Core.Constants.EInvoicingPivotState.Batched);
				var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arInvoice2, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				AssertEquals("PreCond: batch have more than one pivot", 2, invoicingBatch.TransactionPivots.Count);
				ErrorReporter.Clear();

				var converter = new TransactionBatchToGEIConverterForTaxCore(CountryFactory);

				AssertExceptionThrown<NotSupportedException>("batch without pivot expected exception",
					"The batch has 2 transactions. There must be only one transaction per batch to generate the Electronic Invoice.",
					() => converter.Convert(invoicingBatch));

				var expectedDevError = $@"Batch Info:
	PK = {invoicingBatch.PK}
	Type = AccEInvoicingBatch
	Types around row = AccEInvoicingBatch
	Factory Instance = {invoicingBatch.Factory._Instance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None

Properties:
 AIB_BatchNumber = 1
 AIB_EHubAllocatedNumber = 
 AIB_GC = {company.PK} ({company.GC_Code})
 AIB_GovernmentAllocatedNumber = 
 AIB_Status = RDY
 AIB_SystemCreateTimeUtc = 16-Jul-19 15:26:32
 AIB_SystemCreateUser = E
 AIB_SystemLastEditTimeUtc = 16-Jul-19 15:26:32
 AIB_SystemLastEditUser = E

Batch has 2 pivot(s):
Pivot #1:
	PK = {pivot1.PK}
	Type = AccEInvoicingTransactionPivot
	Types around row = AccEInvoicingTransactionPivot
	Factory Instance = {factoryInstance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None

Properties:
 AIP_ActionType = SUB
 AIP_AIB = {invoicingBatch.PK}
 AIP_ErrorDescription = 
 AIP_GC = {company.PK} ({company.GC_Code})
 AIP_IsNotifiedByEmail = N
 AIP_LastResponseReceivedUtc = 
 AIP_LastSentTimeUtc = 
 AIP_ParentID = {arInvoice1.PK}
 AIP_ParentTableCode = AH
 AIP_RN_NKCountryCode = FJ
 AIP_Status = BCH
 AIP_SystemCreateTimeUtc = 16-Jul-19 15:26:32
 AIP_SystemCreateUser = E
 AIP_SystemLastEditTimeUtc = 16-Jul-19 15:26:32
 AIP_SystemLastEditUser = E

Pivot Parent transaction Info:
Header: PK = {arInvoice1.PK}, Ledger = AR, Transaction Type = INV, Invoice Date = 16-Jul-19 15:26:32, Post Date = 16-Jul-19 15:26:32, Invoice Amount = 100.0, GST Amount = 10.0, OS Total = 110.00, Exchange Rate = 1.0, Currency = AUD, Outstanding Amount = 110.0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 00001000, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = Yes, Is Deleted = No, Has Changes = No, Business Contexts = None, System Create Time = 16-Jul-19 15:26:32, System Create User = E, System Last Edit Time = 16-Jul-19 15:26:32, System Last Edit User = E.

Pivot #2:
	PK = {pivot2.PK}
	Type = AccEInvoicingTransactionPivot
	Types around row = AccEInvoicingTransactionPivot
	Factory Instance = {factoryInstance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None

Properties:
 AIP_ActionType = SUB
 AIP_AIB = {invoicingBatch.PK}
 AIP_ErrorDescription = 
 AIP_GC = {company.PK} ({company.GC_Code})
 AIP_IsNotifiedByEmail = N
 AIP_LastResponseReceivedUtc = 
 AIP_LastSentTimeUtc = 
 AIP_ParentID = {arInvoice2.PK}
 AIP_ParentTableCode = AH
 AIP_RN_NKCountryCode = FJ
 AIP_Status = BCH
 AIP_SystemCreateTimeUtc = 16-Jul-19 15:26:32
 AIP_SystemCreateUser = E
 AIP_SystemLastEditTimeUtc = 16-Jul-19 15:26:32
 AIP_SystemLastEditUser = E

Pivot Parent transaction Info:
Header: PK = {arInvoice2.PK}, Ledger = AR, Transaction Type = INV, Invoice Date = 16-Jul-19 15:26:32, Post Date = 16-Jul-19 15:26:32, Invoice Amount = 100.0, GST Amount = 10.0, OS Total = 110.00, Exchange Rate = 1.0, Currency = AUD, Outstanding Amount = 110.0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 00001001, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = Yes, Is Deleted = No, Has Changes = No, Business Contexts = None, System Create Time = 16-Jul-19 15:26:32, System Create User = E, System Last Edit Time = 16-Jul-19 15:26:32, System Last Edit User = E.
";

				AssertMultilineASCIIEquals(expectedDevError, ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
		}

		[TestDate(2018, 7, 16, 15, 26, 32)]
		public void TestConversion_CreditNote()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("FJBXL");
			TestObjectCreator.SetCustomsCodeForOrgHeader(TestObjectCreator.Debtor, CountryFactory.TaxFileCode, CountryCode, "12345678798");
			Factory.Save();
			var branch = company.Branches[0];

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, TestDateAttribute.Date.AddDays(-1)))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				EnsureCorrectFijiUtcOffset(TestDateAttribute.Date);
				var credential = credentialCreator.CreateCertificateCredential();
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
				Factory.Save();

				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AP001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
				arInvoice.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;

				var arInvoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				arInvoicingBatch.AIB_GovernmentAllocatedNumber = "REF001";
				var arInvoicePivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoicingBatch, arInvoice, Core.Constants.EInvoicingPivotState.Succeed);

				Factory.Save();

				var arCreditNote = TestObjectCreator.CreateARCreditNoteWithLine("ARCRD001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Credit Note", job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
				arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
				(arCreditNote as IAmending).FlagAsCreatedAmending();
				arCreditNote.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
				Factory.Save();

				var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(2, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arCreditNote, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForTaxCore(CountryFactory);
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(invoicingBatch);

				AssertNullOrEmpty("ValidationErrors", validationErrors);
				AssertEquals("MessagingSystem", "Fiji electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
				AssertEquals("MessageType", "REQ", eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
				AssertEquals("BatchNumber", invoicingBatch.AIB_BatchNumber.ToString(), eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
				AssertEquals("FileName", string.Empty, eInvoice.Header.ElectronicInvoiceBatchRequest.FileName);
				AssertEquals("Certificate", credential.GP_Certificate, Convert.FromBase64String(eInvoice.Header.ElectronicInvoiceBatchRequest.Certificate));
				var actualPasswordString = EInvoicingTestHelper.DecodePassword(eInvoice.Header.ElectronicInvoiceBatchRequest.Password);
				AssertEquals("Password", credential.CurrentDecryptedCertificatePassphrase, actualPasswordString);
				var expectedJson = @"{""DateAndTimeOfIssue"":""2018-07-16T15:26:00Z"",""BD"":""12345678798"",""IT"":0,""TT"":1,""PaymentType"":0,""InvoiceNumber"":""00001000"",""ReferentDocumentNumber"":""REF001"",""ReferentDocumentDateAndTime"":""2018-07-17T03:26:32Z"",""Options"":{""OmitQRCodeGen"":""1"",""OmitTextualRepresentation"":""1""},""Hash"":""Eyw5dClF5xF3MJQliYc5bQ=="",""Items"":[{""Name"":""Credit Note"",""Quantity"":1,""UnitPrice"":100.0000,""TotalAmount"":100.0000}]}";
				AssertEquals("Error", string.Empty, validationErrors.ToString());
				AssertEquals("Payload", expectedJson, eInvoice.Payload.ToUTF8FromBase64());
			}
		}

		[TestDate(2018, 7, 16, 15, 26, 32)]
		public void TestConversion_CreditNote_BeforeComplianceDate()
		{
			EnsureCorrectFijiUtcOffset(TestDateAttribute.Date);
			AssertConversion_CreditNote_BeforeComplianceDate("2018-07-17T03:26:32Z", "4CeAsJ2lnJVL75NDJotM1Q==");
		}

		[TestDate(2018, 7, 16, 15, 26, 32)]
		public void TestConversion_CreditNote_BeforeComplianceDateWithIncorrectUtcOffset()
		{
			TestConnection.ExecuteNonQuery("DELETE RefDatabase_RefUNLOCOUtcOffset WHERE RLO_RL_NKCOde LIKE 'FJ%'");
			AssertNull(TestConnection.ExecuteScalar($"SELECT Offset FROM dbo.GetTimeZoneOffsetInMinutes('FJBXL', '{ZDateTime.UtcNow}')"));

			AssertConversion_CreditNote_BeforeComplianceDate("2018-07-16T15:26:32Z", "ElQ2Mv/UcgUHCpXe+uUi9Q==");
		}

		void AssertConversion_CreditNote_BeforeComplianceDate(string expectedDateTime, string expectedHash)
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("FJBXL");
			TestObjectCreator.SetCustomsCodeForOrgHeader(TestObjectCreator.Debtor, CountryFactory.TaxFileCode, CountryCode, "12345678798");
			Factory.Save();
			var branch = company.Branches[0];

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, TestDateAttribute.Date.AddDays(1)))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var credential = credentialCreator.CreateCertificateCredential();
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
				Factory.Save();

				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AP001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
				arInvoice.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
				arInvoice.AH_TransactionReference = "REF001";
				arInvoice.AH_PostDate = TestDateAttribute.Date.AddDays(-1);
				Factory.Save();

				var arCreditNote = TestObjectCreator.CreateARCreditNoteWithLine("ARCRD001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Credit Note", job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
				arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
				(arCreditNote as IAmending).FlagAsCreatedAmending();
				arCreditNote.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
				Factory.Save();

				var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arCreditNote, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForTaxCore(CountryFactory);
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(invoicingBatch);

				AssertNullOrEmpty("ValidationErrors", validationErrors);
				AssertEquals("MessagingSystem", "Fiji electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
				AssertEquals("MessageType", "REQ", eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
				AssertEquals("BatchNumber", invoicingBatch.AIB_BatchNumber.ToString(), eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
				AssertEquals("FileName", string.Empty, eInvoice.Header.ElectronicInvoiceBatchRequest.FileName);
				AssertEquals("Certificate", credential.GP_Certificate, Convert.FromBase64String(eInvoice.Header.ElectronicInvoiceBatchRequest.Certificate));
				var actualPasswordString = EInvoicingTestHelper.DecodePassword(eInvoice.Header.ElectronicInvoiceBatchRequest.Password);
				AssertEquals("Password", credential.CurrentDecryptedCertificatePassphrase, actualPasswordString);
				var expectedJson = "{\"DateAndTimeOfIssue\":\"2018-07-16T15:26:00Z\",\"BD\":\"12345678798\",\"IT\":0,\"TT\":1,\"PaymentType\":0,\"InvoiceNumber\":\"00001000\",\"ReferentDocumentNumber\":\"XXXXXXXX-XXXXXXXX-1\",\"ReferentDocumentDateAndTime\":\"" + expectedDateTime + "\",\"Options\":{\"OmitQRCodeGen\":\"1\",\"OmitTextualRepresentation\":\"1\"},\"Hash\":\"" + expectedHash + "\",\"Items\":[{\"Name\":\"Credit Note\",\"Quantity\":1,\"UnitPrice\":100.0000,\"TotalAmount\":100.0000}]}";
				AssertEquals("Error", string.Empty, validationErrors.ToString());
				AssertEquals("Payload", expectedJson, eInvoice.Payload.ToUTF8FromBase64());
			}
		}

		[TestDate(2018, 7, 16, 15, 26, 32)]
		public void TestConversion_CreditNote_BeforeComplianceDate_LessThanOneDayDifference()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("FJBXL");
			TestObjectCreator.SetCustomsCodeForOrgHeader(TestObjectCreator.Debtor, CountryFactory.TaxFileCode, CountryCode, "12345678798");
			Factory.Save();
			var branch = company.Branches[0];

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, TestDateAttribute.Date))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				EnsureCorrectFijiUtcOffset(TestDateAttribute.Date);

				var credential = credentialCreator.CreateCertificateCredential();
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
				Factory.Save();

				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AP001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
				arInvoice.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
				arInvoice.AH_TransactionReference = "REF001";
				arInvoice.AH_PostDate = TestDateAttribute.Date.AddHours(-23.59);
				Factory.Save();

				var arCreditNote = TestObjectCreator.CreateARCreditNoteWithLine("ARCRD001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Credit Note", job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
				arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
				(arCreditNote as IAmending).FlagAsCreatedAmending();
				arCreditNote.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
				Factory.Save();

				var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arCreditNote, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForTaxCore(CountryFactory);
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(invoicingBatch);

				AssertNullOrEmpty("ValidationErrors", validationErrors);
				AssertEquals("MessagingSystem", "Fiji electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
				AssertEquals("MessageType", "REQ", eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
				AssertEquals("BatchNumber", invoicingBatch.AIB_BatchNumber.ToString(), eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
				AssertEquals("FileName", string.Empty, eInvoice.Header.ElectronicInvoiceBatchRequest.FileName);
				AssertEquals("Certificate", credential.GP_Certificate, Convert.FromBase64String(eInvoice.Header.ElectronicInvoiceBatchRequest.Certificate));
				var actualPasswordString = EInvoicingTestHelper.DecodePassword(eInvoice.Header.ElectronicInvoiceBatchRequest.Password);
				AssertEquals("Password", credential.CurrentDecryptedCertificatePassphrase, actualPasswordString);
				var expectedJson = @"{""DateAndTimeOfIssue"":""2018-07-16T15:26:00Z"",""BD"":""12345678798"",""IT"":0,""TT"":1,""PaymentType"":0,""InvoiceNumber"":""00001000"",""ReferentDocumentNumber"":""XXXXXXXX-XXXXXXXX-1"",""ReferentDocumentDateAndTime"":""2018-07-17T03:26:32Z"",""Options"":{""OmitQRCodeGen"":""1"",""OmitTextualRepresentation"":""1""},""Hash"":""4CeAsJ2lnJVL75NDJotM1Q=="",""Items"":[{""Name"":""Credit Note"",""Quantity"":1,""UnitPrice"":100.0000,""TotalAmount"":100.0000}]}";
				AssertEquals("Payload", expectedJson, eInvoice.Payload.ToUTF8FromBase64());
				AssertEquals("Error", string.Empty, validationErrors.ToString());
			}
		}

		[TestDate(2018, 7, 16, 15, 26, 32)]
		public void TestConversion_CreditNote_ComplianceDateEqualsOriginalTransactionCreationDate()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("FJBXL");
			TestObjectCreator.SetCustomsCodeForOrgHeader(TestObjectCreator.Debtor, CountryFactory.TaxFileCode, CountryCode, "12345678798");
			Factory.Save();
			var branch = company.Branches[0];

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2018, 7, 10)))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				EnsureCorrectFijiUtcOffset(TestDateAttribute.Date);

				var credential = credentialCreator.CreateCertificateCredential();
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
				Factory.Save();

				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AP001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
				arInvoice.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
				arInvoice.AH_TransactionReference = "REF001";
				arInvoice.AH_PostDate = new ZDateTime(2018, 7, 9, 15, 30, 0); //For Fiji, local creation date is 10/07/2018
				Factory.Save();

				var arCreditNote = TestObjectCreator.CreateARCreditNoteWithLine("ARCRD001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Credit Note", job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
				arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
				(arCreditNote as IAmending).FlagAsCreatedAmending();
				arCreditNote.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
				Factory.Save();

				var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arCreditNote, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForTaxCore(CountryFactory);
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(invoicingBatch);

				AssertNullOrEmpty("ValidationErrors", validationErrors);
				AssertEquals("MessagingSystem", "Fiji electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
				AssertEquals("MessageType", "REQ", eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
				AssertEquals("BatchNumber", invoicingBatch.AIB_BatchNumber.ToString(), eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
				AssertEquals("FileName", string.Empty, eInvoice.Header.ElectronicInvoiceBatchRequest.FileName);
				AssertEquals("Certificate", credential.GP_Certificate, Convert.FromBase64String(eInvoice.Header.ElectronicInvoiceBatchRequest.Certificate));
				var actualPasswordString = EInvoicingTestHelper.DecodePassword(eInvoice.Header.ElectronicInvoiceBatchRequest.Password);
				AssertEquals("Password", credential.CurrentDecryptedCertificatePassphrase, actualPasswordString);
				var expectedJson = @"{""DateAndTimeOfIssue"":""2018-07-16T15:26:00Z"",""BD"":""12345678798"",""IT"":0,""TT"":1,""PaymentType"":0,""InvoiceNumber"":""00001000"",""ReferentDocumentNumber"":""XXXXXXXX-XXXXXXXX-1"",""ReferentDocumentDateAndTime"":""2018-07-17T03:26:32Z"",""Options"":{""OmitQRCodeGen"":""1"",""OmitTextualRepresentation"":""1""},""Hash"":""4CeAsJ2lnJVL75NDJotM1Q=="",""Items"":[{""Name"":""Credit Note"",""Quantity"":1,""UnitPrice"":100.0000,""TotalAmount"":100.0000}]}";
				AssertEquals("Payload", expectedJson, eInvoice.Payload.ToUTF8FromBase64());
				AssertEquals("Error", string.Empty, validationErrors.ToString());
			}
		}

		[TestDate(2018, 7, 16, 15, 26, 32)]
		public void TestConversion_CreditNote_OriginalTransactionPostDateIsBeforeComplianceDate()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("FJBXL");
			TestObjectCreator.SetCustomsCodeForOrgHeader(TestObjectCreator.Debtor, CountryFactory.TaxFileCode, CountryCode, "12345678798");
			Factory.Save();
			var branch = company.Branches[0];

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2018, 7, 10)))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				EnsureCorrectFijiUtcOffset(TestDateAttribute.Date);
				var credential = credentialCreator.CreateCertificateCredential();
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
				Factory.Save();

				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AP001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
				arInvoice.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
				arInvoice.AH_PostDate = new ZDateTime(2018, 7, 8);

				var createPivotCheck = TransactionHeader.CreateEInvoicingPivotForRequest(Factory, arInvoice, EInvoicingPivotActionType.Submit);
				Assert("Expect no transaction pivot because the arInvoice post date is before the registry EReportingComplianceDate date", !createPivotCheck.pivotCreated);

				Factory.Save();

				var arCreditNote = TestObjectCreator.CreateARCreditNoteWithLine("ARCRD001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Credit Note", job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
				arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
				(arCreditNote as IAmending).FlagAsCreatedAmending();
				arCreditNote.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
				Factory.Save();

				var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(2, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arCreditNote, EInvoicingPivotState.Batched);
				Factory.Save();

				var converter = new TransactionBatchToGEIConverterForTaxCore(CountryFactory);
				var (eInvoice, validationErrors, validationWarnings) = converter.Convert(invoicingBatch);

				AssertNullOrEmpty("ValidationErrors", validationErrors);
				AssertEquals("MessagingSystem", "Fiji electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
				AssertEquals("MessageType", "REQ", eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
				AssertEquals("BatchNumber", invoicingBatch.AIB_BatchNumber.ToString(), eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
				AssertEquals("FileName", string.Empty, eInvoice.Header.ElectronicInvoiceBatchRequest.FileName);
				AssertEquals("Certificate", credential.GP_Certificate, Convert.FromBase64String(eInvoice.Header.ElectronicInvoiceBatchRequest.Certificate));
				var actualPasswordString = EInvoicingTestHelper.DecodePassword(eInvoice.Header.ElectronicInvoiceBatchRequest.Password);
				AssertEquals("Password", credential.CurrentDecryptedCertificatePassphrase, actualPasswordString);
				var expectedJson = @"{""DateAndTimeOfIssue"":""2018-07-16T15:26:00Z"",""BD"":""12345678798"",""IT"":0,""TT"":1,""PaymentType"":0,""InvoiceNumber"":""00001000"",""ReferentDocumentNumber"":""XXXXXXXX-XXXXXXXX-1"",""ReferentDocumentDateAndTime"":""2018-07-17T03:26:32Z"",""Options"":{""OmitQRCodeGen"":""1"",""OmitTextualRepresentation"":""1""},""Hash"":""4CeAsJ2lnJVL75NDJotM1Q=="",""Items"":[{""Name"":""Credit Note"",""Quantity"":1,""UnitPrice"":100.0000,""TotalAmount"":100.0000}]}";
				AssertEquals("Error", string.Empty, validationErrors.ToString());
				AssertEquals("Payload", expectedJson, eInvoice.Payload.ToUTF8FromBase64());
			}
		}

		[TestDate(2018, 7, 16, 15, 26, 32)]
		public void TestConversion_CreditNote_OriginalTransactionCreatedAfterCompliance_AndWhenEInvoicingInactive()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("FJBXL");
			TestObjectCreator.SetCustomsCodeForOrgHeader(TestObjectCreator.Debtor, CountryFactory.TaxFileCode, CountryCode, "12345678798");
			Factory.Save();
			var branch = company.Branches[0];

			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2018, 7, 10)))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var credentialCreator = new TestEInvoicingCertificateCredentialCreator(branch, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(7)))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				EnsureCorrectFijiUtcOffset(TestDateAttribute.Date);

				var credential = credentialCreator.CreateCertificateCredential();

				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;
				Factory.Save();

				var arInvoicePK = ZGuid.Empty;
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AP001", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
					arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
					arInvoice.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
					arInvoice.AH_TransactionReference = "REF001";
					Factory.Save();

					arInvoicePK = arInvoice.PK;
					AssertEquals("Precondition: when eInvoicing is inactive, pivot status is Discarded", EInvoicingPivotState.Discarded, arInvoice.EInvoicingStatus);
					AssertEquals("Precondition: when eInvoicing is inactive, a specific error message is used", Business.EInvoicing.AccEInvoicingTransactionPivot.ErrorDescriptionWhenEInvoicingDisabled(), arInvoice.EInvoicingError);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var arCreditNote = TestObjectCreator.CreateARCreditNoteWithLine("ARCRD001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Credit Note", job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);
					arCreditNote.AH_TransactionBelongsToGroup = arInvoicePK;
					(arCreditNote as IAmending).FlagAsCreatedAmending();
					arCreditNote.Lines[0].AL_AT = TestObjectCreator.GSTFREE1.PK;
					Factory.Save();

					var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
					var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arCreditNote, Core.Constants.EInvoicingPivotState.Batched);
					Factory.Save();

					var converter = new TransactionBatchToGEIConverterForTaxCore(CountryFactory);
					var (eInvoice, validationErrors, validationWarnings) = converter.Convert(invoicingBatch);

					AssertNullOrEmpty("ValidationErrors", validationErrors);
					AssertEquals("MessagingSystem", "Fiji electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
					AssertEquals("MessageType", "REQ", eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
					AssertEquals("BatchNumber", invoicingBatch.AIB_BatchNumber.ToString(), eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
					AssertEquals("FileName", string.Empty, eInvoice.Header.ElectronicInvoiceBatchRequest.FileName);
					AssertEquals("Certificate", credential.GP_Certificate, Convert.FromBase64String(eInvoice.Header.ElectronicInvoiceBatchRequest.Certificate));
					var actualPasswordString = EInvoicingTestHelper.DecodePassword(eInvoice.Header.ElectronicInvoiceBatchRequest.Password);
					AssertEquals("Password", credential.CurrentDecryptedCertificatePassphrase, actualPasswordString);
					var expectedJson = @"{""DateAndTimeOfIssue"":""2018-07-16T15:26:00Z"",""BD"":""12345678798"",""IT"":0,""TT"":1,""PaymentType"":0,""InvoiceNumber"":""00001000"",""ReferentDocumentNumber"":""XXXXXXXX-XXXXXXXX-1"",""ReferentDocumentDateAndTime"":""2018-07-17T03:26:32Z"",""Options"":{""OmitQRCodeGen"":""1"",""OmitTextualRepresentation"":""1""},""Hash"":""4CeAsJ2lnJVL75NDJotM1Q=="",""Items"":[{""Name"":""Credit Note"",""Quantity"":1,""UnitPrice"":100.0000,""TotalAmount"":100.0000}]}";
					AssertEquals("Payload", expectedJson, eInvoice.Payload.ToUTF8FromBase64());
					AssertEquals("Error", string.Empty, validationErrors.ToString());
				}
			}
		}

		protected ITaxCoreCountryEInvoicingObjectFactory CountryFactory => TaxCoreEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCode);

		ZString CountryCode => CountryCodes.Fiji;

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ??= new TestObjectCreator(Factory); }
		}
		TestObjectCreator testObjectCreator;

		// needed as sometimes offset values in test system are incorrect due to transform needing to run from hours to minutes (not 100% sure but would explain why unit tests are failing)
		void EnsureCorrectFijiUtcOffset(DateTime timeFromUtc)
		{
			short offset = 720;
			var unlocoCode = "FJBXL";
			var timeFromUtcAsTSQLString = FormattableString.Invariant($@"'{timeFromUtc.Year}-{timeFromUtc.Month:D2}-{timeFromUtc.Day:D2}T{timeFromUtc.Hour:D2}:{timeFromUtc.Minute:D2}:{timeFromUtc.Second:D2}'");
			var whereClause = FormattableString.Invariant($@" WHERE RLO_RL_NKCode = '{unlocoCode}' AND RLO_StartTimeUtc <= {timeFromUtcAsTSQLString} AND RLO_EndTimeUtc >= {timeFromUtcAsTSQLString}");
			var sql = FormattableString.Invariant($@"IF EXISTS(SELECT * FROM dbo.RefDatabase_RefUNLOCOUtcOffset {whereClause})
BEGIN
	UPDATE dbo.RefDatabase_RefUNLOCOUtcOffset SET RLO_OffsetMinutesFromUtc = {offset} {whereClause}
END ELSE BEGIN
	INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc)
	VALUES (NEWID(), '{unlocoCode}', DATEADD(day, -30, {timeFromUtcAsTSQLString}), DATEADD(day, 30, {timeFromUtcAsTSQLString}), {offset})
END");
			TestConnection.ExecuteNonQuery(sql);
		}
	}
}
