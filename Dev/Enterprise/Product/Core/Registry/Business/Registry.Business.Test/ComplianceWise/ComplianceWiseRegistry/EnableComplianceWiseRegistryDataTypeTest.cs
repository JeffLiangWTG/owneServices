using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EnableComplianceWiseRegistryDataType))]
	sealed class EnableComplianceWiseRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EnableComplianceWiseRegistryDataType>
	{
		protected override EnableComplianceWiseRegistryDataType GetNewDataType() => new EnableComplianceWiseRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var registryBizO1 = ComplianceWiseRegistryHelper.SetValue(false);
			var registryBizO2 = ComplianceWiseRegistryHelper.SetValue(true);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(registryBizO1, DataType.Serialise(registryBizO1)),
				new ValidSampleAndBinaryValueInDB(registryBizO2, DataType.Serialise(registryBizO2)),
			};
		}

		protected override string ExpectedEditorName => "EnableComplianceWiseRegistryItemEditor";
	}
}
