using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.EDIInterchanges;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;
using Utils = Enterprise.Customs.ES.Business.MessageSending.ESInterchangeProviderUtilities;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public class ESInterchangeProvider : InterchangeProviderBase
	{
		public ESInterchangeProvider(NonDependentEDIMessageCollection messages, LoggingInformation logger) : base(messages)
		{
			Logger = logger;
		}

		public readonly LoggingInformation Logger;

		protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

		protected override Type InterchangeType => typeof(ESEDIInterchange);

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var message = GetOneAndOnlyMessageFromCollection(messages);

			try
			{
				if (message.EM_MessageText.IsEmpty)
				{
					throw new InvalidOperationException(NoMessageTextErrorMessage);
				}

				GlbStaff broker = null;
				var entryReference = ZString.Empty;
				var entryMRN = ZString.Empty;

				if (message.EM_MessageType != DeclarationMessageTypeList.Codes.InboxPendingList)
				{
					var messageInfo = GetMessageProvider(message);
					broker = messageInfo.Broker;
					entryReference = messageInfo.EntryReference;
					entryMRN = message.EM_MessageType != DeclarationMessageTypeList.Codes.G3DeclarationOfGoods ? messageInfo.MRN : ZString.Empty;
				}

				var certificateName = message.EM_ApplicationReference;
				if (certificateName.IsEmpty)
				{
					throw new InvalidOperationException(NoApplicationReferenceErrorMessage);
				}

				var certificate = broker != null ? CertificateHelper.GetCertificate(broker, certificateName) : null;
				var isDirectXTInterface = Registry.ESCustomsDataRegistry.Instance.EnableESMessagingThroughDirectxTInterface.Value;
				var isDirectXTInterfaceForInbox = Registry.ESCustomsDataRegistry.Instance.EnableESInboxMessagesThroughDirectxTInterface.Value;
				var isTestMessage = message.EM_IsTestMessage;
				var interchangeReceiver = Utils.GetInterchangeReceiver(message.EM_MessageType, isDirectXTInterface, isTestMessage, isDirectXTInterfaceForInbox);
				if (message.EM_MessageType == DeclarationMessageTypeList.Codes.InboxPendingList
					|| (InterchangeReceiverEdifactOrDirectXtOrDOC.Contains(interchangeReceiver) && isDirectXTInterface)
					|| (InterchangeReceiverAsyncDirectXt.Contains(interchangeReceiver) && isDirectXTInterfaceForInbox))
				{
					interchange.EI_TransportType = EDIInterchange.TransportType.xT;
				}

				if (interchange.ShouldSendViaEHub)
				{
					var xmlExtraData = Utils.GetServiceAndOperation(message.EM_MessageType);
					var certificateThumbPrint = certificate?.GP_UserID ?? ZString.Empty;
					SetInterchangeHeaderTextForEHub(interchange, message, broker.GS_Code, certificateName, certificateThumbPrint, entryReference, xmlExtraData.Service, xmlExtraData.Operation);
				}

				if ((interchangeReceiver == SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt
					|| interchangeReceiver == SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt)
					&& isDirectXTInterfaceForInbox)
				{
					var dictionary = new Dictionary<string, string>()
					{
						{ ESConstants.CustomMsgAttributes.MRNToRequest, entryMRN }
					};
					interchange.SetHeaderTextWithAttributeDictionary(dictionary);
				}

				SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, interchangeReceiver, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
				interchange.EI_SessionGUID = ZGuid.NewZGuid();
				if (interchange.EI_GP.IsEmpty && certificate != null)
				{
					interchange.EI_GP = certificate.PK;
				}

				if (interchangeReceiver == SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms)
				{
					SignBodyForEdifact(interchange, message, certificate);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Logger.LogWarning(ex.ToString());
				message.Notes.AddNew(true, (NoResString)"Processing Log", ex.ToString());

				message.EM_Status = EDIMessageStatusList.Codes.Failed;
				interchange.Delete();
			}
		}

		ImmutableList<ZString> InterchangeReceiverEdifactOrDirectXtOrDOC => interchangeReceiverEdifactOrDirectXtOrDOC
																				??= new List<ZString>()
																						{
																							SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms,
																							SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt,
																							SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt,
																							SpanishCustomsTypeCodeList.Codes.DocumentSpanishCustoms,
																							SpanishCustomsTypeCodeList.Codes.DocumentTestSpanishCustoms
																						}.ToImmutableList();
		ImmutableList<ZString> interchangeReceiverEdifactOrDirectXtOrDOC;
		ImmutableList<ZString> InterchangeReceiverAsyncDirectXt => interchangeReceiverAsyncDirectXt
																				??= new List<ZString>()
																						{
																							SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt,
																							SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt
																						}.ToImmutableList();
		ImmutableList<ZString> interchangeReceiverAsyncDirectXt;

		protected override string GetCollationKey(EDIMessage message) => DoNotCollateType;

		protected override ZString QueuedInterchangeStatusCode(EDIInterchange interchange) =>
			interchange.ShouldSendViaEHub ? EDIInterchange.Status.eHubQueued : EDIInterchange.Status.Queued;

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages) => ZString.Empty;

		EDIMessage GetOneAndOnlyMessageFromCollection(NonDependentEDIMessageCollection messages) => messages.Cast<EDIMessage>().Single();

		public virtual IESMessageInfoProvider GetMessageProvider(EDIMessage message)
		{
			if (message.EM_LinkedObject == null)
			{
				throw new InvalidOperationException(NoLinkedObjectErrorMessage);
			}
			else
			{
				return ((IESMessageInfoProvider)message.EM_LinkedObject);
			}
		}

		#region private methods

		void SetInterchangeHeaderTextForEHub(EDIInterchange interchange, EDIMessage message, ZString brokerCode, ZString certificateName, ZString certificateThumbPrint, ZString entryReference, ZString service, ZString operation)
		{
			var headers = new Headers()
			{
				BrokerCode = brokerCode,
				CertificateName = certificateName,
				CertificateThumbPrint = certificateThumbPrint,
				EntryReferenceNumber = entryReference,
				TestMessage = message.EM_IsTestMessage.ToString(),
				SentEdiMessageNumber = message.EM_MessageNum
			};

			if (!service.IsEmpty)
			{
				headers.Service = service;
			}

			if (!operation.IsEmpty)
			{
				headers.Operation = operation;
			}

			interchange.EI_HeaderText = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.SerializeDefaultSettingsWithoutNamespaces(headers);
		}

		void SignBodyForEdifact(EDIInterchange interchange, EDIMessage message, GlbExternalPassword certificate)
		{
			var certificateBytes = certificate?.GP_Certificate ?? ZBlob.Empty;
			var decryptedCertificatePassphrase = certificate?.CurrentDecryptedCertificatePassphrase ?? ZString.Empty;

			if (!certificateBytes.IsEmpty && !decryptedCertificatePassphrase.IsEmpty)
			{
				interchange.EI_BodyText = new EDIMessageSupporter().GetPayload(message.EM_MessageText, certificateBytes, decryptedCertificatePassphrase);
			}
		}
		#endregion

		#region Message ZStrings

		ZString NoLinkedObjectErrorMessage => Res.GetString("FBB0CA98-3D96-430E-9506-5F9D8256C0B8", "Message's Linked Object is null so can't continue with processing.") + " " + MessageSetToFailedErrorMessage;

		ZString NoApplicationReferenceErrorMessage => Res.GetString("1109E9AF-A7A0-4660-BA57-149BDF1DAFD0", "Message's Application Reference (certificate name) is empty so can't continue with processing.") + " " + MessageSetToFailedErrorMessage;

		ZString NoMessageTextErrorMessage => Res.GetString("B5808494-D0A7-470E-854D-6548EFE07AC7", "Message's Message Text is empty so can't continue with processing.") + " " + MessageSetToFailedErrorMessage;

		ZString MessageSetToFailedErrorMessage => Res.GetString("E721967F-3C9A-400B-9A0B-6AEB9037BD77", "The message's status has been set to 'Failed'.");

		#endregion
	}

	public class XMLExtraData
	{
		public ZString Service;
		public ZString Operation;
	}
}
