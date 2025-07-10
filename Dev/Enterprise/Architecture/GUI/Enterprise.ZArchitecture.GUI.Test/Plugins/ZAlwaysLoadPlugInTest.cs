using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.PlugIn.Testing
{
	class ZAlwaysPlugInTest : TestCaseWithDummy
	{
		public void TestAlwaysSaveNewBusinessEntity()
		{
			using (var form = new TestPlugInForm(Dummy))
			{
				form.Show();
				form.FireSaveButton();

				var alwaysLoadPlugin = form.PlugIns.GetPlugIn(DummyControllerIDs.Dummy2);
				var createdBusinessEntity = Factory.Load<DummyBusinessObject>(alwaysLoadPlugin.BusinessEntity.Identifier);
				AssertNotNull("A ZAlwaysLoadPlugin must always create it's business entity on save", createdBusinessEntity.IsInDatabase);
			}
		}
	}
}
