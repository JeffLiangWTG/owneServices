using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing;

class EDIProjectTest : TestCaseWithFactory
{
	public void TestHTMLLink()
	{
		Enterprise.ProcessManagement.Business.Project project = Factory.NewWithValidTestData<EDIProject>();
		project.WKP_ProjectNumber = "PRJ00005123";
		IARInvoiceSavingNotificationSubscriber subscriber = (IARInvoiceSavingNotificationSubscriber)project;
		ZString expectedString = "<a href=\"" + ShowEditFormUrlHandler.Instance.CreateWithoutApplicationContext(ControllerIDs.Project, project.PK) + "\">" + project.WKP_ProjectNumber + "</a>";
		AssertEquals("Project link", expectedString, subscriber.HTMLLinkForDirectOpen);
	}
}
