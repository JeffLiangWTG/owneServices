using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.DE;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.ES.TemporaryStorage.Business
{
	public abstract class G5CommonResponseMessageProcessor<TResponseProvider, TPrettyMessage> : ESCommonResponseMessageProcessor<TemporaryStorageHeader, TResponseProvider>
		where TResponseProvider : class, ICommonServiceSegment, IResponseCode
		where TPrettyMessage : IMessagePrettyFormatter
	{
		protected G5CommonResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		string ResponseCodeAC => nameof(ResponseCodeDe.Ac);

		protected override string MessageFriendlyNameCore => (NoResString)"ES G5 Generic Response Message Processor";

		protected abstract ZString XsdSchemaEmbeddedResourceName { get; }
		protected ZString AcceptedResponseCode => ResponseCodeAC;

		protected sealed override void ProcessMessageCore(EDIMessage message, TemporaryStorageHeader linkedBusinessObject, TResponseProvider provider)
		{
			message.EM_MessageNum = ((ZString)provider.ServiceSegmentId).Left(AutoEDIMessage.Schema.EM_MessageNumMaxLength);

			var messagePrettyFormatter = GetNewMessagePrettyFormatter(provider, message, linkedBusinessObject);

			ProcessDeclarationCore(provider, message, linkedBusinessObject, messagePrettyFormatter);
			SetMessageStatusAsReceived(message);
			SetCHStatusAsReceived(linkedBusinessObject, messageStatus: ZString.Empty);
		}

		void ProcessDeclarationCore(TResponseProvider response, EDIMessage message, TemporaryStorageHeader temporaryStorageHeader, TPrettyMessage messagePrettyFormatter)
		{
			if (response.ResponseCode == AcceptedResponseCode)
			{
				var extraDataFromProcessing = ProcessAcceptedDeclaration(response, message, temporaryStorageHeader);
				message.EM_MessageInterpretation = messagePrettyFormatter.CreateMessageDetailsAccepted(extraDataFromProcessing);
				SetMessageSubTypeAsAccepted(message);
			}
			else
			{
				ProcessRejectedDeclaration(response, message, temporaryStorageHeader);
				message.EM_MessageInterpretation = messagePrettyFormatter.CreateMessageDetailsRejected();
				SetMessageSubTypeAsRejected(message);
			}
		}

		protected override TResponseProvider GetMessageProviderCore(EDIMessage message)
		{
			return DeserializeMessage(message, XsdSchemaEmbeddedResourceName);
		}

		protected TResponseProvider DeserializeMessage(EDIMessage message, string xsdSchemaEmbeddedResourceName)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
			{
				return ESXmlObjectSerializer.DeserializeWithoutValidation<TResponseProvider>(xsdSchemaEmbeddedResourceName, bodyTextReader, isAES: false, isNCTS: false);
			}
		}

		protected void CreateOrUpdateCusEntryNumber(TemporaryStorageHeader temporaryStorageHeader, ZString entryType, ZString entryNum, ZString entryStatus, ZDateTime issueDate, ZDateTime expiryDate)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(temporaryStorageHeader, entryType, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = entryNum;
			newEntryNumber.CE_EntryStatus = entryStatus;
			newEntryNumber.CE_IssueDate = issueDate;
			newEntryNumber.CE_ExpiryDate = expiryDate;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
		}

		protected void SetEntryNumbers(TemporaryStorageHeader temporaryStorageHeader, ZString mrn, ZString expeditionCircuit, ZString preparationDate, ZString csvClearanceCode, ZString dsdtMRN)
		{
			var circuit = GetCircuitCodeFromText(expeditionCircuit);
			if (!mrn.IsEmpty)
			{
				ZDateTime.TryParseExact(preparationDate, out var admissionDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);
				CreateOrUpdateCusEntryNumber(temporaryStorageHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, mrn, circuit, admissionDate, ZDateTime.Empty);
			}

			if (!csvClearanceCode.IsEmpty)
			{
				CreateOrUpdateCusEntryNumber(temporaryStorageHeader, CusEntryNumberTypes.Spain.ClearanceCSV, csvClearanceCode, ZString.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}

			if (!dsdtMRN.IsEmpty)
			{
				CreateOrUpdateCusEntryNumber(temporaryStorageHeader, CusEntryNumberTypes.Spain.SummaryEntryNumber, dsdtMRN, ZString.Empty, ZDateTime.Empty, ZDateTime.Empty);
			}
		}

		protected void SetCustomsStatus(TemporaryStorageHeader temporaryStorageHeader, ChannelDe? circuit)
		{
			ZString customsStatus = (ChannelDe)circuit switch
			{
				ChannelDe.V => EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance,
				ChannelDe.N or ChannelDe.R => EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.UnderControl,
				_ => ZString.Empty,
			};
			if (!customsStatus.IsEmpty)
			{
				temporaryStorageHeader.CustomsStatus = customsStatus;
			}

			var logTypeCode = (NoResString)"TS Guarantee";
			var log = temporaryStorageHeader.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.ErrorReport.Code && c.Parameters.GetValueOrDefault(EventReferenceParameters.Codes.Type) == logTypeCode).LastOrDefault();
			if (log != null)
			{
				Logger.LogError(log.Parameters.GetValueOrDefault(EventReferenceParameters.Codes.Reason));
			}
		}

		protected abstract ZString ProcessAcceptedDeclaration(TResponseProvider response, EDIMessage message, TemporaryStorageHeader temporaryStorageHeader);

		protected virtual void ProcessRejectedDeclaration(TResponseProvider response, EDIMessage message, TemporaryStorageHeader temporaryStorageHeader) { }

		protected abstract TPrettyMessage GetNewMessagePrettyFormatter(TResponseProvider response, EDIMessage message, TemporaryStorageHeader temporaryStorageHeader);
	}
}
