using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CN2004DataInterfaceModule))]
	public class PreMatchingExportModule0102Test : ZArchitecture.Modules.Testing.ZPopupModuleBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.CN2004DataInterface, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Core, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.None, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(CN2004DataInterfaceController), Module.GetNewController_ForTestOnly().GetType());
		}

		public void TestShow()
		{
			using (IZForm form = new CN2004DataInterfaceController().ShowNewForm())
			{
				AssertNotNull(form);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Module = new CN2004DataInterfaceModule();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CN2004DataInterface;
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}
			base.TearDown();
		}

		protected CN2004DataInterfaceModule Module;

		#endregion
	}
}
