using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService.Testing;

sealed class Eur1RequestContextTest : TestCaseWithFactory
{
	public void TestServiceId()
	{
		IDocumentManagementServiceRequestContext context = new Eur1RequestContext(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew(), new GlbCertificateProvider());
		AssertEquals(Ucc6XmlConstants.DocumentManagementServiceRequest.ServiceIds.Eur1Request , context.ServiceId);
	}
}
