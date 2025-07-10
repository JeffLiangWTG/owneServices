using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[UniversalDataContext(DataContextType.REXNotification)]
	public class QuarantineNexDocNotification : AutoQuarantineNexDocNotification, ICodeDescription, INEXDOCResponse, INEXDOCMessageParent
	{
		#region Schema

		public new class Schema : AutoQuarantineNexDocNotification.Schema
		{
			public const string ObjectName = "ObjectName";
			public const string QN_ReceivedDate = "QN_ReceivedDate";
		}

		#endregion

		public QuarantineNexDocNotification(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var shortcutName = CalculateShortcutName();
				if (shortcutName.IsEmpty)
				{
					return ResString.GetMultilingualString("Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification|HumanReadableName", "NEXDOC Notification");
				}
				else
				{
					return ResString.GetMultilingualString("Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification|HumanReadableNameWithREX", "NEXDOC Notification - {0}", shortcutName);
				}
			}
		}

		string ICodeDescription.Code => QN_RexNumber;
		string ICodeDescription.Description => QN_ExporterReference;

		#region Overrides

		[ResourceStringData("Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification|QN_RexNumber", Caption = "REX Number")]
		public override ZString QN_RexNumber
		{
			get => base.QN_RexNumber;
			set => base.QN_RexNumber = value;
		}

		[ResourceStringData("Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification|QN_ExporterReference", Caption = "Exporter Reference")]
		public override ZString QN_ExporterReference
		{
			get => base.QN_ExporterReference;
			set => base.QN_ExporterReference = value;
		}

		[ResourceStringData("Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification|QN_NotificationType", Caption = "Notification Type")]
		public override ZString QN_NotificationType { get => base.QN_NotificationType; set => base.QN_NotificationType = value; }

		[ResourceStringData("Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification|QN_MessageStatus", Caption = "Message Status")]
		public override ZString QN_MessageStatus { get => base.QN_MessageStatus; set => base.QN_MessageStatus = value; }

		[ResourceStringData("Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification|QN_SystemCreateTimeUtc", Caption = "Created Date (UTC)")]
		public override ZDateTime QN_SystemCreateTimeUtc { get => base.QN_SystemCreateTimeUtc; set => base.QN_SystemCreateTimeUtc = value; }

		[ResourceStringData("Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification|QN_ReceivingExporterID", Caption = "Receiving Exporter ID")]
		public override ZString QN_ReceivingExporterID { get => base.QN_ReceivingExporterID; set => base.QN_ReceivingExporterID = value; }

		[ResourceStringData("Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification|QN_ForwardingGroupID", Caption = "Forwarding Group ID")]
		public override ZString QN_ForwardingGroupID { get => base.QN_ForwardingGroupID; set => base.QN_ForwardingGroupID = value; }

		[ResourceStringData("Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification|QN_TransferringExporterID", Caption = "Transferring Exporter ID")]
		public override ZString QN_TransferringExporterID { get => base.QN_TransferringExporterID; set => base.QN_TransferringExporterID = value; }

		[ResourceStringData("Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification|QN_AcknowledgeStatus", Caption = "Acknowledgement Status")]
		public override ZString QN_AcknowledgeStatus { get => base.QN_AcknowledgeStatus; set => base.QN_AcknowledgeStatus = value; }

		#endregion

		[ResourceStringData("Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification|QN_ReceivedDate", Caption = "Received Date")]
		public ZDateTime QN_ReceivedDate
		{
			get
			{
				if (fQN_ReceivedDate.IsEmpty && !QN_SystemCreateTimeUtc.IsEmpty)
				{
					fQN_ReceivedDate = Env.Time.GetLocalTimeFromUtc(QN_SystemCreateTimeUtc.ToDateTime());
				}
				return fQN_ReceivedDate;
			}
		}
		ZDateTime fQN_ReceivedDate;

		public ZPropertyInfo QN_ReceivedDateInfo
		{
			get { return GetZPropertyInfo(Schema.QN_ReceivedDate); }
		}

		[ResourceStringData("Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification|MessageStatusAndDescription", Caption = "Message Status")]
		public ZString MessageStatusAndDescription
		{
			get
			{
				var status = ZString.Empty;
				if (!QN_MessageStatus.IsEmpty)
				{
					status = QN_MessageStatus + " - " + Lookups.NEXDOCMessageStatusList.GetDescriptionFromCode(QN_MessageStatus);
				}

				return status;
			}
		}

		[ResourceStringData("Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification|AcknowledgeStatusAndDescription", Caption = "Acknowledgement Status")]
		public ZString AcknowledgeStatusAndDescription
		{
			get
			{
				var status = ZString.Empty;
				if (!QN_AcknowledgeStatus.IsEmpty)
				{
					status = QN_AcknowledgeStatus + " - " + Lookups.NEXDOCAcknowledgeStatusList.GetDescriptionFromCode(QN_AcknowledgeStatus);
				}

				return status;
			}
		}

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					AddUxmlEventMessages(fMessages);
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		void AddUxmlEventMessages(EDIMessageCollection messages)
		{
			var logSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.PK, GenPivotSchema.XX_Relation1ID);
			logSubQuery.AddToFilter(StmALogSchema.SL_Parent, PK);
			logSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.MessageReceivedCode, AutoEvents.MessageRejectedCode });

			var genPivotSubQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation2ID, EDIMessageSchema.PK);
			genPivotSubQuery.AddSubQuery(logSubQuery, JoinCondition.And);
			genPivotSubQuery.AddToFilter(GenPivotSchema.XX_RelationType, Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage);

			var interchangeSubQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK, EDIMessageSchema.EM_EI);
			interchangeSubQuery.AddToFilter(EDIInterchangeSchema.EI_From, new[] { Constants.DataProvider.NEXDOCS, Constants.DataProvider.NEXDOCSTest });

			var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			messageQuery.AddSubQuery(genPivotSubQuery, JoinCondition.And);
			messageQuery.AddSubQuery(interchangeSubQuery, JoinCondition.And);
			messageQuery.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, null);

			var rfpMessages = Factory.Load<RFPEDIMessage>(messageQuery);
			messages.AddRange(rfpMessages);
		}

		public bool IsAcknowledgedStatusForSendingMessage => QN_AcknowledgeStatus == NEXDOCAcknowledgeStatus.Codes.NotActioned || QN_AcknowledgeStatus == NEXDOCAcknowledgeStatus.Codes.Error;

		public ZString WrongAcknowledgedStatusForSendingMessage => Res.GetString("668D2BE4-CD7F-4FD3-A526-9A683EFF0313", "Acknowledge status must be NOT - Not Actioned or ERR - Error.");

		bool IsForward => QN_NotificationType == NEXDOCNotificationType.Codes.ForwardAcceptanceRequired;

		bool IsTransfer => QN_NotificationType == NEXDOCNotificationType.Codes.TransferAcceptanceRequired;

		public bool CanSendAcknowledgeForward => IsAcknowledgedStatusForSendingMessage && IsForward;

		public bool CanSendAcknowledgeTransfer => IsAcknowledgedStatusForSendingMessage && IsTransfer;

		ZString ObjectNameTransfer => Res.GetString("CA4BB50A-19DD-438A-8A42-1F01BBB9CF60", "Transfer");

		ZString ObjectNameForward => Res.GetString("433C9290-0C4A-4869-BAF6-5CE0B28A4C0D", "Forward");

		public ZString ObjectName =>
			IsTransfer
			? ObjectNameTransfer
			: IsForward
			? ObjectNameForward
			: ZString.Empty;

		public ZPropertyInfo ObjectNameInfo => GetZPropertyInfo(Schema.ObjectName);

		#region INEXDOCResponse Members

		public ZString JobNumber => ZString.Empty;

		public ZString RexNumber => QN_RexNumber;

		public ZString RexStatus => ZString.Empty;

		public ZString ExportPermitNumber => ZString.Empty;

		public ZString CustomsAuthorityNumber => ZString.Empty;

		public ZString HtmlTemplatePath => "Enterprise.Customs.AU.Declaration.Business.Data.Xml.Universal.NEXDOC.HtmlTemplates.FATAResponse.html";

		#endregion

		#region INEXDOCMessageParent

		ZString INEXDOCMessageParent.RexNumber => QN_RexNumber;

		#endregion
	}
}
