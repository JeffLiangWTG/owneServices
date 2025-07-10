using Enterprise.Customs.GB.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	class CuscarFcsConsignmentSplitter
	{
		public CuscarFcsConsignmentSplitter(EDIMessage inboundEdiMessageForAuditing, ILogger serviceLogger, CuscarWithFlagsToShowWhatsSet flagableCuscar)
		{
			this.inboundMsg = inboundEdiMessageForAuditing;
			this.serviceLogger = serviceLogger;
			this.flagableCuscar = flagableCuscar;
		}

		internal bool SplitConsignment()
		{
			inboundMsg.EM_MessageSubType = CcsukTransmissionMessageFunction.CUSCAR.FCS.Subcode;
			var awb = CuscarParserAndProcessor.GetExistingAwbFromInboundCuscarReferenceNumbers(flagableCuscar, inboundMsg.Factory);
			if (awb != null)
			{
				inboundMsg.EM_Status = EDIMessage.Status.Received;
				serviceLogger.Log(LogType.Information, delegate
				{ return string.Format("Inbound FCS message #{1}, found consignment {0}, about to split it", awb.ReferenceNumber, inboundMsg.EM_MessageNum); });
				awb.Split(flagableCuscar.LinesOnlyForFcsResponses);
				SetInterpretationForSplit(awb);
			}
			else
			{
				inboundMsg.EM_Status = EDIMessage.Status.Failed;
				serviceLogger.Log(LogType.Warning, delegate
				{ return string.Format("Inbound FCS message #{0}, could not find consignment, unable to split. Message text starts: {1}", inboundMsg.EM_MessageNum, inboundMsg.EM_MessageText.SubstringSafe(0, 100)); });
			}
			return true;  // we understood the message even if it talked of unknown consignments
		}

		void SetInterpretationForSplit(ICcsukCusAwb awb)
		{
			awb.Messages.Add(inboundMsg);
			inboundMsg.EM_GB = awb.Branch.PK;
			inboundMsg.EM_MessageInterpretation = string.Format("{0} <h3>Record {1} split using community request</h3> {2}", MessagePrettierCss.CSS, awb.ReferenceNumber, flagableCuscar.WriteLinesForInterpretation());
		}

		readonly EDIMessage inboundMsg;
		readonly ILogger serviceLogger;
		readonly CuscarWithFlagsToShowWhatsSet flagableCuscar;
	}
}
