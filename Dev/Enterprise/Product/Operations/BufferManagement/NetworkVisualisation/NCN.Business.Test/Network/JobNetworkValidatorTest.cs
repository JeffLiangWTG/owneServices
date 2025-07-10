using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	public class JobNetworkValidatorTest : NetworkTestCase
	{
		#region Validate

		public void TestValidate_WhenThereAreNoErrors()
		{
			var diagram = CreateDiagram(Factory, "New Diagram");
			var validator = new JobNetworkValidator();
			var networkViewModel = CreateNetworkViewModel(diagram, validator: validator);
			var network = networkViewModel.GetJobNetwork();

			var shape = CreateShape(diagram);

			validator.Validate(network);
			Assert("There should be no validation error on shapes", !network.DiagramShape.HasErrors);
			Assert("There should be no validation error on shape entities", !network.DiagramEntity.HasErrors);
		}

		public void TestValidate_WhenThereAreErrorsOnShapes()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);

			var validator = new JobNetworkValidator();
			var networkViewModel = CreateNetworkViewModel(diagram, validator: validator);
			var network = networkViewModel.GetJobNetwork();

			var shape = CreateShape(diagram);

			network.ValidateAndCheckThereAreNoErrors();

			using (Factory.TemporarilyDisableValidation())
			{
				shape.BNS_Name = string.Empty;
			}
			AssertNoErrorContaining(shape.BNS_NameInfo, "Please enter a Name");

			validator.Validate(network);
			AssertHasErrorContaining(shape.BNS_NameInfo, "Please enter a Name");
		}

		public void TestValidate_WhenThereAreErrorsOnEntities()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);

			var validator = new JobNetworkValidator();
			var networkViewModel = CreateNetworkViewModel(diagram, validator: validator);
			var network = networkViewModel.GetJobNetwork();

			var shape1 = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);
			var entity1 = shape1.AsEntity(network);
			var shape2 = CreateShape(diagram, "Unique name for buffer entity", shapeType: ShapeTypeList.Codes.Buffer);

			network.ValidateAndCheckThereAreNoErrors();

			using (Factory.TemporarilyDisableValidation())
			{
				entity1.Name = "Unique name for buffer entity";
			}
			AssertNoErrorContaining(entity1.NameInfo, "There is another buffer with the same name in this diagram.");

			validator.Validate(network);
			AssertHasErrorContaining(entity1.NameInfo, "There is another buffer with the same name in this diagram.");
		}

		#endregion
	}
}
