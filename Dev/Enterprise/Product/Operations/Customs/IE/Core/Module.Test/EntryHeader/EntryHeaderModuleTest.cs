using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing
{
	[TestedType(typeof(EntryHeaderModule))]
	class EntryHeaderModuleTest : Customs.Module.Testing.EntryHeaderModuleTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			module = new EntryHeaderModule();
		}

		protected override void TearDown()
		{
			module.Dispose();
			base.TearDown();
		}

		EntryHeaderModule module;
	}
}
