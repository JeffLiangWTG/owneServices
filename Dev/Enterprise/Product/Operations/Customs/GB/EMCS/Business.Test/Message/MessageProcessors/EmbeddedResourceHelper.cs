using System.IO;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	static class EmbeddedResourceHelper
	{
		public static ZString GetdMessageXml(ZString path)
		{
			var assembly = typeof(EmbeddedResourceHelper).Assembly;
			var foundPath = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith("." + path));
			using (var inStream = assembly.GetManifestResourceStream(foundPath))
			{
				return new StreamReader(inStream).ReadToEnd();
			}
		}
	}
}
