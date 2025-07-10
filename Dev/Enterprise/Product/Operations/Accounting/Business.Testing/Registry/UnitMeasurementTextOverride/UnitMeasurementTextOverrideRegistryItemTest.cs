using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(UnitMeasurementTextOverrideRegistryItem))]
	public class UnitMeasurementTextOverrideRegistryItemTest : StronglyTypedRegistryItemTestCase<UnitMeasurementTextOverrideCollection>
	{
		protected override StronglyTypedRegistryItem<UnitMeasurementTextOverrideCollection, UnitMeasurementTextOverrideCollection> GetNewRegistryItem()
		{
			return new UnitMeasurementTextOverrideRegistryItem("", null, null, null, RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
		}
	}

	[TestedType(typeof(UnitMeasurementTextOverrideRegistryDataType))]
	public class UnitMeasurementTextOverrideRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<UnitMeasurementTextOverrideRegistryDataType>
	{
		#region Implementation

		protected override UnitMeasurementTextOverrideRegistryDataType GetNewDataType() => new UnitMeasurementTextOverrideRegistryDataType();

		protected override string ExpectedEditorName => "UnitMeasurementTextOverrideRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var validcollection = new UnitMeasurementTextOverrideCollection();
			var validSample = validcollection.AddNew();
			validSample.UnitMeasurement = "KG";
			validSample.TextOverride = "KG";

			var validcollectionTwo = new UnitMeasurementTextOverrideCollection();
			var validSampleTwo = validcollectionTwo.AddNew();
			validSampleTwo.UnitMeasurement = "CF";
			validSampleTwo.TextOverride = "1";
			return new[] { new ValidSampleAndBinaryValueInDB(validcollection, GetNewDataType().Serialise(validcollection)),
			new ValidSampleAndBinaryValueInDB(validcollectionTwo, GetNewDataType().Serialise(validcollectionTwo)) };
		}

		#endregion
	}
}
