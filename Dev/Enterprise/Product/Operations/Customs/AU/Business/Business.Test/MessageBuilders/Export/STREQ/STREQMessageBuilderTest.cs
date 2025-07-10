using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class STREQMessageBuilderTest : CMRMessageBuilderAbstractTest
	{
		public void TestGenerateRequestStatusOnExportLine()
		{
			ZString expectedMessage = new EmbeddedResourceRetriever().GetString(GetEmbeddedResourcePath("StatusRequest.txt")).Replace("\r\n", "");
			STREQMessageBuilder builder = new STREQMessageBuilder(manifestLine);
			AssertEquals("MessageString", expectedMessage, builder.MessageText);
		}

		protected override CMRMessageBuilder GetMessageBuilderToTest() => new STREQMessageBuilder(manifestLine);

		protected override void SetUp()
		{
			base.SetUp();
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			manifestLine = header.Lines.AddNew();
			manifestLine.EL_CAN = "AAAACRREK";
		}

		ExportCustomsManifestLines manifestLine;

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.MessageBuilders.Export.STREQ.TestFiles." + fileName;
	}
}
