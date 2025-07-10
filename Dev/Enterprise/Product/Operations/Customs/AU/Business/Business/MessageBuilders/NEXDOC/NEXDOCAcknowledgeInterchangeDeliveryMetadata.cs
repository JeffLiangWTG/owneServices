using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Enterprise.Customs.AU.Declaration.Business;

[Serializable]
public class NEXDOCAcknowledgeInterchangeDeliveryMetadata
{
	[XmlArrayItem("Value", IsNullable = false)]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public NEXDOCInterchangeDeliveryMetadataValue[] ValueCollection { get; set; }
}
