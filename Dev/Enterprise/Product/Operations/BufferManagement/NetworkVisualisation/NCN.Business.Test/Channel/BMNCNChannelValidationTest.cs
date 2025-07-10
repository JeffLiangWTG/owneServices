using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class BMNCNChannelValidationTest : BusinessObjectValidationTestCase
	{
		public void TestName_WithDuplicateValues_ShouldHaveError()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel1 = diagram.Channels.AddNew();
			var channel2 = diagram.Channels.AddNew();

			channel1.BNL_Name = "Shmlonathan";
			channel2.BNL_Name = "Shmlonathan";
			channel1.BNL_Sequence = 1;
			channel2.BNL_Sequence = 2;

			AssertHasError(channel1.BNL_NameInfo, "Channel names must be unique for each diagram. Please enter a unique name.");
			AssertHasError(channel2.BNL_NameInfo, "Channel names must be unique for each diagram. Please enter a unique name.");

			channel2.BNL_Name = "Shmlangela";

			AssertNoErrors(channel1.BNL_NameInfo);
			AssertNoErrors(channel2.BNL_NameInfo);
		}

		public void TestSequence_WithDuplicateValues_ShouldHaveError()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel1 = diagram.Channels.AddNew();
			var channel2 = diagram.Channels.AddNew();

			channel1.BNL_Name = "Shmlonathan";
			channel2.BNL_Name = "Shmlangela";
			channel1.BNL_Sequence = 1;
			channel2.BNL_Sequence = 1;

			AssertHasError(channel1.BNL_SequenceInfo, "Each channel must have a unique sequence greater than 0. Please enter a unique sequence.");
			AssertHasError(channel2.BNL_SequenceInfo, "Each channel must have a unique sequence greater than 0. Please enter a unique sequence.");

			channel2.BNL_Sequence = 2;

			AssertNoErrors(channel1.BNL_SequenceInfo);
			AssertNoErrors(channel2.BNL_SequenceInfo);
		}

		public void TestSequence_WhenLessThanOrEqualToZero_ShouldHaveError()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel = diagram.Channels.AddNew();
			channel.BNL_Name = "Shalala";

			channel.BNL_Sequence = 0;
			AssertHasError(channel.BNL_SequenceInfo, "Please enter a 'Sequence' greater than 0.");

			channel.BNL_Sequence = -1;
			AssertHasError(channel.BNL_SequenceInfo, "Please enter a 'Sequence' greater than 0.");

			channel.BNL_Sequence = 1;
			AssertNoErrors(channel.BNL_SequenceInfo);
		}

		public void TestName_WhenEmpty_ShouldHaveError()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel = diagram.Channels.AddNew();

			channel.BNL_Name = ZString.Empty;
			AssertHasError(channel.BNL_NameInfo, "Please enter a Name.");

			channel.BNL_Name = "Jan Michael Vincent";
			AssertNoErrors(channel.BNL_SequenceInfo);
		}

		public void TestHeight_WhenLessThan100_ShouldHaveError()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel = diagram.Channels.AddNew();

			channel.BNL_Height = 99;
			AssertHasError(channel.BNL_HeightInfo, "Please enter a 'Height' greater than or equal to 100.");

			channel.BNL_Height = 100;
			AssertNoErrors(channel.BNL_HeightInfo);
		}

		public void TestHeight_WhenGreaterThan2000_ShouldHaveError()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel = diagram.Channels.AddNew();

			channel.BNL_Height = 2001;
			AssertHasError(channel.BNL_HeightInfo, "Please enter a 'Height' less than or equal to 2000.");

			channel.BNL_Height = 2000;
			AssertNoErrors(channel.BNL_HeightInfo);
		}

		public void TestHeight_ShapeHeightExceedsChannelHeight_ShouldHaveError()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var shape1 = NetworkTestCase.CreateShapeAtLocation(workflow1, diagram, 0, 0, 100, 300, "Daddy Bear");
			var shape2 = NetworkTestCase.CreateShapeAtLocation(workflow2, diagram, 150, 0, 100, 200, "Mummy Bear");
			var shape3 = NetworkTestCase.CreateShapeAtLocation(workflow3, diagram, 300, 0, 100, 100, "Bebe Bear");

			var channel = diagram.ShapeAsRootDiagram.Channels.AddNew();

			channel.BNL_Height = 200;
			AssertHasError(channel.BNL_HeightInfo, "The specified height is too small to contain all of the shapes within this Channel, since the tallest shape has a height of 300.");

			channel.BNL_Height = 300;
			AssertNoErrors(channel.BNL_HeightInfo);
		}

		public void TestHeight_ShapeHeightExceedsChannelHeight_MultipleChannels_ShouldHaveError()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var shape1 = NetworkTestCase.CreateShapeAtLocation(workflow1, diagram, 0, 0, 100, 100, "Daddy Bear");
			var shape2 = NetworkTestCase.CreateShapeAtLocation(workflow2, diagram, 150, 200, 100, 500, "Mummy Bear");
			var shape3 = NetworkTestCase.CreateShapeAtLocation(workflow3, diagram, 300, 500, 100, 100, "Bebe Bear");
			var shape4 = NetworkTestCase.CreateShapeAtLocation(workflow3, diagram, 450, 500, 100, 600, "Yogi Bear");

			var channel1 = NetworkTestCase.CreateChannel(diagram.ShapeAsRootDiagram, "Porridge", height: 200);
			var channel2 = NetworkTestCase.CreateChannel(diagram.ShapeAsRootDiagram, "Chair", height: 200);
			var channel3 = NetworkTestCase.CreateChannel(diagram.ShapeAsRootDiagram, "Bed", height: 200);

			channel2.BNL_Height = 300;
			channel1.Validation.ValidateBNL_Height();
			channel3.Validation.ValidateBNL_Height();

			AssertNoErrors(channel1.BNL_HeightInfo);
			AssertHasError(channel2.BNL_HeightInfo, "The specified height is too small to contain all of the shapes within this Channel, since the tallest shape has a height of 500.");
			AssertHasError(channel3.BNL_HeightInfo, "The specified height is too small to contain all of the shapes within this Channel, since the tallest shape has a height of 600.");

			channel2.BNL_Height = 500;
			channel1.Validation.ValidateBNL_Height();
			channel3.Validation.ValidateBNL_Height();

			AssertNoErrors(channel1.BNL_HeightInfo);
			AssertHasError(channel2.BNL_HeightInfo, "The specified height is too small to contain all of the shapes within this Channel, since the tallest shape has a height of 600.");
			AssertHasError(channel3.BNL_HeightInfo, "The specified height is too small to contain all of the shapes within this Channel, since the tallest shape has a height of 600.");

			channel2.BNL_Height = 600;
			channel1.Validation.ValidateBNL_Height();
			channel3.Validation.ValidateBNL_Height();

			AssertNoErrors(channel1.BNL_HeightInfo);
			AssertNoErrors(channel2.BNL_HeightInfo);
			AssertHasError(channel3.BNL_HeightInfo, "The specified height is too small to contain all of the shapes within this Channel, since the tallest shape has a height of 600.");

			channel2.BNL_Height = 1200;
			channel1.Validation.ValidateBNL_Height();
			channel3.Validation.ValidateBNL_Height();

			AssertNoErrors(channel1.BNL_HeightInfo);
			AssertNoErrors(channel2.BNL_HeightInfo);
			AssertNoErrors(channel3.BNL_HeightInfo);
		}
	}
}
