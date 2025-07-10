using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class BufferPenetrationViewModelTest : NetworkTestCase
	{
		public void TestPenetrationItemsCollection()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = CreateWorkflow(jobHeader, "workflow4");

			var diagram = CreateDiagram(jobHeader, name: "diagram");
			var shape1 = CreateShape(workflow1, diagram, "shape1");
			var shape2 = CreateShape(workflow2, diagram, "shape2");
			var shape3 = CreateShape(workflow3, diagram, "shape3");
			var shape4 = CreateShape(workflow4, diagram, "shape4");

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			shape4.MakeVisiblePrerequisiteOf(shape3, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			Factory.Save();

			var projectBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single(b => b.Type == BufferType.Project);
			var feedingBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single(b => b.Type == BufferType.Feeding);

			var viewModel = new BufferPenetrationViewModel(projectBuffer);
			var collection = viewModel.PenetrationItemsCollection.Cast<BufferedItemBufferPenetrationViewModel>().ToArray();

			AssertContainsExactElementsInAnyOrder(new[] { "shape1", "shape2", "shape3", "shape4" }, collection.Select(i => i.BufferedItemName));

			viewModel = new BufferPenetrationViewModel(shape4);
			collection = viewModel.PenetrationItemsCollection.Cast<BufferedItemBufferPenetrationViewModel>().ToArray();

			AssertContainsExactElementsInAnyOrder(new[] { "Project Buffer", "shape4 Feeding Buffer" }, collection.Select(i => i.BufferName));
		}

		public void TestPenetrationItemsHint()
		{
			var viewModel = new BufferPenetrationViewModel(Factory.New<BMNCNShape>());
			AssertEquals("Buffers which this item can penetrate are listed below.", viewModel.PenetrationItemsHint);
			AssertEquals(
@"For buffers an item directly penetrates, buffer penetration is calculated as: (Time Since Startable, minus Planned Duration, plus Remaining Estimate) divide by Buffer Duration.

A penetration amount greater than 100% on any buffer can 'overflow' onto other buffers downstream in the dependency network. The penetration for these overflow buffers is calculated as: overflow time divide by Buffer Duration.", viewModel.PenetrationCalculationHint);

			viewModel = new BufferPenetrationViewModel(Factory.New<BMNCNBufferShape>());
			AssertEquals("Items which affect the penetration of this buffer are listed below.", viewModel.PenetrationItemsHint);
		}
	}

	[TestedType(typeof(BufferPenetrationViewModel))]
	class BufferPenetrationViewModelNonPersistentBizoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new BufferPenetrationViewModel(Factory.New<BMNCNBufferShape>());
		}
	}
}
