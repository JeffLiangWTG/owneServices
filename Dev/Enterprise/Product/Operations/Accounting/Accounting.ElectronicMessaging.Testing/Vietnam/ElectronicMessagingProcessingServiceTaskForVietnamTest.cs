using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Testing.Vietnam;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForVietnam))]
	sealed class ElectronicMessagingProcessingServiceTaskForVietnamTest : GEIElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForVietnam>
	{
		[TestDate(2020, 08, 20)]
		public override void TestServiceTaskProcessesEInvoiceOnlyForThoseCompaniesWhereFunctionalityIsEnabled()
		{
			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "BR1", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch(CountryCode + "2", "BR2", CountryCode, true);
			Helper.UpdateCustomsCodeForCountry(company1.FirstActiveBranch.OrgProxy, Core.Constants.CountryCodes.VietNam, "VAT", "0100233488");
			Helper.UpdateCustomsCodeForCountry(company2.FirstActiveBranch.OrgProxy, Core.Constants.CountryCodes.VietNam, "VAT", "0100233488");
			Helper.UpdateCustomsCodeForCountry(TestObjectCreator.AALSHI, Core.Constants.CountryCodes.VietNam, "VAT", "1234567890");

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, "AA12345");
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, "AA12345");

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 1);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 1);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1 }, logger);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, Array.Empty<int>(), logger);
			}
		}

		[TestDate(2020, 08, 20)]
		public override void TestSuccessfulCreationOfEDIInterchange_OneCompany()
		{
			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "BRN", CountryCode, true);
			Helper.UpdateCustomsCodeForCountry(company.FirstActiveBranch.OrgProxy, Core.Constants.CountryCodes.VietNam, "VAT", "0100233488");
			Helper.UpdateCustomsCodeForCountry(TestObjectCreator.AALSHI, Core.Constants.CountryCodes.VietNam, "VAT", "0100233488");

			//Create first set of transactions and run the service task.
			Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, "AA12345");

			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 1);

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, new int[] { 1 }, logger);

			//Create another set of transactions and run the service task.
			Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, "AA12345");

			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 1);

			serviceTask = GetCountrySpecificServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, new int[] { 1, 1 }, logger);
		}

		[TestDate(2020, 08, 20)]
		public override void TestSuccessfulCreationOfEDIInterchange_MoreThanOneCompany()
		{
			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "BRN", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch(CountryCode + "2", "BR2", CountryCode, true);
			Helper.UpdateCustomsCodeForCountry(company1.FirstActiveBranch.OrgProxy, Core.Constants.CountryCodes.VietNam, "VAT", "0100233488");
			Helper.UpdateCustomsCodeForCountry(company2.FirstActiveBranch.OrgProxy, Core.Constants.CountryCodes.VietNam, "VAT", "0100233488");
			Helper.UpdateCustomsCodeForCountry(TestObjectCreator.AALSHI, Core.Constants.CountryCodes.VietNam, "VAT", "0100233488");

			//Create first set of transactions for 2 companies and run the service task.

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, "AA12345");
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, "AA12345");

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 1);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 1);

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1 }, logger);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, new int[] { 1 }, logger);

			//Create another set of transactions for 2 companies and run the service task again.
			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, "AA12345");
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS, "AA12345");

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 1);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 1);

			serviceTask = GetCountrySpecificServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1, 1 }, logger);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, new int[] { 1, 1 }, logger);
		}

		[TestDate(2020, 08, 20)]
		public override void TestSuccessfulCreationOfEDIInterchange()
		{
			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "BRN", CountryCode, true);
			Helper.UpdateCustomsCodeForCountry(company1.FirstActiveBranch.OrgProxy, Core.Constants.CountryCodes.VietNam, "VAT", "0100233488");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, company1);
				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", company1.LocalCurrency, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK);
				var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Queued);
				Factory.Save();
			}

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			CombineAssertions(logger.ToString(), () =>
			{
				var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company1.PK, Core.Constants.EInvoicingBatchState.Sent);
				AssertEDIInterchanges(new ZQuery(), 1, (interchangePK) => AssertEDIMessages(interchangePK, company1.FirstActiveBranch.PK, GlbDepartment.CurrentDepartment.PK, GetLinkedObjectIDs(batches)));
			});
		}

		[TestDate(2020, 4, 29)]
		public override void TestSuccessfulCreatedEDIInterchangeBodyText()
		{
			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "BR1", CountryCode, true);
			Helper.UpdateCustomsCodeForCountry(company1.FirstActiveBranch.OrgProxy, Core.Constants.CountryCodes.VietNam, "VAT", "0100233488");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var taxRate = TestObjectCreator.CreateTaxRateWithoutZZ("CAPVAT", "Rate", 10);
				taxRate.SetRate_ForTestOnly(10, 1);
				var job1 = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "charge1", TestObjectCreator.EUR, 10m, TestObjectCreator.Creditor1, TestObjectCreator.EUR, 10m, TestObjectCreator.Debtor1);
				charge1.JR_AT_SellGSTRate = TestObjectCreator.ServiceTax.PK;
				Factory.Save();

				var arInvoice = Factory.NewWithPrimaryKey<ARInvoice>(Guid.Parse("460CC593-1C19-4E3C-ABB8-1CB8D647D419"));
				arInvoice.AH_GC = GlbCompany.CurrentCompany.PK;
				arInvoice.AH_OH = TestObjectCreator.Debtor1.PK;
				arInvoice.AH_Desc = "TEST";
				arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				arInvoice.AH_TransactionType = TransactionTypes.Invoice;
				arInvoice.AH_TransactionNum = TestObjectCreator.GetRandomString(5);
				arInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
				arInvoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
				arInvoice.AH_InvoiceDate = ZDateTime.Today;
				arInvoice.AH_PostDate = ZDateTime.Today;
				arInvoice.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				arInvoice.AH_TransactionReference = "XI123456";
				TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 0.2m, 10m, 1m, 0m, 100m, 10m, 0m);
				arInvoice.Lines[0].GenericCharge = TestObjectCreator.CC1.PK;

				var line = arInvoice.Lines.Cast<ARInvoiceLine>().First();
				line.AL_AT = taxRate.PK;
				line.AL_LocalTaxAmount = 10m;
				line.AL_LocalExTaxAmount = 100m;
				line.AL_OverseasTotal = 22m;
				line.AL_OSTaxAmount = 2m;

				Factory.Save();

				AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 1);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var serviceTask = new ElectronicMessagingProcessingServiceTaskForVietnam();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				var interchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_GB, company1.FirstActiveBranch.PK));
				AssertEquals(1, interchanges.Length);

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchanges[0].PK));
				AssertEquals(1, messages.Length);

				var message = messages[0];
				var xmlSerializer = new XmlSerializer(typeof(GlobalElectronicInvoicing));

				using (var reader = new StringReader(message.EM_MessageText))
				{
					var eInvoice = xmlSerializer.Deserialize(reader) as GlobalElectronicInvoicing;

					AssertEquals("MessagingSystem", "Vietnam electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
					AssertEquals("MessageType", VietnamEInvoiceAPICommandList.Codes.SendReceivablesInvoice, eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
					AssertEquals("BatchNumber", "1", eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
					AssertEquals("branch", "BR1", eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode);
					AssertEquals("Company", "VN1", eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode);
					AssertEquals("IsProductionSystemSpecified", true, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystemSpecified);

					var expectPayload = VietnamEInvoiceTestHelper.GetEmbeddedResourceAsString("VietnamEInvoiceSRNPayload.json");

					var vietnamEInvoice = JsonConvert.DeserializeObject<VietnamEInvoice>(eInvoice.Payload.ToUTF8FromBase64());
					var actualPayload = JsonConvert.SerializeObject(vietnamEInvoice, Formatting.Indented);
					AssertEquals(expectPayload, actualPayload);
				}
			}
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			var configurations = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.Value;
			var configuration = configurations.AddNew();
			configuration.Country = Constants.CountryCodes.VietNam;
			configuration.SubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.CreditNote;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = ZString.Empty;

			configuration = configurations.AddNew();
			configuration.Country = Constants.CountryCodes.VietNam;
			configuration.SubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			configuration.DisbursementRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = ComplianceSubTypeCodesAndLists.CodesAndDescriptions.OriginalRuleCodes.OriginalTransactionOnly;
			configuration.OrganisationLocation = ZString.Empty;

			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations);

			AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "admin");
			AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345");
		}

		protected override ElectronicMessagingProcessingServiceTaskForVietnam GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForVietnam_ForTest();

		protected override string ExpectedServicePoint => "XHUB_VN_EINVOICING";

		protected override ZString CountryCode => Constants.CountryCodes.VietNam;

		sealed class ElectronicMessagingProcessingServiceTaskForVietnam_ForTest : ElectronicMessagingProcessingServiceTaskForVietnam
		{
			protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
			{
				return new MockEDIInterchangeCreatorForVietnamEInvoicingBatch(company, () => new MockAccEInvoiceBatchToGEIConverter());
			}
		}
	}
}
