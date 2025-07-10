using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaManifestUniversalMessagingHelper
	{
		public AsycudaManifestUniversalMessagingHelper(INotifications notifications)
		{
			this.notifications = Argument.NotNull(notifications, nameof(notifications));
		}

		readonly INotifications notifications;
		Dictionary<ZGuid, ZString> messageParentsOldMessagingStatus;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		public string SendViaEHub(AsycudaManifestHeader asycudaManifestHeaderToUniversalise, ZString messageType, ZString messageSubType, IList<IMessageParent> messageParents)
		{
			var factory = asycudaManifestHeaderToUniversalise.Factory;
			using (factory.AddDisposableService())
			{
				var countryCode = asycudaManifestHeaderToUniversalise.AMA_RN_NKCountry;
				var headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, asycudaManifestHeaderToUniversalise);

				AddAdditionalDetails(headerData);

				var parameters = new Dictionary<string, string>();
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, messageType);
				parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageSubType, messageSubType);

				var customsSystem = asycudaManifestHeaderToUniversalise.CustomsSystem;
				var actionPurpose = GetActionPurpose(countryCode, messageType, messageSubType);
				var recipientRoles = GetRecipientRoles();
				ICodeDescriptionDataObject eventType = null;
				var eventReference = ZString.Empty;
				if (!customsSystem.IsEmpty)
				{
					eventType = new CodeDescriptionPair() { Code = Events.MessageRequestedToBeSentCode, Description = Events.MessageRequestedToBeSent.Description };
					eventReference = StmALog.GenerateEventReference(ZString.Empty, parameters);
				}

				var dataContext = headerData.DataContext;
				dataContext.SetWorkflowInfo(new WorkflowInfo()
				{
					EventType = eventType,
					EventReference = eventReference,
					ActionPurpose = actionPurpose,
					RecipientRoles = recipientRoles,
					TriggerType = TriggerType.Manual,
					TriggerDescription = new MessageSubTypeCodes().GetDescriptionFromCode(messageSubType)
				});

				IXmlEDIInterchange interchange = null;
				IEnumerable<EDIMessage> messages = Array.Empty<EDIMessage>();

				try
				{
					interchange = SetDeliveryRecipient(asycudaManifestHeaderToUniversalise, headerData, factory);
					SetMessageStatusOnSend(asycudaManifestHeaderToUniversalise, messageParents, messageSubType);
					asycudaManifestHeaderToUniversalise.Messages.Load();

					if (interchange != null)
					{
						var query = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
						query.FetchOnlyFromLocalCache = true;
						messages = factory.Load<EDIMessage>(query);
					}
					var isFatJob = asycudaManifestHeaderToUniversalise.IsLargeJobWithLotsOfBillsOrPackLinesSoDontTryToConvertXmlToHtml();

					foreach (EDIMessage message in messages)
					{
						if (isFatJob)
						{
							message.EM_MessageInterpretation = "<p>This is a relatively large manifest, so the message interpretation has been suppressed. Please view the XML using the Text tab or the Workflow &amp; Tracking tab.</p>";
						}
						else
						{
							// Small job, it's OK to ask to render the UXML into pretty XML by accessing EM_MessageInterpretation
							var accessGetterOfEmMessageInterpretationBeforeSavingToConvertUxmlIntoHmtlSoWeDontHaveToDoSoAgain = message.EM_MessageInterpretation;
						}
					}
					SaveChanges(factory);
					asycudaManifestHeaderToUniversalise.Messages.Load();
					return Res.GetString("4d394117-349e-47e3-9c31-5e40f1f62d76", "Manifest Created");
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					interchange?.Delete();
					UndoSetMessageStatusOnSend(asycudaManifestHeaderToUniversalise, messageParents, messageSubType);
					return Res.GetString("8ebfe3ac-76fb-4c3c-8130-73fbcaae680f", "The following error was encountered while saving the changes:") + ex.Message;
				}
			}
		}

		protected virtual void SaveChanges(BusinessObjectFactory factory)
		{
			factory.Save();
		}

		protected virtual void AddAdditionalDetails(UniversalShipment headerData)
		{
		}

		protected virtual void SetMessageStatusOnSend(AsycudaManifestHeader header, IList<IMessageParent> messageParents, ZString messageSubType)
		{
			messageParentsOldMessagingStatus = new Dictionary<ZGuid, ZString>();
			foreach (var messageParent in messageParents)
			{
				var oldStatus = messageParent.SetNewCountryMessagingStatus(CalculateMessageStatus(messageParent));
				messageParentsOldMessagingStatus.Add(((BusinessObject)messageParent).PK, oldStatus);
			}
		}

		protected virtual void UndoSetMessageStatusOnSend(AsycudaManifestHeader header, IList<IMessageParent> messageParents, ZString messageSubType)
		{
			if (messageParentsOldMessagingStatus != null)
			{
				foreach (var messageParent in messageParents)
				{
					if (messageParentsOldMessagingStatus.TryGetValue(((BusinessObject)messageParent).PK, out var oldStatus))
					{
						messageParent.SetNewCountryMessagingStatus(oldStatus);
					}
				}
			}
		}

		protected virtual ICodeDescriptionDataObject GetActionPurpose(string countryCode, string messageType, string messageSubType) => null;

		protected virtual IEnumerable<RecipientRoleDetail> GetRecipientRoles() => null;

		protected virtual ZString GetRecipientID(AsycudaManifestHeader header)
		{
			return header.CustomsSystem;
		}

		IXmlEDIInterchange SetDeliveryRecipient(AsycudaManifestHeader header, UniversalShipment shipment, BusinessObjectFactory factory)
		{
			var delivery = new EHubDelivery();
			try
			{
				var context = new DeliveryContext(factory)
				{
					ParentInfo = EntityInfo.New(header),
					ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging,
					MessageTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
					MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalShipment,
					Notifications = notifications
				};

				var mode = new NonPersistentEDICommunicationMode();
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
				mode.EK_Filename = header.GetXmlFileName();

				mode.EK_Destination = GetRecipientID(header);

				mode.EK_ServerAddressSubject = GlbStaff.CurrentUser.GS_EmailAddress; // this is such a hack
				DeliverMessage(delivery, context, mode, new DeliveryStreamWrapperUXML(context.ParentInfo, shipment, new XmlWriter(), null));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				delivery.InterchangeCreated?.Delete();
				throw;
			}
			return delivery.InterchangeCreated;
		}

		protected virtual void DeliverMessage(EHubDelivery delivery, DeliveryContext context, NonPersistentEDICommunicationMode mode, DeliveryStreamWrapperUXML deliveryStream)
		{
			delivery.Deliver(context, mode, deliveryStream);
		}

		protected virtual string CalculateMessageStatus(IMessageParent messageParent) => MessageStatusCodeList.Codes.Awaiting;

		public static T ForMessageLevel<T>(AsycudaManifestHeader header, Func<T> manifestLevel, Func<T> billLevel, Func<T> packLevel)
		{
			if (header.IsPackedItemLevelManifestType)
			{
				return packLevel();
			}

			if (header.IsBillLevelManifestType)
			{
				return billLevel();
			}

			return manifestLevel();
		}
	}
}
