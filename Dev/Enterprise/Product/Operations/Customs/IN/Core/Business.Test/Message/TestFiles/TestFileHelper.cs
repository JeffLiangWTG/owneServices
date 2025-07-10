using CargoWise.IO;

namespace Enterprise.Customs.IN.Business.Testing
{
	static class TestFileHelper
	{
		public static byte[] GetBytesFromEmbeddedResource(string fileName)
		{
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var embeddedResourcePath = "Enterprise.Customs.IN.Business.Testing.Message.TestFiles." + fileName;
				return embeddedResourceRetriever.GetBytes(embeddedResourcePath);
			}
		}
	}
}
