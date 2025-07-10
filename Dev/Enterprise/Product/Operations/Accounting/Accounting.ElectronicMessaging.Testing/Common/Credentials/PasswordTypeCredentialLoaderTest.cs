using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Moq;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public abstract class PasswordTypeCredentialLoaderTest<T> : TestCaseWithFactory
		where T : PasswordTypeCredentialLoader
	{
		public virtual void TestLoadForGEIRequest_Throws_WhenNullArguments()
		{
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var branch = Factory.New<GlbBranch>();

			AssertExceptionThrown<ArgumentNullException>(() =>
				CreateDefaultObjectForTest().LoadForGEIRequest(null, CreateTestBatch(), countryFactoryMock.Object).ToArray()
			);

			AssertExceptionThrown<ArgumentNullException>(() =>
				CreateDefaultObjectForTest().LoadForGEIRequest(branch, null, countryFactoryMock.Object).ToArray()
			);

			AssertExceptionThrown<ArgumentNullException>(() =>
				CreateDefaultObjectForTest().LoadForGEIRequest(branch, CreateTestBatch(), null).ToArray()
			);

			AssertNoExceptionThrown(() =>
				CreateDefaultObjectForTest().LoadForGEIRequest(branch, CreateTestBatch(), countryFactoryMock.Object).ToArray()
			);
		}

		protected abstract T CreateDefaultObjectForTest();

		protected GlbBranch CreateBranchAndCompany()
			=> TestObjectCreator.CreateBranchWithCompany("AUBNE");

		protected GlbBranch CreateBranchAndCompany(ZString homePortCode)
			=> TestObjectCreator.CreateBranchWithCompany(homePortCode);

		protected UniversalTransactionBatch CreateTestBatch()
			=> new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		protected TestObjectCreator testObjectCreator;
	}
}
