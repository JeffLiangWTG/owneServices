using System.IO;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Testing
{
	static class EmbeddedResourceHelper
	{
		public static TextReader GetdMessageXmlStream(ZString path)
		{
			var assembly = typeof(EmbeddedResourceHelper).Assembly;
			var foundPath = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith("." + path));
			var inStream = assembly.GetManifestResourceStream(foundPath);
			return new StreamReader(inStream);
		}
	}
}
