using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(CMRLodgementQuestionModule))]
	sealed class CMRLodgementQuestionModuleTest : CMRSearchOnlyModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.CMRLodgementQuestion;
	}
}
