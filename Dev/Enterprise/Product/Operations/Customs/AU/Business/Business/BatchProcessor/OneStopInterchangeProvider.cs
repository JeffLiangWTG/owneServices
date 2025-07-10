
using CargoWise.Types;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class OneStopInterchangeProvider : OneInterchangeToOneMessageInterchangeProvider
	{
		public OneStopInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages)
		{
		}

		#region Overrides

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			ZString messageProcessingParty = (messages[0].EM_MessageSubType.Left(1)) == "X" ? "CSX" : "1STOP";
			ZString testOrLive = (messages[0].EM_IsTestMessage) ? "TEST" : "LIVE";

			ZString eDIInterchangeReceiverID = messageProcessingParty + testOrLive;
			ZString uNBHeaderReceiverID = (messageProcessingParty == "CSX") ? "CSXWTADL" : "1STOP";
			ZString uNBHeaderSenderID = (messageProcessingParty == "CSX" && messages[0].EM_IsTestMessage) ? "EDIALT" : "EDIAL";

			SetInterchangeValuesForTransmit(interchange, messages, messages[0].EM_MessageType, eDIInterchangeReceiverID, uNBHeaderSenderID);
			ZString interchangeOwner = messages[0].EM_MessageOwner;
			interchange.EI_HeaderText = "UNA:+.? '" + GetUNB(PreparedTime.ToZDateTime(), uNBHeaderReceiverID, ZString.Empty, uNBHeaderSenderID, ZString.Empty, interchangeOwner, "UNOC", "3", ZString.Empty, false, messages[0].EM_IsTestMessage, ZString.Empty).ToString(new UNOCCharacterSet());
		}

		protected override UNCharacterSet CurrentUNCharacterSet
		{
			get { return new UNOCCMRCharacterSet(); }
		}

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get { return "Please contact Cargowise Support to get a new Registration File applied to your system. There is not enough information in your current system registration to generate a 1-Stop Site ID."; }
		}
		#endregion
	}
}
