using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	[TestedType(typeof(Job))]
	class JobSupportCriticalValidationTest : SupportCriticalValidationTestBase
	{
		protected override IConflictWithCriticalFields GetNewBusinessObject()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var jobLoader = new JobHeader.Loader(declaration);
			var job = jobLoader.TryCreateWithoutMutexForTestOnly();
			return (Job)job;
		}
	}
}
