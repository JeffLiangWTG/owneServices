using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(JobRequiredDocumentAddInfoModule))]
	sealed class JobRequiredDocumentAddInfoModuleTest : ZArchitecture.Modules.Testing.ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.JobRequiredDocumentAddInfo;

		protected override bool HasController() => false;

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_View => true;
	}
}
