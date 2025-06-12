using System;
using CargoWise.eHub.Gateway.OpenAPIs.XHGateway;

namespace CargoWise.eHub.Gateway
{
	public class XHGatewayExceptionHandler
	{
		static internal void ThrowSystemUnderMaintananceExceptionIfApplicable(XHGatewayException ex)
		{
			var statusCode = ex.StatusCode;

			if (statusCode == 406 && ex.Message.Contains("Message is rejected by xT Server")) return;

			if (statusCode >= 400 && statusCode <= 499 || statusCode == 500 || statusCode == 503)
			{
				throw new SystemUnderMaintananceException($"{DefaultMessage}{Environment.NewLine}{ex.Message}", ex);
			}
		}

		public const string DefaultMessage = "xHub Gateway is currently down for maintenance. Messages will be resubmitted to xT automatically on next service task run. You do not need to take action unless the server remains offline for an extended period of time.";
	}
}