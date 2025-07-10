using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class RNSMessagingBO : NonPersistentBusinessObject, IRNSRequest, IObsoleteValidation
	{
		public RNSMessagingBO(IRNSPlugInSupport plugInSupport)
			: base(plugInSupport.Master.Factory)
		{
			PlugInSupport = plugInSupport;
			originalReleaseStatus = GetReleaseStatus();

			if (plugInSupport.Master is IManifestProvider)
			{
				((IManifestProvider)plugInSupport.Master).Messages.CountChanged += MasterMessagesCountChanged;
			}
		}

		static class Schema
		{
			public const string RN_ReleaseDate = "RN_ReleaseDate";
			public const string RN_ReleaseStatus = "RN_ReleaseStatus";

			public const string RN_LoginUser = "RN_LoginUser";
			public const string RN_EntryDate = "RN_EntryDate";

			public const string ArrivalCertificationStatus = "ArrivalCertificationStatus";
			public const string ArrivalCertificationDate = "ArrivalCertificationDate";
		}

		public ManualReleaseCancelBO ManualReleaseCancelBo
		{
			get
			{
				if (manualRelease == null)
				{
					manualRelease = new CachedProperty<ManualReleaseCancelBO>(Factory, delegate
					{
						var shipment = this.PlugInSupport.Master as Integration.Customs.CA.IManualReleaseSupport;
						return shipment == null ? null : new ManualReleaseCancelBO(shipment.ManualReleaseNoteText, Factory);
					});
				}

				return manualRelease.Value;
			}
		}
		CachedProperty<ManualReleaseCancelBO> manualRelease;

		#region RN_EntryDate

		[ResourceStringData("RNSMessagingBO|RN_EntryDate", Caption = "Entry Date")]
		public ZDateTime RN_EntryDate
		{
			get
			{
				var manualReleaseBo = ManualReleaseCancelBo;
				if (manualReleaseBo != null && manualReleaseBo.ManualReleaseSystemDate.IsValid)
				{
					return manualReleaseBo.ManualReleaseSystemDate;
				}
				else
				{
					return ZDateTime.Empty;
				}
			}
		}

		public ZPropertyInfo RN_EntryDateInfo
		{
			get { return GetZPropertyInfo(Schema.RN_EntryDate); }
		}

		#endregion

		#region RN_LoginUser

		[ResourceStringData("RNSMessagingBO|RN_LoginUser", Caption = "Login User")]
		public ZString RN_LoginUser
		{
			get
			{
				var manualReleaseBo = ManualReleaseCancelBo;
				if (manualReleaseBo != null && !manualReleaseBo.ManualReleaseUser.IsEmpty)
				{
					return manualReleaseBo.ManualReleaseUser;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo RN_LoginUserInfo
		{
			get { return GetZPropertyInfo(Schema.RN_LoginUser); }
		}
		#endregion

		#region RN_ReleaseDate

		[ResourceStringData("RNSMessagingBO|RN_ReleaseDate", Caption = "Release Date")]
		public ZDateTime RN_ReleaseDate
		{
			get
			{
				var manualReleaseBo = ManualReleaseCancelBo;
				if (manualReleaseBo != null && manualReleaseBo.ManualReleaseDate.IsValid)
				{
					return manualReleaseBo.ManualReleaseDate;
				}
				else
				{
					return ReleaseUpdate != null ? ReleaseUpdate.RL_ReleaseDate : ZDateTime.Empty;
				}
			}
		}

		public ZPropertyInfo RN_ReleaseDateInfo
		{
			get { return GetZPropertyInfo(Schema.RN_ReleaseDate); }
		}

		#endregion

		#region RN_ReleaseStatus

		[ResourceStringData("RNSMessagingBO|RN_ReleaseStatus", Caption = "Release Status")]
		public ZString RN_ReleaseStatus
		{
			get
			{
				var manualReleaseBo = ManualReleaseCancelBo;
				if (manualReleaseBo != null && !manualReleaseBo.ManualReleaseReason.IsEmpty)
				{
					return manualReleaseBo.ManualReleaseReason;
				}
				else
				{
					return ReleaseUpdate != null
						? ReleaseUpdate.ProcessingIndicatorCodeDescription.ToString()
						: !RNSOnlyMessages.Any()
							? new MessageStatusList().GetDescriptionFromCode(MessageStatusList.Codes.NotSent)
							: new MessageStatusList(RNSOnlyMessages.Last().MultilingualMessageSubTypeDescription)
								.GetDescriptionFromCode(MessageStatusList.Codes.AwaitingOriginal);
				}
			}
		}

		public ZPropertyInfo RN_ReleaseStatusInfo
		{
			get { return GetZPropertyInfo(Schema.RN_ReleaseStatus); }
		}

		#endregion

		#region ArrivalCertificationStatus

		[ResourceStringData("RNSMessagingBO|ArrivalCertificationStatus", Caption = "Arrival Certification Status")]
		public ZString ArrivalCertificationStatus
		{
			get
			{
				return RecentArrivalCertificationMessage != null ?
					new EDIMessageStatusList().GetDescriptionFromCode(RecentArrivalCertificationMessage.EM_Status) : new MessageStatusList().GetDescriptionFromCode(MessageStatusList.Codes.NotSent);
			}
		}

		public ZPropertyInfo ArrivalCertificationStatusInfo
		{
			get { return GetZPropertyInfo(Schema.ArrivalCertificationStatus); }
		}

		public RNSRequestMessage RecentArrivalCertificationMessage
		{
			get
			{
				if (recentArrivalCertificationMessage == null)
				{
					recentArrivalCertificationMessage = RNSRequestMessage.GetRecentArrivalCertificationMessage(this.Messages);
				}

				return recentArrivalCertificationMessage;
			}
		}

		RNSRequestMessage recentArrivalCertificationMessage;

		#endregion

		#region ArrivalCertificationDate

		[ResourceStringData("RNSMessagingBO|ArrivalCertificationDate", Caption = "Arrival Certification Date")]
		public ZDateTime ArrivalCertificationDate
		{
			get
			{
				return RecentArrivalCertificationMessage != null && RecentArrivalCertificationMessage.EM_Status == MessageStatusList.Codes.Sent ?
					RecentArrivalCertificationMessage.EM_MessageDateTime : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo ArrivalCertificationDateInfo
		{
			get { return GetZPropertyInfo(Schema.ArrivalCertificationDate); }
		}

		#endregion

		#region Implementation of IEDIFACTMessageAttachee

		void IEDIFACTMessageAttachee.AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			((IManifestProvider)PlugInSupport.Master).Messages.Add(message);
		}

		Enterprise.Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages
		{
			get { return Messages; }
		}

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get
			{
				var lastMessage = RNSOnlyMessages.LastOrDefault();
				return lastMessage != null && lastMessage.IsTransmitMessage
								? MessageStatusList.Codes.AwaitingOriginal : string.Empty;
			}
			set { } //Not applicable, MessageStatus is calculated
		}

		ZString IEDIFACTMessageAttachee.JobStatus //Not applicable for Consol and Shipment
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString IEDIFACTMessageAttachee.JobIdentification
		{
			get { throw new NotSupportedException(); }
		}

		bool IEDIFACTMessageAttachee.HasChanges
		{
			get { return PlugInSupport.Master.HasChanges; }
		}

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject
		{
			get { return PlugInSupport.Master; }
		}

		bool ICAEDIFACTMessageAttachee.IsCancelled
		{
			get { return false; }
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage
		{
			get { return true; }
		}

		#endregion

		#region Implementation of IRNSRequestData

		ZString IRNSRequestData.HouseBillNumber
		{
			get { return PlugInSupport.HouseBillNumber; }
		}

		ZDateTime IRNSRequestData.DateOfArrival
		{
			get { return PlugInSupport.DateOfArrival; }
		}

		ZString IRNSRequestData.CargoControlNumber
		{
			get { return PlugInSupport.CargoControlNumber; }
		}

		ZString IRNSRequestData.TransactionNumber
		{
			get { return PlugInSupport.TransactionNumber; }
		}

		ZString IRNSRequestData.OfficeCode
		{
			get { return PlugInSupport.OfficeCode; }
		}

		ZString IRNSRequestData.SubLocationCode
		{
			get { return PlugInSupport.SubLocationCode; }
		}

		#endregion

		#region Release Status Logs

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			AddOrCancelCustomsClearedLogIfRequired();
		}

		void AddOrCancelCustomsClearedLogIfRequired()
		{
			if (PlugInSupport.ReleaseStatusEventsSupported)
			{
				var releaseStatus = GetReleaseStatus();
				var logs = PlugInSupport.Logs;
				if (!IsStatusClear(originalReleaseStatus) && IsStatusClear(releaseStatus))
				{
					StatusLogManager.AddCustomsClearedEvent(logs, RN_ReleaseStatus, RN_ReleaseDate.ToOffset());
				}
				else if (IsStatusClear(originalReleaseStatus) && releaseStatus == EDIReleaseImportEntryStatusList.Codes.Cancelled)
				{
					StatusLogManager.CancelCustomsClearedEvent(logs);
				}
				originalReleaseStatus = releaseStatus;
			}
		}

		string GetReleaseStatus()
		{
			return ReleaseUpdate != null ? ReleaseUpdate.RL_ReleaseStatus : ZString.Empty;
		}

		static bool IsStatusClear(string status)
		{
			return status == EDIReleaseImportEntryStatusList.Codes.GoodsReleased
					 || status == EDIReleaseImportEntryStatusList.Codes.Y51ReleaseDocumentsRequired;
		}

		string originalReleaseStatus;

		#endregion

		#region ReleaseUpdate

		public static ZString[] ReleaseSubTypesToIgnore
		{
			get
			{
				return new ZString[] { EDIReleaseImportEntryStatusList.Codes.Error, EDIReleaseImportEntryStatusList.Codes.SyntaxError, EDIReleaseImportEntryStatusList.Codes.MessageContentRejected };
			}
		}

		ReleaseStatus ReleaseUpdate
		{
			get
			{
				if (releaseUpdate == null)
				{
					var lastStatusUpdate = EDIReleaseMessage.GetLastReleaseStatusMessage(Messages, ReleaseSubTypesToIgnore);
					if (lastStatusUpdate != null)
					{
						releaseUpdate = new ReleaseStatus(lastStatusUpdate);
					}
				}
				return releaseUpdate;
			}
		}

		ReleaseStatus releaseUpdate;

		#endregion

		#region Messages

		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					var parent = PlugInSupport.Master;
					var messageTypes = new[] { MessageTypeList.Codes.RNSRequest, MessageTypeList.Codes.EDIRelease, MessageTypeList.Codes.ACIHouseBill };
					var query = new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, messageTypes);
					messages = new EDIMessageCollection(parent, query);
					messages.Sort(EDIMessageSchema.EM_SystemCreateTimeUtc.Name);
					messages.Load();
					parent.RegisterEditableChildObject(messages);
					messages.CountChanged += (s, e) => { releaseUpdate = null; recentArrivalCertificationMessage = null; rnsOnlyMessages = null; };
					needReloadMessages = false;
				}

				if (needReloadMessages)
				{
					messages.Load();
					needReloadMessages = false;
				}

				return messages;
			}
		}
		EDIMessageCollection messages;
		bool needReloadMessages;

		IOrderedEnumerable<EDIMessage> RNSOnlyMessages
		{
			get
			{
				return rnsOnlyMessages ?? (rnsOnlyMessages = Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType != MessageTypeList.Codes.ACIHouseBill).OrderBy(x => x, new EDIMessageComparer(ListSortDirection.Ascending)));
			}
		}
		IOrderedEnumerable<EDIMessage> rnsOnlyMessages;

		public EDIMessageForDisplayCollection<EDIMessage> MessagesForDisplay
		{
			get
			{
				if (messagesForDisplay == null)
				{
					var query = new ZQuery();
					query.AddToFilter(EDIMessageQueryHelper.GetEDIMessageGenPivotQuery(new[] { PlugInSupport.Master.PK }, new ZString[] { EDIMessageSubTypeList.Codes.XmlUniversalEvent, UniversalEventMessageTypes.Codes.IIDResponses, UniversalEventMessageTypes.Codes.D4Notices }));
					query.AddToFilter(Messages.CompleteFilter, JoinCondition.Or);
					messagesForDisplay = new EDIMessageForDisplayCollection<EDIMessage>(Factory, query);
					messagesForDisplay.Load();
					messagesForDisplay.SetReadOnlyIncludingChildren(true);
				}
				return messagesForDisplay;
			}
		}
		EDIMessageForDisplayCollection<EDIMessage> messagesForDisplay;

		public EDIMessage LatestNoticeMessage
		{
			get
			{
				return MessagesForDisplay.Where(m => m.EM_ReceiveTransmit == EDIMessage.Direction.Receive && m.RNSProcessingDate.IsValid && (m is UniversalEventMessage || m.EM_MessageType == MessageTypeList.Codes.ACIHouseBill))
						.CollectMaxBy(m => m.RNSProcessingDate)
						.CollectMaxBy(m => m.EDIFACTInterchangeNumber)
						.CollectMaxBy(m => m.EDIFACTMessageNumber).FirstOrDefault();
			}
		}

		#endregion

		#region MasterMessagesCountChanged

		void MasterMessagesCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			needReloadMessages = true;
			rnsOnlyMessages = null;
			releaseUpdate = null;
			recentArrivalCertificationMessage = null;

			this.RefreshBinding();
		}

		#endregion

		public IRNSPlugInSupport PlugInSupport { get; private set; }

		#region IRNSRequest

		void IRNSRequest.RefreshMessagesForDisplay()
		{
			MessagesForDisplay.Reload(true);
		}

		#endregion
	}
}
