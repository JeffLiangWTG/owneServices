using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing
{
	class ElectronicMessagingGlobalEInvoicingTest_AR : TransactionBatchEventMessageProcessorTestBase
	{
		#region TestMessage_WithAllElements_AndTwentyTransactions

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithAllElements_AndTwentyTransactions()
		{
			var eventTransactions = Enumerable.Range(0, 20)
				.Select(i =>
				{
					var debtors = new[]
					{
						ObjectCreator.AALSHI,
						ObjectCreator.ABIGAS,
						ObjectCreator.LocalClient,
						ObjectCreator.ZECTRA,
						ObjectCreator.XLINDU,
					};
					var debtor = debtors[i % debtors.Length];
					var (invoice, _) = CreateInvoiceAndPivot(transactionNumber: $"{i + 1000:00000000}", organisation: debtor);
					return CreateEventTransactionObject(invoice,
						govId: "9b7952e0ce3f2b042892d445a50d7edc",
						complianceNumber: "BBS17829",
						counter: "62/66NS",
						number: "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
						dateTime: "2021-02-18T15:46:05.0000000+00:00",
						idType: "GVT",
						authorisationData: "WFVFLkdlbmVyaWMuQ29tcG9uZW50",
						idNumber: "APXPU3617F",
						iTransactionHash: "SVRYTjM5NDgxMjQ5WE0=",
						publicKey: "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnVZVzFsSWpvaVZtbHJZWE1pZlEuZjJTWEUtSzBsV0U5SDVtSUpJdk5BUmMtT2NiRk1kMnRiUkxsdGxVMDN1TQ==",
						verificationUrl: "https://xyz.com/abc");
				})
				.ToList();

			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions, governmentAllocatedNumber: "gvt", eHubAllocatedNumber: "ehub");
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			var batchInNewFactory = newFactory.Load<AccEInvoicingBatch>(Batch.PK);

			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, batchInNewFactory);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			var expectedHitCounts = new Dictionary<string, int>
			{
				{ AccEInvoicingBatchSchema.Constants.TableName, 1 },
				{ AccTransactionHeaderSchema.Constants.TableName, 1 },
				{ AccEInvoicingTransactionPivotSchema.Constants.TableName, 1 },
				{ AccTransactionHeaderAuthorisationRecordSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
			};
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertDbHits(expectedHitCounts, newFactory);
		}

		#endregion TestMessage_WithAllElements_AndTwentyTransactions

		protected override InvoicingBase CreateInvoice(string transactionNumber = "00001000", OrgHeader organisation = null)
		{
			return ObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), transactionNumber, ObjectCreator.EUR, 1m, 100m, 0, 100m, 0m, organisation, ObjectCreator.CC1.PK);
		}

		protected override string InvoiceTypeName { get => "AR"; }
	}

	class ElectronicMessagingGlobalEInvoicingTest_AP : TransactionBatchEventMessageProcessorTestBase
	{
		protected override InvoicingBase CreateInvoice(string transactionNumber = "00001000", OrgHeader organisation = null)
		{
			return ObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), transactionNumber, ObjectCreator.EUR, 1m, 100m, 0, 100m, 0m);
		}

		protected override string InvoiceTypeName { get => "AP"; }
	}

	abstract class TransactionBatchEventMessageProcessorTestBase : TestCaseWithFactory
	{
		#region Batch Level IRJ Tests

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIRJMessage_WithBatchStatusDiscarded()
		{
			Batch.AIB_Status = EInvoicingBatchState.Discarded;
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IRJ", reasonMessage: "A big failure. But batch got discarded first.");

			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch();
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIRJMessage_WithBatchStatusSent()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IRJ", reasonMessage: "A big failure.");

			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 1, expectedInfoMessage: emailSendingUnsuccessfulBatch);
			AssertBatch();
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Failed, expectedErrorDescription: "A big failure.");
			AssertTransaction(invoice);
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIRJMessage_WithBatchStatusSent_AndTransactionData()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(
					eventType: "IRJ",
					reasonMessage: "A big failure.",
					governmentAllocatedNumber: "21978",
					eHubAllocatedNumber: "72cf24e4-366e-11eb-adc1-0242ac120002");

			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 1, expectedInfoMessage: emailSendingUnsuccessfulBatch);
			AssertBatch(expectedGovtNumber: "21978", expectedEHubNumber: "72cf24e4-366e-11eb-adc1-0242ac120002");
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Failed, expectedErrorDescription: "A big failure.");
			AssertTransaction(invoice);
		}

		#endregion

		#region Batch Level IAK Tests

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithBatchStatusDiscarded_AndNoAdditionalData()
		{
			Batch.AIB_Status = EInvoicingBatchState.Discarded;
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK");

			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch();
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithBatchStatusSent_AndNoAdditionalData()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] { CreateEventTransactionObject(invoice) };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice);
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithBatchStatusSent_AndBatchFieldsOnly()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] { CreateEventTransactionObject(invoice) };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(
					eventType: "IAK",
					governmentAllocatedNumber: "21978",
					eHubAllocatedNumber: "72cf24e4-366e-11eb-adc1-0242ac120002",
					transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch(expectedGovtNumber: "21978", expectedEHubNumber: "72cf24e4-366e-11eb-adc1-0242ac120002");
			AssertPivot(pivot);
			AssertTransaction(invoice);
		}

		public void TestIAKMessage_WithBatchStatusSent_AndBatchFieldsOnly_WithLegacyGovernmentNumber()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] { CreateEventTransactionObject(invoice) };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(
					eventType: "IAK",
					legacyGovernmentAllocatedNumber: "21978-old",
					eHubAllocatedNumber: "72cf24e4-366e-11eb-adc1-0242ac120002",
					transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch(expectedGovtNumber: "21978-old", expectedEHubNumber: "72cf24e4-366e-11eb-adc1-0242ac120002");
			AssertPivot(pivot);
			AssertTransaction(invoice);
		}

		public void TestIAKMessage_WithBatchStatusSent_AndBatchFieldsOnly_WithNewBatchGovernmentNumber_AndOldGovernmentNumber()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] { CreateEventTransactionObject(invoice) };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(
					eventType: "IAK",
					governmentAllocatedNumber: "new-32089",
					legacyGovernmentAllocatedNumber: "21978-old",
					eHubAllocatedNumber: "72cf24e4-366e-11eb-adc1-0242ac120002",
					transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch(expectedGovtNumber: "new-32089", expectedEHubNumber: "72cf24e4-366e-11eb-adc1-0242ac120002");
			AssertPivot(pivot);
			AssertTransaction(invoice);
		}

		#endregion

		#region TestMessage_GovernmentAllocatedId

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_GovernmentAllocatedId()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, govId: "ff79-9aa30403209846741") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice, expectedGovernmentId: "ff79-9aa30403209846741");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIRJMessage_GovernmentAllocatedId()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, govId: "ff79-9aa30403209846741") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IRJ", reasonMessage: "Batch level failure", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 1, expectedInfoMessage: emailSendingUnsuccessfulBatch);
			AssertBatch();
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Failed, expectedErrorDescription: "Batch level failure");
			AssertTransaction(invoice, expectedGovernmentId: "");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_GovernmentAllocatedId_SameAsExistingValue()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot(govId: "e274-fe345c17ef5a59657");
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, govId: "e274-fe345c17ef5a59657") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice, expectedGovernmentId: "e274-fe345c17ef5a59657");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_GovernmentAllocatedId_DifferentThanExistingValue()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot(govId: "e274-fe345c17ef5a59657");
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, govId: "ff79-9aa30403209846741") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(
				expectedLogCount: 1,
				expectedValidationWarnings: $"Government Allocated ID can't be overridden. Existing value: 'e274-fe345c17ef5a59657' & New value: 'ff79-9aa30403209846741' for invoice batch {Batch.AIB_BatchNumber}, transaction {InvoiceTitle000}, in {Batch.Company.CompanyName}.");
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice, expectedGovernmentId: "e274-fe345c17ef5a59657");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_GovernmentAllocatedId_ExistingValueAndEmpty()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot(govId: "e274-fe345c17ef5a59657");
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, govId: "") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice, expectedGovernmentId: "e274-fe345c17ef5a59657");
		}

		#endregion TestMessage_TransactionReference

		#region TestMessage_TransactionReference

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_ComplianceNumber()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, complianceNumber: "BJK-463872") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice, expectedComplianceNumber: "BJK-463872");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIRJMessage_ComplianceNumber()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, complianceNumber: "BJK-463872") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IRJ", reasonMessage: "Batch level failure", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 1, expectedInfoMessage: emailSendingUnsuccessfulBatch);
			AssertBatch();
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Failed, expectedErrorDescription: "Batch level failure");
			AssertTransaction(invoice, expectedComplianceNumber: "");		// Note: multi-transaction does NOT save a compliance number for IRJ. Although this is implementation specific; there is no technical or business reason why it could not.
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_ComplianceNumber_SameAsExistingValue()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot(complianceNumber: "BJK-463872");
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, complianceNumber: "BJK-463872") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice, expectedComplianceNumber: "BJK-463872");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_ComplianceNumber_DifferentThanExistingValue()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot(complianceNumber: "KJS-09123");
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, complianceNumber: "BJK-463872") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(
				expectedLogCount: 1,
				expectedValidationWarnings: $"Compliance Number can't be overridden. Existing value: 'KJS-09123' & New value: 'BJK-463872' for invoice batch {Batch.AIB_BatchNumber}, transaction {InvoiceTitle000}, in {Batch.Company.CompanyName}.");
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice, expectedComplianceNumber: "KJS-09123");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_ComplianceNumber_ExistingValueAndEmpty()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot(complianceNumber: "KJS-09123");
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, complianceNumber: "") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice, expectedComplianceNumber: "KJS-09123");
		}

		#endregion TestMessage_TransactionReference

		#region TestIAKMessage_AuthorisationRecord

		public void TestIAKMessage_AuthorisationRecord_IfAllNullExpectNotExists()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] {
				CreateEventTransactionObject(invoice,
					counter: null,
					number: null,
					dateTime: null,
					idType: null,
					idNumber: null)
			};
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice,
				expectAuthRecordExists: false);
		}

		public void TestIAKMessage_AuthorisationRecord_IfNullCounterNumberDateTimeIdTypeFields()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] {
				CreateEventTransactionObject(invoice,
					counter: null,
					number: null,
					dateTime: null,
					idType: null,
					idNumber: null,
					verificationUrl: "https://xyz.com/abc")
			};
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice,
				expectAuthRecordExists: true,
				expectedCounter: null,
				expectedNumber: null,
				expectedDateTime: null,
				expectedIdType: null,
				expectedVerificationUrl: "https://xyz.com/abc");
		}

		public void TestIAKMessage_AuthorisationRecord_IfInvalidDateFormatSupplied()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] {
				CreateEventTransactionObject(invoice,
					counter: "62/66NS",
					number: "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
					dateTime: "2021-13-18T15:46:05.0000000+00:00",
					idType: "GVT")
			};
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 1, expectedValidationWarnings: $"Failed to set EINV_DateTime as '2021-13-18T15:46:05.0000000+00:00' cannot be converted to date and time for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International.");
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice);
		}

		public void TestIAKMessage_AuthorisationRecord_IfInvalidBlobFormatSupplied()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] {
				CreateEventTransactionObject(invoice,
					counter: "62/66NS",
					number: "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
					dateTime: "2021-02-18T15:46:05.0000000+00:00",
					idType: "GVT",
					authorisationData: "not base64")
			};
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 1, expectedValidationWarnings: $"Failed to set EINV_AuthorisationData as it is not a valid Base64 string for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International.");
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice);
		}

		public void TestIAKMessage_AuthorisationRecord_IfRecordAlreadyExists_AndFieldsAreBlankOrPlaceholder()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] {
				CreateEventTransactionObject(invoice,
					counter: "62/66NS",
					number: "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
					dateTime: "2021-02-18T15:46:05.0000000+00:00",
					idType: "GVT",
					authorisationData: "WFVFLkdlbmVyaWMuQ29tcG9uZW50",
					idNumber: "APXPU3617F",
					iTransactionHash: "SVRYTjM5NDgxMjQ5WE0=",
					publicKey: "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnVZVzFsSWpvaVZtbHJZWE1pZlEuZjJTWEUtSzBsV0U5SDVtSUpJdk5BUmMtT2NiRk1kMnRiUkxsdGxVMDN1TQ==",
					verificationUrl: "https://xyz.com/abc")
			};
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice,
				expectAuthRecordExists: true,
				expectedCounter: "62/66NS",
				expectedNumber: "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				expectedDateTime: DateTimeOffset.Parse("2021-02-18T15:46:05.0000000+00:00", CultureInfo.InvariantCulture),
				expectedIdType: "GVT",
				expectedAuthorisationData: Convert.FromBase64String("WFVFLkdlbmVyaWMuQ29tcG9uZW50"),
				expectedIdNumber: "APXPU3617F",
				expectedITransactionHash: Convert.FromBase64String("SVRYTjM5NDgxMjQ5WE0="),
				expectedPublicKey: Convert.FromBase64String("ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnVZVzFsSWpvaVZtbHJZWE1pZlEuZjJTWEUtSzBsV0U5SDVtSUpJdk5BUmMtT2NiRk1kMnRiUkxsdGxVMDN1TQ=="),
				expectedVerificationUrl: "https://xyz.com/abc");
		}

		public void TestIAKMessage_AuthorisationRecord_IfRecordAlreadyExists_AndFieldsAreSet()
		{
			var (invoice, pivot) = CreateInvoiceAndPivotAndAuthRecord(
					counter: "11/66NS",
					number: "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
					dateTime: DateTimeOffset.Parse("2021-04-22T10:31:05.0000000+00:00", CultureInfo.InvariantCulture),
					idType: "GVT",
					authorisationData: Convert.FromBase64String("RGlmZmVyZW50LkF1dGhvcmlzYXRpb24uRGF0YQ=="),
					idNumber: "AOIVD8801",
					iTransactionHash: Convert.FromBase64String("S0pBU0hENDkzODI3NDk4MzJTQUtK"),
					publicKey: Convert.FromBase64String("ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnpkV0lpT2lJeE1qTTBOVFkzT0Rrd0lpd2libUZ0WlNJNklrcHZhRzRnUkc5bElpd2lhV0YwSWpveE5URTJNak01TURJeWZRLlNmbEt4d1JKU01lS0tGMlFUNGZ3cE1lSmYzNlBPazZ5SlZfYWRRc3N3NWM="),
					verificationUrl: "https://server.gov.au/verify/fnh0982i3jnjilojs09"
			);
			var eventTransactions = new[] {
				CreateEventTransactionObject(invoice,
					counter: "62/66NS",
					number: "1",
					dateTime: "2021-02-18T15:46:05.0000000+00:00",
					idType: "QQQ",
					authorisationData: "WFVFLkdlbmVyaWMuQ29tcG9uZW50",
					idNumber: "APXPU3617F",
					iTransactionHash: "SVRYTjM5NDgxMjQ5WE0=",
					publicKey: "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnVZVzFsSWpvaVZtbHJZWE1pZlEuZjJTWEUtSzBsV0U5SDVtSUpJdk5BUmMtT2NiRk1kMnRiUkxsdGxVMDN1TQ==",
					verificationUrl: "https://xyz.com/abc")
			};
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 1, expectedValidationWarnings: $"Authorization record field(s) EINV_AuthorisationData,EINV_Counter,EINV_DateTime,EINV_IDNumber,EINV_IDType,EINV_ITransactionHash,EINV_Number,EINV_PublicKey,EINV_VerificationURL have already been set for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International. You may only set AccTransactionHeaderAuthorisationRecord fields once.");
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice,
				expectAuthRecordExists: true,
				expectedCounter: "11/66NS",
				expectedNumber: "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				expectedDateTime: DateTimeOffset.Parse("2021-04-22T10:31:05.0000000+00:00", CultureInfo.InvariantCulture),
				expectedIdType: "GVT",
				expectedAuthorisationData: Convert.FromBase64String("RGlmZmVyZW50LkF1dGhvcmlzYXRpb24uRGF0YQ=="),
				expectedIdNumber: "AOIVD8801",
				expectedITransactionHash: Convert.FromBase64String("S0pBU0hENDkzODI3NDk4MzJTQUtK"),
				expectedPublicKey: Convert.FromBase64String("ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnpkV0lpT2lJeE1qTTBOVFkzT0Rrd0lpd2libUZ0WlNJNklrcHZhRzRnUkc5bElpd2lhV0YwSWpveE5URTJNak01TURJeWZRLlNmbEt4d1JKU01lS0tGMlFUNGZ3cE1lSmYzNlBPazZ5SlZfYWRRc3N3NWM="),
				expectedVerificationUrl: "https://server.gov.au/verify/fnh0982i3jnjilojs09");
		}

		public void TestIAKMessage_AuthorisationRecord_IfRecordAlreadyExists_IsIdempotent()
		{
			var (invoice, pivot) = CreateInvoiceAndPivotAndAuthRecord(
					counter: "11/66NS",
					number: "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
					dateTime: DateTimeOffset.Parse("2021-04-22T10:31:05.0000000+00:00", CultureInfo.InvariantCulture),
					idType: "GVT",
					authorisationData: Convert.FromBase64String("RGlmZmVyZW50LkF1dGhvcmlzYXRpb24uRGF0YQ=="),
					idNumber: "AOIVD8801",
					iTransactionHash: Convert.FromBase64String("S0pBU0hENDkzODI3NDk4MzJTQUtK"),
					publicKey: Convert.FromBase64String("ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnpkV0lpT2lJeE1qTTBOVFkzT0Rrd0lpd2libUZ0WlNJNklrcHZhRzRnUkc5bElpd2lhV0YwSWpveE5URTJNak01TURJeWZRLlNmbEt4d1JKU01lS0tGMlFUNGZ3cE1lSmYzNlBPazZ5SlZfYWRRc3N3NWM="),
					verificationUrl: "https://server.gov.au/verify/fnh0982i3jnjilojs09"
			);
			var eventTransactions = new[] {
				CreateEventTransactionObject(invoice,
					counter: "11/66NS",
					number: "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
					dateTime: "2021-04-22T10:31:05.0000000+00:00",
					idType: "GVT",
					authorisationData: "RGlmZmVyZW50LkF1dGhvcmlzYXRpb24uRGF0YQ==",
					idNumber: "AOIVD8801",
					iTransactionHash: "S0pBU0hENDkzODI3NDk4MzJTQUtK",
					publicKey: "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnpkV0lpT2lJeE1qTTBOVFkzT0Rrd0lpd2libUZ0WlNJNklrcHZhRzRnUkc5bElpd2lhV0YwSWpveE5URTJNak01TURJeWZRLlNmbEt4d1JKU01lS0tGMlFUNGZ3cE1lSmYzNlBPazZ5SlZfYWRRc3N3NWM=",
					verificationUrl: "https://server.gov.au/verify/fnh0982i3jnjilojs09")
			};
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch();
			AssertPivot(pivot);
			AssertTransaction(invoice,
				expectAuthRecordExists: true,
				expectedCounter: "11/66NS",
				expectedNumber: "f2SXE-K0lWE9H5mIJIvNARc-OcbFMd2tbRLltlU03uM",
				expectedDateTime: DateTimeOffset.Parse("2021-04-22T10:31:05.0000000+00:00", CultureInfo.InvariantCulture),
				expectedIdType: "GVT",
				expectedAuthorisationData: Convert.FromBase64String("RGlmZmVyZW50LkF1dGhvcmlzYXRpb24uRGF0YQ=="),
				expectedIdNumber: "AOIVD8801",
				expectedITransactionHash: Convert.FromBase64String("S0pBU0hENDkzODI3NDk4MzJTQUtK"),
				expectedPublicKey: Convert.FromBase64String("ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnpkV0lpT2lJeE1qTTBOVFkzT0Rrd0lpd2libUZ0WlNJNklrcHZhRzRnUkc5bElpd2lhV0YwSWpveE5URTJNak01TURJeWZRLlNmbEt4d1JKU01lS0tGMlFUNGZ3cE1lSmYzNlBPazZ5SlZfYWRRc3N3NWM="),
				expectedVerificationUrl: "https://server.gov.au/verify/fnh0982i3jnjilojs09");
		}

		#endregion TestIAKMessage_AuthorisationRecord

		#region TestMessage_PivotStatus

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithPriorPivotStatusDiscarded()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			pivot.AIP_Status = EInvoicingPivotState.Discarded;
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, complianceNumber: "BJK-463872") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", eHubAllocatedNumber: "abc", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 1, expectedValidationWarnings: $"No update performed due to transaction pivot having 'DCD' status for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International.");
			AssertBatch(expectedEHubNumber: "abc");
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Discarded, expectedResponseReceived: ZDateTime.Empty);
			AssertTransaction(invoice, expectedComplianceNumber: "");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithPriorPivotStatusSucceeded()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			pivot.AIP_Status = EInvoicingPivotState.Succeed;
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, complianceNumber: "BJK-463872") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", eHubAllocatedNumber: "abc", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 1, expectedValidationWarnings: $"No update performed due to transaction pivot having 'SUC' status for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International.");
			AssertBatch(expectedEHubNumber: "abc");
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Succeed, expectedResponseReceived: ZDateTime.Empty);
			AssertTransaction(invoice, expectedComplianceNumber: "");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithPriorPivotStatusDelivered()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			pivot.AIP_Status = EInvoicingPivotState.Delivered;
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, complianceNumber: "BJK-463872") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", eHubAllocatedNumber: "abc", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch(expectedEHubNumber: "abc");
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Succeed);
			AssertTransaction(invoice, expectedComplianceNumber: "BJK-463872");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithPriorPivotStatusFailure()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			pivot.AIP_Status = EInvoicingPivotState.Failed;
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, complianceNumber: "BJK-463872") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", eHubAllocatedNumber: "abc", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 1, expectedValidationWarnings: $"No update performed due to transaction pivot having 'FAL' status for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International.");
			AssertBatch(expectedEHubNumber: "abc");
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Failed, expectedResponseReceived: ZDateTime.Empty);
			AssertTransaction(invoice, expectedComplianceNumber: "");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithNewPivotStatusSucceeded()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, status: EInvoicingPivotState.Succeed, complianceNumber: "BJK-463872") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", eHubAllocatedNumber: "abc", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch(expectedEHubNumber: "abc");
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Succeed);
			AssertTransaction(invoice, expectedComplianceNumber: "BJK-463872");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithNewPivotStatusFailure()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, status: EInvoicingPivotState.Failed, error: "Transaction level error") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", eHubAllocatedNumber: "abc", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 1, expectedInfoMessage: emailSendingUnsuccessfulTransaction);
			AssertBatch(expectedEHubNumber: "abc");
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Failed, expectedErrorDescription: "Transaction level error");
			AssertTransaction(invoice);
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithNewPivotStatusFailure_AndComplianceNumber()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, status: EInvoicingPivotState.Failed, error: "Transaction level error", complianceNumber: "BJK-463872") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", eHubAllocatedNumber: "abc", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 2, expectedInfoMessage: emailSendingUnsuccessfulTransaction, expectedValidationWarnings: $"Compliance Number can't be updated as EINV_PivotStatus is status 'FAL' for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International.");
			AssertBatch(expectedEHubNumber: "abc");
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Failed, expectedErrorDescription: "Transaction level error");
			AssertTransaction(invoice, expectedComplianceNumber: "");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithNewPivotStatusDelivered()
			=> AssertIAKMessage_WithNewPivotStatusDelivered(EInvoicingPivotState.Delivered);

		protected void AssertIAKMessage_WithNewPivotStatusDelivered(string expectedStatus)
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, status: EInvoicingPivotState.Delivered) };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", eHubAllocatedNumber: "abc", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch(expectedEHubNumber: "abc");
			AssertPivot(pivot, expectedStatus: expectedStatus);
			AssertTransaction(invoice);
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithNewPivotStatusDiscarded()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, status: EInvoicingPivotState.Discarded) };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", eHubAllocatedNumber: "abc", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch(expectedEHubNumber: "abc");
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Discarded);
			AssertTransaction(invoice);
		}

		public void TestIAKMessage_WithMissingPivotStatus()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var eventTransactions = new[] { CreateEventTransactionObject(invoice, status: "") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(eventType: "IAK", eHubAllocatedNumber: "abc", transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 1, expectedValidationWarnings: $"Unable to update transaction as EINV_PivotStatus is missing for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International.");
			AssertBatch(expectedEHubNumber: "abc");
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Sent, expectedResponseReceived: ZDateTime.Empty);
			AssertTransaction(invoice);
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithExtraTransactionInEvent()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			Factory.Save();
			var (extraInvoice, extraPivot) = CreateInvoiceAndPivot(transactionNumber: "00001001");
			Factory.Save();
			Batch.TransactionPivots.Remove(extraPivot);
			Factory.Save();

			var eventTransactions = new[] { CreateEventTransactionObject(invoice, complianceNumber: "invoice"), CreateEventTransactionObject(extraInvoice, complianceNumber: "extraInvoice") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 1, expectedValidationWarnings: $"Universal event contained transaction not in invoice batch 1, transaction {InvoiceTitle001}, in Eagle Datamation International. No changes were made to this transaction.");
			AssertBatch();
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Succeed);
			AssertPivot(extraPivot, expectedStatus: EInvoicingPivotState.Sent, expectedResponseReceived: ZDateTime.Empty);
			AssertTransaction(invoice, expectedComplianceNumber: "invoice");
			AssertTransaction(extraInvoice, expectedComplianceNumber: "");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithMissingTransactionInEvent()
		{
			var (invoice, pivot) = CreateInvoiceAndPivot();
			var (extraInvoice, extraPivot) = CreateInvoiceAndPivot(transactionNumber: "00001001");

			var eventTransactions = new[] { CreateEventTransactionObject(invoice, complianceNumber: "invoice") };
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions(expectedLogCount: 1, expectedValidationWarnings: $"Universal event was missing transaction in invoice batch 1, transaction {InvoiceTitle001}, in Eagle Datamation International. No changes were made to this transaction.");
			AssertBatch();
			AssertPivot(pivot, expectedStatus: EInvoicingPivotState.Succeed);
			AssertPivot(extraPivot, expectedStatus: EInvoicingPivotState.Sent, expectedResponseReceived: ZDateTime.Empty);
			AssertTransaction(invoice, expectedComplianceNumber: "invoice");
			AssertTransaction(extraInvoice, expectedComplianceNumber: "");
		}

		#endregion TestMessage_PivotStatus

		#region TestMessage Multiple Transactions

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithThreeSuccessfulTransactions()
		{
			var (invoice1, pivot1) = CreateInvoiceAndPivot(transactionNumber: "00001000");
			var (invoice2, pivot2) = CreateInvoiceAndPivot(transactionNumber: "00001001");
			var (invoice3, pivot3) = CreateInvoiceAndPivot(transactionNumber: "00001002");

			var eventTransactions = new[]
			{
				CreateEventTransactionObject(invoice1, complianceNumber: "invoice1", govId: "OfficialIdNumberOne"),
				CreateEventTransactionObject(invoice2, complianceNumber: "invoice2", govId: "OfficialIdNumberTwo"),
				CreateEventTransactionObject(invoice3, complianceNumber: "invoice3", govId: "OfficialIdNumberThree"),
			};
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			CommonAssertions();
			AssertBatch();
			AssertPivot(pivot1, expectedStatus: EInvoicingPivotState.Succeed);
			AssertPivot(pivot2, expectedStatus: EInvoicingPivotState.Succeed);
			AssertPivot(pivot3, expectedStatus: EInvoicingPivotState.Succeed);
			AssertTransaction(invoice1, expectedComplianceNumber: "invoice1", expectedGovernmentId: "OfficialIdNumberOne");
			AssertTransaction(invoice2, expectedComplianceNumber: "invoice2", expectedGovernmentId: "OfficialIdNumberTwo");
			AssertTransaction(invoice3, expectedComplianceNumber: "invoice3", expectedGovernmentId: "OfficialIdNumberThree");
		}

		[TestDate(2021, 4, 20, 9, 45, 05)]
		public void TestIAKMessage_WithFourTransactions_WithSomeFailures()
		{
			var (invoice1, pivot1) = CreateInvoiceAndPivot(transactionNumber: "00001000");
			var (invoice2, pivot2) = CreateInvoiceAndPivot(transactionNumber: "00001001");
			var (invoice3, pivot3) = CreateInvoiceAndPivot(transactionNumber: "00001002");
			var (invoice4, pivot4) = CreateInvoiceAndPivot(transactionNumber: "00001003");

			var eventTransactions = new[]
			{
				CreateEventTransactionObject(invoice1, complianceNumber: "invoice1", govId: "One", status: EInvoicingPivotState.Failed, error: "Invoice 1 failed"),
				CreateEventTransactionObject(invoice2, complianceNumber: "invoice2", govId: "Two", status: EInvoicingPivotState.Delivered),
				CreateEventTransactionObject(invoice3, complianceNumber: "invoice3", govId: "Three"),
				CreateEventTransactionObject(invoice4, complianceNumber: "invoice4", govId: "Four", status: EInvoicingPivotState.Failed, error: "Invoice 4 failed"),
			};
			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent(transactions: eventTransactions);
			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, Batch);
			var processor = CountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider().GetEventMessageProcessor(eventMessageProcessorData);
			processor.Process();

			var expectedWarnings = @$"
