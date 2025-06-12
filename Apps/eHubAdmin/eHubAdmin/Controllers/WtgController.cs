using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace eServices.eHubAdmin.Controllers
{
	[Authorize]
    public class WtgController : ApiController
    {
        [HttpGet]
		[HttpHead]
		[AllowAnonymous]
		[Route("wtg/status")]
        public HttpResponseMessage Status()
        {
            var documentText = new HealthCheck.eHubAdminHealthCheckHttpTaskAsyncHandler().GetDocumentText().Result;
            var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            httpResponseMessage.Content = new StringContent(documentText, Encoding.UTF8, "text/plain");

            return httpResponseMessage;
        }
    }
}
