using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	public class ED801DocumentCertProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED801DocumentCertProvider(null));
		}

		public void TestDescription()
		{
			document.DocumentDescription = "Document Description 01";
			AssertEquals("Document Description 01", eD801DocumentCertProvider.Description);
		}

		public void TestReference()
		{
			document.DocumentReference = "ROD001";
			AssertEquals("ROD001", eD801DocumentCertProvider.Reference);
		}

		public void TestType()
		{
			document.DocumentType = "T01";
			AssertEquals("T01", eD801DocumentCertProvider.Type);
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = new ED801EBodyEadContainerDocumentCertificate();
			eD801DocumentCertProvider = new ED801DocumentCertProvider(document);
		}
		ED801EBodyEadContainerDocumentCertificate document;
		IEMCSDocumentCert eD801DocumentCertProvider;
	}
}