Government Allocated ID can't be updated as EINV_PivotStatus is status 'FAL' for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International.
Compliance Number can't be updated as EINV_PivotStatus is status 'FAL' for invoice batch 1, transaction {InvoiceTitle000}, in Eagle Datamation International.
Government Allocated ID can't be updated as EINV_PivotStatus is status 'FAL' for invoice batch 1, transaction {InvoiceTitle003}, in Eagle Datamation International.
Compliance Number can't be updated as EINV_PivotStatus is status 'FAL' for invoice batch 1, transaction {InvoiceTitle003}, in Eagle Datamation International.
".Trim();
			CommonAssertions(expectedLogCount: 5, expectedInfoMessage: EmailSendingUnsuccessfulTransactions(2), expectedValidationWarnings: expectedWarnings);
			AssertBatch();
			AssertPivot(pivot1, expectedStatus: EInvoicingPivotState.Failed, expectedErrorDescription: "Invoice 1 failed");
			AssertPivot(pivot2, expectedStatus: EInvoicingPivotState.Delivered);
			AssertPivot(pivot3, expectedStatus: EInvoicingPivotState.Succeed);
			AssertPivot(pivot4, expectedStatus: EInvoicingPivotState.Failed, expectedErrorDescription: "Invoice 4 failed");
			AssertTransaction(invoice1, expectedComplianceNumber: "",         expectedGovernmentId: "");
			AssertTransaction(invoice2, expectedComplianceNumber: "invoice2", expectedGovernmentId: "Two");
			AssertTransaction(invoice3, expectedComplianceNumber: "invoice3", expectedGovernmentId: "Three");
			AssertTransaction(invoice4, expectedComplianceNumber: "",         expectedGovernmentId: "");
		}

		#endregion

		[ExpectNoExceptions]
		public void TestAfterEventMessageProcessed_IsCalled_WithCorrectParameters()
		{
			var mockXUEFunctionalityProvider = new Mock<IGlobalXUEFunctionalityProvider>();

			var mockCountryFactory = new Mock<ICountryEInvoicingObjectFactory>();
			mockCountryFactory.Setup(x => x.GetGlobalXUEFunctionalityProvider()).Returns(mockXUEFunctionalityProvider.Object);

			var (ediMessage, universalEvent) = GetEDIMessageAndUniversalEvent();
			var invoiceBatch = new TestObjectCreator(Factory).CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);

			var eventMessageProcessorData = new EventMessageProcessorData("ANY", "***", Logger, ediMessage, universalEvent, invoiceBatch);
			var processor = new TransactionBatchEventMessageProcessor(eventMessageProcessorData, mockCountryFactory.Object);

			processor.Process();

			mockXUEFunctionalityProvider.Verify(processor => processor.AfterEventMessageProcessed(universalEvent, invoiceBatch, Logger));
		}

		#region Helper Methods

		protected void CommonAssertions(
			int expectedLogCount = 0,
			string expectedValidationError = "",
			string expectedValidationWarnings = "",
			string expectedInfoMessage = "")
		{
			AssertEquals("Postcondition:ValidationError", expectedValidationError, ErrorReporter.LastMessageReported);

			var actualWarnings = string.Join("\n", Logger.Logs
				.Where(l => l.Type == Enterprise.Integration.LogType.Warning)
				.Select(l => l.Message)
			);
			AssertMultilineASCIIEquals("Postcondition:ValidationWarning", expectedValidationWarnings, actualWarnings);

			var actualInfo = Logger.Logs.FirstOrDefault(l => l.Type == Enterprise.Integration.LogType.Information)?.Message ?? string.Empty;
			AssertEquals("Postcondition:Information", expectedInfoMessage, actualInfo);

			AssertEquals("Postcondition:LogCount", expectedLogCount, Logger.Logs.Count());
			ErrorReporter.Clear();
		}

		protected void AssertPivot(AccEInvoicingTransactionPivot pivot,
			ZDateTime? expectedResponseReceived = null,
			string expectedStatus = EInvoicingPivotState.Succeed,
			string expectedErrorDescription = "")
		{
			var expectedTimestamp = expectedResponseReceived ?? EventTime;

			AssertEquals("AIP_LastResponseReceivedUtc", GetFormattedDateTime(expectedTimestamp), GetFormattedDateTime(pivot.AIP_LastResponseReceivedUtc));
			AssertEquals("AIP_Status", expectedStatus, pivot.AIP_Status);
			AssertEquals("AIP_ErrorDescription", expectedErrorDescription, pivot.AIP_ErrorDescription);
		}

		protected void AssertBatch(string expectedGovtNumber = "", string expectedEHubNumber = "")
		{
			AssertEquals("AIB_GovernmentAllocatedNumber", expectedGovtNumber, Batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("AIB_EHubAllocatedNumber", expectedEHubNumber, Batch.AIB_EHubAllocatedNumber);
		}

		protected void AssertTransaction(InvoicingBase transaction,
			string expectedGovernmentId = "",
			string expectedComplianceNumber = "",
			bool expectAuthRecordExists = false,
			string expectedNumber = null,
			string expectedCounter = null,
			string expectedIdType = null,
			string expectedIdNumber = null,
			ZDateTimeOffset? expectedDateTime = null,
			string expectedVerificationUrl = null,
			ZBlob? expectedPublicKey = null,
			ZBlob? expectedAuthorisationData = null,
			ZBlob? expectedITransactionHash = null)
		{
			AssertEquals("AH_GovernmentAllocatedID", expectedGovernmentId, transaction.AH_GovernmentAllocatedID);
			AssertEquals("AH_TransactionReference", expectedComplianceNumber, transaction.AH_TransactionReference);

			var authRecord = GetAuthorisationRecord(transaction);
			if (!expectAuthRecordExists)
			{
				AssertNull("AuthorizationRecord", authRecord);
			}
			else
			{
				AssertNotNull("AuthorizationRecord", authRecord);
				AssertEquals("AuthorizationRecord:AHF_Number", expectedNumber ?? ZString.Empty, authRecord.AHF_Number);
				AssertEquals("AuthorizationRecord:AHF_Counter", expectedCounter ?? ZString.Empty, authRecord.AHF_Counter);
				AssertEquals("AuthorizationRecord:AHF_IDType", expectedIdType ?? ZString.Empty, authRecord.AHF_IDType);
				AssertEquals("AuthorizationRecord:AHF_IDNumber", expectedIdNumber ?? ZString.Empty, authRecord.AHF_IDNumber);
				AssertEquals("AuthorizationRecord:AHF_DateTime", expectedDateTime ?? ZDateTimeOffset.Empty, authRecord.AHF_DateTime);
				AssertEquals("AuthorizationRecord:AHF_VerificationUrl", expectedVerificationUrl ?? ZString.Empty, authRecord.AHF_VerificationUrl);
				AssertEquals("AuthorizationRecord:AHF_PublicKey", expectedPublicKey ?? ZBlob.Empty, authRecord.AHF_PublicKey);
				AssertEquals("AuthorizationRecord:AHF_AuthorisationData", expectedAuthorisationData ?? ZBlob.Empty, authRecord.AHF_AuthorisationData);
				AssertEquals("AuthorizationRecord:AHF_ITransactionHash", expectedITransactionHash ?? ZBlob.Empty, authRecord.AHF_ITransactionHash);
			}
		}

		protected (EDIMessage ediMessage, UniversalEvent universalEvent) GetEDIMessageAndUniversalEvent(
			string eventType = "IAK",
			string reasonMessage = "",
			string governmentAllocatedNumber = "",
			string legacyGovernmentAllocatedNumber = "",
			string eHubAllocatedNumber = "",
			IReadOnlyCollection<UniversalEventTransactionDataObject> transactions = null)
		{
#pragma warning disable CS0618 // Type or member is obsolete: backwards compatibility of Legacy_GovernmentAllocatedNumber / EINV_GovtAllocatedRefNumber
			var universalEventText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>{Batch.AIB_BatchNumber}</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>{GetFormattedDateTime(EventTime)}</EventTime>
		<EventType>{eventType}</EventType>
		<EventParameters>
			<MessageType>IN</MessageType>
			<MessageSubType>ANY</MessageSubType>{GetReasonIfNeeded(reasonMessage)}
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>{GlbCompany.CurrentCompany.GC_Code}</Value>
			</Context>
{GetContextNodeIfNeeded(EventContextTypeCode.AIB_GovernmentAllocatedNumber_Explicit, governmentAllocatedNumber) +
GetContextNodeIfNeeded(EventContextTypeCode.Legacy_GovernmentAllocatedNumber, legacyGovernmentAllocatedNumber) +
GetContextNodeIfNeeded(EventContextTypeCode.AIB_EHubAllocatedNumber, eHubAllocatedNumber)}

{GetTransactionContextNodesIfNeeded()}
		</ContextCollection>
	</Event>
</UniversalEvent>";
#pragma warning restore CS0618 // Type or member is obsolete
			var (ediMessage, universalEvent) = GetEDIMessage(universalEventText);

			return (ediMessage, universalEvent);

			string GetContextNodeIfNeeded(string type, ZString value)
			{
				if (!value.IsEmpty)
				{
					return $@"
			<Context>
				<Type>{type}</Type>
				<Value>{value}</Value>
			</Context>";
				}
				return string.Empty;
			}

			string GetTransactionContextNodesIfNeeded()
			{
				var result = new StringBuilder();
				foreach (var t in (transactions ?? Enumerable.Empty<UniversalEventTransactionDataObject>()))
				{
					var json = Newtonsoft.Json.JsonConvert.SerializeObject(t);
					result.Append(GetContextNodeIfNeeded(EventContextTypeCode.BatchResponseObject, json));
				}
				return result.ToString();
			}

			string GetReasonIfNeeded(ZString value)
			{
				if (!value.IsEmpty)
				{
					return $@"
			<Reason>{value}</Reason>";
				}
				return string.Empty;
			}
		}

		static string GetFormattedDateTime(ZDateTime zDateTime)
		{
			return zDateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
		}

		protected static UniversalEventTransactionDataObject CreateEventTransactionObject(
			InvoicingBase transaction,
			string status = EInvoicingPivotState.Succeed,
			string error = "",
			string govId = null,
			string complianceNumber = "",
			string number = null,
			string counter = null,
			string idType = null,
			string idNumber = null,
			string dateTime = null,
			string verificationUrl = null,
			string publicKey = null,
			string authorisationData = null,
			string iTransactionHash = null)
		{
			return new UniversalEventTransactionDataObject()
			{
				TransactionLedger = transaction.AH_Ledger,
				TransactionType = transaction.AH_TransactionType,
				TransactionNumber = transaction.AH_TransactionNum,
				TransactionOrgHeaderCode = transaction.Header?.OH_Code ?? ZString.Empty,

				PivotStatus = status,
				FailureReason = error,

				ComplianceNumber = complianceNumber,

				GovernmentAllocatedId = govId,
				Number = number,
				Counter = counter,
				IDType = idType,
				IDNumber = idNumber,
				DateTime = dateTime,
				VerificationUrl = verificationUrl,
				PublicKey = publicKey,
				AuthorisationData = authorisationData,
				ITransactionHash = iTransactionHash,
			};
		}

		AccTransactionHeaderAuthorisationRecord GetAuthorisationRecord(InvoicingBase transaction)
		{
			if (transaction == null)
			{
				return null;
			}

			var filter = new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
			filter.AddToFilter(JoinCondition.And, AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, SQLComparisonOperator.Equal, transaction.PK);
			filter.AddToFilter(JoinCondition.And, AccTransactionHeaderAuthorisationRecordSchema.AHF_RecordType, SQLComparisonOperator.Equal, CountryEInvoicingObjectFactory.AuthorizationRecordType);
			return Batch.Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(filter);
		}

		(EDIMessage ediMessage, UniversalEvent universalEvent) GetEDIMessage(string universalEventText)
		{
			var ediMessage = Factory.New<EDIMessage>();
			ediMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			ediMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			ediMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_Status = EDIMessage.Status.Queued;
			ediMessage.EM_MessageText = universalEventText;
			var universalEvent = ediMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();

			return (ediMessage, universalEvent);
		}

		public (InvoicingBase invoice, AccEInvoicingTransactionPivot) CreateInvoiceAndPivot(
			string transactionNumber = "00001000",
			string govId = "",
			string complianceNumber = "",
		
			OrgHeader organisation = null)
		{
			organisation ??= ObjectCreator.AALSHI;
			var invoice = CreateInvoice(transactionNumber, organisation);
			invoice.AH_GovernmentAllocatedID = govId;
			invoice.AH_TransactionReference = complianceNumber;
			var pivot = ObjectCreator.CreateEInvoicingTransactionPivot(Batch, invoice, EInvoicingPivotState.Sent);

			return (invoice, pivot);
		}
		public (InvoicingBase invoice, AccEInvoicingTransactionPivot) CreateInvoiceAndPivotAndAuthRecord(
			string transactionNumber = "00001000",
			string govId = "",
			string complianceNumber = "",
			string number = null,
			string counter = null,
			string idType = null,
			string idNumber = null,
			ZDateTimeOffset? dateTime = null,
			string verificationUrl = null,
			ZBlob? publicKey = null,
			ZBlob? authorisationData = null,
			ZBlob? iTransactionHash = null,
			OrgHeader organisation = null)
		{
			var (invoice, pivot) = CreateInvoiceAndPivot(
				transactionNumber = "00001000",
				govId = "",
				complianceNumber = "",
				organisation = null);

			var authRecord = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			authRecord.AHF_RecordType = CountryEInvoicingObjectFactory.AuthorizationRecordType;
			authRecord.AHF_ParentId = invoice.PK;
			authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;

			authRecord.AHF_Number = number ?? ZString.Empty;
			authRecord.AHF_Counter = counter ?? ZString.Empty;
			authRecord.AHF_IDType = idType ?? ZString.Empty;
			authRecord.AHF_IDNumber = idNumber ?? ZString.Empty;
			authRecord.AHF_DateTime = dateTime ?? ZDateTimeOffset.Empty;
			authRecord.AHF_VerificationUrl = verificationUrl ?? ZString.Empty;
			authRecord.AHF_PublicKey = publicKey ?? ZBlob.Empty;
			authRecord.AHF_AuthorisationData = authorisationData ?? ZBlob.Empty;
			authRecord.AHF_ITransactionHash = iTransactionHash ?? ZBlob.Empty;
			
			return (invoice, pivot);
		}

		protected abstract InvoicingBase CreateInvoice(string transactionNumber, OrgHeader organisation);

		protected override void SetUp()
		{
			base.SetUp();

			Logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			ObjectCreator = new TestObjectCreator(Factory);
			CountryEInvoicingObjectFactory = new DummyCountryEInvoicingObjectFactory(CreateTransactionBatchEventMessageProcessor);
			Batch = ObjectCreator.CreateEInvoicingBatch(1, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			Factory.Save();
		}

		readonly string emailSendingUnsuccessfulBatch = $@"Error: Email Notification was not sent for Eagle Datamation International. Email (Subject: 'E-Reporting error notification for Electronic Invoicing Batch 1 [EDI]', For Group: {AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.HumanReadableRegistryPath()}) must have at least one recipient, CC or BCC
E-Reporting Email Notification task completed.
";
		string emailSendingUnsuccessfulTransaction => $@"Error: Email Notification was not sent for Eagle Datamation International. Email (Subject: 'E-Reporting error notification for Transaction {InvoiceTitle000} [EDI]', For Group: {AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.HumanReadableRegistryPath()}) must have at least one recipient, CC or BCC
E-Reporting Email Notification task completed.
";
		static string EmailSendingUnsuccessfulTransactions(int number)
			=> $@"Error: Email Notification was not sent for Eagle Datamation International. Email (Subject: 'E-Reporting error notification for {number} items [EDI]', For Group: {AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.HumanReadableRegistryPath()}) must have at least one recipient, CC or BCC
E-Reporting Email Notification task completed.
";

		protected ZDateTime EventTime = ZDateTime.UtcNow.AddSeconds(300);
		protected IXmlSessionTracker Logger;
		protected TestObjectCreator ObjectCreator;
		protected AccEInvoicingBatch Batch;
		protected ICountryEInvoicingObjectFactory CountryEInvoicingObjectFactory;

		protected string InvoiceTitle000 => $"{InvoiceTypeName} INV 00001000";
		protected string InvoiceTitle001 => $"{InvoiceTypeName} INV 00001001";
		protected string InvoiceTitle002 => $"{InvoiceTypeName} INV 00001002";
		protected string InvoiceTitle003 => $"{InvoiceTypeName} INV 00001003";
		protected abstract string InvoiceTypeName { get; }

		protected virtual TransactionBatchEventMessageProcessor CreateTransactionBatchEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData, ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			=> new TransactionBatchEventMessageProcessor(eventMessageProcessorData, countryEInvoicingObjectFactory);

		delegate TransactionBatchEventMessageProcessor CreateTransactionBatchEventMessageProcessorDelegate(EventMessageProcessorData eventMessageProcessorData, ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory);

		class DummyCountryEInvoicingObjectFactory : CountryEInvoicingObjectFactory
		{
			readonly CreateTransactionBatchEventMessageProcessorDelegate createTransactionBatchEventMessageProcessor;

			public DummyCountryEInvoicingObjectFactory(CreateTransactionBatchEventMessageProcessorDelegate createTransactionBatchEventMessageProcessor)
			{
				this.createTransactionBatchEventMessageProcessor = createTransactionBatchEventMessageProcessor;
			}

			protected override ZString CountryCode => Core.Constants.CountryCodes.India;

			protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
				=> new DummyUniversalEventFunctionalityProvider(this, createTransactionBatchEventMessageProcessor);
		}

		class DummyUniversalEventFunctionalityProvider : GlobalXUEFunctionalityProvider
		{
			readonly CreateTransactionBatchEventMessageProcessorDelegate createTransactionBatchEventMessageProcessor;

			public DummyUniversalEventFunctionalityProvider(ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory,
				CreateTransactionBatchEventMessageProcessorDelegate createTransactionBatchEventMessageProcessor)
				: base(countryEInvoicingObjectFactory)
			{
				this.createTransactionBatchEventMessageProcessor = createTransactionBatchEventMessageProcessor;
			}

			protected override bool IsBatchEventMessageProcessSupported(string eventType, string messageSubType)
				=> true;

			protected override EInvoicingEventMessageProcessor GetEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData)
				=> createTransactionBatchEventMessageProcessor(eventMessageProcessorData, base.CountryEInvoicingObjectFactory);
		}

		#endregion Helper Methods
	}
}
