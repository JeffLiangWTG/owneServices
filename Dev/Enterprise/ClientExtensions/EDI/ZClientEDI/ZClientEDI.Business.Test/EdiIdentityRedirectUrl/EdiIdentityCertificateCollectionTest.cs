using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;

namespace ZClientEDI.Business.Test.EdiIdentityRedirectUrl
{
	class EdiIdentityCertificateCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiIdentityCertificateCollection>
	{
		protected override EdiIdentityCertificateCollection GetCollectionToTest()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			return new EdiIdentityCertificateCollection(Factory, application);
		}
	}
}
