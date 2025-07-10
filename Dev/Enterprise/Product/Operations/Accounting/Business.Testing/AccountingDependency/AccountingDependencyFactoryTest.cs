using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingDependency.Testing
{
	public class AccountingDependencyFactoryTest : TestCaseWithFactory
	{
		public void TestGetIAccountingDependencyFactory()
		{
			AssertType<AccountingDependencyFactory>(ObjectFactory.Get<IAccountingDependencyFactory>());
		}

		public void TestGetCountrySpecificLabelTranslator()
		{
			AssertType<CountrySpecificLabelTranslator>(ObjectFactory.Get<IAccountingDependencyFactory>().GetCountrySpecificLabelTranslator());
		}

		public void TestAccountingDependencyFactory_GetBranchLevelPostingHelper()
		{
			AssertType<BranchLevelPostingHelper>(ObjectFactory.Get<IAccountingDependencyFactory>().GetBranchLevelPostingHelper());
		}

		public void TestAccountingDependencyFactory_GetSingleActionPerTransaction()
		{
			AssertType<SingleActionPerTransactionOnDifferentLevels>(ObjectFactory.Get<IAccountingDependencyFactory>().GetSingleActionPerTransactionOnDifferentLevels());
		}

		public void TestAccountingDependencyFactory_GetJobCostingPlugInHelpers()
		{
			AssertType<JobCostingPlugInHelpers>(ObjectFactory.Get<IAccountingDependencyFactory>().GetJobCostingPlugInHelpers());
		}

		public void TestGetWithholdingJournalCreationManager()
		{
			AssertType<WithholdingJournalCreationManager>(ObjectFactory.Get<IAccountingDependencyFactory>().GetWithholdingJournalCreationManager());
		}

		public void TestGetCollectionBatchFileGeneratorProvider()
		{
			AssertNull(ObjectFactory.Get<IAccountingDependencyFactory>().GetCollectionBatchFileGeneratorProvider(null, Environment.Env.Instance));

			var accCollectionBatch = Factory.New<AccCollectionBatch>();
			accCollectionBatch.ACB_CollectionFileFormat = CollectionFileFormatList.Codes.deFormat;
			AssertNotEquals("Precondition: ", CollectionFileFormatList.Codes.itauBank, accCollectionBatch.ACB_CollectionFileFormat);
			AssertNull(ObjectFactory.Get<IAccountingDependencyFactory>().GetCollectionBatchFileGeneratorProvider(accCollectionBatch, Environment.Env.Instance));

			accCollectionBatch.ACB_CollectionFileFormat = CollectionFileFormatList.Codes.itauBank;
			AssertEquals("Precondition: ", CollectionFileFormatList.Codes.itauBank, accCollectionBatch.ACB_CollectionFileFormat);
			AssertNotNull(ObjectFactory.Get<IAccountingDependencyFactory>().GetCollectionBatchFileGeneratorProvider(accCollectionBatch, Environment.Env.Instance));
		}
	}
}
