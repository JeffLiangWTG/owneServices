using System.Xml.Serialization;

namespace CargoWise.eHub.Gateway.ITCustoms
{
	[XmlType(AnonymousType = true,
		Namespace = "http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse")]
	[XmlRoot(Namespace = "http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse",
		IsNullable = false)]
	public class Files
	{
		[XmlElement("File")]
		public File[] File { get; set; }
	}

	[XmlType(AnonymousType = true,
		Namespace = "http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse")]
	public class File
	{
		public string Name { get; set; }
	}
}