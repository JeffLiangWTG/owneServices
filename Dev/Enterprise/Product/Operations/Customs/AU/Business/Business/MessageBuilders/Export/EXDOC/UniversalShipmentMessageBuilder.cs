using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class UniversalShipmentMessageBuilder
	{
		public UniversalShipmentMessageBuilder(QuarantineExDocHeader header, string messageType, INotifications notifications)
		{
			this.header = header;
			this.messageType = messageType;
			this.notifications = Argument.NotNull(notifications, nameof(notifications));
		}
		readonly QuarantineExDocHeader header;
		readonly string messageType;
		readonly INotifications notifications;

		bool IsSendToEU => UniversalReferenceHelper.Errata51Enabled() && (header?.Declaration?.PortOfArrival?.IsInEU ?? false);

		#region Constants

		const string EventType = "MRS";
		const string RecipientID = "NEXDOCS"; // Xml Content
		const string RecipientIDForTest = "NEXDOCSTest"; // Xml Content

		#endregion

		#region Methods

		public ZBool GenerateMessage(bool suspend = false)
		{
			var writerConfig = new DeclarationDataObjectWriterConfiguration();
			writerConfig.ShouldPopulateAttachedDocumentCollection = ZBool.True;

			var factory = new BusinessObjectFactory { NameForDebugging = "Send QuarantineExDocHeader via eHub" };
			using (var shipment = header.InvoiceHeader.JobDeclaration.GetUniversalShipment(writerConfig, RecipientRoleType.ORP, DataContextType.CustomsDeclaration))
			{
				var dataContext = shipment.DataContext;
				var userCode = StaticCurrentFetcher.Instance.CurrentUserCode;
				dataContext.SetWorkflowInfo(new WorkflowInfo
				{
					EventType = new CodeDescriptionPair { Code = EventType },
					EventReference = GetEventReference(),
					EventUser = new Staff { Code = userCode, Name = GetEventUserName(userCode, factory) }
				});

				if (messageType == NEXDOCMessageType.Codes.TransferEDN && header.Declaration?.RelatedDeclarationForTransferEDN != null)
				{
					var exdocHeader = header.Declaration.RelatedDeclarationForTransferEDN.QuarantineInvoice?.QuarantineExDocHeader;

					if (exdocHeader != null)
					{
						var noteCollection = shipment.NoteCollection;

						if (noteCollection == null)
						{
							noteCollection = new UniversalDataBuss.DataObjects.Core.DataObjectList<Note>();
							shipment.SetNoteCollection(() => noteCollection);
						}

						noteCollection.Add(new Note() { Description = "EXDOC trf REX", NoteText = exdocHeader.RexNumber });
						noteCollection.Add(new Note() { Description = "EXDOC trf LastAmendTime", NoteText = exdocHeader.QH_LastAmendDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture) });
					}
				}

				IXmlEDIInterchange interchange = null;
				using (factory.AddDisposableService())
				{
					interchange = SetDeliveryRecipient(shipment, factory);

					if (suspend && interchange != null)
					{
						var ediInterchangeCreated = (EDIInterchange)interchange;
						var messageCreated = ediInterchangeCreated.ContainedMessages[0];

						ediInterchangeCreated.EI_Status = EDIInterchange.Status.SendPending;
						messageCreated.EM_Status = EDIMessage.Status.Pending;
					}

					if (messageType != NEXDOCMessageType.Codes.ReadREX && messageType != NEXDOCMessageType.Codes.PreviewCertificate)
					{
						header.InvoiceHeader.JobDeclaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
					}

					if (messageType == NEXDOCMessageType.Codes.ManualAmend)
					{
						header.AddInfo.ZH_AmendmentResponseStatus = RFPMessage.Status.AwaitingResponse;
					}

					try
					{
						factory.Save();
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}

					header.Messages.Load();
				}

				return interchange != null;
			}
		}

		protected virtual IXmlEDIInterchange SetDeliveryRecipient(Shipment shipment, BusinessObjectFactory factory)
		{
			var context = new DeliveryContext(factory)
			{
				ParentInfo = EntityInfo.New(header),
				ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging,
				MessageTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
				MessageSubTypeCode = messageType,
				Notifications = notifications
			};

			var delivery = new EHubDelivery();
			var mode = new NonPersistentEDICommunicationMode();
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			mode.EK_Destination = AUCustomsDataRegistry.Instance.IsNEXDOCSTestingSystem ? RecipientIDForTest : RecipientID;

			delivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(context.ParentInfo, shipment, new XmlWriter(), null));
			return delivery.InterchangeCreated;
		}

		ZString GetEventReference()
		{
			var messageTypeEventReference = ZString.Empty;
			switch (messageType)
			{
				case NEXDOCMessageType.Codes.Order:
					messageTypeEventReference = "ORDER";
					break;
				case NEXDOCMessageType.Codes.Lodge:
					messageTypeEventReference = "LODGE";
					break;
				case NEXDOCMessageType.Codes.Withdrawal:
					messageTypeEventReference = "WITHDRAW";
					break;
				case NEXDOCMessageType.Codes.Amend:
				case NEXDOCMessageType.Codes.ManualAmend:
					messageTypeEventReference = "AMEND";
					break;
				case NEXDOCMessageType.Codes.TransferEDN:
					messageTypeEventReference = "TRFEDN";
					break;
				case NEXDOCMessageType.Codes.CancelEDN:
					messageTypeEventReference = "CANEDN";
					break;
				case NEXDOCMessageType.Codes.Cancellation:
					messageTypeEventReference = "CANREX";
					break;
				case NEXDOCMessageType.Codes.ReissueCertificate:
					messageTypeEventReference = "REISSUE";
					break;
				case NEXDOCMessageType.Codes.ReplacementCertificate:
					messageTypeEventReference = "REPLACE";
					break;
				case NEXDOCMessageType.Codes.PreviewCertificate:
					messageTypeEventReference = "PREVIEW";
					break;
				case NEXDOCMessageType.Codes.ReadREX:
					messageTypeEventReference = "READREX";
					break;
			}
			var parameters = new Dictionary<string, string>();
			parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, messageTypeEventReference);
			if (IsSendToEU)
			{
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, Core.Constants.CountryCodes.EuropeanUnion);
			}
			return StmALog.GenerateEventReference(ZString.Empty, parameters);
		}

		ZString GetEventUserName(ZString code, BusinessObjectFactory factory)
		{
			return factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, code))?.GS_FullName ?? ZString.Empty;
		}

		#endregion
	}
}
