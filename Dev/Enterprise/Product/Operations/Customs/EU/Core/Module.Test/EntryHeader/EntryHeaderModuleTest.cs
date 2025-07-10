using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(EntryHeaderModule))]
	public class EntryHeaderModuleTest : Customs.Module.Testing.EntryHeaderModuleTest
	{
		public void TestGetNewController()
		{
			AssertType<EntryHeaderController>("Controller must be of type EntryHeaderController", module.GetNewController());
		}

		public void TestHasActions()
		{
			AssertEquals("HasActions must be true", true, module.HasActions);
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (var filterControl = module.GetNewFilterControlForGrid())
			{
				AssertType<EntryHeaderFilterUserControl>("FilterControl Type", filterControl);
			}
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew must be false", false, module.AllowNew);
		}

		public void TestAllowDelete()
		{
			AssertEquals("AllowDelete must be false", false, module.AllowDelete);
		}

		public void TestFilterBusinessObject()
		{
			AssertEquals("Is FilterBusinessObject an instance of EntryHeaderFilterBusinessObject?", true, typeof(EntryHeaderFilterBusinessObject).IsInstanceOfType(module.FilterBusinessObject));
		}

		protected override void SetUp()
		{
			base.SetUp();
			module = GetNewEntryHeaderModule();
		}

		protected virtual EntryHeaderModule GetNewEntryHeaderModule() => new EntryHeaderModule();

		protected override void TearDown()
		{
			module.Dispose();
			base.TearDown();
		}
		EntryHeaderModule module;
	}
}
