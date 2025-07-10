using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business
{
	public abstract class T2LPOUSCommonResponseMessageProcessor<TResponseProvider, TPrettyMessage> : XMLResponseMessageProcessor<TResponseProvider, TPrettyMessage>
		where TResponseProvider : class, ICommonServiceSegment, IResponseCode
		where TPrettyMessage : IMessagePrettyFormatter
	{
		protected T2LPOUSCommonResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected sealed override ZString AcceptedResponseCode => ResponseMessageCodeList.AcceptedDeclarationT2LPOUS;

		protected void ProcessCommonAcceptedDeclaration(CusEntryHeader entryHeader, string circuit, string csvClearance, EDIMessage message, string preparationDateAndTime = null, bool shouldUseTRMForDocTrigger = true)
		{
			var admissionDate = ZDateTime.Empty;
			if (preparationDateAndTime != null)
			{
				ZDateTime.TryParseExact(preparationDateAndTime, out admissionDate, CustomsDateTimeExtension.DateTimeFormatyyyyMMddTHHmmss);
			}

			SetAdmissionDateAndCircuit(entryHeader, admissionDate, circuit);

			SetEntryStatus(entryHeader, admissionDate, circuit);

			SetCSVClearanceAndTriggerDocumentRequest(entryHeader, message, csvClearance, shouldUseTRMForDocTrigger ? entryHeader.T2CMovementReferenceNumber : entryHeader.MovementReferenceNumber);
		}

		protected void SetAdmissionDateAndCircuit(CusEntryHeader entryHeader, ZDateTime admissionDate, ZString circuit)
		{
			SetMovementReferenceNumber(entryHeader, admissionDate, GetCircuitCodeFromText(circuit));
		}

		protected void SetEntryStatus(CusEntryHeader entryHeader, ZDateTime admissionDate, string circuit)
		{
			if (circuit == MessageFunctionCodeList.Codes.GreenCircuitText)
			{
				entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
				if (!admissionDate.IsEmpty)
				{
					entryHeader.CH_EntryReleaseDate = admissionDate;
				}
			}
			else if (circuit
					 is MessageFunctionCodeList.Codes.RedCircuitText
					 or MessageFunctionCodeList.Codes.OrangeCircuitText)
			{
				entryHeader.CH_EntryStatus = EntryStatusCodes.CustomsDeclarationAccepted;
			}
		}

		protected sealed override TResponseProvider DererializeMessage(EDIMessage message, string xsdSchemaEmbeddedResourceName)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
			{
				return ESXmlObjectSerializer.DeserializeWithoutValidation<TResponseProvider>(xsdSchemaEmbeddedResourceName, bodyTextReader, false, false);
			}
		}

		protected void SetT2CMovementReferenceNumber(CusEntryHeader entryHeader, string mrnJec)
		{
			var cusEntryNumber = entryHeader.Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Spain.T2CMovementReferenceNumber;
			cusEntryNumber.CE_ParentID = entryHeader.PK;
			cusEntryNumber.CE_ParentTable = entryHeader.TableName;
			cusEntryNumber.CE_EntryNum = mrnJec;
		}
	}
}
