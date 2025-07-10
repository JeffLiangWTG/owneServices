using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(SystemProductRegistryDataType))]
	class SystemProductRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SystemProductRegistryDataType>
	{
		protected override bool HasEditor => false;

		protected override SystemProductRegistryDataType GetNewDataType()
		{
			return new SystemProductRegistryDataType(new SystemProductCollection());
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new SystemProductCollection();
			var item = collection.AddNew();
			item.Code = "AU";
			item.Description = "Australia";

			var collection2 = new SystemProductCollection();
			var item2 = collection2.AddNew();
			item2.Code = "NZ";
			item2.Description = "New Zealand";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new SystemProductRegistryDataType(new SystemProductCollection()).Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new SystemProductRegistryDataType(new SystemProductCollection()).Serialise(collection2))
			};
		}
	}
}
