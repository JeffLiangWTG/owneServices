using System;
using System.Configuration;
using CargoWise.eHub.Common;

namespace CargoWise.eHub.Gateway
{
	public class NZCustomsTestMessageHandler : NZCustomsMessageHandler
	{
		protected override bool CheckSenderLicenceType(string senderID, out string failReason)
		{
			failReason = string.Empty;
			return true;
		}
	}
}