using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ControlVisibilityBehaviourTest : TestCaseWithFactory
	{
		public void TestGetControlDependentValues()
		{
			var bo = Factory.New<DummyBusinessObject>();
			var visibilityState = new ControlVisibilityBehaviour<DummyBaseBusinessObject>(b => b.Z0_Number == 2);

			bo.Z0_Number = 1;
			AssertEquals(false, visibilityState.GetControlDependentValues(bo).First());

			visibilityState.UpdateBehaviour(control, bo);
			bo.Z0_Number = 3;
			AssertEquals(false, visibilityState.GetControlDependentValues(bo).First());

			visibilityState.UpdateBehaviour(control, bo);
			bo.Z0_Number = 2;
			AssertEquals(true, visibilityState.GetControlDependentValues(bo).First());
		}

		public void TestUpdateBehaviour()
		{
			var bo = Factory.New<DummyBusinessObject>();
			var visibilityState = new ControlVisibilityBehaviour<DummyBaseBusinessObject>(b => b.Z0_Number == 2);

			visibilityState.UpdateBehaviour(control, bo);
			AssertEquals("Visible", false, control.Visible);

			bo.Z0_Number = 2;
			visibilityState.UpdateBehaviour(control, bo);
			AssertEquals("Visible", true, control.Visible);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new DummyControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		DummyControl control;

		class DummyControl : Control
		{
		}
	}
}
