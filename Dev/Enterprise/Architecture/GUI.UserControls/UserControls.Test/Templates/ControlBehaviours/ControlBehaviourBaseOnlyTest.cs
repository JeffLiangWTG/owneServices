using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ControlBehaviourBaseOnlyTest : TestCaseWithFactory
	{
		public void TestBahviourName()
		{
			var visibilityState = new ControlVisibilityBehaviour<DummyBaseBusinessObject>(d => true);
			AssertEquals("Behaviour Name", typeof(ControlVisibilityBehaviour<>).FullName, visibilityState.Name);

			var dummyControlBehaviour = new DummyControlBehaviour();
			AssertEquals("Behaviour Name", "Enterprise.ZArchitecture.GUI.Testing.ControlBehaviourBaseOnlyTest+DummyControlBehaviour", dummyControlBehaviour.Name);

			var dummyBehaviourWithCustomName = new DummyBehaviourWithCustomName(false);
			AssertEquals("Behaviour Name", "DummyBehaviourDoingSomethingName", dummyBehaviourWithCustomName.Name);

			var dummyBehaviourWithCustomName2 = new DummyBehaviourWithCustomName(true);
			AssertEquals("Behaviour Name", "Enterprise.ZArchitecture.GUI.Testing.ControlBehaviourBaseOnlyTest+DummyBehaviourWithCustomName", dummyBehaviourWithCustomName2.Name);
		}

		class DummyControlBehaviour : ControlBehaviour<ZTextBox, DummyBusinessObject>
		{
			protected override void UpdateBehaviourCore(ZTextBox control, DummyBusinessObject dataItem)
			{
				throw new NotImplementedException();
			}

			protected override IEnumerable<IZType> GetControlDependentValuesCore(DummyBusinessObject dataItem)
			{
				yield return dataItem.Z0_Number;
			}
		}

		class DummyBehaviourWithCustomName : ControlBehaviourWithCustomName<ZTextBox, DummyBusinessObject>
		{
			public DummyBehaviourWithCustomName(bool returnCustomNameAsNullOrEmpty)
			{
				this.returnCustomNameAsNullOrEmpty = returnCustomNameAsNullOrEmpty;
			}

			protected override void UpdateBehaviourCore(ZTextBox control, DummyBusinessObject dataItem)
			{
				throw new NotImplementedException();
			}

			protected override string GetCustomBehaviourNameCore() => returnCustomNameAsNullOrEmpty ? string.Empty : "DummyBehaviourDoingSomethingName";

			readonly bool returnCustomNameAsNullOrEmpty;
		}
	}
}
