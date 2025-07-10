using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DbHealthWarningListRegistryDataType))]
	sealed class DbHealthWarningListRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DbHealthWarningListRegistryDataType>
	{
		protected override string ExpectedEditorName => "DbHealthWarningListRegistryItemEditor";
		protected override DbHealthWarningListRegistryDataType GetNewDataType()
		{
			return new DbHealthWarningListRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new DbHealthWarningRegistryCollection();
			var registryWarning = new DbHealthWarningRegistryElement("Source", "Type", "Description", true);
			collection.Add(registryWarning);

			var collection2 = new DbHealthWarningRegistryCollection();
			var registryWarning2 = new DbHealthWarningRegistryElement("Source 2", "Type 2", "Description 2", true);
			collection2.Add(registryWarning2);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new DbHealthWarningListRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new DbHealthWarningListRegistryDataType().Serialise(collection2))
			};
		}
	}
}
