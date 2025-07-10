using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(Job.Loader))]
	class JobLoaderTestCase : JobHeaderLoaderTest
	{
		protected override BusinessObject.Loader GetNewLoaderToTest() => new Job.Loader(Parent);
	}
}
