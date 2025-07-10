using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService.Testing;

sealed class SummaryProspectusRequestContextTest : TestCaseWithFactory
{
	public void TestServiceId()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		IDocumentManagementServiceRequestContext context = new SummaryProspectusRequestContext(entryHeader, new GlbCertificateProvider());
		AssertEquals("ServiceId", "richiestaProspettoSintesi", context.ServiceId);
	}
}
