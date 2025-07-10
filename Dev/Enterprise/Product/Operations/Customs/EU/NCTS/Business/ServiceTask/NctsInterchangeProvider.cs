using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.EU.NCTS.Business.ServiceTask
{
	public class NctsInterchangeProvider : InterchangeProviderBase
	{
		public NctsInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages)
		{
		}

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get { return Res.GetString("26859884-7FA3-4691-82B0-B593D82DCCFE", "Country code and principal trader id - EORI or TIN or TURN"); }
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.EuNcts;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_To = "NCTS";

			foreach (EDIMessage message in messages)
			{
				message.EM_Status = EDIMessageStatusList.Codes.Pending;
				interchange.EI_From = message.EM_MessageOwner;
				interchange.ContainedMessages.Add(message);
			}

			var unb = GetUNB(ZDateTime.Now, "NCTS", "", interchange.EI_From, "", "", "UNOC", "3", "", true, false, "");
			interchange.EI_HeaderText = unb.ToString(new Edifact.UNOCCharacterSet());
			interchange.EI_BodyText = messages[0].EM_MessageText;
		}
	}
}
