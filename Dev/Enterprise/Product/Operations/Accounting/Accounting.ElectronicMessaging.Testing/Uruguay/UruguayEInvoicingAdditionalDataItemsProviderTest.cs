using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Moq;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	public class UruguayEInvoicingAdditionalDataItemsProviderTest : TestCaseWithFactory
	{
		public void TestCompanyBankName_WhenBankName_IsEmpty()
		{
			var batch = CreateBatchWithInvoiceAndBankAccount();

			var additionalDataItems = GetAdditionalDataItems(batch);

			AssertCollectionNotContains("CompanyBankName must not exist", "CompanyBankName", additionalDataItems);
		}

		public void TestCompanyBankName_WhenBankName_HasValue()
		{
			var keyTest = "CompanyBankName";
			var valueTest = "Bank Name Test";

			var batch = CreateBatchWithInvoiceAndBankAccount(bankName: valueTest);

			var additionalDataItems = GetAdditionalDataItems(batch);

			AssertCollectionContains(additionalDataItems, x => x.Key == keyTest && x.Value == valueTest);
		}

		public void TestCompanyBankName_WhenAccountNum_IsEmpty()
		{
			var batch = CreateBatchWithInvoiceAndBankAccount();

			var additionalDataItems = GetAdditionalDataItems(batch);

			AssertCollectionNotContains("CompanyBankAccountNumber must not exist", "CompanyBankAccountNumber", additionalDataItems);
		}

		public void TestCompanyBankName_WhenAccountNum_HasValue()
		{
			var keyTest = "CompanyBankAccountNumber";
			var valueTest = "Account Num Test";

			var batch = CreateBatchWithInvoiceAndBankAccount(accountNum: valueTest);

			var additionalDataItems = GetAdditionalDataItems(batch);

			AssertCollectionContains(additionalDataItems, x => x.Key == keyTest && x.Value == valueTest);
		}

		public void TestCompanyBankName_When_SWIFT_IsEmpty()
		{
			var batch = CreateBatchWithInvoiceAndBankAccount();

			var additionalDataItems = GetAdditionalDataItems(batch);

			AssertCollectionNotContains("CompanyBankSWIFTCode must not exist", "CompanyBankSWIFTCode", additionalDataItems);
		}

		public void TestCompanyBankName_When_SWIFT_HasValue()
		{
			var keyTest = "CompanyBankSWIFTCode";
			var valueTest = "SWIFT Test";

			var batch = CreateBatchWithInvoiceAndBankAccount(sWIFT: valueTest);

			var additionalDataItems = GetAdditionalDataItems(batch);

			AssertCollectionContains(additionalDataItems, x => x.Key == keyTest && x.Value == valueTest);
		}
		public void TestInvoiceWhenReceiptBankAccountIsNull()
		{
			var company = GlbCompany.CurrentCompany;
			var batch = Factory.New<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;

			var pivot = batch.TransactionPivots.AddNew();
			pivot.AIP_ParentID = Factory.New<ARInvoice>().PK;

			var additionalDataItems = GetAdditionalDataItems(batch);

			AssertCollectionNotContains("CompanyBankName must not exist when BankAccount Is Null", "CompanyBankName", additionalDataItems);
			AssertCollectionNotContains("CompanyBankAccountNumber must not exist when BankAccount Is Null", "CompanyBankAccountNumber", additionalDataItems);
			AssertCollectionNotContains("CompanyBankSWIFTCode must not exist when BankAccount Is Null", "CompanyBankSWIFTCode", additionalDataItems);
		}

		AccEInvoicingBatch CreateBatchWithInvoiceAndBankAccount(string bankName = "", string accountNum = "", string sWIFT = "")
		{
			var company = GlbCompany.CurrentCompany;
			var batch = Factory.New<AccEInvoicingBatch>();
			batch.AIB_GC = company.PK;

			var bank = Factory.New<AccBankAccount>();
			bank.AB_BankName = bankName;
			bank.AB_AccountNum = accountNum;
			bank.AB_SWIFT = sWIFT;
			bank.AB_IsDefaultReceiptBankAccount = true;
			bank.AB_GC = company.PK;

			var orgHeader = Factory.New<OrgHeader>();
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = orgHeader.PK;

			var pivot = batch.TransactionPivots.AddNew();
			pivot.AIP_AIB = batch.PK;
			pivot.AIP_ParentID = invoice.PK;
			pivot.SetCompanyAndCountryCode(batch.Company);

			return batch;
		}

		List<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem> GetAdditionalDataItems(AccEInvoicingBatch batch)
		{
			var additionalDataItemsProvider = new UruguayEInvoicingAdditionalDataItemsProvider() as IAdditionalDataItemsProvider;
			var additionalDataItem = additionalDataItemsProvider.GetAdditionalHeaderDataItems(batch, batch.Company.Branches[0], new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), new Mock<ICountryEInvoicingObjectFactory>().Object, new Mock<INotifications>().Object);

			return additionalDataItem.ToList<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem>();
		}
	}
}
