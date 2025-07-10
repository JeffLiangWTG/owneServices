using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocJobInvoicingJobCollection))]
	public class DocJobInvoicingJobCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocJobInvoicingJobCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var jobBizO = Factory.NewJobForTesting<Job>();
			return DocJobInvoicingJob.New(jobBizO, Factory);
		}

		protected override DocJobInvoicingJobCollection GetCollectionToTest()
		{
			return new DocJobInvoicingJobCollection(Factory);
		}
	}
}
