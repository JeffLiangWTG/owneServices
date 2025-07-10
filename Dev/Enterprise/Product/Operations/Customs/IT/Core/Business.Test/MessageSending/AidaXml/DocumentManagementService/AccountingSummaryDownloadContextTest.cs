using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService.Testing;

sealed class AccountingSummaryDownloadContextTest : TestCaseWithFactory
{
	public void TestServiceId()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		IDocumentManagementServiceRequestContext context = new AccountingSummaryDownloadContext(entryHeader, new GlbCertificateProvider());
		AssertEquals("ServiceId", "downloadProspettoContabile", context.ServiceId);
	}
}
