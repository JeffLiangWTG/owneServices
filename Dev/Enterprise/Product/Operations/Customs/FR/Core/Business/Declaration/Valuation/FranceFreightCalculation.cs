using System.Xml.Serialization;

namespace Enterprise.Customs.FR.Business.Declaration
{
	[XmlSerializerAssembly("Enterprise.Customs.FR.Business.XmlSerializers")]
	[XmlRoot(Namespace = "http://www.edi.com.au/IncoTermsConfiguration.xsd")]
	public class FranceFreightCalculation
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[XmlElement]
		public Mapping[] Mapping { get; set; }
	}

	public partial class Mapping
	{
		[XmlElement]
		public Results Results { get; set; }
	}

	public partial class Filter
	{
		[XmlElement]
		public string Direction { get; set; }
	}

	public class Results
	{
		[XmlElement]
		public Result CumulTiers { get; set; }

		[XmlElement]
		public Result CumulAerienTiers { get; set; }

		[XmlElement]
		public Result CumulCEHorsFRInclus { get; set; }

		[XmlElement]
		public Result CumulCEHorsFRExclus { get; set; }

		[XmlElement]
		public Result CumulAerienFR { get; set; }

		[XmlElement]
		public Result CumulFRInclus { get; set; }

		[XmlElement]
		public Result CumulFRExclus { get; set; }
	}

	public class Result
	{
		[XmlElement]
		public Cost Freight { get; set; }

		[XmlElement]
		public Cost Insurance { get; set; }
	}

	public class Cost
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[XmlElement]
		public Charge Charge { get; set; }
	}
}
