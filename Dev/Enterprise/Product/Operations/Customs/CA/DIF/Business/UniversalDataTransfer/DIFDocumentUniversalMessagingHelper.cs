using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Business;
using AttachedDocument = Enterprise.UniversalDataBuss.DataObjects.Universal.AttachedDocument;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DIF.Business.UniversalDataTransfer
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used in the future")]
	public class DIFDocumentUniversalMessagingHelper
	{
		public ZString SendEventViaEHub(DIFDocument document, string functionCode)
		{
			Argument.NotNull(document, nameof(document));
			var eventManager = (IEventDataContextManager)document.GetUniversalDataContextManager();
			var actionInfo = new ActionInfo(null, document);
			var writer = eventManager.GetEventDataObjectWriter(new DataWritingManager(actionInfo));
			var eventTime = ZDateTimeOffset.Now;
			var log = ((IStmALogParent)document).Logs.AddNew(new EventValue(Enterprise.ZArchitecture.Business.Events.DocumentSent, eventTime: eventTime));
			using (var headerData = (UniversalEvent)writer.GetDataObject(log))
			{
				headerData.EventParameters = new EventParameters();
				headerData.EventParameters.ReferenceNumber = document.DocumentNumber;
				headerData.EventParameters.RequestNumber = document.URN;
				headerData.EventParameters.MessageType = DIFConstants.UniversalEventConstants.MessageType;
				headerData.EventParameters.MessageSubType = functionCode;
				var attachedDocumentCollection = GetAttachedDocumentValues(document);
				if (attachedDocumentCollection != null)
				{
					headerData.AttachedDocumentCollection = attachedDocumentCollection;
				}

				SetDeliveryRecipient(document.RequiredDocumentAddInfo, headerData);
			}

			try
			{
				document.RequiredDocumentAddInfo.Factory.Save();
				return ZString.Empty;
			}
			catch (ZSaveException ex)
			{
				return Res.GetString("5358C951-5B5D-471A-8BCF-23D56D3316B0", "The following error was encountered while saving DIF {0}: {1}", document.URN, ex.Message);
			}
		}

		IXmlEDIInterchange SetDeliveryRecipient(JobRequiredDocumentAddInfo jobRequiredDocumentAddInfo, UniversalEvent @event)
		{
			var context = new DeliveryContext(jobRequiredDocumentAddInfo.Factory)
			{
				ParentInfo = EntityInfo.New(jobRequiredDocumentAddInfo),
				ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging,
				MessageTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalEvent,
				MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalEvent,
				Notifications = new DoNothingNotifications()
			};

			var delivery = new EHubDelivery();
			var mode = new NonPersistentEDICommunicationMode();
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			mode.EK_Destination = CACustomsDoc;
			delivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(context.ParentInfo, @event, new XmlWriter(), null));
			return delivery.InterchangeCreated;
		}

		const string CACustomsDoc = "CACustomsDoc";

		List<AttachedDocument> GetAttachedDocumentValues(DIFDocument document)
		{
			List<AttachedDocument> result = null;
			var disHost = document.HostWrapper.DISHost;
			var eDocBO = disHost?.EDocs.FirstOrDefault(eDoc => !eDoc.IsDeleted && eDoc.UniqueKey == document.EDocsDocumentPK);
			if (eDocBO != null)
			{
				var attachedDocument = new AttachedDocumentDataObjectWriter().GenerateAttachedDocument(false, eDocBO, GenerateContextForAttachedDocument(document));
				result = new List<AttachedDocument>() { attachedDocument };
			}
			return result;
		}

		IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GenerateContextForAttachedDocument(DIFDocument document)
		{
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();
			contextValues.AddIfNotEmpty(AttachedDocument.ContextTypes.CustomsDocumentType, document.DocumentType);
			contextValues.AddIfNotEmpty(AttachedDocument.ContextTypes.ImporterBusinessNumber, document.HostWrapper?.DISHost?.ImporterBusinessNumber ?? ZString.Empty);
			contextValues.AddIfNotEmpty(AttachedDocument.ContextTypes.EffectiveDate, document.EffectiveDate);
			contextValues.AddIfNotEmpty(AttachedDocument.ContextTypes.ExpiryDate, document.ExpiryDate);
			return contextValues;
		}

		class DoNothingNotifications : INotifications
		{
			public void Add(INotification notification)
			{
				// In theory these logs should go somewhere, but where?
			}
		}
	}
}
