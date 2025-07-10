using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeConfigurationRegistryDataType))]
	public class ElectronicProcessingChargeConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ElectronicProcessingChargeConfigurationRegistryDataType>
	{
		protected override ElectronicProcessingChargeConfigurationRegistryDataType GetNewDataType()
		{
			return new ElectronicProcessingChargeConfigurationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var testObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			var collection = new ElectronicProcessingChargeConfigurationCollection
			{
				new ElectronicProcessingChargeConfiguration { JobType = "SHP", StartDate = ZDate.Today, EndDate = ZDate.Today.AddDays(1) }
			};

			var collection1 = new ElectronicProcessingChargeConfigurationCollection
			{
				new ElectronicProcessingChargeConfiguration { JobType = "BRK", StartDate = ZDate.Today, EndDate = ZDate.Today.AddDays(1) }
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new ElectronicProcessingChargeConfigurationRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection1, new ElectronicProcessingChargeConfigurationRegistryDataType().Serialise(collection1))
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "ElectronicProcessingChargeConfigurationRegistryItemEditor"; }
		}
	}
}
