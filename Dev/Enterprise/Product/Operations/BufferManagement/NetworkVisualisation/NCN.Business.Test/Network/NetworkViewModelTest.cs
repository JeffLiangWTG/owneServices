using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.NetworkVisualisation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNetworkViewModel))]
	class NetworkViewModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDoesNotInitialiseWithJobHeader()
		{
			var diagram = Factory.NewWithValidTestData<BMNCNShape>();
			AssertNull(diagram.ProcessHeader);

			var viewModel = new BMNetworkViewModel(diagram);
			AssertNull(diagram.ProcessHeader);
		}

		public void TestDoesNotDisplayBuffersAccidentallyPlacedInNormalDiagram()
		{
			// due to some defects like WI00608931 a user may be able to place a buffer to a normal non-scaled diagram
			// we need to ensure the system does not generate exceptions for such a case and still displays these buffers

			NetworkTestCase.EnableBMSInRegistry();

			var diagram = NetworkTestCase.CreateDiagram(Factory, "Diagram");
			var shape = NetworkTestCase.CreateShape(diagram, "Shape");

			var buffer = Factory.NewWithValidTestData<BMNCNBufferShape>();
			buffer.BufferType = BufferTypeList.Codes.Project;
			buffer.BNS_Name = "Accidentally misplaced buffer";
			buffer.MakeChildOf(diagram);

			Factory.Save();

			NetworkViewModel networkViewModel;
			AssertNoExceptionThrown(() =>
			{
				networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram, nodeViewModelProvider: new JobNetworkNodeViewModelProvider());
			});

			AssertEquals("Somehow a buffer was incorrectly placed on a non-scaled diagram possibly because of a defect. A data transformation to remove incorrectly placed buffer may be required.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BMNetworkViewModel(VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory).GetDefaultDiagram());
		}
	}
}
