using System;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using NctsTransitStatusList = Enterprise.Customs.FR.Business.NctsTransitStatusList;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public abstract class DTBaseProcessor<T> : ApplicationTypeMessageProcessor
					 where T : CargoWise.Customs.FR.MessageDefinitions.DeltaT.INctsXmlMessage
	{
		protected virtual ZString GetNewMessageStatus(T messageObject) => ZString.Empty;

		protected virtual ZString GetNewDepartureStatus(T messageObject) => ZString.Empty;

		protected virtual ZString GetNewArrivalStatus(T messageObject) => ZString.Empty;

		protected virtual ZString GetNewDetailedDepartureStatus(T messageObject) => ZString.Empty;

		protected virtual ZString GetNewDetailedArrivalStatus(T messageObject) => ZString.Empty;

		protected DTBaseProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(loggingInformation)
		{
			ServiceLogger = serviceLogger;
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T processor";

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.EuNcts;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			var messageText = Business.MessageProcessors.ProcessorHelper.AdaptMessageToExpectedFormat(message.EM_MessageText);

			var messageObject = Extensions.Deserialize<T>(messageText);
			UpdateEDIMessage(message, messageObject);
			UpdateOutgoingMessageStatus(message);
			UpdateNCTSHeader(messageObject);
			UpdateGuaranteeTransactionsIfNeeded(messageObject, message);
			GenerateDocuments(message);
			DoExtraProcessing(messageObject, message);
			ServiceLogger.Log(LogType.Information, () =>
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)"Processing received message #{0}, type {1} {2}", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType);
			});
		}

		protected virtual void DoExtraProcessing(T messageObject, EDIMessage inboundMessage)
		{
		}

		void UpdateOutgoingMessageStatus(EDIMessage inboundMessage)
		{
			var outgoingRelatedMessage = NctsHeader.GetOutgoingMessage(inboundMessage);
			if (outgoingRelatedMessage != null)
			{
				outgoingRelatedMessage.EM_Status = EDIMessage.Status.Acknowledged;
			}
		}

		protected virtual void UpdateNCTSHeader(T messageObject)
		{
			if (NctsHeader == null)
			{
				ServiceLogger.Log(LogType.Error, FormattableString.Invariant($"No NCTS header found with ID {messageObject.MesIdeMes19}"));
			}
			else
			{
				if (NctsHeader.IsDepartureMovement)
				{
					var newDeclarationStatus = GetNewDepartureStatus(messageObject);
					if (!newDeclarationStatus.IsEmpty)
					{
						NctsHeader.MovementHeader.BM_CustomsStatus = newDeclarationStatus;
					}
					var newDetailedDepartureStatus = GetNewDetailedDepartureStatus(messageObject);
					if (!newDetailedDepartureStatus.IsEmpty)
					{
						NctsHeader.DetailedDepartureStatusCode = newDetailedDepartureStatus;
					}
				}

				if (NctsHeader.IsArrivalMovement)
				{
					var newArrivalStatus = GetNewArrivalStatus(messageObject);
					if (!newArrivalStatus.IsEmpty)
					{
						NctsHeader.ArrivalMovementHeader.BM_CustomsStatus = newArrivalStatus;
					}

					var newDetailedArrivalStatus = GetNewDetailedArrivalStatus(messageObject);
					if (!newDetailedArrivalStatus.IsEmpty)
					{
						NctsHeader.DetailedArrivalStatusCode = newDetailedArrivalStatus;
					}
				}

				var newMessageStatus = GetNewMessageStatus(messageObject);
				if (!newMessageStatus.IsEmpty)
				{
					NctsHeader.EffectiveMessageStatus = newMessageStatus;
				}
			}
		}

		protected virtual void UpdateEDIMessage(EDIMessage message, T messageObject)
		{
			var messageTypeFomProcessor = messageObject.MesTypMes20;
			if (messageTypeFomProcessor != null && messageTypeFomProcessor.Length >= 3)
			{
				if (messageTypeFomProcessor == "CCF15A")
				{
					message.EM_MessageType = "15F";
				}
				else
				{
					message.EM_MessageType = new ZString(messageTypeFomProcessor).KeepNumericCharacters();
				}
				message.EM_MessageSubType = MessageSubTypeList.Codes.DT;
				message.EM_Status = GetNewMessageStatus(messageObject);
				var messageInterpretation = GetMessageInterpretation(messageObject);
				if (!messageInterpretation.IsEmpty)
				{
					message.EM_MessageInterpretation = messageInterpretation;
				}
				NctsHeader.Messages.Add(message);
			}
		}
		internal virtual void UpdateGuaranteeTransactionsIfNeeded(T messageObject, EDIMessage inboundMessage) { }

		protected virtual void GenerateDocuments(EDIMessage inboundMessage)
		{
		}

		protected ZString GetEmailsubject(NctsHeader dataProvider)
		{
			var responseReference = dataProvider.BH_JobReference;
			var readableResponseReference = responseReference != ZString.Empty ? (ZString)Res.GetString("78004D73-1E6D-434E-827D-CF4F68254978", " Reference: {0}", responseReference) : ZString.Empty;

			return Res.GetString("943DD269-94F7-4BC3-A062-6CDA59578987", "New Transit response received.{0}", readableResponseReference);
		}

		protected static ZString GetEmailBody(string messageBody)
		{
			return Res.GetString("2F66C4EF-FAB4-4A2C-9002-7D6447048F7B", "A Transit response has been received. {0}.", messageBody);
		}

		protected static IRegistryItem GetEmailGroupRegistryItem() => FRCustomsDataRegistry.Instance.DeltaTResponseNotificationGroup;

		#region Message Interpretation

		protected virtual ZString GetMessageInterpretation(T messageObject)
		{
			return ZString.Empty;
		}

		protected ZString GetMessageStatusInterpretation(T messageObject)
		{
			return GetParagraphInterpretation(FormattableString.Invariant($"New message status: {new EU.NCTS.Business.NctsMessageStatusList().GetDescriptionFromCode(GetNewMessageStatus(messageObject))}"));
		}

		protected ZString GetDepartureStatusInterpretation(T messageObject)
		{
			return GetParagraphInterpretation(FormattableString.Invariant($"New departure status: {new NctsTransitStatusList().GetDescriptionFromCode(GetNewDepartureStatus(messageObject))}"));
		}

		protected ZString GetArrivalStatusInterpretation(T messageObject)
		{
			return GetParagraphInterpretation(FormattableString.Invariant($"New arrival status: {new NctsTransitStatusList().GetDescriptionFromCode(GetNewArrivalStatus(messageObject))}"));
		}

		protected ZString GetDetailedDepartureStatusInterpretation(T messageObject)
		{
			var rawText = new NctsDetailedStatusList().GetDescriptionFromCode(GetNewDetailedDepartureStatus(messageObject));
			return rawText != ZString.Empty ? GetParagraphInterpretation(FormattableString.Invariant($"New detailed departure status: {rawText}")) : ZString.Empty;
		}

		protected ZString GetDetailedArrivalStatusInterpretation(T messageObject)
		{
			var rawText = new NctsDetailedStatusList().GetDescriptionFromCode(GetNewDetailedArrivalStatus(messageObject));
			return rawText != ZString.Empty ? GetParagraphInterpretation(FormattableString.Invariant($"New detailed arrival status: {rawText}")) : ZString.Empty;
		}

		protected ZString GetDetailedStatusInterpretation(T messageObject)
		{
			return ZString.Empty;
		}

		protected ZString GetGrantedTimeInterpretation(T messageObject)
		{
			return GetParagraphInterpretation(FormattableString.Invariant($"Status granted on: {GetReadableStatusDateAndTime(messageObject.DatOfPreMes9, messageObject.TimOfPreMes10)}"));
		}

		protected ZString GetReadableStatusDateAndTime(string date, string time)
		{
			string dateTime = "20" + date + time;
			if (ZDateTime.TryParseExact(dateTime, out var result, "yyyyMMddhhmm"))
			{
				dateTime = result.ToString("dd/MM/yyyy hh:mm");
			}
			return dateTime;
		}

		protected ZString GetReadableDate(string date)
		{
			if (ZDateTime.TryParseExact(date, out var result, "yyyyMMdd"))
			{
				date = result.ToString("dd/MM/yyyy");
			}
			return date;
		}

		protected ZString GetKeyValuePairInterpretation(ZString key, ZString value)
		{
			return GetParagraphInterpretation(FormattableString.Invariant($"{key}: {value}"));
		}

		protected ZString GetParagraphInterpretation(ZString text)
		{
			return text.IsEmpty ? ZString.Empty : new ZString(FormattableString.Invariant($"<p>{text}</p>"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Html strings")]
		protected ZString GetFunctionalErrorInterpretation(CargoWise.Customs.FR.MessageDefinitions.DeltaT.INctsFunctionalErrorMessage messageObject)
		{
			var result = new ZStringBuilder();
			if (messageObject.Errors != null && messageObject.Errors.Any())
			{
				result.Append("<p>");
				foreach (var error in messageObject.Errors)
				{
					var explainedErrorPointer = error.ErrorPointer != null ? ZString.Format("{0} is in error", ExplainErrorPointer(error.ErrorPointer).Trim()) : ZString.Empty;
					var errorReason = error.ErrorReason != null ? ZString.Format(" : Infrigement to rule {0}", error.ErrorReason.Trim()) : ZString.Empty;
					var originalValue = error.ErrorOriginalValue != null ? ZString.Format(" - Original value : {0}", error.ErrorOriginalValue.Trim()) : ZString.Empty;
					result.Append(ZString.Format("{0}{1}{2}<br>", explainedErrorPointer, errorReason, originalValue));
				}
				result.Append("</p>");
			}
			return result.ToString();
		}

		/// <summary>
		///  Convert 'GUA(3).REF(5), Access code' into 'Access code of Guarantee Reference 5 in Guarantee 3
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "No translation required")]
		public string ExplainErrorPointer(ZString p)
		{
			var explanation = "";
			var errorPointersList = new ErrorPointers();
			p = p.ToUpperInvariant().Replace(",", ".");
			ZString[] members = p.Split('.');

			if (members.Length == 3)
			{
				var mainGroup = GetDataGroupItem(errorPointersList, members[0]);
				var subGroup = GetDataGroupItem(errorPointersList, members[1]);
				var dataItem = GetDataGroupItem(errorPointersList, members[2]);
				explanation = subGroup + " in " + mainGroup;
				if (dataItem != "")
				{
					explanation = dataItem + " of " + explanation;
				}
			}
			else if (members.Length == 2)
			{
				var dataGroup = GetDataGroupItem(errorPointersList, members[0]);
				var dataItem = GetDataGroupItem(errorPointersList, members[1]);
				explanation = dataGroup;
				if (dataItem != "")
				{
					explanation = dataItem + " of " + explanation;
				}
			}
			else if (members.Length == 1)
			{
				explanation = GetDataGroupItem(errorPointersList, members[0]);
			}

			return explanation;
		}

		ZString GetDataGroupItem(ErrorPointers errorPointersList, ZString dataGroupItemCode)
		{
			var explanation = ZString.Empty;

			if (!HasValidDataGroupPattern(dataGroupItemCode))
			{
				explanation = dataGroupItemCode;
			}
			else
			{
				var code = dataGroupItemCode.Left(3); // e.g. GUA
				explanation = errorPointersList.GetDescriptionFromCode(code);
				var number = "";

				if (!explanation.IsEmpty)
				{
					if (System.Text.RegularExpressions.Regex.IsMatch(dataGroupItemCode, @"[A-Za-z0-9]{3}(\(|\[)[0-9]{1}(\)|\])"))
					{
						number = " " + dataGroupItemCode.SubstringSafe(4, 1);
					}
					else if (System.Text.RegularExpressions.Regex.IsMatch(dataGroupItemCode, @"[A-Za-z0-9]{3}(\(|\[)[0-9]{2}(\)|\])"))
					{
						number = " " + dataGroupItemCode.SubstringSafe(4, 2);
					}
					explanation = explanation + number;
				}
			}
			return explanation;
		}

		static bool HasValidDataGroupPattern(ZString dataGroupItemCode)
		{
			return dataGroupItemCode.EndsWith(")", StringComparison.OrdinalIgnoreCase) || dataGroupItemCode.EndsWith("]", StringComparison.OrdinalIgnoreCase) || (dataGroupItemCode.Length == 3 && new ErrorPointers().ContainsCode(dataGroupItemCode));
		}

		#endregion

		public NctsHeader NctsHeader;
		protected readonly ILogger ServiceLogger;
	}
}
