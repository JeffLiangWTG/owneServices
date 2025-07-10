using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.Testing
{
	public class UniversalResponse<T>
			where T : TopLevelDataObject, new()
	{
		public UniversalResponse(string universalResponseXml)
		{
			var xmlResponse = System.Xml.Linq.XDocument.Parse(universalResponseXml);
			var universalResponse = xmlResponse.Document.Elements().FirstOrDefault(e => e.Name.LocalName == "UniversalResponse");

			Status = universalResponse.Descendants().Where(e => e.Name.LocalName == "Status")?.FirstOrDefault()?.Value;
			ProcessingLog = universalResponse.Descendants().Where(e => e.Name.LocalName == "ProcessingLog")?.FirstOrDefault()?.Value;
			var data = universalResponse.Descendants().Where(ur => ur.Name.LocalName == "Data")?.FirstOrDefault()?.FirstNode?.ToString() ?? "";
			ResponseDataObject = TopLevelDataObjectConverter.Deserialize<T>(data);
		}

		public ZString Status { get; }
		public ZString ProcessingLog { get; }
		public T ResponseDataObject { get; }
	}
}
