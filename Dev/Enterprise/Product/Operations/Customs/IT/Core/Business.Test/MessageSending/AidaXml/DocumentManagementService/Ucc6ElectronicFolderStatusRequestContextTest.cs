using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService.Testing;

sealed class Ucc6ElectronicFolderStatusRequestContextTest : TestCaseWithFactory
{
	public void TestServiceId()
	{
		IDocumentManagementServiceRequestContext context = new Ucc6ElectronicFolderStatusRequestContext(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew(), new GlbCertificateProvider());
		AssertEquals(Ucc6XmlConstants.DocumentManagementServiceRequest.ServiceIds.EFStatusRequest, context.ServiceId);
	}
}
