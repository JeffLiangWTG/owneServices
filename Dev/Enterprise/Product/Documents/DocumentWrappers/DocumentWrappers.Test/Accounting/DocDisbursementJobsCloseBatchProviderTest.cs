using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class DocDisbursementJobsCloseBatchProviderTest : TestCaseWithFactory
	{
		public void TestCreateDocDsbJobCloseBatch()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var dsbJobCloseBatch = testObjectCreator.CreateDsbJobCloseBatch("112233");
			var docDisbursementJobsCloseBatchProvider = new DocDisbursementJobsCloseBatchProvider();
			var docDisbursementJobsCloseBatch = docDisbursementJobsCloseBatchProvider.CreateDocDsbJobCloseBatch(dsbJobCloseBatch, Factory);

			Assert("The type of created object should be DocDisbursementJobsCloseBatch", docDisbursementJobsCloseBatch is DocDisbursementJobsCloseBatch);
		}
	}
}
