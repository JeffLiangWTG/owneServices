using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.SaudiArabia.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForSaudiArabia))]
	public class ElectronicMessagingProcessingServiceTaskForSaudiArabiaTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForSaudiArabia>
	{
		protected override ZString CountryCode => CountryCodes.SaudiArabia;
		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		protected override string ExpectedMessageTypeForGenerateCancellationRequest => EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 3;
		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1, 1 };
		protected override ElectronicMessagingProcessingServiceTaskForSaudiArabia GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForSaudiArabia();
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes =>
			new TaskNudgeInformationForTest[]
			{
				new TaskNudgeInformationForTest(
					AccEInvoicingTransactionPivotSchema.Constants.TableName,
					null,
					AccEInvoicingTransactionPivotSchema.Constants.AIP_Status           + " IN ('" + EInvoicingPivotState.Queued + "', '" + EInvoicingPivotState.Succeed + "', '" + EInvoicingPivotState.Failed + "')" ,
					AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=SA"),
			};

		[TestDate(2022, 1, 1)]
		public override void TestServiceTaskProcessesEInvoiceOnlyForThoseCompaniesWhereFunctionalityIsEnabled()
		{
			TestDateAttribute.Date = TestDate;
			AddCountrySpecificData();

			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch(CountryCode + "2", "B02", CountryCode, true);
			AddAdditionalInformationForCompany(company1);
			AddAdditionalInformationForCompany(company2);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company1.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company2.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForDebtor(TestObjectCreator.AALSHI);

			CreateCertificateCredential(company1);
			CreateCertificateCredential(company2);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);

			SimulateUserActionIfRequiredWithAssertions(company1);
			SimulateUserActionIfRequiredWithAssertions(company2);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingElectronicMessagingRegistry.Instance.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, 3))
			{
				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions, logger);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, Array.Empty<int>(), logger);
			}
		}

		[TestDate(2022, 1, 1)]
		public override void TestSuccessfulCreatedEDIInterchangeBodyText()
		{
			TestDateAttribute.Date = TestDate;
			AddCountrySpecificData();

			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			AddAdditionalInformationForCompany(company);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForDebtor(TestObjectCreator.Debtor1);
			AddCountrySpecificCredentialsForCompanyOrBranch(company.FirstActiveBranch);

			CreateCertificateCredential(company);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", CurrencyOfTestTransaction, 10m, TestObjectCreator.Creditor1, CurrencyOfTestTransaction, 10m, TestObjectCreator.Debtor1);
				charge.JR_AT_SellGSTRate = TestObjectCreator.ServiceTax.PK;
				Factory.Save();

				var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV123456", CurrencyOfTestTransaction, 1m, 10m, 0M, 10m, 0m, TestObjectCreator.Debtor1, TestObjectCreator.CC1.PK);
				arInvoice.Lines[0].AL_AT = charge.JR_AT_SellGSTRate;
				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
				BeforeSaveOfARAPINVCRDADJTransactions(arInvoice, null, null, null, null, null);
				Factory.Save();

				AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 1);

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var serviceTask = GetCountrySpecificServiceTask();
					var logger = InitialiseAndRunTaskSchedule(serviceTask);

					var interchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_GB, company.FirstActiveBranch.PK));
					AssertEquals("One interchange should be created", 1, interchanges.Length);

					var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchanges[0].PK));
					AssertEquals("One message should be created", 1, messages.Length);
					var message = messages[0];

					var xmlSerializer = new XmlSerializer(typeof(GlobalElectronicInvoicing));
					using (var reader = new StringReader(message.EM_MessageText))
					{
						var eInvoice = (GlobalElectronicInvoicing)xmlSerializer.Deserialize(reader);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem), $"{CountryCode} Electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType), ExpectedMessageTypeForGenerateInvoiceRequest, eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber), "1", eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode), company.FirstActiveBranch.GB_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode), company.GC_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystemSpecified), true, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystemSpecified);

						AssertCountrySpecificGEIMessageContent(eInvoice);
					}
				}
			}
		}

		[TestDate(2022, 1, 1)]
		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
		{
			TestDateAttribute.Date = TestDate;
			AddCountrySpecificData();

			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			AddAdditionalInformationForCompany(company);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForDebtor(TestObjectCreator.Debtor1);
			AddCountrySpecificCredentialsForCompanyOrBranch(company.FirstActiveBranch);

			CreateCertificateCredential(company);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", CurrencyOfTestTransaction, 10m, TestObjectCreator.Creditor1, CurrencyOfTestTransaction, 10m, TestObjectCreator.Debtor1);
				charge.JR_AT_SellGSTRate = TestObjectCreator.ServiceTax.PK;
				Factory.Save();

				// Original transaction (already sent)
				var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV123456", CurrencyOfTestTransaction, 1m, 10m, 0M, 10m, 0m, TestObjectCreator.Debtor1, TestObjectCreator.CC1.PK);
				arInvoice.Lines[0].AL_AT = charge.JR_AT_SellGSTRate;
				var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: "SUC");
				pivot.AIP_LastSentTimeUtc = ZDateTime.Now;
				pivot.AIP_LastResponseReceivedUtc = ZDateTime.Now;
				var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, "SNT");
				batch.AIB_EHubAllocatedNumber = "329a0994-f5ac-48c1-8d49-4df6a3e167f7";
				batch.AIB_GovernmentAllocatedNumber = GetGovernmentAllocatedNumberForCancellation;
				batch.TransactionPivots.Add(pivot);
				BeforeSaveOfARAPINVCRDADJTransactions(arInvoice, null, null, null, null, null);
				Factory.Save();

				// Subsequent reversal (being sent)
				var arCreditNote = (ARCreditNote)Helper.ObjectCreator.ReverseTransaction(arInvoice, out _);
				var reversalPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, actionType: Core.Constants.EInvoicingPivotActionType.Cancel, status: Core.Constants.EInvoicingPivotState.Queued);
				BeforeSaveOfARAPINVCRDADJTransactions(null, arCreditNote, null, null, null, null);
				Factory.Save();

				AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 1);

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var serviceTask = GetCountrySpecificServiceTask();
					var logger = InitialiseAndRunTaskSchedule(serviceTask);

					var interchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_GB, company.FirstActiveBranch.PK));
					AssertEquals("One interchange should be created", 1, interchanges.Length);

					var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchanges[0].PK));
					AssertEquals("One message should be created", 1, messages.Length);
					var message = messages[0];

					var xmlSerializer = new XmlSerializer(typeof(GlobalElectronicInvoicing));
					using (var reader = new StringReader(message.EM_MessageText))
					{
						var eInvoice = (GlobalElectronicInvoicing)xmlSerializer.Deserialize(reader);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem), $"{CountryCode} Electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType), ExpectedMessageTypeForGenerateCancellationRequest, eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber), "1", eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode), company.FirstActiveBranch.GB_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode), company.GC_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystemSpecified), true, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystemSpecified);

						AssertCountrySpecificGEIMessageContentForReversal(eInvoice);
					}
				}
			}
		}

		[TestDate(2022, 1, 1)]
		public override void TestSuccessfulCreationOfEDIInterchange()
		{
			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			AssertSuccessfulCreationOfEDIInterchange(company, ZString.Empty);
		}

		[TestDate(2022, 1, 1)]
		public override void TestSuccessfulCreationOfEDIInterchange_OneCompany()
		{
			TestDateAttribute.Date = TestDate;
			AddCountrySpecificData();

			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			AddAdditionalInformationForCompany(company);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForDebtor(TestObjectCreator.AALSHI);
			CreateCertificateCredential(company);

			// Create a set of transactions and run the service task.
			Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			SimulateUserActionIfRequiredWithAssertions(company);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);

			using (AccountingElectronicMessagingRegistry.Instance.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, 3))
			{
				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions, logger);

				// Create another set of transactions and run the service task.
				Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
				SimulateUserActionIfRequiredWithAssertions(company);
				AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);

				serviceTask = GetCountrySpecificServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions, logger);
			}
		}

		[TestDate(2022, 1, 1)]
		public override void TestSuccessfulCreationOfEDIInterchange_MoreThanOneCompany()
		{
			TestDateAttribute.Date = TestDate;
			AddCountrySpecificData();

			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch(CountryCode + "2", "B02", CountryCode, true);
			AddAdditionalInformationForCompany(company1);
			AddAdditionalInformationForCompany(company2);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company1.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company2.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForDebtor(TestObjectCreator.AALSHI);
			CreateCertificateCredential(company1);
			CreateCertificateCredential(company2);

			// Create a set of transactions for both companies and run the service task.
			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);

			SimulateUserActionIfRequiredWithAssertions(company1);
			SimulateUserActionIfRequiredWithAssertions(company2);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);

			using (AccountingElectronicMessagingRegistry.Instance.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, 3))
			using (AccountingElectronicMessagingRegistry.Instance.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, 3))
			{
				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions, logger);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions, logger);

				// Create another set of transactions for both companies and run the service task.
				Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
				Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);

				SimulateUserActionIfRequiredWithAssertions(company1);
				SimulateUserActionIfRequiredWithAssertions(company2);

				AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);
				AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);

				serviceTask = GetCountrySpecificServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions, logger);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions, logger);
			}
		}

		[TestDate(2022, 1, 1)]
		public override void TestSuccessfulCreationOfEDIInterchange_ServicePointWithSuffix()
		{
			var suffix = "SUFFIX";
			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			AccountingElectronicMessagingRegistry.Instance.eInvoicingServicePointSuffix.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, suffix);

			AssertSuccessfulCreationOfEDIInterchange(company, $"_{suffix}");
		}

		[TestDate(2022, 1, 1)]
		public void TestAllQueuedInvoicesAreNotSentInOneGo()
		{
			TestDateAttribute.Date = TestDate;
			AddCountrySpecificData();

			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch(CountryCode + "2", "B02", CountryCode, true);
			AddAdditionalInformationForCompany(company1);
			AddAdditionalInformationForCompany(company2);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company1.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company2.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForDebtor(TestObjectCreator.AALSHI);
			CreateCertificateCredential(company1);
			CreateCertificateCredential(company2);

			// Create a set of transactions for both companies and run the service task.
			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 15);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 15);

			using (AccountingElectronicMessagingRegistry.Instance.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, 3))
			using (AccountingElectronicMessagingRegistry.Instance.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, 3))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertPivotCount(company1.PK, EInvoicingPivotState.Sent, 3);
				AssertPivotCount(company2.PK, EInvoicingPivotState.Sent, 3);

				serviceTask = GetCountrySpecificServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertPivotCount(company1.PK, EInvoicingPivotState.Sent, 3); //No new invoice is sent, as the previous batches are still in SNT status
				AssertPivotCount(company2.PK, EInvoicingPivotState.Sent, 3); //No new invoice is sent, as the previous batches are still in SNT status

				TestConnection.ExecuteNonQuery("UPDATE [AccEInvoicingTransactionPivot] SET AIP_STATUS = 'SUC', AIP_SystemLastEditTimeUtc = GETUTCDATE(), AIP_SystemLastEditUser = 'TST' WHERE AIP_STATUS = 'SNT'");
				AssertPivotCount(company1.PK, EInvoicingPivotState.Succeed, 3);
				AssertPivotCount(company2.PK, EInvoicingPivotState.Succeed, 3);

				serviceTask = GetCountrySpecificServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertPivotCount(company1.PK, EInvoicingPivotState.Sent, 3);
				AssertPivotCount(company2.PK, EInvoicingPivotState.Sent, 3);

				TestConnection.ExecuteNonQuery("UPDATE [AccEInvoicingTransactionPivot] SET AIP_STATUS = 'SUC', AIP_SystemLastEditTimeUtc = GETUTCDATE(), AIP_SystemLastEditUser = 'TST' WHERE AIP_STATUS = 'SNT'");
				AssertPivotCount(company1.PK, EInvoicingPivotState.Succeed, 6);
				AssertPivotCount(company2.PK, EInvoicingPivotState.Succeed, 6);

				serviceTask = GetCountrySpecificServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertPivotCount(company1.PK, EInvoicingPivotState.Sent, 3);
				AssertPivotCount(company2.PK, EInvoicingPivotState.Sent, 3);

				TestConnection.ExecuteNonQuery("UPDATE [AccEInvoicingTransactionPivot] SET AIP_STATUS = 'SUC', AIP_SystemLastEditTimeUtc = GETUTCDATE(), AIP_SystemLastEditUser = 'TST' WHERE AIP_STATUS = 'SNT'");
				AssertPivotCount(company1.PK, EInvoicingPivotState.Succeed, 9);
				AssertPivotCount(company2.PK, EInvoicingPivotState.Succeed, 9);

				AssertPivotCount(company1.PK, EInvoicingPivotState.Queued, 6);
				AssertPivotCount(company2.PK, EInvoicingPivotState.Queued, 6);
			}

			void AssertPivotCount(ZGuid companyPK, ZString pivotStatus, int expectedCount)
			{
				var pivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(companyPK, pivotStatus);
				AssertEquals(expectedCount, pivots.Length);
			}
		}

		public override void TestDataProviderType()
		{
			var serviceTask = GetCountrySpecificServiceTask();
			AssertType<ElectronicMessagingProcessingServiceTaskDataProviderForSaudiArabia>(serviceTask.DataProvider);
		}

		void CreateCertificateCredential(GlbCompany company)
		{
			var credentialCreator = new TestEInvoicingCertificateCredentialCreator(company.FirstActiveBranch, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(7));
			var certificateCredential = credentialCreator.CreateCertificateCredential();
			certificateCredential.GP_PasswordType = PasswordTypesList.Codes.EIM;
		}

		void SimulateUserActionIfRequiredWithAssertions(GlbCompany company)
		{
			if (CountryUsesPendingPivotStatus)
			{
				AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 0);
				var pendingPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, EInvoicingPivotState.Pending);
				Assert("At least one PEN Pivot should be created for company " + company.GC_Code, pendingPivots.Length > 0);
				SimulateUserAction(pendingPivots[0].Factory, pendingPivots);
			}
		}

		void AssertSuccessfulCreationOfEDIInterchange(GlbCompany company, ZString servicePointSuffix)
		{
			TestDateAttribute.Date = TestDate;
			AddCountrySpecificData();

			AddAdditionalInformationForCompany(company);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForDebtor(TestObjectCreator.AALSHI);
			CreateCertificateCredential(company);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, company);
				var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", company.LocalCurrency, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
				var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Queued);
				BeforeSaveOfARAPINVCRDADJTransactions(arInvoice, null, null, null, null, null);
				Factory.Save();
			}

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			CombineAssertions(logger.ToString(), () =>
			{
				var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK, Core.Constants.EInvoicingBatchState.Sent);
				AssertEDIInterchanges(new ZQuery(), 1, (interchangePK) => AssertEDIMessages(interchangePK, company.FirstActiveBranch.PK, GlbDepartment.CurrentDepartment.PK, GetLinkedObjectIDs(batches)), servicePointSuffix);
			});
		}
	}
}
