using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.CusDec;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Customs.GB.Registry;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using CusEntryHeader = Enterprise.Customs.EU.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.GB.Chief
{
	public abstract class GbChiefEdifactTransmissionMessageGenerator : GbTransmissionMessageGenerator
	{
		protected GbChiefEdifactTransmissionMessageGenerator(Customs.Business.CusdecMessageFunction declarationMessageFunction)
		{
			this.declarationMessageFunction = declarationMessageFunction;
		}

		public override IBuilderResult Generate(CusEntryHeader entryHeader)
		{
			Entry = entryHeader;
			errorCollector = new ErrorCollector();

			AddPermitRecordsIfNeeded(entryHeader);
			GenerateMessageTextButDoNotSave(entryHeader);
			CheckMaximumPayloadSizeForChief();

			if (errorCollector.GetErrors(ErrorCollector.ErrorType.NONCRTICAL).Any())
			{
				var continueToSend = SendMessagesToCustoms?.AskUserToContinueWithAction(errorCollector.GetErrorsAsString(ErrorCollector.ErrorType.NONCRTICAL), "Permit Warnings", entryHeader);

				if (!continueToSend.HasValue || !continueToSend.Value)
				{
					//turn warnings into errors
					errorCollector.AddError("You selected not to generate the message because of the following problem(s) with permits:");
					foreach (var error in errorCollector.GetErrors(ErrorCollector.ErrorType.NONCRTICAL).ToArray())
					{
						errorCollector.AddError(error);
					}
				}
			}

			var result = new BuilderResult(entryHeader, errorCollector.GetErrors(ErrorCollector.ErrorType.CRITICAL), base.AfterFullSuccess);
			return SaveResultAsNewMessageUponSuccess(entryHeader, result);
		}

		void AddPermitRecordsIfNeeded(CusEntryHeader entryHeader)
		{
			if (IsNewMessage(entryHeader) || IsAmendMessage(entryHeader))
			{
				try
				{
					PermitProcessor.AddPermitRecordsAndLockMutexIfNeeded();
				}
				finally
				{
					PermitProcessor?.UnlockPermitMutexes();
				}

				foreach (var error in PermitProcessor.ErrorList.Distinct())
				{
					errorCollector.AddError(error, false);
				}
			}
		}

		protected virtual void CheckMaximumPayloadSizeForChief()
		{
			var sizeOfPayload = generatedMessageText.Length;
			// THese lengths taken from real interchanges, with the interchange control reference (i.e interchange number) padded to the maximum permissible value.
			// Annoyingly EDIInterchangeSchema.EI_headerText.MaxLength gives int.Maximum (2 trillion or something). 
			var maxSizeOfHeader = "UNB+UNOA:2+CUKFFW98000CAR:IATA+CUKCTM98CHFIMP:IATA+100624:1348+12345678901234'".Length;
			var maxSizeOfFooter = "UNZ+1+12345678901234".Length;  // Interchange control reference = 14 chars long
			var totalSize = sizeOfPayload + maxSizeOfFooter + maxSizeOfHeader;
			var chiefMaxSize = GBCustomsDataRegistry.Instance.ChiefMaximumMessageSizeInDecimalBytes.Value;
			if (totalSize > chiefMaxSize)
			{
				errorCollector.AddError(string.Format(CultureInfo.CurrentCulture, "The message that has been generated is {0} characters in length. CHIEF supports messages up to {1} characters.\r\n\tThe declaration needs to be adjusted to ensure a smaller message (e.g. shorter description of goods, fewer lines, merge).", totalSize, chiefMaxSize),
					new ErrorInfo("", "", false));
			}
		}

		protected virtual void GenerateMessageTextButDoNotSave(CusEntryHeader entryHeader)
		{
			generatedMessageText = string.Empty;
			em_MessageType = string.Empty;
			messageSubType = string.Empty;
			var queryMessageFunction = declarationMessageFunction as QueryMessageFunction;
			var mucrOrInventoryMessageFunction = declarationMessageFunction as GbDes242MessageFunction;
			if (queryMessageFunction == null && mucrOrInventoryMessageFunction == null)
			{
				// Regular D04A full CUSDEC
				var chooser = new CusDecFunctionChooser(entryHeader, this.declarationMessageFunction);
				var chiefCusDecEngine = new CusDecCreator(entryHeader.Declaration, entryHeader, chooser.Function, errorCollector);
				em_MessageType = chooser.MessageSubType;
				generatedMessageText = chiefCusDecEngine.CreateEdifactString(CharSet);
				messageSubType = chooser.MessageTypeForEdiMessage;
			}
			else if (mucrOrInventoryMessageFunction != null)
			{
				// UKCINV messages - EAC (ass/dis/close) etc
				var cusDecCreatorUkcinv = new CusDecCreatorUkcinv(entryHeader.Declaration, entryHeader, mucrOrInventoryMessageFunction, errorCollector);
				em_MessageType = mucrOrInventoryMessageFunction.FunctionCode;
				generatedMessageText = cusDecCreatorUkcinv.Create(CharSet);
				messageSubType = mucrOrInventoryMessageFunction.SubFunctionCode;
			}
			else if (queryMessageFunction != null)
			{
				// Special messages, e.g. D04A LEM/DEM/etc CUSDEC messages - querying chief
				var chiefCusDecEngine = new CusDecCreator(entryHeader.Declaration, entryHeader, queryMessageFunction, errorCollector);
				em_MessageType = queryMessageFunction.FunctionCode;
				generatedMessageText = chiefCusDecEngine.CreateInterrogationMessage().ToString(CharSet);
				messageSubType = queryMessageFunction.SubFunction;
			}
		}

		protected virtual IBuilderResult SaveResultAsNewMessageUponSuccess(CusEntryHeader entryHeader, BuilderResult result)
		{
			result.Message = GetNewEdiMessage(entryHeader);
			var applicationCode = GetApplicationCode(entryHeader);
			result.Message.EM_ApplicationCode = applicationCode;
			result.Message.MessageNumberStrategy = new GbMessageNumberStrategy(result.Message.Factory, applicationCode);
			result.Message.EM_SendWithMessageErrors = entryHeader.Declaration.HasErrors || entryHeader.Declaration.HasMessageErrors;
			result.Message.EM_ApplicationReference = GetApplicationReference(entryHeader) + MessageTypeAddition;
			result.Message.EM_LinkedObject = entryHeader;
			result.Message.EM_MessageOwner = entryHeader.Declaration.JE_CustomsProfile.Left(EDIMessage.Schema.EM_MessageOwnerMaxLength);
			result.Message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			result.Message.EM_IsTestMessage = entryHeader.Declaration.ZG_IsTrainingDeclaration;
			result.Message.EM_MessageText = generatedMessageText;
			SetMessageTypesAndStatus(result);
			return result;
		}

		protected virtual void SetMessageTypesAndStatus(BuilderResult result)
		{
			result.Message.EM_MessageSubType = messageSubType;
			result.Message.EM_MessageType = em_MessageType;
			result.Message.EM_Status = EDIMessage.Status.Queued;
		}

		protected virtual EDIMessage GetNewEdiMessage(CusEntryHeader entry)
		{
			return entry.Factory.New<GbEDIMessage>();
		}

		protected abstract ZString GetApplicationCode(Customs.Business.CusEntryHeader entry);

		public override ZString MakePrettyForInterpretation(EDIMessage message)
		{
			return new GbEdifactPrettier(message.EM_MessageText, CharSet, declarationMessageFunction).MakeHumanReadable();
		}

		protected abstract ZString GetApplicationReference(CusEntryHeader entryHeader);

		protected virtual UNCharacterSet CharSet => new UkCharSet();

		protected virtual ZString MessageTypeAddition
		{
			get
			{
				ZString messageTypeAddition = ChiefConstants.MessageTypeSeparatorInApplicationReference;

				switch (declarationMessageFunction)
				{
					case Customs.Business.CusdecMessageFunction.Amended _:
						messageTypeAddition += GBMessageTypeList.Codes.Amend;
						break;
					case Customs.Business.CusdecMessageFunction.Deleted _:
						messageTypeAddition += GBMessageTypeList.Codes.Cancel;
						break;
					case Customs.Business.CusdecMessageFunction.New _:
						messageTypeAddition += GBMessageTypeList.Codes.New;
						break;
					default:
						messageTypeAddition = ZString.Empty;
						break;
				}

				return messageTypeAddition;
			}
		}

		protected ErrorCollector errorCollector;
		protected string generatedMessageText;
		string em_MessageType;
		string messageSubType;
	}
}
