using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;

public static class ESG5TestFileReader
{
	public static string GetEmbeddedFileText(string embeddedResourcePath, string fileName)
	{
		var fileReader = new TestFileReader(typeof(ESG5TestFileReader));
		return fileReader.GetEmbeddedFileText(embeddedResourcePath, fileName);
	}
}
