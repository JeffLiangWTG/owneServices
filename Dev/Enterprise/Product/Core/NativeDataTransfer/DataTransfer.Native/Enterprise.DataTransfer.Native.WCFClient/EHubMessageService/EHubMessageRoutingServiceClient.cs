using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.WCFClient.EHubMessageService
{
	/// <summary>
	/// Adapter for INativeServiceClient to EHubMessageRoutingServiceClient
	/// </summary>
	public partial class EHubMessageRoutingServiceClient : INativeServiceClient
	{
		public IConverter<IRequestMessage, MessageSendRequest> RequestConverter { get; set; }

		public IResponseMessage Update(IRequestMessage input)
		{
			var updateRequest = RequestConverter.Convert(input);

			if (updateRequest.MessageAction != "UpdateData")
			{
				return MessageSendResponse.InvalidResponse(Res.GetString("b9dc852b-45ad-4188-8285-215555a80dda", "Invalid Request for Update"));
			}
			return Send(updateRequest);
		}
	}
}
