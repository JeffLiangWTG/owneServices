using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	[TestedType(typeof(DocumentAdditionalInformationProvider))]
	sealed class DocumentAdditionalInformationProviderTest : TestCase
	{
		public void TestDocumentType()
		{
			AssertEquals("Z270", provider.DocumentType);
		}

		public void TestDocumentComplementaryInformation()
		{
			AssertEquals("Info1", provider.RequestInformation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new DocumentAdditionalInformationProvider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.DocumentAdditionalInformationType
			{
				DocumentType = "Z270",
				DocumentComplementaryInformation = "Info1",
			});
		}

		DocumentAdditionalInformationProvider provider;
	}
}
