using NUnit.Framework;

namespace Enterprise.Customs.IN.Module.Testing;

[TestedType(typeof(JobDeclarationFilterBusinessObject))]
sealed class JobDeclarationFilterBusinessObjectTest : Customs.Module.Testing.JobDeclarationFilterBusinessObjectTest
{
	public void TestLookups()
	{
		var filterBizObj = new JobDeclarationFilterBusinessObject();
		AssertType<JobDeclarationFilterLookups>("Lookups of correct type", filterBizObj.Lookups);
	}
}
