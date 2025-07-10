using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CA.Business
{
	public class CFSShipmentRNSStatusProvider : Freight.CFS.Business.CFSShipmentRNSStatusProvider, Integration.Customs.CA.ICFSShipmentRNSStatusProvider
	{
		#region Implementation

		public CFSShipmentRNSStatusProvider(CFSShipment shipment)
			: base(shipment)
		{
			shipment.Messages.CountChanged += Messages_CountChanged;
		}

		void Messages_CountChanged(object sender, CargoWise.EntityFramework.CollectionCountChangedEventArgs e)
		{
			lastestMessage = null;
			releaseUpdate = null;
			rnsOnlyMessages = null;
			recentArrivalCertificationMessage = null;
		}

		#endregion

		#region Properties

		EDIReleaseMessage RNSStatusMessage
		{
			get
			{
				if (lastestMessage == null)
				{
					lastestMessage = EDIReleaseMessage.GetLastReleaseStatusMessage(shipment.Messages, RNSMessagingBO.ReleaseSubTypesToIgnore);
				}

				return lastestMessage;
			}
		}

		EDIReleaseMessage lastestMessage;

		internal ReleaseStatus ReleaseUpdate
		{
			get
			{
				if (releaseUpdate == null)
				{
					if (RNSStatusMessage != null)
					{
						releaseUpdate = new ReleaseStatus(RNSStatusMessage);
					}
				}
				return releaseUpdate;
			}
		}

#if DEBUG
		protected
#endif
		ReleaseStatus releaseUpdate;

		IOrderedEnumerable<Enterprise.Messaging.Business.EDIMessage> RNSOnlyMessages
		{
			get
			{
				return rnsOnlyMessages ?? (rnsOnlyMessages = shipment.Messages.Cast<Enterprise.Messaging.Business.EDIMessage>()
					.Where(x => x.EM_MessageType == MessageTypeList.Codes.RNSRequest || x.EM_MessageType == MessageTypeList.Codes.EDIRelease)
					.OrderBy(x => x.EM_SystemCreateTimeUtc));
			}
		}

		IOrderedEnumerable<Enterprise.Messaging.Business.EDIMessage> rnsOnlyMessages;

		RNSRequestMessage RecentArrivalCertificationMessage
		{
			get
			{
				if (recentArrivalCertificationMessage == null)
				{
					recentArrivalCertificationMessage = RNSRequestMessage.GetRecentArrivalCertificationMessage(shipment.Messages);
				}

				return recentArrivalCertificationMessage;
			}
		}

		RNSRequestMessage recentArrivalCertificationMessage;

		public ZString MessageInterpretation
		{
			get
			{
				return RNSStatusMessage == null ? ZString.Empty : RNSStatusMessage.EM_MessageInterpretation;
			}
		}

		#endregion

		#region ICFSShipmentRNSStatusProvider

		protected override ZString TransactionNumberCore()
		{
			return RNSStatusMessage == null ? ZString.Empty : RNSStatusMessage.TransactionNumber;
		}

		#region RNS Release Status

		protected override ZDateTime ReleaseDateCore()
		{
			return ReleaseUpdate == null ? ZDateTime.Empty : ReleaseUpdate.RL_ReleaseDate;
		}

		protected override ZString ReleaseStatusCodeCore()
		{
			return ReleaseUpdate != null ? ReleaseUpdate.RL_ReleaseStatus.ToString()
				: !RNSOnlyMessages.Any() ? MessageStatusList.Codes.NotSent : MessageStatusList.Codes.AwaitingOriginal;
		}

		protected override ZString ReleaseStatusCore()
		{
			return ReleaseUpdate != null ? ReleaseUpdate.ProcessingIndicatorCodeDescription.ToString()
							: !RNSOnlyMessages.Any() ? new MessageStatusList().GetDescriptionFromCode(MessageStatusList.Codes.NotSent)
						: new MessageStatusList(((EDIMessage)RNSOnlyMessages.Last()).MultilingualMessageSubTypeDescription).GetDescriptionFromCode(MessageStatusList.Codes.AwaitingOriginal);
		}

		#endregion

		#region Arrival Certification Status

		protected override ZString ArrivalCertificationStatusCore()
		{
			return RecentArrivalCertificationMessage != null ?
				new EDIMessageStatusList().GetDescriptionFromCode(RecentArrivalCertificationMessage.EM_Status) : new MessageStatusList().GetDescriptionFromCode(MessageStatusList.Codes.NotSent);
		}

		protected override ZString ArrivalCertificationStatusCodeCore()
		{
			return RecentArrivalCertificationMessage != null ?
				RecentArrivalCertificationMessage.EM_Status.ToString() : MessageStatusList.Codes.NotSent;
		}

		protected override ZDateTime ArrivalCertificationDateCore()
		{
			return RecentArrivalCertificationMessage != null && RecentArrivalCertificationMessage.EM_Status == MessageStatusList.Codes.Sent ?
				RecentArrivalCertificationMessage.EM_MessageDateTime : ZDateTime.Empty;
		}

		#endregion

		#endregion
	}
}
