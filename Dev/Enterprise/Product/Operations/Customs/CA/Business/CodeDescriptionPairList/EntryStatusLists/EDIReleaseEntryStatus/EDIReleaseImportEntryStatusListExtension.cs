namespace Enterprise.Customs.CA.Business
{
	using System.Linq;
	using Enterprise.Customs.Common.Shared;
	using Enterprise.Edifact.D96A.Elements;

	public class EDIReleaseImportEntryStatusList : Common.CA.EDIReleaseImportEntryStatusList
	{
		public static string GetEntryStatusByProcessingIndicatorCoded(string processingIndicator, string messageSubType = MessageSubTypeCodes.Codes.Original)
		{
			return GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList.GetFromString(processingIndicator), messageSubType);
		}

		public static string GetEntryStatusByProcessingIndicatorCoded(ProcessingIndicatorCodedList processingIndicator, string messageSubType = MessageSubTypeCodes.Codes.Original)
		{
			var result = string.Empty;
			if (processingIndicator == ProcessingIndicatorCodedList.MessageContentAccepted) // 1
			{
				result = Codes.MessageContentAccepted;
			}
			else if (processingIndicator == ProcessingIndicatorCodedList.MessageContentRejectedWithComment) // 2
			{
				result = Codes.MessageContentRejected;
			}
			else if (processingIndicator == ProcessingIndicatorCodedList.GoodsReleased) // 4
			{
				result = Codes.GoodsReleased;
			}
			else if (processingIndicator == ProcessingIndicatorCodedList.GoodsRequiredForExamination) // 5
			{
				result = Codes.GoodsRequiredForExamination;
			}
			else if (processingIndicator == ProcessingIndicatorCodedList.AllDocumentsOrAsSpecifiedToBeProduced) // 6
			{
				result = Codes.Y51ReleaseDocumentsRequired;
			}
			else if (processingIndicator == ProcessingIndicatorCodedList.GoodsDetained) // 7
			{
				result = Codes.GoodsDetained;
			}
			else if (processingIndicator == ProcessingIndicatorCodedList.GoodsMayMoveUnderCustomsTransfer) // 8
			{
				result = Codes.GoodsMayMove;
			}
			else if (processingIndicator == ProcessingIndicatorCodedList.DeclarationAcceptedAwaitingGoodsArrival) // 9
			{
				result = IsCancellation(messageSubType) ? Codes.Cancelled : Codes.DeclarationAccepted;
			}
			else if (processingIndicator == ProcessingIndicatorCodedList.Transit) // 24
			{
				result = Codes.Transit;
			}
			else if (processingIndicator == ProcessingIndicatorCodedList.ErrorMessage) // 14
			{
				result = Codes.Error;
			}
			else if (processingIndicator == ProcessingIndicatorCodedList.TransactionAwaitingProcessing) //34
			{
				result = Codes.AwaitingCustomsProcessing;
			}
			else if (processingIndicator == ProcessingIndicatorCodedList.Import) //23
			{
				result = Codes.AuthorisedToDeliver;
			}
			else if (processingIndicator == ProcessingIndicatorCodedList.DeclarationAcceptedWithWarning) //128
			{
				result = Codes.AcceptedWithWarning;
			}

			return result;
		}

		internal static bool IsCancellation(string messageSubType)
		{
			return messageSubType == MessageSubTypeCodes.Codes.Cancellation || messageSubType == Codes.Cancelled;
		}

		internal static bool IsError(string entryStatus)
		{
			return new[] { Codes.Error, Codes.SyntaxError, Codes.MessageContentRejected }.Contains(entryStatus);
		}
	}
}
