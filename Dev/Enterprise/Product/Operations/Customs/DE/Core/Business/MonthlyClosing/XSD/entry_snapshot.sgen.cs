using System;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	[CargoWise.Customs.Shared.MessageDefinitions.EmbeddedResource("Enterprise.Customs.DE.Business.MonthlyClosing.XSD.cusrecon_snapshot.xsd")]
	[System.Xml.Serialization.XmlSerializerAssembly("Enterprise.Customs.DE.Business.XmlSerializers")]
	public partial class DEMonthlyClosingEntrySnapshot : IMonthlyClosingXmlObject
	{
		public DEMonthlyClosingEntrySnapshot()
		{
			Document = Array.Empty<DEMonthlyClosingEntrySnapshotDocument>();
		}
	}
}
