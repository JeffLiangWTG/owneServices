using System;
using System.IO;
using CargoWise.BrandManager;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.Netting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	sealed class NettingClearingJournalMessageActionTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2015, 04, 10)]
		public void TestNettingClearingJournalImportViaXMSServiceTask_SavingError()
		{
			testObjectCreator.CreateExchangeRate(testObjectCreator.EUR, "SEL", 1.8M);
			testObjectCreator.CreateExchangeRate(testObjectCreator.EUR, "BUY", 1.8M);

			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, "SEL", 0.70M);
			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, "BUY", 0.70M);

			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, org3.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader1.PK.ToGuid());

			var testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupPeriods();

			string org1eHubID = "HYEDUSDAT";
			org1.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, org1eHubID);

			string org2eHubID = "HYEDAUDAT";
			org2.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, org2eHubID);

			var transaction1 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "USD 1000", testObjectCreator.USD, 1m, 1000M, 0M, 1000M, 0M, org2, testObjectCreator.GLHeader1.PK, ZDateTime.Today.AddDays(-5), ZDateTime.Today, ZDateTime.Today, false);
			transaction1.AH_ChequeOrReference = "USD 1000";

			var transaction2 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "USD 400", testObjectCreator.USD, 1m, 400M, 0M, 400M, 0M, org2, testObjectCreator.GLHeader1.PK, ZDateTime.Today.AddDays(-5), ZDateTime.Today, ZDateTime.Today, false);

			var transaction3 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "EUR 875", testObjectCreator.EUR, 1m, 875M, 0M, 875M, 0M, org2, testObjectCreator.GLHeader1.PK, ZDateTime.Today.AddDays(-5), ZDateTime.Today, ZDateTime.Today, false);
			transaction3.AH_ChequeOrReference = "EUR 875";

			var transaction4 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "EUR 350", testObjectCreator.EUR, 1m, 350M, 0M, 350M, 0M, org2, testObjectCreator.GLHeader1.PK, ZDateTime.Today.AddDays(-5), ZDateTime.Today, ZDateTime.Today, false);

			Factory.Save();

			var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.Netting.Testing\HYEDUSDAT.csv");
			var org1CsvContent = reader.ReadToEnd();

			var notification = new NotificationBuffer();
			var converter = new PaymentReceiptRemittanceFileConverter(notification, Factory);
			var org1Xsd = converter.ConvertCSVContentToXsd(org1CsvContent);

			AssertEquals("Precondition: EDIMessage table should be empty.", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));

			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.XMS;
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageSubType = "NCL";
			message.EM_MessageText = WriteMessage(org1Xsd);
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;

			Factory.Save();
			AssertEquals("EDIMessage table should have 1 message.", 1, Factory.GetDatabaseCount(typeof(EDIMessage)));

			TxnHeaderProcessorBase.SimulateSavingFailure = true;

			var processor = new ServiceManager.Tasks.StandardXMLProcessor.StandardXMLMessageProcessor();
			processor.Process(notification);

			Assert(notification.HasErrors);

			var expectedNotificationBeforeCallStack = $@"Processing message batch. Number of messages: '1'
Processing message '{message.EM_MessageNum}'
Combining message text
Running import
Begin processing Transaction AR JNL AALSHI  : 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Error: Could not execute Message Action.

CargoWise.EntityFramework.ZCannotSaveException: Test saving error
";
			var expectedNotificationAfterCallStack = $@"Warning: Error(e.g. XML validation error) that prevent save happened. Message process failed.
