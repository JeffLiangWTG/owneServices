using System.Diagnostics;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.Connection.Testing
{
	/// <summary>
	/// This is a parser for inbound request messages, used ONLY by the TestProgramExe when it's pretending to be the ccsuk host
	/// </summary>
	public class RequestParser : ResponseParser
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Testing")]
		protected override Body LoadMessageFromTextCore(string bodyText)
		{
			Debug.WriteLine("Request parser: " + bodyText);
			ZString zBody = (ZString)bodyText;
			if (bodyText.StartsWith(HandShake.HandShakeIdentifier))
			{
				HandShakeRequest request = new HandShakeRequest("127.9.9.9");  // any old crap for test
				request.IpForCallbackDeclaredByParticipantTESTonly = zBody.Substring(30, 15).Trim();
				request.PortNumber = int.Parse(zBody.Substring(45, 5));
				return request;
			}
			else if (bodyText.StartsWith(ServiceMessage.ServiceMessageIdentifier + ShortMessageTypeCodes.Codes.LogonRequest))
			{
				return new LogonMessage(bodyText);
			}
			else if (bodyText.StartsWith(ServiceMessage.ServiceMessageIdentifier + ShortMessageTypeCodes.Codes.ConnectionPoll))
			{
				return new PollRequestMessage();
			}
			else if (bodyText.StartsWith(ServiceMessage.ServiceMessageIdentifier + ShortMessageTypeCodes.Codes.LogoffRequest))
			{
				return new LogoffMessage();
			}
			else if (bodyText.Length == 0)
			{
				return null;
			}
			else
			{
				return new CargoMessage(bodyText);
			}
		}
	}
}
