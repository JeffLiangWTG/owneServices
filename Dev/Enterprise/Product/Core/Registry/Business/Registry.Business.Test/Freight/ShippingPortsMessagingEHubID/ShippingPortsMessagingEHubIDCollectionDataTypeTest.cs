using System.Text;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ShippingPortsMessagingEHubIDCollectionDataType))]
	public class ShippingPortsMessagingEHubIDCollectionDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ShippingPortsMessagingEHubIDCollectionDataType>
	{
		protected override ShippingPortsMessagingEHubIDCollectionDataType GetNewDataType()
		{
			return new ShippingPortsMessagingEHubIDCollectionDataType(new ShippingPortsMessagingEHubIDCollection());
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = ShippingPortsMessagingEHubIDCollection.NewWithDefaultValues();
			var xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<ArrayOfShippingPortsMessagingEHubID xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
					"<ShippingPortsMessagingEHubID><Port>ESBCN</Port><Module>xHub</Module><RecipientID>SHIPPING_PORT_MESSAGING</RecipientID></ShippingPortsMessagingEHubID>" +
					"<ShippingPortsMessagingEHubID><Port>ESGAN</Port><Module>xHub</Module><RecipientID>SHIPPING_PORT_MESSAGING</RecipientID></ShippingPortsMessagingEHubID>" +
					"<ShippingPortsMessagingEHubID><Port>ESPDS</Port><Module>xHub</Module><RecipientID>SHIPPING_PORT_MESSAGING</RecipientID></ShippingPortsMessagingEHubID>" +
					"<ShippingPortsMessagingEHubID><Port>ESVLC</Port><Module>xHub</Module><RecipientID>SHIPPING_PORT_MESSAGING</RecipientID></ShippingPortsMessagingEHubID>" +
					"<ShippingPortsMessagingEHubID><Port /><Module>eHub</Module><RecipientID>ShippingPortMessaging</RecipientID></ShippingPortsMessagingEHubID>" +
				"</ArrayOfShippingPortsMessagingEHubID>";

			var collection2 = new ShippingPortsMessagingEHubIDCollection();
			var xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfShippingPortsMessagingEHubID xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" />";

			var collection3 = new ShippingPortsMessagingEHubIDCollection();
			var shippingPortsMessagingEHubID = collection3.AddNew();
			shippingPortsMessagingEHubID.Port = "ESBCN";
			shippingPortsMessagingEHubID.Module = ModuleTypes.Codes.eHub;
			shippingPortsMessagingEHubID.RecipientID = "ForwardingConsol";
			var xml3 = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<ArrayOfShippingPortsMessagingEHubID xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
					"<ShippingPortsMessagingEHubID><Port>ESBCN</Port><Module>eHub</Module><RecipientID>ForwardingConsol</RecipientID></ShippingPortsMessagingEHubID>" +
				"</ArrayOfShippingPortsMessagingEHubID>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, Encoding.Unicode.GetBytes(xml1)),
				new ValidSampleAndBinaryValueInDB(collection2, Encoding.Unicode.GetBytes(xml2)),
				new ValidSampleAndBinaryValueInDB(collection3, Encoding.Unicode.GetBytes(xml3))
			};
		}

		protected override string ExpectedEditorName => "ShippingPortsMessagingEHubIDRegistryItemEditor";
	}
}
