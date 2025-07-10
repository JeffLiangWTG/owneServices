using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1.Testing
{
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
			provider = new DocumentAdditionalInformationProvider(new DocumentAdditionalInformationType
			{
				DocumentType = "Z270",
				DocumentComplementaryInformation = "Info1",
			});
		}

		DocumentAdditionalInformationProvider provider;
	}
}
