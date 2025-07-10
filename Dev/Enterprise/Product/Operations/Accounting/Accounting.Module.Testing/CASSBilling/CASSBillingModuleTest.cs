using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Testing
{
	public class CassBillingModuleTest : TestCaseWithFactory
	{
		public void TestGetNewController()
		{
			AssertEquals(typeof(CASSBillingController), Module.GetNewController_ForTestOnly().GetType());
		}

		public void TestShow()
		{
			using (IZForm form = Module.GetNewController_ForTestOnly().ShowNewForm())
			{
				AssertNotNull(form);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Module = new CASSBillingModule();
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}
			base.TearDown();
		}

		protected CASSBillingModule Module;

		#endregion
	}
}
