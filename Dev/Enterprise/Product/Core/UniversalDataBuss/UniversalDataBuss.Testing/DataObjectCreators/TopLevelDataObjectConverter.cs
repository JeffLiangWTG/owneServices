using System.IO;
using System.Text;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;

namespace Enterprise.UniversalDataBuss.Testing
{
	public static class TopLevelDataObjectConverter
	{
		public static Stream SerializeToStream(IDataObject dataObject)
		{
			var stream = new CargoWise.IO.Shim.SubStreamableStream();
			new XmlWriter().WriteXML(dataObject, stream);
			return stream;
		}

		public static T Deserialize<T>(string xmlString)
			where T : TopLevelDataObject, new()
		{
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes(xmlString)))
			{
				return stream.Parse<T>();
			}
		}
	}
}
