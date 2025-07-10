using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Module.Testing;

public class JobDeclarationFilterControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
{
	[ExpectNoExceptions]
	public void TestConstructor()
	{
		BaseJobDeclarationCollection jobDecs = new BaseJobDeclarationCollection(Factory);
		JobDeclarationFilterStripBusinessObject filterObject = new JobDeclarationFilterStripBusinessObject();
		using (JobDeclarationFilterControl filterControl = new JobDeclarationFilterControl(null, jobDecs, filterObject))
		{
		}
	}
}
