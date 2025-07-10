using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class ContrlParserAndProcessor_PresenceOnNetworkHelper
	{
		internal void UpdateJobPresenceOnNetwork(EDIMessage outgoingMessage, ZString capitalisedErrorMessage)
		{
			var hawbMessageOwner = outgoingMessage.EM_LinkedObject as CusHAWB;
			if (hawbMessageOwner != null)
			{
				if (IsFRI(outgoingMessage))
				{
					if (capitalisedErrorMessage.Contains("RECORD ALREADY EXISTS"))
					{
						hawbMessageOwner.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
					}
					else if (capitalisedErrorMessage.Contains("MASTER NOT YET CREATED") && !hawbMessageOwner.CS_IsMasterHouse)  // try to make house before master, denied, know that the hosue cannot yet exist on network
					{
						hawbMessageOwner.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDb;
					}
					else if (capitalisedErrorMessage.Contains("DB-CLP MESSAGE PARSE FAILED")
							|| capitalisedErrorMessage.Contains("REJECTED"))
					{
						if (hawbMessageOwner.Messages.FirstOutgoingMessage == outgoingMessage   // is response to the very first FRI message
							&& hawbMessageOwner.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.OnCommDb)
						{
							hawbMessageOwner.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDb;
						}
						else
						{
							hawbMessageOwner.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck;
						}
					}
				}
				else if (IsCusdecRemovalRequest(outgoingMessage))
				{
					if (capitalisedErrorMessage.Contains("REJECTED - NOP IN MESSAGE NOT EQUAL TO NPX IN CONSIGNMENT") // the word "consignment" refers to one on the network
						||
						capitalisedErrorMessage.Contains("REJECTED - STATUS 3 ALREADY SET ON CONSIGNMENT"))  // Some kind of customs clearance/release exists
					{
						hawbMessageOwner.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
					}
				}
			}
		}

		bool IsFRI(EDIMessage ediMessage)
		{
			return ediMessage.EM_MessageType == CcsukTransmissionMessageFunction.CUSCAR.FRI.Code && ediMessage.EM_MessageSubType == CcsukTransmissionMessageFunction.CUSCAR.FRI.Subcode;
		}

		bool IsCusdecRemovalRequest(EDIMessage ediMessage)
		{
			return ediMessage.EM_MessageType == CcsukTransmissionMessageFunction.CUSDEC.Code;  //CDC
		}
	}
}
