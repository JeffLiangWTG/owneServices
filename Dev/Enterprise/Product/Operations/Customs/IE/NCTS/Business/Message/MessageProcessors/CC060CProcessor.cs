using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using static Enterprise.Customs.IE.NCTS.Business.Constants;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC060CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC060CProvider>
	{
		public CC060CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("F9ECCEFF-231E-4007-BB7E-442751124E60", "CC060C: CONTROL DECISION NOTIFICATION");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC060CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC060CProvider provider)
		{
			switch (provider.NotificationType)
			{
				case "0":
					return NCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
				case "1":
					return NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest;
				case "2":
					return NCTS5DepartureCustomsStatusList.Codes.IntentionToControl;
				default:
					return NCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
			}
		}

		protected override Type MessageInterpreterType => typeof(CC060CMessageInterpreter);

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, CC060CProvider provider)
		{
			if (messageAttachee is NctsDepartureMovementHeader movementHeader && movementHeader.Header is NctsHeader nctsHeader)
			{
				PopulateRequestedDocuments(nctsHeader, provider);
			}
		}

		static void PopulateRequestedDocuments(NctsHeader nctsHeader, CC060CProvider provider)
		{
			foreach (var typeOfControls in provider.TypeOfControls)
			{
				switch (typeOfControls.Type)
				{
					case TypeOfControlTypes._10:
						AddRequestedDocuments(nctsHeader, provider.RequestedDocuments, provider.ControlNotificationDateAndTime, RequestedDocumentStatusList.Codes.RequestOpened);
						return;
					case TypeOfControlTypes._40:
						AddRequestedDocuments(nctsHeader, provider.RequestedDocuments, provider.ControlNotificationDateAndTime, RequestedDocumentStatusList.Codes.PhysicallyPresentDocument);
						return;
				}
			}
		}

		static void AddRequestedDocuments(NctsHeader nctsHeader, IReadOnlyCollection<CC060RequestedDocumentProvider> reqDocsFromMessage, ZDateTime controlNotificationDateAndTime, string requestOpenedCode)
		{
			foreach (var reqDocFromMessageIn in reqDocsFromMessage)
			{
				var requestedDocument = nctsHeader.RequestedDocuments.AddNew();
				requestedDocument.CSI_Code = reqDocFromMessageIn.DocumentType;
				requestedDocument.RequestInformation = reqDocFromMessageIn.Description;
				requestedDocument.CSI_DateOfIssue = controlNotificationDateAndTime;
				requestedDocument.CSI_DateOfExpiry = ZDateTime.Empty;
				requestedDocument.CSI_Status = requestOpenedCode;
			}
		}
	}
}
