using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ServiceLevelRegistryDataType))]
	public class ServiceLevelRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ServiceLevelRegistryDataType>
	{
		protected override ServiceLevelRegistryDataType GetNewDataType()
		{
			return new ServiceLevelRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			ServiceLevelRegistryBusinessObjectCollection collection = new ServiceLevelRegistryBusinessObjectCollection();
			ServiceLevelRegistryBusinessObject setup = collection.AddNew();

			setup.ServiceLevel = "STD";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection))
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "ServiceLevelRegistryItemEditor"; }
		}
	}
}
