using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.Testing
{
	[TestedType(typeof(DocumentSigningJobModule))]
	class DocumentSigningJobModuleTest : PrintJobModuleTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DocumentSigningJob;
		}

		protected override SecurityCheckpointNonOperationalAllowed ExpectedSecurityCheckpoint => Env.Security.DocumentSigningJobs;

		protected override PrintJobModule GetNewModule()
		{
			return new DocumentSigningJobModule();
		}

		protected override IPrintJobModuleForTest GetNewModuleForTest()
		{
			return new DocumentSigningJobModuleForTest();
		}
	}
}
