using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.ZArchitecture.Modules;

namespace ZClientEDI.GUI.Test.UrlHandlers
{
	class ShowFormUrlHandlerEDITest : TestCaseWithFactory
	{
		public void TestShowFormUrlHandler_ShouldShowIncidentForm()
		{
			// see also TestShouldNotHandleIncidentRequests_WhenNotInEDIExtension in Enterprise.ZArchitecture for the counterpart test
			AssertEquals("Precondition: in EDI extension", Clients.EDI, ClientHookLoader.Instance?.Client);

			var incident = Factory.NewWithValidTestData<SupportIncident>();

			Factory.Save();

			AssertNotNull("Precondition", incident.Request);
			var url = $"edient:Command=ShowEditForm&ControllerID=IncidentRequest&BusinessEntityPK={incident.Request.PK}";

			AssertNoExceptionThrown(() => EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, waitForAppToStart: false));
			Application.DoEvents();
			var openedForm = Application.OpenForms.OfType<SupportIncidentForm>().SingleOrDefault();
			AssertNotNull("Should open the incident form", openedForm);
			AssertEquals(incident.PK, openedForm.BusinessEntity.PK);
			openedForm.Dispose();
		}
	}
}
