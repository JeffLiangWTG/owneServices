using System;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Germany.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForGermany))]
	public class ElectronicMessagingProcessingServiceTaskForGermanyTest : GEIElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForGermany>
	{
		protected override ElectronicMessagingProcessingServiceTaskForGermany GetCountrySpecificServiceTask()
		{
			return new ElectronicMessagingProcessingServiceTaskForGermany_ForTest();
		}

		[TestDate(2020, 12, 1)]
		public override void TestServiceTaskProcessesEInvoiceOnlyForThoseCompaniesWhereFunctionalityIsEnabled()
		{
			var company1 = Helper.CreateCompanyAndBranch("DE1", "BR1", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch("DE2", "BR2", CountryCode, true);
			TestObjectCreator.AALSHI.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			TestObjectCreator.AALSHI.OH_Category = OrgConstants.Category.Government;

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 2);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 2);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1, 1 }, logger);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, Array.Empty<int>(), logger);
			}
		}

		[TestDate(2020, 12, 1)]
		public override void TestSuccessfulCreationOfEDIInterchange_OneCompany()
		{
			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "BRN", CountryCode, true);
			TestObjectCreator.AALSHI.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			TestObjectCreator.AALSHI.OH_Category = OrgConstants.Category.Government;

			//Create first set of transactions and run the service task.
			Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 2);

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, new int[] { 1, 1 }, logger);

			//Create another set of transactions and run the service task.
			Helper.CreateARAPINVCRDADJTransactions(company.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 2);

			serviceTask = GetCountrySpecificServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, new int[] { 1, 1, 1, 1 }, logger);
		}

		[TestDate(2020, 12, 1)]
		public override void TestSuccessfulCreationOfEDIInterchange_MoreThanOneCompany()
		{
			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "BRN", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch(CountryCode + "2", "BR2", CountryCode, true);
			TestObjectCreator.AALSHI.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			TestObjectCreator.AALSHI.OH_Category = OrgConstants.Category.Government;

			//Create first set of transactions for 2 comapnies and run the service task.

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 2);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 2);

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1, 1 }, logger);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, new int[] { 1, 1 }, logger);

			//Create another set of transactions for 2 comapnies and run the service task again.
			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, TestObjectCreator.AALSHI, TestObjectCreator.ABIGAS);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 2);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 2);

			serviceTask = GetCountrySpecificServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1, 1, 1, 1 }, logger);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, new int[] { 1, 1, 1, 1 }, logger);
		}

		[TestDate(2020, 12, 01, 0, 0, 0)]
		public override void TestSuccessfulCreatedEDIInterchangeBodyText()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var company1 = Helper.CreateCompanyAndBranch("DE1", "BR1", CountryCode, true);
			var branch = company1.FirstActiveBranch;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AccBankAccount bank1 = Factory.NewWithValidTestData<AccBankAccount>();
				bank1.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				bank1.AB_Code = "Bank1";
				bank1.AB_IsDefaultReceiptBankAccount = true;
				bank1.IBAN = "DE22123412341234123412";

				Helper.AddCustomsCodeForCountryIfMissing(objectCreator.DebtorDE, Core.Constants.CountryCodes.Germany, "LID", "abc");
				var job1 = objectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge1 = objectCreator.CreateCharge(job1, objectCreator.CC1, "charge1", objectCreator.EUR, 10m, objectCreator.Creditor1, objectCreator.EUR, 10m, objectCreator.DebtorDE);
				Factory.Save();

				var postResult = objectCreator.PostJobAsBillingTab(job1, JobInvoicingPostingOption.Revenue);
				Factory.Save();
				AssertEquals("Precondition: AR invoice must be posted from job", 1, postResult.ARTransactionsCount);
				AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 1);

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var serviceTask = new ElectronicMessagingProcessingServiceTaskForGermany();
					var logger = InitialiseAndRunTaskSchedule(serviceTask);

					var interchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_GB, company1.FirstActiveBranch.PK));
					AssertEquals(1, interchanges.Length);

					var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchanges[0].PK));
					AssertEquals(1, messages.Length);

					var message = messages[0];

					// Approximate XML expected:
					//	var expectedText = @"<GlobalElectronicInvoicing xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"">
					//  <Header>
					//    <ElectronicInvoiceBatchRequest>
					//      <MessagingSystem>Germany electronic invoicing system</MessagingSystem>
					//      <MessageType>REQ</MessageType>
					//      <BatchNumber>1</BatchNumber>
					//		<CompanyCode>DE1</CompanyCode>
					//		<BranchCode>BR1</BranchCode >
					//    </ElectronicInvoiceBatchRequest>
					//  </Header>
					//  <Payload><![CDATA[<MissingInXUT><BuyersReference>abc</BuyersReference><SellersIBANNumber>DE22123412341234123412</SellersIBANNumber></MissingInXUT>]]></Payload>
					//  <Transaction>Base64 encoded XUT</Transaction>
					//</GlobalElectronicInvoicing>";

					AssertContains("MessagingSystem", "<MessagingSystem>Germany electronic invoicing system</MessagingSystem>", message.EM_MessageText);
					AssertContains("MessageType", "<MessageType>REQ</MessageType>", message.EM_MessageText);
					AssertContains("BatchNumber", "<BatchNumber>1</BatchNumber>", message.EM_MessageText);
					AssertContains("BranchCode", "<CompanyCode>DE1</CompanyCode>", message.EM_MessageText);
					AssertContains("CompanyCode", "<CompanyCode>DE1</CompanyCode>", message.EM_MessageText);

					// Base64 blobs make for pointless test failures; other unit tests assert content of <Payload> and <Transaction>
					Assert("Payload with BuyersReference", message.EM_MessageText.Contains("<BuyersReference>abc</BuyersReference>", StringComparison.OrdinalIgnoreCase));
					Assert("Payload with Sellers IBAN", message.EM_MessageText.Contains("<SellersIBANNumber>DE22123412341234123412</SellersIBANNumber>", StringComparison.OrdinalIgnoreCase));
					var transactionRegex = new Regex("<Transaction>[a-zA-Z0-9=+\\/]{1,}<\\/Transaction>");
					Assert("Transaction", transactionRegex.IsMatch(message.EM_MessageText));

					var transactionXml =
						Encoding.UTF8.GetString(
							Convert.FromBase64String(
								transactionRegex.Match(message.EM_MessageText).Value.
									Replace("<Transaction>", string.Empty).
									Replace("</Transaction>", string.Empty)
							)
						);
					Assert("Does Not Contain <ShipmentCollection>", !transactionXml.Contains("<ShipmentCollection>"));
				}
			}
		}

		protected OrgHeader Organisation;

		protected override ZString CountryCode => CountryCodes.Germany;
		protected override string ExpectedServicePoint => "XHUB_DE_EINVOICING";

#region Inner Class

		public class ElectronicMessagingProcessingServiceTaskForGermany_ForTest : ElectronicMessagingProcessingServiceTaskForGermany
		{
			protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
			{
				return new MockEDIInterchangeCreatorForGermanyEInvoicingBatch(company, () => new MockAccEInvoiceBatchToGEIConverter());
			}
		}

#endregion
	}
}
