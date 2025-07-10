using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHMasterCloseWrapper : IACIForwarderCloseProvider
	{
		public CusCAeMHMasterCloseWrapper(CusCAeMHMaster cusCAeMHMaster)
		{
			this.cusCAeMHMaster = cusCAeMHMaster;
			this.allCCNs = new List<HouseCCNInstruction>();
			foreach (var houseBill in cusCAeMHMaster.HouseBills)
			{
				var houseCCN = houseBill.BW_HouseCCN;
				if (!houseCCN.IsEmpty)
				{
					this.allCCNs.Add(new HouseCCNInstruction { CCN = houseCCN, IsShouldSend = false });
				}
			}
		}

		IACIForwarderMessageProvider BaseProvider
		{
			get { return cusCAeMHMaster; }
		}

		readonly CusCAeMHMaster cusCAeMHMaster;

		public ZString PreviousCCN
		{
			get
			{
				var masterHouseCCN = cusCAeMHMaster.BP_MasterHouseCCN;
				return masterHouseCCN.IsEmpty ? cusCAeMHMaster.BP_PrimaryCCN : masterHouseCCN;
			}
		}

		public ZString PreviousCCNForWithdraw
		{
			get
			{
				var lastAcceptedMessage = LastAcceptedMessage;
				return lastAcceptedMessage != null ? lastAcceptedMessage.BGMReference.SubstringSafe(4) : PreviousCCN;
			}
		}

		ACIForwarderCloseMessage LastAcceptedMessage
		{
			get
			{
				return cusCAeMHMaster.Messages
					.GetMatchingMessages(EDIMessage.ApplicationCodes.CAACI, new ZString[] { MessageTypeList.Codes.ACIForwarderClose }, EDIMessage.Direction.Receive, true, ListSortDirection.Descending)
					.Cast<ACIForwarderCloseMessage>().FirstOrDefault(message => message.EM_MessageSubType != MessageSubTypeCodes.Codes.Cancellation && new EManifestResponseWrapper(message).IsAccepted);
			}
		}

		public ZString CarrierCode
		{
			get { return cusCAeMHMaster.BP_CBSACarrierCode; }
		}

		public ZString CarrierCodeForWithdraw
		{
			get
			{
				var lastAcceptedMessage = LastAcceptedMessage;
				return lastAcceptedMessage != null ? lastAcceptedMessage.BGMReference.SubstringSafe(0, 4) : cusCAeMHMaster.BP_CBSACarrierCode;
			}
		}

		readonly List<HouseCCNInstruction> allCCNs;
		public List<HouseCCNInstruction> AllCCNs
		{
			get { return allCCNs; }
		}

		public IEnumerable<ZString> RelatedCCNs
		{
			get
			{
				return allCCNs.Where(inst => inst.IsShouldSend).Select(inst => inst.CCN);
			}
		}

		public void UpdateIsShouldSend(Func<CusCAeMHHouse, ZBool> isShouldSendDelegate)
		{
			AllCCNs.ForEach(inst => inst.IsShouldSend = cusCAeMHMaster.HouseBills.Any(houseBill => isShouldSendDelegate(houseBill) && houseBill.BW_HouseCCN == inst.CCN));
		}

		#region IACIForwarderMessageProvider implementations

		public bool IsPostArrival
		{
			get { return cusCAeMHMaster.IsPostArrival; }
		}

		public ZString AmendmentReason
		{
			get { return BaseProvider.AmendmentReason; }
		}

		public bool IsCancelled
		{
			get { return cusCAeMHMaster.IsCancelled; }
		}

		public void AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			BaseProvider.AddMessage(message);
		}

		public bool HasChanges
		{
			get { return cusCAeMHMaster.HasChanges; }
		}

		public ZString JobIdentification
		{
			get { return BaseProvider.JobIdentification; }
		}

		public ZString JobStatus
		{
			get
			{
				return BaseProvider.JobStatus;
			}
			set
			{
				BaseProvider.JobStatus = value;
			}
		}

		public ZString MessageStatus
		{
			get
			{
				return BaseProvider.MessageStatus;
			}
			set
			{
				BaseProvider.MessageStatus = value;
			}
		}

		public BusinessObject TopLevelBusinessObject
		{
			get { return BaseProvider.TopLevelBusinessObject; }
		}

		public BusinessObjectFactory Factory
		{
			get { return cusCAeMHMaster.Factory; }
		}

		public Enterprise.Messaging.Business.EDIMessageCollection Messages
		{
			get { return cusCAeMHMaster.Messages; }
		}

		public ZBool ReadyToClose => cusCAeMHMaster.ReadyToClose;

		public ZString AmendReasonCode => cusCAeMHMaster.BP_AmendReasonCode;

		public ZDateTime ATA => cusCAeMHMaster.BP_ATA;

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;

		#endregion

	}
}
