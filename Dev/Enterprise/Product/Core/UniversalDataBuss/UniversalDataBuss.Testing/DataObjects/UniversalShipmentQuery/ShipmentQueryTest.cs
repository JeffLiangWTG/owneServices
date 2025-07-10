using System.IO;
using CargoWise.Application;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(ShipmentRequest))]
	class ShipmentRequestTest : TopLevelDataObjectTestCase<ShipmentRequest>
	{
		public void TestEmptyObjectWritesFineThroughXmlWriter()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var shipmentRequest = new ShipmentRequest();

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					var xmlWriter = ObjectFactory.Get<IXmlWriter>();
					xmlWriter.WriteXML(shipmentRequest, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UberShipmentRequest", EmptyShipmentRequestXML.Trim(), result);
					}
				}
			}
		}

		#region EmptyShipmentRequestXML

		const string EmptyShipmentRequestXML = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
  </ShipmentRequest>
</UniversalShipmentRequest>
";
		#endregion
	}
}

