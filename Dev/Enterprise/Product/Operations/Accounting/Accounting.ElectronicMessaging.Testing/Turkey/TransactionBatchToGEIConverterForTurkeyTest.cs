using System.Linq;
using System.Xml;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.Export.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.CountryCompliance.TurkeyComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	sealed class TransactionBatchToGEIConverterForTurkeyTest : TestCaseWithFactory
	{
		ICountryEInvoicingObjectFactory GetTestCountryFactory() => new TurkeyEInvoicingObjectFactory();

		[TestTimeZoneUNLOCO("TRIST")]
		[TestDate(2020, 1, 29, 23, 08, 00)]
		public void TestInvoiceConversionIsCorrect_EIN() => TestInvoiceConversionIsCorrect(ComplianceSubTypeCodes.EIN);

		[TestTimeZoneUNLOCO("TRIST")]
		[TestDate(2020, 1, 29, 23, 08, 00)]
		public void TestInvoiceConversionIsCorrect_EIN_GOVOrganisation() => TestInvoiceConversionIsCorrect(ComplianceSubTypeCodes.EIN, true);

		[TestTimeZoneUNLOCO("TRIST")]
		[TestDate(2020, 1, 29, 23, 08, 00)]
		public void TestInvoiceConversionIsCorrect_EAR() => TestInvoiceConversionIsCorrect(ComplianceSubTypeCodes.EAR);

		void TestInvoiceConversionIsCorrect(string complianceSubType, bool isGovOrganization = false)
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TRY"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "kuruş");
			Factory.Save();
			using (Helper.SetUpForTestingEInvoicingTurkeyWithControlAccounts())
			{
				var signatureCredential = Helper.CreateCompanySignatureCredential(Helper.TurkeyBranch.Company);
				var debtor = Helper.TestObjectCreator.DebtorTR;
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR001", complianceSubType, true, true, isGovOrganization);
				var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice, 2);
				var pivot = Helper.TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arInvoice, EInvoicingPivotState.Batched);
				var xmlFileContent = (isGovOrganization
					? TurkeyEInvoiceExportedXmlLocalCurrency_GOV
					: complianceSubType == ComplianceSubTypeCodes.EIN
					? TurkeyEInvoiceExportedXmlLocalCurrency
					: TurkeyEInvoiceExportedXmlLocalCurrency_EAR);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());

				using (var converter = new TransactionBatchToGEIConverterForTurkey(GetTestCountryFactory()))
				using (var transactionBatch = exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					var uInvoice = transactionBatch.TransactionCollection.First();
					Helper.SetTransactionShipment1(uInvoice);
					Helper.SetTransactionShipment2(uInvoice);
					var (eInvoiceRIN, validationErrorsNotifications, validationWarningsNotifications) = new RINGlobalElectronicInvoiceBuilderForTurkey(invoicingBatch.AIB_BatchNumber.ToString(), transactionBatch).Create();
					var batchRequest = eInvoiceRIN.Header.ElectronicInvoiceBatchRequest;
					AssertEquals("MessagingSystem", "Turkey E-Invoicing System", batchRequest.MessagingSystem);
					AssertEquals("MessageType", TurkeyEInvoiceAPICommandList.Codes.SendReceivablesInvoice, batchRequest.MessageType);
					AssertEquals("CompanyCode", Helper.TurkeyBranch.Company.GC_Code, batchRequest.CompanyCode);
					AssertEquals("BatchNumber", invoicingBatch.AIB_BatchNumber.ToString().PadLeft(5, '0'), batchRequest.BatchNumber);
					AssertEquals("FileName", string.Empty, batchRequest.FileName);
					AssertEquals("UserID", signatureCredential.GP_UserID, batchRequest.Certificate);
					var actualPasswordString = EInvoicingTestHelper.DecodePassword(batchRequest.Password);
					AssertEquals("Password", signatureCredential.CurrentDecryptedPassword, actualPasswordString);
					AssertEquals("No Error", string.Empty, validationErrorsNotifications.ToString());

					var xmlDocumentExpected = new XmlDocument();
					xmlDocumentExpected.LoadXml(xmlFileContent);
					var xmlDocumentPayLoad = new XmlDocument();
					xmlDocumentPayLoad.LoadXml(eInvoiceRIN.Payload.ToUTF8FromBase64());
					AssertEquals("Payload", xmlDocumentExpected.OuterXml, xmlDocumentPayLoad.OuterXml);

					pivot.AIP_Status = EInvoicingPivotState.Succeed;
					invoicingBatch.AIB_GovernmentAllocatedNumber = Helper.GovermentAllocatedNumberForTest;
					invoicingBatch.AIB_Status = EInvoicingBatchState.Sent;
					Factory.Save();

					var batch = Helper.CreateEInvoicingBatch(3, EInvoicingBatchState.Ready, arInvoice, invoicingBatch.AIB_GovernmentAllocatedNumber, EInvoicingPivotActionType.DocumentAction, EInvoicingPivotState.Queued);
					Factory.Save();

					var (eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);
					batchRequest = eInvoice.Header.ElectronicInvoiceBatchRequest;
					AssertEquals("MessagingSystem", "Turkey E-Invoicing System", batchRequest.MessagingSystem);
					AssertEquals("MessageType", TurkeyEInvoiceAPICommandList.Codes.RequestPDFofReceivablesInvoice, batchRequest.MessageType);
					AssertEquals("CompanyCode", Helper.TurkeyBranch.Company.GC_Code, batchRequest.CompanyCode);
					AssertEquals("BatchNumber", batch.AIB_BatchNumber.ToString().PadLeft(5, '0'), batchRequest.BatchNumber);
					AssertEquals("FileName", string.Empty, batchRequest.FileName);
					AssertEquals("UserID", signatureCredential.GP_UserID, batchRequest.Certificate);
					actualPasswordString = EInvoicingTestHelper.DecodePassword(batchRequest.Password);
					AssertEquals("Password", signatureCredential.CurrentDecryptedPassword, actualPasswordString);
					AssertEquals("No Error", string.Empty, validationErrors.ToString());

					AssertEquals("Payload", batch.AIB_GovernmentAllocatedNumber, eInvoice.Payload.ToUTF8FromBase64());

					var arCreditNote = Helper.CreateReverseTransaction(arInvoice);
					batch = Helper.CreateEInvoicingBatch(1, EInvoicingBatchState.Ready, arCreditNote, invoicingBatch.AIB_GovernmentAllocatedNumber, EInvoicingPivotActionType.Cancel, EInvoicingPivotState.Queued);
					Factory.Save();

					(eInvoice, validationErrors, validationWarnings) = converter.Convert(batch);
					batchRequest = eInvoice.Header.ElectronicInvoiceBatchRequest;
					AssertEquals("MessagingSystem", "Turkey E-Invoicing System", batchRequest.MessagingSystem);
					AssertEquals("MessageType", TurkeyEInvoiceAPICommandList.Codes.CancelReceivablesInvoice, batchRequest.MessageType);
					AssertEquals("CompanyCode", Helper.TurkeyBranch.Company.GC_Code, batchRequest.CompanyCode);
					AssertEquals("BatchNumber", batch.AIB_BatchNumber.ToString().PadLeft(5, '0'), batchRequest.BatchNumber);
					AssertEquals("FileName", string.Empty, batchRequest.FileName);
					AssertEquals("UserID", signatureCredential.GP_UserID, batchRequest.Certificate);
					actualPasswordString = EInvoicingTestHelper.DecodePassword(batchRequest.Password);
					AssertEquals("Password", signatureCredential.CurrentDecryptedPassword, actualPasswordString);
					AssertEquals("No Error", string.Empty, validationErrors.ToString());

					xmlDocumentExpected = new XmlDocument();
					xmlDocumentExpected.LoadXml(Helper.GetTurkeyEInvoiceGEIMessageXml(TurkeyEInvoiceAPICommandList.Codes.CancelReceivablesInvoice));
					xmlDocumentPayLoad = new XmlDocument();
					xmlDocumentPayLoad.LoadXml(eInvoice.Payload.ToUTF8FromBase64());
					var payloadXml = xmlDocumentExpected.ChildNodes[0].LastChild;
					var payload = payloadXml.InnerXml.ToUTF8FromBase64();
					AssertEquals("Payload UTF8", payload, eInvoice.Payload.ToUTF8FromBase64());
					AssertEquals("Payload Base64", payloadXml.InnerXml, eInvoice.Payload);
				}
			}
		}

		#region Implementation

		TurkeyEInvoiceTestHelper Helper => helper ?? (helper = new TurkeyEInvoiceTestHelper());
		TurkeyEInvoiceTestHelper helper;

		string TurkeyEInvoiceExportedXmlLocalCurrency => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceExportedXml.xml");
		string TurkeyEInvoiceExportedXmlLocalCurrency_GOV => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceExportedXml_GOV.xml");
		string TurkeyEInvoiceExportedXmlLocalCurrency_EAR => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceExportedXml_EAR.xml");

		protected override void SetUp()
		{
			base.SetUp();

			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			DataAccess = new BatchExportDataAccess(connection, transaction);
		}

		BatchExportDataAccess DataAccess;

		#endregion
	}
}
