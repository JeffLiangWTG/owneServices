using System.Xml;
using System.Xml.Serialization;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business.Testing
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.Test.XmlSerializers")]
	sealed class PurgeSettingsWithAllHistoricalElementsForTest : PurgeSettings
	{
		protected override void WriteElements(XmlWriter writer)
		{
			var collectionSerialiser = ZXmlSerializer.New(typeof(ApplicationCodeObjCollection));
			collectionSerialiser.Serialize(writer, new ApplicationCodeObjCollection());
			writer.WriteElementString("BillingMonths", "BillingMonthsValue");
			collectionSerialiser.Serialize(writer, new ApplicationCodeObjCollection());
		}
	}
}
