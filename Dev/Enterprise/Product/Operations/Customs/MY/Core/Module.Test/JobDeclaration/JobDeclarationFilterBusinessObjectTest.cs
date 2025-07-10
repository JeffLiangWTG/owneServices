using NUnit.Framework;

namespace Enterprise.Customs.MY.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	class JobDeclarationFilterBusinessObjectTest : Customs.Module.Testing.JobDeclarationFilterBusinessObjectTest
	{
		public void TestLookups()
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			AssertType<JobDeclarationFilterLookups>(filterBizObj.Lookups);
		}
	}
}
