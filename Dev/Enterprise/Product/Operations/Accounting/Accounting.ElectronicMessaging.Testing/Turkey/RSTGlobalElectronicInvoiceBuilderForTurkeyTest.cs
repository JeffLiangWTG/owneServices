using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.CountryCompliance.TurkeyComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	public class RSTGlobalElectronicInvoiceBuilderForTurkeyTest : TestCaseWithFactory
	{
		ICountryEInvoicingObjectFactory GetTestCountryFactory() => new TurkeyEInvoicingObjectFactory();

		[TestDate(2020, 1, 29, 15, 28, 00, 000)]
		public void TestRSTGlobalElectronicInvoiceIsGeneratedCorrectly()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var signatureCredential = Helper.CreateCompanySignatureCredential(Helper.TurkeyBranch.Company);
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR001", ComplianceSubTypeCodes.EAR);
				var batch = Helper.CreateEInvoicingBatch(1, EInvoicingBatchState.Ready, arInvoice, "1234", EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Queued);

				using (var converter = new TransactionBatchToGEIConverterForTurkey(GetTestCountryFactory()))
				{
					var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);
					var batchRequest = eInvoice.Header.ElectronicInvoiceBatchRequest;

					AssertEquals("MessagingSystem", "Turkey E-Invoicing System", batchRequest.MessagingSystem);
					AssertEquals("MessageType", TurkeyEInvoiceAPICommandList.Codes.StatusRequestForReceivablesInvoice, batchRequest.MessageType);
					AssertEquals("BatchNumber", batch.AIB_BatchNumber.ToString().PadLeft(5, '0'), batchRequest.BatchNumber);
					AssertEquals("FileName", string.Empty, batchRequest.FileName);
					AssertEquals("UserID", signatureCredential.GP_UserID, batchRequest.Certificate);
					var actualPasswordString = EInvoicingTestHelper.DecodePassword(batchRequest.Password);
					AssertEquals("Password", signatureCredential.CurrentDecryptedPassword, actualPasswordString);
					AssertEquals("No Error", string.Empty, validationErrors.ToString());

					AssertEquals("Payload", batch.AIB_GovernmentAllocatedNumber, eInvoice.Payload.ToUTF8FromBase64());
				}
			}
		}

		[TestDate(2020, 1, 29, 15, 28, 00, 000)]
		public void TestRSTGlobalElectronicEInvoiceMessageMissingCompanyCredential()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR001", ComplianceSubTypeCodes.EAR);
				var batch = Helper.CreateEInvoicingBatch(1, EInvoicingBatchState.Ready, arInvoice, "1234", EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Queued);

				using (var converter = new TransactionBatchToGEIConverterForTurkey(GetTestCountryFactory()))
				{
					var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);

					var expectedError = "Credential required for sending invoice is missing for company: " + Helper.TurkeyBranch.Company.GC_Code;
					AssertNull(eInvoice);
					AssertEquals("Error", expectedError, validationErrors.ToString());
				}
			}
		}

		[TestDate(2020, 1, 29, 15, 28, 00, 000)]
		public void TestRSTGlobalElectronicEInvoiceMessageEmptyOrNullInvoiceId()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDate.Today.AddDays(-1).ToDateTime()))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var signatureCredential = Helper.CreateCompanySignatureCredential(Helper.TurkeyBranch.Company);
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR001", ComplianceSubTypeCodes.EAR);
				var batch = Helper.CreateEInvoicingBatch(1, EInvoicingBatchState.Ready, arInvoice, "", EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Queued);
				
				using (var converter = new TransactionBatchToGEIConverterForTurkey(GetTestCountryFactory()))
				{
					AssertExceptionThrown("Invoice Id cannot be null", typeof(ArgumentException), expectedExceptionMessage: "Value cannot be empty string (\"\").\r\nParameter name: invoiceId", () => converter.Convert(batch));
				}
			}
		}

		TurkeyEInvoiceTestHelper Helper => helper ?? (helper = new TurkeyEInvoiceTestHelper());
		TurkeyEInvoiceTestHelper helper;
	}
}
