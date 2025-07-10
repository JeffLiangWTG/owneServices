using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE616V4Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE628V4Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE919V4Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static CargoWise.EventReference.Constants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class EXSResponseMessageProcessor : XMLResponseMessageProcessor<EXSResponse, IMessagePrettyFormatter>
	{
		public EXSResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver) : base(logger, eDocsSaver)
		{
		}

		const string ResponseTypePresentation = "AL";
		const string ResponseTypeAmendment = "MO";
		const string ResponseTypeCancellation = "AN";

		protected override string MessageFriendlyNameCore => (NoResString)"Exit Summary Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => ZString.Empty;

		protected override ZString AcceptedResponseCode => CC628ACode;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ExitSummaryDeclaration };

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(EXSResponse response, EDIMessage message, CusEntryHeader entryHeader) => new EXSMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(EXSResponse response, EDIMessage message, CusEntryHeader entryHeader)
		{
			var acceptedResponse = response as Cc628A;
			if (acceptedResponse != null)
			{
				message.EM_IsTestMessage = acceptedResponse.TesIndMes18 == "1";

				SetHeaderData(acceptedResponse.Heahea, message, entryHeader);
			}

			return ZString.Empty;
		}

		void SetHeaderData(CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE628V4Sal.Heahea acceptedResponseHeader, EDIMessage message, CusEntryHeader entryHeader)
		{
			if (acceptedResponseHeader != null)
			{
				ZDateTime.TryParseExact(acceptedResponseHeader.DecRegDatTimHea115, out var acceptanceDate, CustomsDateTimeExtension.DateTimeFormatLong);
				SetMovementReferenceNumber(entryHeader, acceptanceDate);

				var csvClearance = acceptedResponseHeader.RelCsvHea;
				if (!string.IsNullOrEmpty(csvClearance))
				{
					SetCSVClearanceAndTriggerDocumentRequest(entryHeader, message, csvClearance);
					entryHeader.CH_EntryReleaseDate = acceptanceDate;
				}

				SetCircuitAndEntryStatus(acceptedResponseHeader, message, entryHeader);
			}
		}

		void SetCircuitAndEntryStatus(CargoWise.Customs.ES.MessageDefinitions.Version1.EXS.IE628V4Sal.Heahea acceptedResponseHeader, EDIMessage message, CusEntryHeader entryHeader)
		{
			var entryStatus = ZString.Empty;

			var responseType = acceptedResponseHeader.DocOpeHea2;

			if (responseType == ResponseTypePresentation || responseType == ResponseTypeAmendment)
			{
				var circuitCode = GetCircuitCodeFromText(acceptedResponseHeader.CusChanHea);
				if (!circuitCode.IsEmpty)
				{
					entryHeader.SetMovementReferenceNumberEntryStatus(circuitCode);
					entryStatus = (string)circuitCode switch
					{
						CircuitCodeList.Codes.GREEN => EntryStatusCodes.Cleared,
						CircuitCodeList.Codes.RED or CircuitCodeList.Codes.ORANGE => EntryStatusCodes.CustomsDeclarationAccepted,
						_ => ZString.Empty
					};
				}
			}
			else if (responseType == ResponseTypeCancellation)
			{
				entryStatus = EntryStatusCodes.Cancelled;
			}

			SetEntryStatus(entryHeader, entryStatus);
		}

		void SetEntryStatus(CusEntryHeader entryHeader, ZString entryStatus)
		{
			if (!entryStatus.IsEmpty)
			{
				entryHeader.CH_EntryStatus = entryStatus;

				var logTypeCode = (NoResString)"TS Guarantee";
				var log = entryHeader.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.ErrorReport.Code && c.Parameters.GetValueOrDefault(EventReferenceParameters.Codes.Type) == logTypeCode).LastOrDefault();
				if (log != null)
				{
					Logger.LogError(log.Parameters.GetValueOrDefault(EventReferenceParameters.Codes.Reason));
				}
			}
		}

		protected override void ProcessRejectedDeclaration(EXSResponse response, EDIMessage message, CusEntryHeader entryHeader)
		{
			if (response is Cc616AType rejectedResponse)
			{
				message.EM_IsTestMessage = rejectedResponse.TesIndMes18 == "1";
			}
			else if (response is Cd919B rejectedWithErrorResponse)
			{
				message.EM_IsTestMessage = rejectedWithErrorResponse.TesIndMes18 == "1";
			}

			if (EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(entryHeader.CountryCode))
			{
				EU.Business.TemporaryStorageHelper.CancelPendingRegLineTransactions(message.Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType);
			}
		}

		protected override EXSResponse GetMessageProviderCore(EDIMessage message)
		{
			try
			{
				return GetMessageProviderWithOrWithoutValidation(message, true);
			}
			catch (Exception ex) when (ex is XmlSchemaValidationException || ex is InvalidOperationException)
			{
				Logger.LogWarning(GetValidationErrorLogDescription(message));
				Logger.LogWarning(GetExceptionLogDescription(ex));

				if (ex.ToString().Contains(FirstTagCode))
				{
					return GetMessageProviderWithOrWithoutValidation(message, false);
				}
				else
				{
					throw;
				}
			}
		}

		EXSResponse GetMessageProviderWithOrWithoutValidation(EDIMessage message, bool useValidation)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
			{
				if (message.EM_MessageText.Contains(CC628ACode))
				{
					return GetSpecificMessageProvider<Cc628A>(CC628ASchema, bodyTextReader, useValidation);
				}
				else if (message.EM_MessageText.Contains(CC616ACode))
				{
					return GetSpecificMessageProvider<Cc616AType>(CC616ASchema, bodyTextReader, useValidation);
				}
				else if (message.EM_MessageText.Contains(CD919BCode))
				{
					return GetSpecificMessageProvider<Cd919B>(CD919BSchema, bodyTextReader, useValidation);
				}
				else
				{
					throw new InvalidOperationException(Res.GetString("96248BEB-1264-43A9-856E-54C13917B25D", "The Response is not a correct EXS response"));
				}
			}
		}

		EXSResponse GetSpecificMessageProvider<T>(string schema, StringReader bodyTextReader, bool useValidation)
			where T : EXSResponse
		{
			if (useValidation)
			{
				return ESXmlObjectSerializer.DeserializeWithValidation<T>(schema, bodyTextReader);
			}
			else
			{
				return ESXmlObjectSerializer.DeserializeWithoutValidation<T>(schema, bodyTextReader, isAES: false, isNCTS: false);
			}
		}

		ZString GetValidationErrorLogDescription(EDIMessage message) => Res.GetString("57F066AD-3E82-410B-B547-DB4F36846F5D", "Validation error for message: Type:{0}, Ref:{1}, Text:{2} ",
																																				message.EM_MessageType, message.EM_ApplicationReference, message.EM_MessageText);

		protected override CommonDocumentRequest<CusEntryHeader> GetNewDocumentRequest(CusEntryHeader businessObject, ZString certName, EDIMessage message) => new EXSDocumentRequest(businessObject, certName);

		protected override Dictionary<string, string> FileNamesDict(string mrn, string oldCSVClearance, EDIMessage message) => EDocHelper.FileNamesDictEXS(mrn, oldCSVClearance);

		const string FirstTagCode = "MesSenMES3";
		const string CC628ACode = "CC628A";
		const string CC616ACode = "CC616A";
		const string CD919BCode = "CD919B";
		const string CC628ASchema = "CargoWise.Customs.ES.MessageDefinitions.Version1.SummaryDeclarations.EXS.Incoming.IE628V4Sal.xsd";
		const string CC616ASchema = "CargoWise.Customs.ES.MessageDefinitions.Version1.SummaryDeclarations.EXS.Incoming.IE616V4Sal.xsd";
		const string CD919BSchema = "CargoWise.Customs.ES.MessageDefinitions.Version1.SummaryDeclarations.EXS.Incoming.IE919V4Sal.xsd";
	}
}
