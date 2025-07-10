using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.Tests.Business.DownloadHandler
{
	static class TestInterchangeMessage
	{
		internal static EDIInterchange GetNew(ZGuid branchPk, string from, string to, string receiveTransmit, BusinessObjectFactory factory, bool onlyCreateInterchange)
		{
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_GB = branchPk;
			interchange.EI_From = from;
			interchange.EI_To = to;
			interchange.EI_SessionGUID = interchange.PK;
			interchange.EI_ReceiveTransmit = receiveTransmit;

			if (!onlyCreateInterchange)
			{
				var message = interchange.ContainedMessages.AddNew();
				message.EM_ReceiveTransmit = receiveTransmit;
				message = interchange.ContainedMessages.AddNew();
				message.EM_ReceiveTransmit = receiveTransmit;
			}
			return interchange;
		}

		internal static void EditAs(this EDIInterchange interchange, string status, string receiveTransmit, BusinessObjectFactory factory)
		{
			interchange.EI_Status = status;
			interchange.EI_ReceiveTransmit = receiveTransmit;
			factory.Save();
		}

		internal static void EditAs(this EDIInterchange interchange, string status, string receiveTransmit, Guid sessionGuid, BusinessObjectFactory factory)
		{
			interchange.EI_Status = status;
			interchange.EI_ReceiveTransmit = receiveTransmit;
			interchange.EI_SessionGUID = sessionGuid;
			factory.Save();
		}
	}
}