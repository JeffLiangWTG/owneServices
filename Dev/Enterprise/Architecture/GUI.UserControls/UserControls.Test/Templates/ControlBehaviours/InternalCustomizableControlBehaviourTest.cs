using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class InternalCustomizableControlBehaviourTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new InternalCustomizableControlBehaviour<Control, DummyBusinessObject>(updateControlBehaviourAction: null));
			AssertExceptionThrown<ArgumentException>(() => new InternalCustomizableControlBehaviour<Control, DummyBusinessObject>("", (c, b) => c.Enabled = false));
		}

		public void TestUpdateBehaviour_CallsAction()
		{
			var dummyBusinessObject = Factory.New<DummyBusinessObject>();
			var behaviour = new InternalCustomizableControlBehaviour<Control, DummyBusinessObject>((c, b) => c.Enabled = false);
			behaviour.UpdateBehaviour(control, dummyBusinessObject);
			AssertEquals("Control Enable Property Changed", false, control.Enabled);

			control.Enabled = true;
			behaviour.UpdateBehaviour(control, dummyBusinessObject);
			AssertEquals("Control Enable Property Changed", false, control.Enabled);
		}

		public void TestBehaviourName()
		{
			var behaviour = new InternalCustomizableControlBehaviour<Control, DummyBusinessObject>((c, b) => c.Enabled = false);
			AssertEquals("Behaviour Name", "Enterprise.ZArchitecture.GUI.InternalCustomizableControlBehaviour`2", behaviour.Name);

			var behaviourWithCustomName = new InternalCustomizableControlBehaviour<Control, DummyBusinessObject>("ChangeControlAccessBehaviour", (c, b) => c.Enabled = false);
			AssertEquals("Behaviour name", "ChangeControlAccessBehaviour", behaviourWithCustomName.Name);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new Control { Enabled = true, Name = "TestControl" };
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		Control control;
	}
}
