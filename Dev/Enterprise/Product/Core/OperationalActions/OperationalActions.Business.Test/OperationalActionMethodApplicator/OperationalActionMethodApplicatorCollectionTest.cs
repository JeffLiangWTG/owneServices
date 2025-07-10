using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(OperationalActionMethodApplicatorCollection))]
	internal sealed class OperationalActionMethodApplicatorCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OperationalActionMethodApplicatorCollection>
	{
		readonly ZGuid NonExistent = new ZGuid("{3ED72BF4-4A58-487c-BAB4-34A5F79F4048}");
		public void TestLoad()
		{
			OperationalActionSupporter supporter = Runner.Action.Context.Supporter;
			OperationalActionMethod methodWithGui = supporter.Methods.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithGUI);
			OperationalActionMethod methodWithoutGui = supporter.Methods.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithoutGUI);
			OperationalActionMethod methodNonExistent = supporter.Methods.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, NonExistent);
			AssertNotNull("precondition:", methodWithGui);
			AssertNotNull("precondition:", methodWithoutGui);
			AssertNull("precondition:", methodNonExistent);
			OperationalActionMethodDescriptor methodDescriptor1 = Runner.Action.MethodDescriptors.AddNew();
			methodDescriptor1.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			methodDescriptor1.MethodID = TestingConstants.DummyActionMethodWithGUI;
			OperationalActionMethodDescriptor methodDescriptor2 = Runner.Action.MethodDescriptors.AddNew();
			methodDescriptor2.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			methodDescriptor2.MethodID = TestingConstants.DummyActionMethodWithoutGUI;
			OperationalActionMethodDescriptor methodDescriptor3 = Runner.Action.MethodDescriptors.AddNew();
			methodDescriptor3.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			methodDescriptor3.MethodID = NonExistent;
			OperationalActionMethodApplicatorCollection collection = new OperationalActionMethodApplicatorCollection(Runner);
			collection.Load();
			AssertEquals("Count", 2, collection.Count);
			AssertContainsExactElementsInAnyOrder("GetMethods() should return the correct methods.", new OperationalActionMethod[] { methodWithGui, methodWithoutGui }, collection.GetMethods());
			AssertNotNull("collection[methodWithGui]", collection[methodWithGui]);
			AssertNotNull("collection[methodWithoutGui]", collection[methodWithoutGui]);
		}

		public void TestGetApplicators()
		{
			var methodWithGui = Runner.Action.Context.Supporter.Methods.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodWithGUI);
			var methodWithoutGui = Runner.Action.Context.Supporter.Methods.GetOperationalActionMethod(ActionMethodProviderIDs.DummyWithMethods, TestingConstants.DummyActionMethodRunWithoutUI);
			var methodDescriptor1 = Runner.Action.MethodDescriptors.AddNew();
			methodDescriptor1.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			methodDescriptor1.MethodID = TestingConstants.DummyActionMethodWithGUI;
			var methodDescriptor2 = Runner.Action.MethodDescriptors.AddNew();
			methodDescriptor2.MethodGroup = ActionMethodProviderIDs.DummyWithMethods.Guid;
			methodDescriptor2.MethodID = TestingConstants.DummyActionMethodRunWithoutUI;
			var collection = new OperationalActionMethodApplicatorCollection(Runner);
			collection.Load();
			AssertEquals(2, collection.Count);
			var subCollection = collection.GetApplicators(OperationalActionMethodUIMode.UIOnly);
			AssertEquals(1, subCollection.Count());
			AssertCollectionContains(collection[methodWithGui], subCollection);
			AssertCollectionNotContains(collection[methodWithoutGui], subCollection);
			subCollection = collection.GetApplicators(OperationalActionMethodUIMode.NonUIOnly);
			AssertEquals(1, subCollection.Count());
			AssertCollectionNotContains(collection[methodWithGui], subCollection);
			AssertCollectionContains(collection[methodWithoutGui], subCollection);
			subCollection = collection.GetApplicators(OperationalActionMethodUIMode.All);
			AssertEquals(2, subCollection.Count());
			AssertCollectionContains(collection[methodWithGui], subCollection);
			AssertCollectionContains(collection[methodWithoutGui], subCollection);
		}

		#region Implementation
		protected override OperationalActionMethodApplicatorCollection GetCollectionToTest()
		{
			return new OperationalActionMethodApplicatorCollection(Runner);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DummyOperationalActionMethodApplicator("Dummy Applicator", Factory, null);
		}

		public OperationalActionSupporter ActionSupporter
		{
			get
			{
				if (actionSupporter == null)
				{
					actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter;
					actionSupporter.Methods.Add(ActionMethodProviderIDs.DummyWithMethods);
				}

				return actionSupporter;
			}
		}

		OperationalActionSupporter actionSupporter;
		public OperationalAction Action
		{
			get
			{
				if (action == null)
				{
					action = Factory.New<OperationalAction>();
					action.Context = Context;
				}

				return action;
			}
		}

		OperationalAction action;
		OperationalActionContext Context
		{
			get
			{
				return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name"));
			}
		}

		OperationalActionContext context;
		public OperationalActionRunner Runner
		{
			get
			{
				return runner ?? (runner = new OperationalActionRunner(Action, typeof(DummyBusinessObjectWithDocumentSupport), new SelectedRecords()));
			}
		}

		OperationalActionRunner runner;
		#endregion
	}
}
