using System;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DataTransfer.Native.Utils.Policies;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery.LocalService
{
	public class NativeDataService : INativeDataService
	{
		public INativeServiceClient Client { get; set; }

		public IResponseMessage Update(IRequestMessage request)
		{
			IResponseMessage result;

			if (Client == null)
			{
				var response = new ResponseMessageData();
				response.HasError = true;
				response.ErrorMessage = ResString.GetMultilingualString("07d590ef-1748-44ab-94f2-872dd85012e9", "Fail to create Client application");
				return response;
			}

			try
			{
				var retryPolicy = new RetryPolicy<Exception>();
				retryPolicy.RetryCount = 3;

				result = retryPolicy.Do(() =>
				{
					return Client.Update(request);
				});
			}
			finally
			{
				Client.CloseOrAbort();
			}
			return result;
		}
	}
}
