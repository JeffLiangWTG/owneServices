using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(TransportAndChargeCodeMappingRegistryDataType))]
	class TransportAndChargeCodeMappingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TransportAndChargeCodeMappingRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "TransportAndChargeCodeMappingRegistryItemEditor"; }
		}

		protected override TransportAndChargeCodeMappingRegistryDataType GetNewDataType()
		{
			return new TransportAndChargeCodeMappingRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			TransportAndChargeCodeMappingRegistryBusinessObjectCollection collection = new TransportAndChargeCodeMappingRegistryBusinessObjectCollection();
			TransportAndChargeCodeMappingRegistryBusinessObject bizObj = collection.AddNew();
			bizObj.TransportModeCode = "SEA";
			bizObj.NominalCostCode = "BOB";

			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)) };
		}

		#region Factory
		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
		#endregion
	}
}
