using System.Text;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(WorkflowConfigurationStepCollectionRegistryDataType))]
	sealed class WorkflowConfigurationStepCollectionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<WorkflowConfigurationStepCollectionRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "WorkflowConfigurationStepCollectionRegistryItemEditor"; }
		}

		protected override WorkflowConfigurationStepCollectionRegistryDataType GetNewDataType()
		{
			return new WorkflowConfigurationStepCollectionRegistryDataType(WorkflowConfigurationStepTest.GetCodesProviderForTesting());
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new WorkflowConfigurationStepCollection(WorkflowConfigurationStepTest.GetCodesProviderForTesting());
			collection.AddNew().Code = SharedConstants.DataProcessingType.Code.OrganizationMatching;
			collection.AddNew().Code = SharedConstants.DataProcessingType.Code.ProductCodeMatching;
			collection.AddNew().Code = SharedConstants.DataProcessingType.Code.NotifyDownstreamServices;

			var xml =
				@"<?xml version=""1.0"" encoding=""utf-16""?>
					<ArrayOfWorkflowConfigurationStep>
						<WorkflowConfigurationStep>
							<Code>ORM</Code>
						</WorkflowConfigurationStep>
						<WorkflowConfigurationStep>
							<Code>PCM</Code>
						</WorkflowConfigurationStep>
						<WorkflowConfigurationStep>
							<Code>NDS</Code>
						</WorkflowConfigurationStep>
					</ArrayOfWorkflowConfigurationStep>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, Encoding.Unicode.GetBytes(xml))
			};
		}
	}
}
