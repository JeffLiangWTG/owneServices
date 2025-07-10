using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AddressListRegistryDataType))]
	sealed class AddressListRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AddressListRegistryDataType>
	{
		#region Implementation

		protected override AddressListRegistryDataType GetNewDataType()
		{
			return new AddressListRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "AddressListRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new AddressListCollection();
			var addressList = collection.AddNew();

			addressList.AddressType = "BuyerDocumentaryAddress";
			addressList.ControllerName = "JobShipment";

			var collection2 = new AddressListCollection();
			var addressList2 = collection2.AddNew();

			addressList2.AddressType = "ConsignorPickupDeliveryAddress";
			addressList2.ControllerName = "JobShipment";

			#region ByteArrayValue

			var stream = new System.IO.MemoryStream();
			var stream2 = new System.IO.MemoryStream();
			new System.Xml.Serialization.XmlSerializer(typeof(AddressListCollection)).Serialize(stream, collection);
			new System.Xml.Serialization.XmlSerializer(typeof(AddressListCollection)).Serialize(stream2, collection2);
			var byteArrayValue = stream.ToArray();
			var byteArrayValue2 = stream2.ToArray();

			stream.Close();
			stream2.Close();

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue),
				new ValidSampleAndBinaryValueInDB(collection2, byteArrayValue2),
			};

			#endregion
		}

		#endregion
	}
}
