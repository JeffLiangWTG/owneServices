using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService.Testing;

sealed class ReleaseProspectusRequestContextTest : TestCaseWithFactory
{
	public void TestServiceId()
	{
		IDocumentManagementServiceRequestContext context = new ReleaseProspectusRequestContext(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew(), new GlbCertificateProvider());
		AssertEquals(Ucc6XmlConstants.DocumentManagementServiceRequest.ServiceIds.ReleaseProspectusRequest, context.ServiceId);
	}
}
