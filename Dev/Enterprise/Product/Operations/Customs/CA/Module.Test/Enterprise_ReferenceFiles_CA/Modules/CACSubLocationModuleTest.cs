using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Universal.Module.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CACSubLocationModule))]
	sealed class CACSubLocationModuleTest : ZZRefCusCodeListWrapperModuleTest<CACSubLocation>
	{
		public void TestModuleAllows()
		{
			using (var module = new CACSubLocationModule())
			{
				AssertEquals("module.AllowDelete", false, module.AllowDelete);
				AssertEquals("module.AllowNew", false, module.AllowNew);
				AssertEquals("module.AllowEdit", false, module.AllowEdit);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.SubLocation;

		protected override ZFilterGridModule GetNewTestModule() => new CACSubLocationModule();

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);
			CACSubLocationTest.CreateSubLocation(collection.Factory, "1111", "1111 desc", "0001");
			CACSubLocationTest.CreateSubLocation(collection.Factory, "2222", "2222 desc", "0002");
			collection.Factory.Save();
		}

		protected override string ExpectedCodeCaption => "Sub-Location Code";

		protected override string ExpectedDescriptionCaption => "Warehouse Name";
	}
}
