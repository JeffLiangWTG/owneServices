using CargoWise.IO;

namespace Enterprise.Client.EDI.IssueManager.Business.Test;

public static class SampleFileRetriever
{
	public static string GetFileContent(string fileName)
	{
		var resourceRetriever = new EmbeddedResourceRetriever(typeof(HelpErrorLogStackLineExtractorTest).Assembly);
		var sampleFilesPrefix = "ZClientEDI.Business.Test.IssueManager.SampleFiles.";
		return resourceRetriever.GetString(sampleFilesPrefix + fileName);
	}
}

