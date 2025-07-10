using CargoWise.IO;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;

namespace Enterprise.Client.Wow
{
	public abstract class WowDocumentSupportTest : DocumentSupporterTest
	{
		protected override string GetClientXmlFilePath() => documentsFilePath;

		protected override void SetUp()
		{
			base.SetUp();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
			documentsFilePath = embeddedResourceRetriever.SaveResourceToFile("WOWDocuments.xml");
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
			embeddedResourceRetriever = null;
		}

		string documentsFilePath;
		EmbeddedResourceRetriever embeddedResourceRetriever;
	}
}
