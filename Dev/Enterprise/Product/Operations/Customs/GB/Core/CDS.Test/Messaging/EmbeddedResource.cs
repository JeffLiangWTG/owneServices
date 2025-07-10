using System.IO;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	static class EmbeddedResource
	{
		public static ZString GetExpectedMessageXml(ZString path)
		{
			var assembly = typeof(EmbeddedResource).Assembly;
			var foundPath = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith("." + path));
			using (var inStream = assembly.GetManifestResourceStream(foundPath))
			{
				return new StreamReader(inStream).ReadToEnd();
			}
		}
	}
}
