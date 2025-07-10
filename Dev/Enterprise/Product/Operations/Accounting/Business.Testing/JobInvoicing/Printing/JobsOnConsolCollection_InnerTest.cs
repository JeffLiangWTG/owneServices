using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.Business.JobInvoicing.JobInvoicePrintingFilter;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobsOnConsolCollection))]
	public class JobsOnConsolCollection_InnerTest : JobHeaderCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobsOnConsolCollection(Factory);
		}
	}
}
