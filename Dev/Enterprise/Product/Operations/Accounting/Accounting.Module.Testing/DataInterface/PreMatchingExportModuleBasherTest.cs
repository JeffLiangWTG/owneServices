using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CNDataInterfaceModule))]
	public class PreMatchingExportModuleBasherTest : ZArchitecture.Modules.Testing.ZPopupModuleBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.CNDataInterface, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Core, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.ChinaDataInterface, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(CNDataInterfaceController), Module.GetNewController_ForTestOnly().GetType());
		}

		public void TestShow()
		{
			using (IZForm form = new CNDataInterfaceController().ShowNewForm())
			{
				AssertNotNull(form);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Module = new CNDataInterfaceModule();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CNDataInterface;
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}
			base.TearDown();
		}

		protected CNDataInterfaceModule Module;

		#endregion
	}
}
