using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Moq;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	abstract class SpawningIndependentNetworkActionGuiTestCase<T> : NetworkGUITestCase where T : SpawningIndependentNetworkAction
	{
		#region Ensuring Network Action Correctly Uses the Designated Method for Creating Factories and Therefore Testable by Other Tests

		public void TestShouldCreateSpawnedNetworkUsingDesignatedFactory()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var networkViewModel = GetNetworkViewModel(mocks, Factory);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			action.Execute();

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNotNull(form);

				Application.DoEvents();

				var spawnedNetwork = form.NetworkViewModel.GetJobNetwork();
				AssertNotNull(spawnedNetwork);

				var factoryForSpawnedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;

				AssertNotNull("Action should generate a new factory", factoryForSpawnedNetwork);
				AssertNotEquals("The generated factory should differ from the original factory", networkViewModel.GetJobNetwork().DiagramShape.Factory, factoryForSpawnedNetwork);
				AssertEquals("Action should use the specific method for generating factories to make other tests work", factoryForSpawnedNetwork, spawnedNetwork.DiagramShape.Factory);
			}
		}

		public void TestShouldGenerateNewFactoryForEverySpawnedNetwork()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var networkViewModel = GetNetworkViewModel(mocks, Factory);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			action.Execute();

			var previouslyUsedFactory = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(previouslyUsedFactory);

			NetworkDiagramForm previouslyCreatedForm;

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNotNull(form);
				previouslyCreatedForm = form;
			}

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			action.Execute();

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNotNull(form);
				AssertNotEquals(previouslyCreatedForm, form);

				var factoryForSpawnedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;

				AssertNotNull(factoryForSpawnedNetwork);
				AssertNotEquals("Should generate new factory each time", previouslyUsedFactory, factoryForSpawnedNetwork);
			}
		}

		#endregion

		#region Ensuring Networks Save Independently

		public void TestSavingOriginalDiagram_ShouldNotResultInSavingSpawnedDiagram()
		{
			AssertSavingOriginalDiagram_DoNotResultInSavingSpawnedDiagram((originalNetworkViewModel) => originalNetworkViewModel.GetJobNetwork().DiagramEntity.Factory.Save());
		}

		public void TestSavingSpawnedDiagram_ShouldNotResultInSavingOriginalDiagram()
		{
			AssertSavingSpawnedDiagram_DoNotResultInSavingOriginalDiagram((spawnedNetworkViewModel) => spawnedNetworkViewModel.GetJobNetwork().DiagramEntity.Factory.Save());
		}

		public void TestSavingOriginalDiagramUsingController_ShouldNotResultInSavingSpawnedDiagram()
		{
			AssertSavingOriginalDiagram_DoNotResultInSavingSpawnedDiagram((originalNetworkViewModel) => originalNetworkViewModel.GetJobController().TriggerSaveAction());
		}

		public void TestSavingSpawnedDiagramUsingController_ShouldNotResultInSavingOriginalDiagram()
		{
			AssertSavingSpawnedDiagram_DoNotResultInSavingOriginalDiagram((spawnedNetworkViewModel) => spawnedNetworkViewModel.GetJobController().TriggerSaveAction());
		}

		#endregion

		#region Preventing Memory Leaks
#if !WINZOR
		// This test is specifically for WPF, we have a similar test for Winzor
		public void TestShouldNotLeakNetworkDiagramForm_EvenIfActionIsKeptInMemoryByWPFInfrastructure()
		{
			var initialFormsCount = Application.OpenForms.Count;

			var result = ExecuteActionAndGetReferences();
			var formRef = result.Item1;
			var actionKeptInMemoryByWPF = result.Item2;
			AssertEquals(initialFormsCount + 1, Application.OpenForms.Count);

			GC.Collect(); // We are testing against memory leaks.
			GC.WaitForFullGCComplete();

			Assert(formRef.IsAlive);

			DisposeNetworkDiagramForm(formRef);
			AssertEquals(initialFormsCount, Application.OpenForms.Count);

			// we need to create a new form to make the old form's button lose focus otherwise KeyboardNavigation.FocusVisualAdorner will keep reference to the button and as the result - to the whole form
			var newFormRef = ExecuteActionAndGetReferences().Item1;

			GC.Collect(); // We are testing against memory leaks.
			GC.WaitForFullGCComplete();

			Assert(!formRef.IsAlive);

			DisposeNetworkDiagramForm(newFormRef);
		}

		Tuple<WeakReference, INetworkAction> ExecuteActionAndGetReferences()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var factory = new BusinessObjectFactory(); // we don't want the objects in question to be kept in memory by the standard test factory so we need another one
			var networkViewModel = GetNetworkViewModel(mocks, factory);
			var action = GetAction(networkViewModel);

			NetworkDiagramForm form;

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			action.Execute();

			form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault();
			AssertNotNull(form);
			Application.DoEvents();

			var spawnedNetwork = form.NetworkViewModel.GetJobNetwork();
			AssertNotNull(spawnedNetwork);

			return Tuple.Create<WeakReference, INetworkAction>(new WeakReference(form), action);
		}

		void DisposeNetworkDiagramForm(WeakReference reference)
		{
			var form = (NetworkDiagramForm)reference.Target;
			form.Dispose();
		}
#endif
		#endregion

		#region Implementation

		#region Assertion Methods

		void AssertSavingOriginalDiagram_DoNotResultInSavingSpawnedDiagram(Action<INetworkViewModel> originalDiagramSaveAction)
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var networkViewModel = GetNetworkViewModel(mocks, Factory);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			action.Execute();

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNotNull(form);

				Application.DoEvents();

				var originalDiagramEntity = network.DiagramEntity;
				var spawnedDiagramEntity = form.NetworkViewModel.GetJobNetwork().DiagramEntity;

				Assert("Precondition", spawnedDiagramEntity.HasChanges);

				CreateShape(network.DiagramShape);
				Assert("Precondition", originalDiagramEntity.HasChanges);

				originalDiagramSaveAction.Invoke(networkViewModel);

				Assert("Original diagram entity should be saved", !originalDiagramEntity.HasChanges);
				Assert("Spawned diagram entity should not be saved", spawnedDiagramEntity.HasChanges);
			}
		}

		public void AssertSavingSpawnedDiagram_DoNotResultInSavingOriginalDiagram(Action<INetworkViewModel> spawnedDiagramSaveAction)
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var networkViewModel = GetNetworkViewModel(mocks, Factory);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			action.Execute();

			using (var form = Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault())
			{
				AssertNotNull(form);

				Application.DoEvents();

				var originalDiagramEntity = network.DiagramEntity;
				var spawnedDiagramEntity = form.NetworkViewModel.GetJobNetwork().DiagramEntity;

				Assert("Precondition", spawnedDiagramEntity.HasChanges);

				CreateShape(network.DiagramShape);
				Assert("Precondition", originalDiagramEntity.HasChanges);

				spawnedDiagramSaveAction.Invoke(form.NetworkViewModel);

				Assert("Original diagram entity should not be saved", originalDiagramEntity.HasChanges);
				Assert("Spawned diagram entity should be saved", !spawnedDiagramEntity.HasChanges);
			}
		}

		#endregion

		#region Abstract Methods

		protected abstract INetworkViewModel GetNetworkViewModel(MockRepository mocks, BusinessObjectFactory factory);

		protected abstract T GetAction(INetworkViewModel networkViewModel);

		#endregion

		#endregion
	}
}
