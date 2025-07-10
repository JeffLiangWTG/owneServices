using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.Module.Test
{
	[TestedType(typeof(MENTAgedScoreExtractionModule))]
	class MENTAgedScoreExtractionModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.MENTAgedScoreExtraction;
		}

		public void TestShouldAllowCopyHyperlinksToClipboard()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(((MENTAgedScoreExtractionModule)module).AllowCopyFilterGridHyperlinkToClipboard, true);
			}
		}
	}
}
