using CargoWise.Types;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InterfaceConnectorTemporarilyEnabledUntilRegistryDataType))]
	sealed class InterfaceConnectorTemporarilyEnabledUntilRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<InterfaceConnectorTemporarilyEnabledUntilRegistryDataType>
	{
		protected override string ExpectedEditorName => "InterfaceConnectorTemporarilyEnabledUntilRegistryItemEditor";

		protected override InterfaceConnectorTemporarilyEnabledUntilRegistryDataType GetNewDataType()
		{
			return new InterfaceConnectorTemporarilyEnabledUntilRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var result = new InterfaceConnectorTemporarilyEnabledUntil { EnabledUntil = ZDateTime.Now.AddDays(5).Date };
			var result2 = new InterfaceConnectorTemporarilyEnabledUntil { EnabledUntil = ZDateTime.Now.AddDays(6).Date };

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(result, new InterfaceConnectorTemporarilyEnabledUntilRegistryDataType().Serialise(result)),
				new ValidSampleAndBinaryValueInDB(result2, new InterfaceConnectorTemporarilyEnabledUntilRegistryDataType().Serialise(result2))
			};
		}
	}
}