Unable to process messages in batch. Each message will be reprocessed separately.
Processing message '{message.EM_MessageNum}'
Combining message text
Running import
Begin processing Transaction AR JNL AALSHI  : 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AP JNL TESNAMSYD1  : 
Successfully matched organization with code 'TESNAMSYD1', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: TESNAMSYD1, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AR JNL TESNAMSYD1  : 
Successfully matched organization with code 'TESNAMSYD1', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: TESNAMSYD1, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AP JNL TESNAMSYD1  : 
Successfully matched organization with code 'TESNAMSYD1', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: TESNAMSYD1, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AR JNL TESNAMSYD1  : 
Successfully matched organization with code 'TESNAMSYD1', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: TESNAMSYD1, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AP JNL AALSHI  : 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AR JNL AALSHI  : 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Link message to Job
Import finished
Warning: Notification group is not specified or invalid. Please check 'System->Registry->Notification->Netting Clearing Journal Import Notification Group'
Finished message processing.";

			AssertContains("NotificationsBeforeCallStack", expectedNotificationBeforeCallStack, notification.AsString);
			AssertContains("NotificationsAfterCallStack", expectedNotificationAfterCallStack, notification.AsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2015, 04, 10)]
		public void TestNettingClearingJournalImportViaXMSServiceTask()
		{
			testObjectCreator.CreateExchangeRate(testObjectCreator.EUR, "SEL", 1.8M);
			testObjectCreator.CreateExchangeRate(testObjectCreator.EUR, "BUY", 1.8M);

			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, "SEL", 0.70M);
			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, "BUY", 0.70M);

			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, org3.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader1.PK.ToGuid());

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupPeriods();

			string org1eHubID = "HYEDUSDAT";
			org1.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, org1eHubID);

			string org2eHubID = "HYEDAUDAT";
			org2.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, org2eHubID);

			var transaction1 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "USD 1000", testObjectCreator.USD, 1m, 1000M, 0M, 1000M, 0M, org2, testObjectCreator.GLHeader1.PK, ZDateTime.Today.AddDays(-5), ZDateTime.Today, ZDateTime.Today, false);
			transaction1.AH_ChequeOrReference = "USD 1000";

			var transaction2 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "USD 400", testObjectCreator.USD, 1m, 400M, 0M, 400M, 0M, org2, testObjectCreator.GLHeader1.PK, ZDateTime.Today.AddDays(-5), ZDateTime.Today, ZDateTime.Today, false);

			var transaction3 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "EUR 875", testObjectCreator.EUR, 1m, 875M, 0M, 875M, 0M, org2, testObjectCreator.GLHeader1.PK, ZDateTime.Today.AddDays(-5), ZDateTime.Today, ZDateTime.Today, false);
			transaction3.AH_ChequeOrReference = "EUR 875";

			var transaction4 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "EUR 350", testObjectCreator.EUR, 1m, 350M, 0M, 350M, 0M, org2, testObjectCreator.GLHeader1.PK, ZDateTime.Today.AddDays(-5), ZDateTime.Today, ZDateTime.Today, false);

			Factory.Save();

			var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.Netting.Testing\HYEDUSDAT.csv");
			var org1CsvContent = reader.ReadToEnd();

			var notification = new NotificationBuffer();
			var converter = new PaymentReceiptRemittanceFileConverter(notification, Factory);
			var org1Xsd = converter.ConvertCSVContentToXsd(org1CsvContent);

			AssertEquals("Precondition: EDIMessage table should be empty.", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));

			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.XMS;
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageSubType = "NCL";
			message.EM_MessageText = WriteMessage(org1Xsd);
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;

			Factory.Save();
			AssertEquals("EDIMessage table should have 1 message.", 1, Factory.GetDatabaseCount(typeof(EDIMessage)));

			var processor = new ServiceManager.Tasks.StandardXMLProcessor.StandardXMLMessageProcessor();
			processor.Process(notification);

			AssertEquals(false, notification.HasErrors);

			var expectedNotification = $@"Processing message batch. Number of messages: '1'
Processing message '{message.EM_MessageNum}'
Combining message text
Running import
Begin processing Transaction AR JNL AALSHI  : 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AP JNL TESNAMSYD1  : 
Successfully matched organization with code 'TESNAMSYD1', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: TESNAMSYD1, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AR JNL TESNAMSYD1  : 
Successfully matched organization with code 'TESNAMSYD1', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: TESNAMSYD1, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AP JNL TESNAMSYD1  : 
Successfully matched organization with code 'TESNAMSYD1', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: TESNAMSYD1, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AR JNL TESNAMSYD1  : 
Successfully matched organization with code 'TESNAMSYD1', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: TESNAMSYD1, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AP JNL AALSHI  : 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AR JNL AALSHI  : 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Link message to Job
Import finished
Warning: Notification group is not specified or invalid. Please check 'System->Registry->Notification->Netting Clearing Journal Import Notification Group'
Processing finished. Saving changes...
Finished message processing.
";
			AssertMultilineASCIIEquals("Notifications", expectedNotification, notification.AsString);
		}

		string WriteMessage(IValueObject xsd)
		{
			string result = string.Empty;
			using (var stringWriter = new StringWriter())
			{
				stringWriter.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<NettingClearingJournals xmlns=\"http://www.edi.com.au/EnterpriseService/\">");

				var xmlValueObjectSerializer = new XmlValueObjectSerializer(typeof(Xsd.FinancialInvoices));
				xmlValueObjectSerializer.Serialize(stringWriter, xsd);

				stringWriter.Write("</NettingClearingJournals>");

				result = stringWriter.ToString();

				result = result.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", String.Empty);
				result = result.Replace("xmlns=\"http://www.edi.com.au/EnterpriseService/\"", String.Empty);
				result = result.Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", String.Empty);
				result = result.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", String.Empty);

				return result;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new NettingObjectCreator(Factory);
			testObjectCreator.AALSHI.OH_IsDebtor = true;

			var nettingSystem = testObjectCreator.CreateNettingSystem("NS", "Netting System", GlbCompany.CurrentCompany);

			org1 = testObjectCreator.CreateOrgHeader("AAAAA", true, true, "AUSYD");
			testObjectCreator.CreateNettingOrganisation(nettingSystem, org1, "FUL");
			testObjectCreator.CreateNewCompany("CM1", orgProxy: org1);

			org2 = testObjectCreator.CreateOrgHeader("BBBBB", true, true, "AUSYD");
			testObjectCreator.CreateNettingOrganisation(nettingSystem, org2, "FUL");
			testObjectCreator.CreateNewCompany("CM2", orgProxy: org2);

			org3 = testObjectCreator.CreateOrgHeader("CCCCC", true, true, "AUSYD");
			testObjectCreator.CreateNettingOrganisation(nettingSystem, org3, "FUL");

			Factory.Save();
		}

		NettingObjectCreator testObjectCreator;
		OrgHeader org1, org2, org3;
	}
}
