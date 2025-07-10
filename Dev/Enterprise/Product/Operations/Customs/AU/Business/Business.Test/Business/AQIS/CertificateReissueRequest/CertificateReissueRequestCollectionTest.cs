using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CertificateReissueRequestCollection))]
	sealed class CertificateReissueRequestCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CertificateReissueRequestCollection>
	{
		public void TestCreateNew()
		{
			var certificates = new CodeDescriptionPairList();
			certificates.AddPair("AU11111");
			certificates.AddPair("AU22222");
			certificates.AddPair("AU33333");
			var collection = new CertificateReissueRequestCollection(Factory, certificates);

			var request = collection.AddNew();
			AssertEquals("AU11111, AU22222, AU33333", request.Certificates.CodesAsString);
		}

		protected override CertificateReissueRequestCollection GetCollectionToTest() => new CertificateReissueRequestCollection(Factory, new CodeDescriptionPairList());

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CertificateReissueRequest(Factory, new CodeDescriptionPairList());
	}
}
