using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(DsbJobCloseBatchTransactionLineCollection))]
	public class DsbJobCloseBatchTransactionLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var tparReport = Factory.New<DsbJobCloseBatch>();
			return new DsbJobCloseBatchTransactionLineCollection(tparReport);
		}
	}
}
