using System.Collections.Generic;
using System.Linq;
using CargoWise.EventReference;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Common.AU
{
	public static class PRAMessageTypeConstants
	{
		public enum MessageType { Submit, Cancel, ReSubmit }
	}

	public static class PRAMessageEvent
	{
		const string PRAMessageType = "PRA";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant")]
		const string PRAWithdrawCancelRequest = "PRA Cancellation";
		const string OneStop = "1-stop";

		public static KeyValuePair<string, string>[] GetEventParameters(Event eventToCheck)
		{
			var parameters = new Dictionary<string, string>();
			parameters[Constants.EventReferenceParameters.Codes.MessageType] = Equals(eventToCheck, Events.MessageWithdrawCancelRequest)
				? PRAWithdrawCancelRequest
				: PRAMessageType;
			parameters[Constants.EventReferenceParameters.Codes.Department] = OneStop;

			return parameters.ToArray();
		}
	}
}
