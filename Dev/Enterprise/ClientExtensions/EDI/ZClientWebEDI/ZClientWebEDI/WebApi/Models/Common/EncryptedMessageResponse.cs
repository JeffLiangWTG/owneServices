using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using WTG.TrustedMessaging;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class EncryptedMessageResponse : OkResult
	{
		public EncryptedMessageResponse(string message, EdiTrustedSystem trustedSystem, ApiController controller) : base(controller)
		{
			this.message = message;
			this.trustedSystem = trustedSystem;
		}
		readonly string message;
		readonly EdiTrustedSystem trustedSystem;

		public override async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var certProvider = ObjectFactory.New<ICertificatesProvider>(trustedSystem);
				var messenger = new TrustedMessenger(certProvider);
				var trustedMessage = messenger.CreateMessage(message, trustedSystem.ETS_SecretKey);

				var response = await base.ExecuteAsync(cancellationToken);
				response.Content = new StringContent(trustedMessage.EncryptedContent);
				response.Headers.Add("SIGNED", trustedMessage.Signature);
				response.Headers.Add("WTG_I", trustedMessage.IV);

				return response;
			}
		}
	}
}
