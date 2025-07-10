using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania.Testing
{
	internal class RomaniaElectronicMessagingProcessingServiceTaskDataProviderTest : ElectronicMessagingProcessingServiceTaskDataProviderTest
	{
		public override ElectronicMessagingProcessingServiceTaskDataProvider GetProvider() => new RomaniaElectronicMessagingProcessingServiceTaskDataProvider();

		public void TestGetPKsOfCompaniesWithPeriodicRequests()
		{
			var company1 = CreateCompanyAndBranch("TC1", "TB1", CountryCodes.Australia);
			var company2 = CreateCompanyAndBranch("TC2", "TB2", CountryCodes.Romania);
			var company3 = CreateCompanyAndBranch("TC3", "TB3", CountryCodes.Romania);
			var company4 = CreateCompanyAndBranch("TC4", "TB4", CountryCodes.Romania);
			var company5 = CreateCompanyAndBranch("TC5", "TB5", CountryCodes.Romania);

			var company6 = CreateCompanyAndBranch("TC6", "TB6", CountryCodes.Australia);
			var company7 = CreateCompanyAndBranch("TC7", "TB7", CountryCodes.Romania);
			var company8 = CreateCompanyAndBranch("TC8", "TB8", CountryCodes.Romania);
			var company9 = CreateCompanyAndBranch("TC9", "TB9", CountryCodes.Romania);

			PrepareData("001", company1, EInvoicingPivotState.Delivered, EInvoicingBatchState.Ready);
			PrepareData("002", company2, EInvoicingPivotState.Delivered, EInvoicingBatchState.Ready);
			PrepareData("003", company3, EInvoicingPivotState.Delivered, EInvoicingBatchState.Sent);
			PrepareData("004", company4, EInvoicingPivotState.Delivered, EInvoicingBatchState.Ready);
			PrepareData("005", company5, EInvoicingPivotState.Queued, EInvoicingBatchState.Ready);

			CreateTransactionAndPivot("006", company6, EInvoicingPivotState.Delivered); //Skipped due to not in RO
			PrepareData("007", company7, EInvoicingPivotState.Delivered, EInvoicingBatchState.Sent); //Skipped due to already has batch
			CreateTransactionAndPivot("008", company8, EInvoicingPivotState.Queued); //Skipped due to pivot is not DLV status
			CreateTransactionAndPivot("009", company9, EInvoicingPivotState.Delivered);

			Factory.Save();

			var companyPKs = GetProvider().GetPKsOfCompaniesWithPeriodicRequests(CountryCodes.Romania);

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { company2.PK, company4.PK, company9.PK }, companyPKs);
		}

		void PrepareData(ZString transactionNum, GlbCompany company, string pivotStatus, string batchStatus)
		{
			var pivot = CreateTransactionAndPivot(transactionNum, company, pivotStatus);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 001, batchStatus);
		}

		AccEInvoicingTransactionPivot CreateTransactionAndPivot(ZString transactionNum, GlbCompany company, string pivotStatus)
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>(transactionNum, TestObjectCreator.EUR, 1m, TestObjectCreator.Debtor);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: pivotStatus);
			pivot.SetCompanyAndCountryCode(company);

			return pivot;
		}

		GlbCompany CreateCompanyAndBranch(ZString companyCode, ZString branchCode,  ZString countryCode)
		{
			var company = TestObjectCreator.CreateNewCompany(companyCode, countryCode);
			TestObjectCreator.CreateNewBranch(company, branchCode);

			return company;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
