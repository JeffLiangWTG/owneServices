using System;
using Xware.Xt.Grpc.Application;

namespace Enterprise.xTMessaging.Shared
{
	public class BasicSubmitMsgAttributeModifier : ISubmitMsgAttributeModifier
	{
		public BasicSubmitMsgAttributeModifier(Func<string, string> receiverTransformer = null)
		{
			if (receiverTransformer != null)
			{
				_receiverTransformer = receiverTransformer;
			}
		}

		public BasicSubmitMsgAttributeModifier(string receiverValue) : this(_ => receiverValue)
		{
		}

		readonly Func<string, string> _receiverTransformer = ValidateEndpoint;

		public void AddParserFields(SubmitMsgMessage submitMsg)
		{
			submitMsg.Parserattr[(int)StdParserFieldId.PfReceiver] = _receiverTransformer(submitMsg.Msgattr[Constants.CustomMsgAttributes.DestinationParty]).ToUpper();
		}

		static string ValidateEndpoint(string endpoint)
		{
			if (string.IsNullOrEmpty(endpoint))
			{
				throw new ArgumentException("The Destination Party cannot be null or empty.");
			}

			return endpoint.Length switch
			{
				6 => endpoint,
				9 => $"{endpoint.Substring(0, 3)}{endpoint.Substring(6, 3)}",
				_ => throw new ArgumentException("The Destination Party should be 6 or 9 character long CargoWise register code.")
			};
		}
	}
}
