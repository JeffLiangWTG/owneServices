using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ControlBehaviourContainerTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ControlBehaviourContainer<DummyBaseBusinessObject>(controlBehaviour: null, dependencies: null));
			AssertNoExceptionThrown(() => new ControlBehaviourContainer<DummyBaseBusinessObject>(new DummyControlBehaviourOne(), dependencies: null));
		}

		public void TestGetDependencies()
		{
			var bo = Factory.New<DummyBusinessObject>();
			var behaviourContainer = new ControlBehaviourContainer<DummyBaseBusinessObject>(new DummyControlBehaviourOne(), new Func<DummyBaseBusinessObject, ZPropertyInfo>[] { (d) => d.Z0_NumberInfo });
			var dependencies = behaviourContainer.GetDependencies(bo);

			AssertNotNull(dependencies);
			AssertEquals("Dependency Count", 1, dependencies.Count);
			Assert("NumberInfo are same", object.Equals(bo.Z0_NumberInfo, dependencies.First()));

			dependencies = behaviourContainer.GetDependencies(null);
			AssertNotNull(dependencies);
			AssertEquals("Dependency Count", 0, dependencies.Count);
		}

		public void TestIfRefreshRequired()
		{
			var bo = Factory.New<DummyBusinessObject>();
			var behaviourContainerOne = new ControlBehaviourContainer<DummyBaseBusinessObject>(new DummyControlBehaviourOne(), new Func<DummyBaseBusinessObject, ZPropertyInfo>[] { (d) => d.Z0_NumberInfo });
			CombineAssertions("Actual Dependency Values", () =>
			{
				bo.Z0_Number = 1;
				AssertEquals("RefreshRequired", true, behaviourContainerOne.IsRefreshRequired(bo, textBoxControl));
				behaviourContainerOne.UpdateControlBehaviour(textBoxControl, bo);

				bo.Z0_Number = 1;
				AssertEquals("RefreshRequired", false, behaviourContainerOne.IsRefreshRequired(bo, textBoxControl));

				bo.Z0_Number = 22;
				AssertEquals("RefreshRequired", true, behaviourContainerOne.IsRefreshRequired(bo, textBoxControl));
			});

			var behaviourContainerTwo = new ControlBehaviourContainer<DummyBaseBusinessObject>(new DummyControlBehaviourTwo(), new Func<DummyBaseBusinessObject, ZPropertyInfo>[] { (d) => d.Z0_NumberInfo });
			CombineAssertions("CustomDependencies", () =>
			{
				bo.Z0_Number = 1;
				AssertEquals("RefreshRequired", true, behaviourContainerTwo.IsRefreshRequired(bo, textBoxControl));
				behaviourContainerTwo.UpdateControlBehaviour(textBoxControl, bo);

				bo.Z0_Number = 2;
				AssertEquals("RefreshRequired", true, behaviourContainerTwo.IsRefreshRequired(bo, textBoxControl));
				behaviourContainerTwo.UpdateControlBehaviour(textBoxControl, bo);

				bo.Z0_Number = 22;
				AssertEquals("RefreshRequired", false, behaviourContainerTwo.IsRefreshRequired(bo, textBoxControl));

				bo.Z0_Number = 33;
				AssertEquals("RefreshRequired", false, behaviourContainerTwo.IsRefreshRequired(bo, textBoxControl));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			textBoxControl = new ZTextBox();
		}

		protected override void TearDown()
		{
			base.TearDown();
			textBoxControl.Dispose();
		}

		ZTextBox textBoxControl;

		class DummyControlBehaviourOne : ControlBehaviour<ZTextBox, DummyBaseBusinessObject>
		{
			protected override void UpdateBehaviourCore(ZTextBox control, DummyBaseBusinessObject dataItem)
			{
				//Intentionally kept blank
			}
		}

		class DummyControlBehaviourTwo : ControlBehaviour<ZTextBox, DummyBaseBusinessObject>
		{
			public override bool UseControlDependentValues => true;

			protected override IEnumerable<IZType> GetControlDependentValuesCore(DummyBaseBusinessObject dataItem)
			{
				yield return (ZBool)(dataItem.Z0_Number == 1);
			}

			protected override void UpdateBehaviourCore(ZTextBox control, DummyBaseBusinessObject dataItem)
			{
				//Intentionally kept blank
			}
		}
	}
}
