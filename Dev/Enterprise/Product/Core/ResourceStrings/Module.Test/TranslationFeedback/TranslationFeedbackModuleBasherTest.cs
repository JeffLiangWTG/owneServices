using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Module.TranslationFeedback.Testing
{
	[TestedType(typeof(TranslationFeedbackModule))]
	internal class TranslationFeedbackModuleBasherTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.TranslationFeedback;
		}
	}
}
