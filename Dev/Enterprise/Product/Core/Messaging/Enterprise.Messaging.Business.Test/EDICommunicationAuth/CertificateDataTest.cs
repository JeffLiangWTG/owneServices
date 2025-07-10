using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business.EDICommunicationAuthInbound;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	[TestedAsNonPersistentBusinessObject]
	[TestedType(typeof(CertificateData))]
	public class CertificateDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCertificateDataTest_Success()
		{
			var obj = new CertificateData();
			obj.CommonName = "Test";
			AssertEquals("Test", obj.CommonName);
		}
	}
}
