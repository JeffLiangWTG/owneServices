using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ControlBehaviourContainerCollectionTest : TestCase
	{
		public void TestAdd()
		{
			var containerCollection = new ControlBehaviourContainerCollection();
			var firsBehaviour = new DummyControlBehaviour();
			var secondBehaviour = new DummyControlBehaviour();

			AssertNoExceptionThrown(() => containerCollection.Add(new ControlBehaviourContainer<DummyBusinessObject>(firsBehaviour, null)));
			AssertNoExceptionThrown(() => containerCollection.Add(new ControlBehaviourContainer<DummyBusinessObject>(secondBehaviour, null)));

			var allBehaviours = containerCollection.GetAll();
			AssertEquals("Count", 1, allBehaviours.Count);
			AssertSame("Behavior is same", secondBehaviour, allBehaviours.First().ControlBehaviour);
		}

		public void TestGetAll()
		{
			var containerCollection = new ControlBehaviourContainerCollection
			{
				new ControlBehaviourContainer<DummyBusinessObject>(new DummyControlBehaviour(), null),
				new ControlBehaviourContainer<DummyBaseBusinessObject>(new ControlVisibilityBehaviour<DummyBusinessObject>(d => true), null)
			};

			var allStates = containerCollection.GetAll();

			AssertNotNull(allStates);
			AssertEquals("Count", 2, containerCollection.Count);
			Assert("DummyControlState Exists", allStates.Any(c => c.ControlBehaviour.GetType() == typeof(DummyControlBehaviour)));
			Assert("ControlVisibilityState Exists", allStates.Any(c => c.ControlBehaviour.GetType() == typeof(ControlVisibilityBehaviour<DummyBusinessObject>)));
		}

		class DummyControlBehaviour : ControlBehaviour
		{
			public override void UpdateBehaviour(Control control, object dataItem)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}
