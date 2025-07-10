using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRCUSDECMessage : CMRMessage
	{
		public CMRCUSDECMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IsWithdrawalMessage
		public
#if DEBUG
		virtual
#endif
 bool IsWithdrawalMessage
		{
			get { return RevisionNumber == "50"; }
		}
		#endregion

		#region RevisionNumber

		protected string RevisionNumber
		{
			get
			{
				if (string.IsNullOrEmpty(fRevisionNumber))
				{
					if (CUSDEC != null && CUSDEC.BGM.Count > 0)
					{
						fRevisionNumber = CUSDEC.BGM[0].MessageFunctionCode.ToString();
					}
				}

				return fRevisionNumber;
			}
		}
		string fRevisionNumber = "";

		#endregion

		#region BranchIdentifier

		public string BranchIdentifier
		{
			get
			{
				if (fAgencyBranchID == null)
				{
					fAgencyBranchID = CUSDEC?.Group6?.Cast<SegmentGroup6>().SelectMany(g => g.NAD.Cast<NADSegment>())
						.FirstOrDefault(nad => nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.Branch)?.PartyIdentificationDetails.PartyIdentifier ?? string.Empty;
				}

				return fAgencyBranchID;
			}
		}
		string fAgencyBranchID;

		#endregion

		#region GetIncomingMessageByBGMRefAndVersion
		public EDIMessage GetSingleIncomingMessageByBGMRefVersionAndType(EDIMessageCollection messages, ZString messageType)
		{
			ZString sendersReferenceCached = BGMReference;
			ZInt sendersReferenceVersionCached = BGMReferenceVersion;
			foreach (EDIMessage message in messages)
			{
				CMRCUSRESMessage cMRMessage = message as CMRCUSRESMessage;
				if (cMRMessage != null && cMRMessage.EM_ReceiveTransmit == EDIMessage.Direction.Receive && cMRMessage.BGMMessageType == messageType && cMRMessage.SendersReference == sendersReferenceCached && cMRMessage.SendersReferenceVersion == sendersReferenceVersionCached)
				{
					return message;
				}
			}
			return null;
		}
		#endregion

		#region CUSDEC
		protected CUSDECMessage CUSDEC
		{
			get
			{
				if (fCUSDEC == null)
				{
					fCUSDEC = GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new Edifact.UNOCCMRCharacterSet()) as CUSDECMessage;
				}
				return fCUSDEC;
			}
		}
		CUSDECMessage fCUSDEC;
		#endregion
	}
}
