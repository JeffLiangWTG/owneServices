using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.ElectronicMessaging.China.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForChina))]
	public class ElectronicMessagingProcessingServiceTaskForChinaTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForChina>
	{
		protected override ZString CountryCode => CountryCodes.China;

		protected override ElectronicMessagingProcessingServiceTaskForChina GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForChina();

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => ChinaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override RefCurrency CurrencyOfTestTransaction => TestObjectCreator.CNY;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1 };

		protected override void AddCountrySpecificCustomsCodesForBranchOrgProxy(OrgHeader orgProxy)
		{
			var regNo = "123456789012345";
			var cusCode = orgProxy.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_RN_NKCodeCountry == CountryCode && x.OK_CodeType == OrgCusCode.CodeTypes.VATCode);
			if (cusCode == null)
			{
				cusCode = orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, regNo);
				cusCode.OK_RN_NKCodeCountry = CountryCode;
			}
			else
			{
				cusCode.OK_CustomsRegNo = regNo;
			}
		}

		protected override void AddCountrySpecificCustomsCodesForDebtor(OrgHeader arOrg)
		{
			arOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789012345678", CountryCodes.China);

			var address = TestObjectCreator.CreateAddress(arOrg, OrgAddressType.Receivables, true, "Address1", "Address2", "Test city", "Test state", "CN", "1", "Test phone", "Test email");
			address.Language = "ZH-CN";
			address.OA_CompanyNameOverride = "Test company name";

			var account = arOrg.CompanyData.ARAccountDetailsCollection.AddNew();
			account.A1_IsDefaultAccount = true;
			account.A1_PaymentMethod = AccARAccountDetails.ARBankAccPayment;
			account.A1_RN_NKCountryCode = CountryCodes.China;
			account.A1_RX_NKAccountCurrency = CurrencyCodes.China;
			account.A1_BankName = "Bank name";
			account.A1_BankAccount = "Bank account";

			TestObjectCreator.FRT.AC_LocalLanguageDescription = "Local desc";
		}

		protected override void BeforeSaveOfARAPINVCRDADJTransactions(ARInvoice arInvoice, ARCreditNote arCreditNote, ARAdjustmentNote arAdjustmentNote, APInvoice apInvoice, APCreditNote apCreditNote, APAdjustmentNote apAdjustmentNote)
		{
			if (arInvoice != null)
			{
				arInvoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;

				var reference = Factory.NewWithValidTestData<AccTransactionHeaderReference>();
				reference.AH1_AH = arInvoice.PK;
				reference.AH1_Type = AccTransactionHeaderReferenceTypes.CDS;
				reference.AH1_Reference = ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDD.Code;
			}
			if (apInvoice != null)
			{
				apInvoice.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.ETA;
			}
		}

		[TestDate(2024, 3, 19)]
		public override void TestServiceTaskProcessesEInvoiceOnlyForThoseCompaniesWhereFunctionalityIsEnabled()
		{
			var debtor = TestObjectCreator.AALSHI;
			var creditor = TestObjectCreator.ABIGAS;
			AddCountrySpecificCustomsCodesForDebtor(debtor);

			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "BJS", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch(CountryCode + "2", "SHA", CountryCode, true);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company1.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company2.FirstActiveBranch.OrgProxy);

			CreateBranchCredential(company1);
			CreateBranchCredential(company2);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, debtor, creditor, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, debtor, creditor, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);

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

		[TestDate(2024, 3, 19)]
		public override void TestSuccessfulCreatedEDIInterchangeBodyText()
		{
			var debtor = TestObjectCreator.AALSHI;
			var creditor = TestObjectCreator.ABIGAS;
			AddCountrySpecificCustomsCodesForDebtor(debtor);

			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "BJS", CountryCode, true);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company.FirstActiveBranch.OrgProxy);
			CreateBranchCredential(company);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, debtor, creditor, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
				AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);

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

		[TestDate(2024, 3, 19)]
		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
			=> Assert("Not applicable; China does not handle cancellations.", true);

		[TestDate(2024, 3, 19)]
		public override void TestSuccessfulCreationOfEDIInterchange()
		{
			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "BJS", CountryCode, true);
			AssertSuccessfulCreationOfEDIInterchange(company, ZString.Empty);
		}

		[TestDate(2024, 3, 19)]
		public override void TestSuccessfulCreationOfEDIInterchange_ServicePointWithSuffix()
		{
			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "BJS", CountryCode, true);
			var suffix = "SUFFIX";
			AccountingElectronicMessagingRegistry.Instance.eInvoicingServicePointSuffix.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, suffix);

			AssertSuccessfulCreationOfEDIInterchange(company, $"_{suffix}");
		}

		[TestDate(2024, 3, 19)]
		public override void TestSuccessfulCreationOfEDIInterchange_MoreThanOneCompany()
		{
			var debtor = TestObjectCreator.AALSHI;
			var creditor = TestObjectCreator.ABIGAS;
			AddCountrySpecificCustomsCodesForDebtor(debtor);

			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "BJS", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch(CountryCode + "2", "SHA", CountryCode, true);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company1.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company2.FirstActiveBranch.OrgProxy);

			CreateBranchCredential(company1);
			CreateBranchCredential(company2);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, debtor, creditor, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, debtor, creditor, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions, logger);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions, logger);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, debtor, creditor, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, debtor, creditor, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);

			serviceTask = GetCountrySpecificServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);

			var doubleExpectedBatches = ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions.Concat(ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, doubleExpectedBatches, logger);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, doubleExpectedBatches, logger);
		}

		[TestDate(2024, 3, 19)]
		public override void TestSuccessfulCreationOfEDIInterchange_OneCompany()
		{
			var debtor = TestObjectCreator.AALSHI;
			var creditor = TestObjectCreator.ABIGAS;
			AddCountrySpecificCustomsCodesForDebtor(debtor);

			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "BJS", CountryCode, true);
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company.FirstActiveBranch.OrgProxy);
			CreateBranchCredential(company);

			Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, debtor, creditor, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions, logger);

			Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, debtor, creditor, beforeSave: BeforeSaveOfARAPINVCRDADJTransactions, currency: CurrencyOfTestTransaction);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions);

			serviceTask = GetCountrySpecificServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);
			var doubleExpectedBatches = ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions.Concat(ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, doubleExpectedBatches, logger);
		}

		void CreateBranchCredential(GlbCompany company)
		{
			AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetValue(Guid.Empty, company.FirstActiveBranch.PK.ToGuid(), Guid.Empty, "0001");
		}

		void AssertSuccessfulCreationOfEDIInterchange(GlbCompany company, ZString servicePointSuffix)
		{
			var debtor = TestObjectCreator.AALSHI;
			AddCountrySpecificCustomsCodesForBranchOrgProxy(company.FirstActiveBranch.OrgProxy);
			AddCountrySpecificCustomsCodesForDebtor(debtor);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, company);
				var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", company.LocalCurrency, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, debtor, TestObjectCreator.CC1.PK);
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
