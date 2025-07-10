using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeDescriptionOverrideRegistryDataType))]
	public class ElectronicProcessingChargeDescriptionOverrideRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ElectronicProcessingChargeDescriptionOverrideRegistryDataType>
	{
		protected override ElectronicProcessingChargeDescriptionOverrideRegistryDataType GetNewDataType()
		{
			return new ElectronicProcessingChargeDescriptionOverrideRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			var collection = new ElectronicProcessingChargeDescriptionOverrideCollection
			{
				new ElectronicProcessingChargeDescriptionOverride { Transport = "All", Container = "All", ShipmentType = "All", Origin = "SCC", Destination = "SCC", PrefixSuffix = "PRE", Text = "TEST1" },
			};

			var collection1 = new ElectronicProcessingChargeDescriptionOverrideCollection
			{
				new ElectronicProcessingChargeDescriptionOverride { Transport = "AIR", Container = "All", ShipmentType = "All", Origin = "SCC", Destination = "SCC", PrefixSuffix = "PRE", Text = "TEST2" },
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new ElectronicProcessingChargeDescriptionOverrideRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection1, new ElectronicProcessingChargeDescriptionOverrideRegistryDataType().Serialise(collection1))
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "ElectronicProcessingChargeDescriptionOverrideRegistryItemEditor"; }
		}
	}
}
