using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;
using WTG.TrustedMessaging.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class InternalErrorWithErrorMessages : InternalServerErrorResult
	{
		public InternalErrorWithErrorMessages(ErrorMessages errorMessages, ApiController controller)
			: base(controller)
		{
			this.errorMessages = errorMessages;
		}
		readonly ErrorMessages errorMessages;

		public override async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			var response = await base.ExecuteAsync(cancellationToken);
			response.Content = new StringContent(JsonConvert.SerializeObject(errorMessages));
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			return response;
		}
	}
}
