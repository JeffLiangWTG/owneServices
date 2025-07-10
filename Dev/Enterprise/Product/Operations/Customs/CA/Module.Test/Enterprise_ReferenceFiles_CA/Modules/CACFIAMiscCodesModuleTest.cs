using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal.Module.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CACFIAMiscCodesModule))]
	sealed class CACFIAMiscCodesModuleTest : ZZRefCusCodeListWrapperModuleTest<CACFIAMiscCodes>
	{
		public void TestModuleAllows()
		{
			using (var module = new CACFIAMiscCodesModule())
			{
				AssertEquals("module.AllowDelete", false, module.AllowDelete);
				AssertEquals("module.AllowNew", false, module.AllowNew);
				AssertEquals("module.AllowEdit", false, module.AllowEdit);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.CFIAMiscCodes;

		protected override ZFilterGridModule GetNewTestModule() => new CACFIAMiscCodesModule();

		protected override string ExpectedCodeCaption => "Miscellaneous Code";

		protected override string ExpectedDescriptionCaption => "Description";

		protected override int ExpectedCodeCaptionColumnWidth => 200;

		protected override int ExpectedDescriptionColumnWidth => 500;
	}
}
