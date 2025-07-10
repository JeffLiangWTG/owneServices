using System.Xml.Serialization;

namespace Enterprise.Customs.FR.Business.Declaration
{
	[XmlSerializerAssembly("Enterprise.Customs.FR.Business.XmlSerializers")]
	[XmlRoot(Namespace = "http://www.edi.com.au/IncoTermsConfiguration.xsd")]
	public class IncoTermsConfiguration
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[XmlElement]
		public Mapping[] Mapping { get; set; }
	}

	public partial class Mapping
	{
		[XmlElement]
		public Filter Filter { get; set; }

		[XmlElement]
		public Charges Charges { get; set; }
	}

	public partial class Filter
	{
		[XmlElement]
		public string IncoTerm { get; set; }

		[XmlElement]
		public string AgreedPlace { get; set; }

		[XmlElement]
		public TransportModes TransportModes { get; set; }

		[XmlElement]
		public AirRouteTypes AirRouteTypes { get; set; }
	}

	public class TransportModes
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[XmlElement]
		public string[] TransportMode { get; set; }
	}

	public class AirRouteTypes
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[XmlElement]
		public string[] AirRouteType { get; set; }
	}

	public class Charges
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[XmlElement]
		public Charge[] Charge { get; set; }
	}

	public class Charge
	{
		[XmlAttribute]
		public string ChargeType { get; set; }

		[XmlAttribute]
		public bool IsIncludedInInvoice { get; set; }

		[XmlAttribute]
		public bool IsMandatory { get; set; }

		[XmlAttribute]
		public bool IsDutiable { get; set; }

		[XmlAttribute]
		public bool IsStatisticalValueApplicable { get; set; }

		[XmlAttribute]
		public bool IsGSTApplicable { get; set; }

		[XmlAttribute]
		public bool NoFilter { get; set; }

		[XmlAttribute]
		public bool GetThirdCountryCharges { get; set; }

		[XmlAttribute]
		public bool GetEUCharges { get; set; }

		[XmlAttribute]
		public bool GetDomesticCharges { get; set; }

		[XmlAttribute]
		public bool GetExportCharges { get; set; }
	}
}
