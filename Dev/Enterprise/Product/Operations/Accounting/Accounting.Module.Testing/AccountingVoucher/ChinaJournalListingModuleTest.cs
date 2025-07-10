using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ChinaJournalListingModule))]
	public class ChinaJournalListingModuleTest : ZArchitecture.Modules.Testing.ZPopupModuleBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.ChinaJournalListing, Module.ID);
		}

		public void TestLicenseCheckpoint()
		{
			AssertEquals(Env.Licence.Core, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.ChinaJournalListing, Module.SecurityCheckpoint);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(ChinaJournalListingController), Module.GetNewController_ForTestOnly().GetType());
		}

		public void TestShow()
		{
			using (IZForm form = new ChinaJournalListingController().ShowNewForm())
			{
				AssertNotNull(form);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Module = new ChinaJournalListingModule();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ChinaJournalListing;
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}
			base.TearDown();
		}

		protected ChinaJournalListingModule Module;

		#endregion
	}
}
