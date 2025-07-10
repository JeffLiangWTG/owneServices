using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(EventsRegistryDataType))]
	class EventsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EventsRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "EventsRegistryItemEditor"; }
		}
		protected override EventsRegistryDataType GetNewDataType()
		{
			return new EventsRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			EventRegistryBusinessObjectCollection collection = new EventRegistryBusinessObjectCollection();
			EventRegistryBusinessObject bizObj = collection.AddNew();
			bizObj.Code = Events.CustomsEntryStatus.Code;
			bizObj.Reference = "WOF";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection))
			};
		}
	}
}
