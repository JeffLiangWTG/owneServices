using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Accounting.Utility.Testing.eInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing
{
	public abstract class GlobalElectronicMessagingProcessingServiceTaskTest<T> : GEIElectronicMessagingProcessingServiceTaskTest<T> where T : ElectronicMessagingProcessingServiceTask
	{
		protected override void AssertEDIInterchange(IXmlEDIInterchange interchange, ZString servicePointSuffix)
		{
			EInvoicingTestHelper.AssertGEIEInvoicingEDIInterchange(interchange, expectedMessageType: ExpectedMessageTypeForGenerateInvoiceRequest, expectedTo: ExpectedServicePoint + servicePointSuffix);
		}

		protected virtual DateTime TestDate => new DateTime(2010, 01, 01);

		protected virtual string ExpectedTransportType => EDIInterchangeTransportTypeList.Codes.eHub;
		protected virtual string ExpectedTransportTypeForFeatureControl => ExpectedTransportType;

		protected virtual string ExpectedMessageTypeForGenerateInvoiceRequest => "REQ";
		protected virtual string ExpectedMessageTypeForGenerateCancellationRequest => "CAN";
		protected virtual string ExpectedMessageTypeForFeatureControl => ExpectedMessageTypeForGenerateInvoiceRequest;
		protected virtual string ExpectedMessagingSystem => $"{CountryCode} Electronic invoicing system";

		protected override string ExpectedServicePoint => $"XHUB_{CountryCode}_EINVOICING";
		protected virtual string ExpectedServicePointForFeatureControl => ExpectedServicePoint;

		protected virtual void AddCountrySpecificData() { }
		protected virtual void AddCountrySpecificCustomsCodesForBranchOrgProxy(OrgHeader orgProxy) { }
		protected virtual void AddCountrySpecificCustomsCodesForDebtor(OrgHeader arOrg) { }
		protected virtual void AddCountrySpecificCredentialsForCompanyOrBranch(GlbBranch branchWithCompany) { }
		protected virtual void AddAdditionalInformationForCompany(GlbCompany glbCompany) { }

		protected virtual int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 1;

		protected abstract IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions { get; }

		protected virtual bool CountryUsesPendingPivotStatus => false;

		protected virtual void BeforeSaveOfARAPINVCRDADJTransactions(ARInvoice arInvoice, ARCreditNote arCreditNote, ARAdjustmentNote arAdjustmentNote, APInvoice apInvoice, APCreditNote apCreditNote, APAdjustmentNote apAdjustmentNote) { }

		protected virtual string GetGovernmentAllocatedNumberForCancellation => "DefaultGovernmentAllocatedNumber314159";

		// This should simulate any user action required to transaction all pivots from PEN to QUE status.
		// It includes setting pivots to QUE status.
		protected virtual void SimulateUserAction(BusinessObjectFactory factory, IEnumerable<AccEInvoicingTransactionPivot> pivots) { }

		protected virtual RefCurrency CurrencyOfTestTransaction => TestObjectCreator.EUR;

		// This is the Feature Control settings expected to be deployed in production.
		protected virtual EInvoicingFeatureSettingsBuilder FeatureControlSettings => null;

		// This should be the JSON we deploy to production.
		// Functional testing should be done using equivalent JSON.
		protected virtual string ExpectedFeatureControlSettingsAsJson => null;

		public void TestFeatureControlJson()
		{
			if (FeatureControlSettings == null)
			{
				AssertNullOrEmpty(ExpectedFeatureControlSettingsAsJson);
				return;
			}

			var json = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(FeatureControlSettings.BuildJson()), Formatting.Indented);
			AssertMultilineASCIIEquals("The Feature Control Parameter we intend to deploy to production should be saved as a unit test, so that we keep our sanity when troubleshooting.", json, ExpectedFeatureControlSettingsAsJson);
		}

		[TestDate(2010, 1, 1)]
		public override void TestSuccessfulCreationOfEDIInterchange()
		{
			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			AssertSuccessfulCreationOfEDIInterchange(company, ZString.Empty);
		}

		[TestDate(2010, 1, 1)]
		public virtual void TestSuccessfulCreationOfEDIInterchange_ServicePointWithSuffix()
		{
			var suffix = "SUFFIX";
			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			AccountingElectronicMessagingRegistry.Instance.eInvoicingServicePointSuffix.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, suffix);

			AssertSuccessfulCreationOfEDIInterchange(company, $"_{suffix}");
		}

		void AssertSuccessfulCreationOfEDIInterchange(GlbCompany company, ZString servicePointSuffix)
		{
			TestDateAttribute.Date = TestDate;
			AddCountrySpecificData();

			AddAdditionalInformationForCompany(company);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForDebtor(TestObjectCreator.AALSHI);
			AddCountrySpecificCredentialsForCompanyOrBranch(company.FirstActiveBranch);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var batch = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Ready, company);
				var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", company.LocalCurrency, 1m, 100m, 10m, 100m, 10m, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
				var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Queued);
				BeforeSaveOfARAPINVCRDADJTransactions(arInvoice, null, null, null, null, null);
				Factory.Save();
				PrepareTestDataBeforeRunServiceTask(arInvoice);
			}

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			CombineAssertions(logger.ToString(), () =>
			{
				var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK, EInvoicingBatchState.Sent);
				AssertEDIInterchanges(new ZQuery(), 1, (interchangePK) => AssertEDIMessages(interchangePK, company.FirstActiveBranch.PK, GlbDepartment.CurrentDepartment.PK, GetLinkedObjectIDs(batches)), servicePointSuffix);
			});
		}

		[TestDate(2010, 1, 1)]
		[SuspendCriticalValidation]
		public override void TestSuccessfulCreationOfEDIInterchange_OneCompany()
		{
			TestDateAttribute.Date = TestDate;
			AddCountrySpecificData();

			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			AddAdditionalInformationForCompany(company);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForDebtor(TestObjectCreator.AALSHI);
			AddCountrySpecificCredentialsForCompanyOrBranch(company.FirstActiveBranch);

			// Create a set of transactions and run the service task.
			Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			SimulateUserActionIfRequiredWithAssertions(company);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions, logger);

			// Create another set of transactions and run the service task.
			Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			SimulateUserActionIfRequiredWithAssertions(company);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);

			serviceTask = GetCountrySpecificServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);

			var expectedPivotsPerBatchesDoubled = ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions
													.Concat(ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, expectedPivotsPerBatchesDoubled, logger);
		}

		[TestDate(2010, 1, 1)]
		[SuspendCriticalValidation]
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
			AddCountrySpecificCredentialsForCompanyOrBranch(company1.FirstActiveBranch);
			AddCountrySpecificCredentialsForCompanyOrBranch(company2.FirstActiveBranch);

			// Create a set of transactions for both companies and run the service task.
			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);

			SimulateUserActionIfRequiredWithAssertions(company1);
			SimulateUserActionIfRequiredWithAssertions(company2);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);

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

			var expectedPivotsPerBatchesDoubled = ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions
													.Concat(ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, expectedPivotsPerBatchesDoubled, logger);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, expectedPivotsPerBatchesDoubled, logger);
		}

		[TestDate(2010, 1, 1)]
		[SuspendCriticalValidation]
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
			AddCountrySpecificCredentialsForCompanyOrBranch(company1.FirstActiveBranch);
			AddCountrySpecificCredentialsForCompanyOrBranch(company2.FirstActiveBranch);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);

			SimulateUserActionIfRequiredWithAssertions(company1);
			SimulateUserActionIfRequiredWithAssertions(company2);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions, logger);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, Array.Empty<int>(), logger);
			}
		}

		[TestDate(2010, 1, 1)]
		public override void TestSuccessfulCreatedEDIInterchangeBodyText()
		{
			TestDateAttribute.Date = TestDate;
			AddCountrySpecificData();

			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			AddAdditionalInformationForCompany(company);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForDebtor(TestObjectCreator.Debtor1);
			AddCountrySpecificCredentialsForCompanyOrBranch(company.FirstActiveBranch);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", CurrencyOfTestTransaction, 10m, TestObjectCreator.Creditor1, CurrencyOfTestTransaction, 10m, TestObjectCreator.Debtor1);
				charge.JR_AT_SellGSTRate = TestObjectCreator.ServiceTax.PK;
				Factory.Save();

				var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV123456", CurrencyOfTestTransaction, 1m, 10m, 0m, 10m, 0m, TestObjectCreator.Debtor1, TestObjectCreator.CC1.PK);
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
					AssertInterchangeAndMessageForObjectFactoryConfiguration(interchanges[0], message);

					var xmlSerializer = new XmlSerializer(typeof(GlobalElectronicInvoicing));
					using (var reader = new StringReader(message.EM_MessageText))
					{
						var eInvoice = (GlobalElectronicInvoicing)xmlSerializer.Deserialize(reader);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem), ExpectedMessagingSystem, eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
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

		protected virtual void AssertCountrySpecificGEIMessageContent(GlobalElectronicInvoicing geiMessage) { }

		[TestDate(2010, 1, 1)]
		public virtual void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
		{
			TestDateAttribute.Date = TestDate;
			AddCountrySpecificData();

			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
			AddAdditionalInformationForCompany(company);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForDebtor(TestObjectCreator.Debtor1);
			AddCountrySpecificCredentialsForCompanyOrBranch(company.FirstActiveBranch);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", CurrencyOfTestTransaction, 10m, TestObjectCreator.Creditor1, CurrencyOfTestTransaction, 10m, TestObjectCreator.Debtor1);
				charge.JR_AT_SellGSTRate = TestObjectCreator.ServiceTax.PK;
				Factory.Save();

				// Original transaction (already sent)
				var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV123456", CurrencyOfTestTransaction, 1m, 10m, 0m, 10m, 0m, TestObjectCreator.Debtor1, TestObjectCreator.CC1.PK);
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
				var reversalPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, actionType: EInvoicingPivotActionType.Cancel, status: EInvoicingPivotState.Queued);
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
					AssertInterchangeAndMessageForObjectFactoryConfiguration(interchanges[0], message);

					var xmlSerializer = new XmlSerializer(typeof(GlobalElectronicInvoicing));
					using (var reader = new StringReader(message.EM_MessageText))
					{
						var eInvoice = (GlobalElectronicInvoicing)xmlSerializer.Deserialize(reader);
						AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem), ExpectedMessagingSystem, eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
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

		protected virtual void AssertCountrySpecificGEIMessageContentForReversal(GlobalElectronicInvoicing geiMessage) { }

		protected virtual void PrepareTestDataBeforeRunServiceTask(InvoicingBase invoicingBase) { }

		[TestDate(2010, 1, 1)]
		public virtual void TestSuccessfulCreatedEDIInterchangeBodyText_WhenFeatureControlIsEnabled()
		{
			if (FeatureControlSettings == null)
			{
				Assert("Feature Control is not configured; test is not applicable. Override FeatureControlSettings to enable this test.", true);
				return;
			}

			TestDateAttribute.Date = TestDate;
			using (FeatureControlSettings.BuildAndRegisterFeatureControlMock())
			{
				AddCountrySpecificData();

				var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "B01", CountryCode, true);
				AddAdditionalInformationForCompany(company);
				AddCountrySpecificCustomsCodesForBranchOrgProxy(company.FirstActiveBranch.OrgProxy);
				AddCountrySpecificCustomsCodesForDebtor(TestObjectCreator.Debtor1);
				AddCountrySpecificCredentialsForCompanyOrBranch(company.FirstActiveBranch);

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
						AssertInterchangeAndMessageForFeatureControlConfiguration(interchanges[0], message);

						var xmlSerializer = new XmlSerializer(typeof(GlobalElectronicInvoicing));
						using (var reader = new StringReader(message.EM_MessageText))
						{
							var eInvoice = (GlobalElectronicInvoicing)xmlSerializer.Deserialize(reader);
							AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem), $"{CountryCode} Electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
							AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType), ExpectedMessageTypeForFeatureControl, eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
							AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber), "1", eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
							AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode), company.FirstActiveBranch.GB_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode);
							AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode), company.GC_Code, eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode);
							AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystemSpecified), true, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystemSpecified);

							AssertCountrySpecificGEIMessageContentForFeatureControl(eInvoice);
						}
					}
				}
			}
		}

		protected virtual void AssertInterchangeAndMessageForObjectFactoryConfiguration(IXmlEDIInterchange interchange, EDIMessage message)
		{
			AssertEquals(ExpectedServicePoint, interchange.EI_To);
			AssertEquals(ExpectedTransportType, interchange.EI_TransportType);
		}

		protected virtual void AssertInterchangeAndMessageForFeatureControlConfiguration(IXmlEDIInterchange interchange, EDIMessage message)
		{
			AssertEquals(ExpectedServicePointForFeatureControl, interchange.EI_To);
			AssertEquals(ExpectedTransportTypeForFeatureControl, interchange.EI_TransportType);
		}

		protected virtual void AssertCountrySpecificGEIMessageContentForFeatureControl(GlobalElectronicInvoicing geiMessage)
		{
			AssertCountrySpecificGEIMessageContent(geiMessage);
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
	}
}
