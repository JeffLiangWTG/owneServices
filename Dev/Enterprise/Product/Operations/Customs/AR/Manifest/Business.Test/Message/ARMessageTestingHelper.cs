using System.IO;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	public class ARMessageTestingHelper
	{
		public static ZString GetExpectedMessageXML(ZString path)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(path);

			return doc.OuterXml;
		}

		public string ReadManifestResourceContent(string resourceName)
		{
			using (var stream = GetType().Assembly.GetManifestResourceStream($"Enterprise.Customs.AR.Manifest.Business.Testing.Message.TestFiles.XT.{resourceName}"))
			{
				return new StreamReader(stream).ReadToEnd();
			}
		}
	}
}
