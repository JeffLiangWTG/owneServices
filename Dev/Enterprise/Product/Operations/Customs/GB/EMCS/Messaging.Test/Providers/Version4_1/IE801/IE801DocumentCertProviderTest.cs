using System;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE801DocumentCertProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE801DocumentCertProvider(null));
		}

		public void TestDescription()
		{
			document.DocumentDescription = new LsdDocumentDescriptionType
			{
				Value = "Document Description 01",
				Language = "en",
			};
			AssertEquals("Document Description 01", ie801DocumentCertProvider.Description);
		}

		public void TestReference()
		{
			document.DocumentReference = "ROD001";
			AssertEquals("ROD001", ie801DocumentCertProvider.Reference);
		}

		public void TestType()
		{
			document.DocumentType = "T01";
			AssertEquals("T01", ie801DocumentCertProvider.Type);
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = new DocumentCertificateType();
			ie801DocumentCertProvider = new IE801DocumentCertProvider(document);
		}
		DocumentCertificateType document;
		IEMCSDocumentCert ie801DocumentCertProvider;
	}
}
