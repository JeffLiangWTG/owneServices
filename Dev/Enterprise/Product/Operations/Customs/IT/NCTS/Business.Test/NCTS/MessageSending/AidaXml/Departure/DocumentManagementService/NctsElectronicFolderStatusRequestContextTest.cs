using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class NctsElectronicFolderStatusRequestContextTest : TestCaseWithFactory
{
	public void TestServiceId()
	{
		IDocumentManagementServiceRequestContext context = new NctsElectronicFolderStatusRequestContext(nctsHeader, new GlbCertificateProvider());
		AssertEquals(Ucc6XmlConstants.DocumentManagementServiceRequest.ServiceIds.EFStatusRequest, context.ServiceId);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
	}

	NctsHeader nctsHeader;
}
