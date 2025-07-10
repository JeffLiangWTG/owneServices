using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WorkflowValidationProcessTypeRegistryDataType))]
	sealed class WorkflowValidationProcessTypeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<WorkflowValidationProcessTypeRegistryDataType>
	{
		protected override WorkflowValidationProcessTypeRegistryDataType GetNewDataType()
		{
			return new WorkflowValidationProcessTypeRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var firstCollection = new WorkflowValidationProcessTypeCollection();
			var bizoA = firstCollection.AddNew();
			bizoA.ProcessType = "SHP";

			var secondCollection = new WorkflowValidationProcessTypeCollection();
			var bizoB = secondCollection.AddNew();
			bizoB.ProcessType = "BRK";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(firstCollection, new WorkflowValidationProcessTypeRegistryDataType().Serialise(firstCollection)),
				new ValidSampleAndBinaryValueInDB(secondCollection, new WorkflowValidationProcessTypeRegistryDataType().Serialise(secondCollection))
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "WorkflowValidationProcessTypeRegistryItemEditor"; }
		}
	}
}
