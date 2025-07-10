using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public static class ESH7TestFileReader
	{
		public static string GetEmbeddedFileText(string embeddedResourcePath, string fileName)
		{
			var fileReader = new TestFileReader(typeof(ESH7TestFileReader));
			return fileReader.GetEmbeddedFileText(embeddedResourcePath, fileName);
		}
	}
}
