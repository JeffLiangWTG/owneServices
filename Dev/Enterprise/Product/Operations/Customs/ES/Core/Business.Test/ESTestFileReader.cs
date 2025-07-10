using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.ES.Business.Testing;

public static class ESTestFileReader
{
	public static string GetEmbeddedFileText(string embeddedResourcePath, string fileName)
	{
		var fileReader = new TestFileReader(typeof(ESTestFileReader));
		return fileReader.GetEmbeddedFileText(embeddedResourcePath, fileName);
	}
}
