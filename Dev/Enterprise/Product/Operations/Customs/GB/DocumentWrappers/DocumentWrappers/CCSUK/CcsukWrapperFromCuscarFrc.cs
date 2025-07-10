using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk
{
	public class CcsukWrapperFromCuscarFrc : CcsukWrapperFromFsn
	{
		public CcsukWrapperFromCuscarFrc(EDIMessage inboundFrcMessage, BusinessObjectFactory factoryToWrap)
			: base(inboundFrcMessage.GetInboundFsnEdiMessage(), factoryToWrap)
		{
		}
	}

	public static class CcsukWrapperFromCuscarFrcExtensionHelper
	{
		public static EDIMessage GetInboundFsnEdiMessage(this EDIMessage inboundFrcMessage)
		{
			var houseOrWorker = inboundFrcMessage.EM_LinkedObject as CusHAWB;
			if (houseOrWorker != null)
			{
				var result = (from EDIMessage m in houseOrWorker.Messages
							  where m.EM_ReceiveTransmit == EDIMessage.Direction.Receive
								  && m.EM_MessageType == CcsukTransmissionMessageFunction.CIM.Code
								  && (m.EM_ApplicationReference.IsEmpty || m.EM_ApplicationReference == inboundFrcMessage.EM_ApplicationReference)
								  && m.EM_MessageSubType == CcsukTransmissionMessageFunction.CIM.CUKFSR.FSN.Subcode
								  && m.EM_MessageText.Contains(GetCsnAndCacAndSplitNumber(m, inboundFrcMessage, houseOrWorker))
							  orderby m.EM_SystemCreateTimeUtc descending
							  select m).FirstOrDefault();
				return result;
			}
			return null;
		}

		static string GetCsnAndCacAndSplitNumber(EDIMessage candidateFsn, EDIMessage frc, CusHAWB houseOrWorker)
		{
			ICcsukCusAwb awbForThisFrc = null;
			if (houseOrWorker.CS_IsMasterHouse && houseOrWorker.MAWB.HasSplits && frc.EM_ApplicationReference.Length == 2)
			{
				awbForThisFrc = houseOrWorker.MAWB.Splits[frc.EM_ApplicationReference];
			}
			else if (!houseOrWorker.CS_IsMasterHouse && houseOrWorker.HasSplits && frc.EM_ApplicationReference.Length == 2)
			{
				awbForThisFrc = houseOrWorker.Splits[frc.EM_ApplicationReference];
			}
			else if (!houseOrWorker.CS_IsMasterHouse && !houseOrWorker.HasSplits)
			{
				awbForThisFrc = houseOrWorker;
			}
			else if (houseOrWorker.CS_IsMasterHouse && !houseOrWorker.HasSplits)
			{
				awbForThisFrc = houseOrWorker.MAWB;
			}

			var segmentIdAndCac = "CSN/" + awbForThisFrc.CustomsActionCode;
			var result = candidateFsn.EM_ApplicationReference.Length == 2 ?
					segmentIdAndCac + "-" + candidateFsn.EM_ApplicationReference + "/"  //  e.g. CSN/CW-01/
					:
					segmentIdAndCac + "/";  //  e.g. CSN/CW/
			return result;
		}
	}
}
