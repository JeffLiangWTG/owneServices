using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccountingVoucherModule))]
	public class AccountingVoucherModuleTest : ZArchitecture.Modules.Testing.ZPopupModuleBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.AccountingVoucher, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Core, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.AccountingVoucher, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(AccountingVoucherController), Module.GetNewController_ForTestOnly().GetType());
		}

		public void TestShow()
		{
			using (IZForm form = new AccountingVoucherController().ShowNewForm())
			{
				AssertNotNull(form);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Module = new AccountingVoucherModule();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccountingVoucher;
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}
			base.TearDown();
		}

		protected AccountingVoucherModule Module;

		#endregion
	}
}
