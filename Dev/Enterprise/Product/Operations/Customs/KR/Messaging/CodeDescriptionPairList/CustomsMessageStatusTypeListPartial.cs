using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Messaging
{
	partial class CustomsMessageStatusTypeList : IStatusList
	{
		public static bool IsMessageAccepted(string code)
		{
			return code == Codes.OriginalAccepted
				|| code == Codes.AmendmentAccepted
				|| code == Codes.CancellationAccepted
				|| code == Codes.CancellationApprovedByCustoms;
		}

		public static bool IsMessageRejectedOrFailToDeliver(string code)
		{
			return code == Codes.ErrorSendingOriginal
				|| code == Codes.ErrorSendingAmendment
				|| code == Codes.ErrorSendingCancellation
				|| code == Codes.OriginalRejected
				|| code == Codes.AmendmentRejected
				|| code == Codes.CancellationRejected;
		}

		public static bool IsCancellationStatus(string code)
		{
			return code == Codes.CancellationSent
				|| code == Codes.CancellationRejected
				|| code == Codes.CancellationAccepted
				|| code == Codes.ErrorSendingCancellation
				|| code == Codes.CancellationApprovedByCustoms;
		}

		public static bool IsOriginalMessageAllowed(string code)
		{
			return GetCodesToAllowOriginalMessage().Contains(code);
		}

		public static string[] GetCodesToAllowOriginalMessage()
		{
			return new string[] { string.Empty, Codes.ErrorSendingOriginal, Codes.OriginalRejected };
		}
		public static bool IsAmendmentOrCancellationMessageAllowed(string code)
		{
			return code == Codes.ErrorSendingAmendment
				|| code == Codes.ErrorSendingCancellation
				|| code == Codes.OriginalAccepted
				|| code == Codes.AmendmentRejected
				|| code == Codes.AmendmentAccepted
				|| code == Codes.CancellationRejected
				|| code == Codes.CancellationAccepted //Cancellation request can be declined by Customs after review
				|| code == Codes.CancellationDeclined;
		}

		public static bool IsWaitingForResponse(string code)
		{
			return code == Codes.AmendmentSent
				|| code == Codes.OriginalSent
				|| code == Codes.CancellationSent;
		}

		public static string[] GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(string messageType)
		{
			switch (messageType)
			{
				case ElectronicDocumentTypeList.Codes._5DR:
				case ElectronicDocumentTypeList.Codes._5DS:
				case ElectronicDocumentTypeList.Codes._DF3:
					return new string[] { Codes.OriginalAccepted, Codes.AmendmentAccepted, Codes.CancellationAccepted };
				case ElectronicDocumentTypeList.Codes._5FN:
				case ElectronicDocumentTypeList.Codes._5SC:
				case ElectronicDocumentTypeList.Codes._DHR:
				case ElectronicDocumentTypeList.Codes._5BD:
				case ElectronicDocumentTypeList.Codes._5SG:
				case ElectronicDocumentTypeList.Codes._5SI:
				case ElectronicDocumentTypeList.Codes._5BA:
				case ElectronicDocumentTypeList.Codes._5TE:
				case ElectronicDocumentTypeList.Codes._5TM:
				case ElectronicDocumentTypeList.Codes._5UL:
				case ElectronicDocumentTypeList.Codes._D72:
					return new string[] { Codes.AmendmentAccepted, Codes.CancellationAccepted };
				case ElectronicDocumentTypeList.Codes._5AS:
				case ElectronicDocumentTypeList.Codes._5FE:
					return new string[] { Codes.CancellationAccepted };
				case ElectronicDocumentTypeList.Codes._DKJ:
				case ElectronicDocumentTypeList.Codes._5BF:
					return new string[] { Codes.AmendmentAccepted };
				default:
					return System.Array.Empty<string>();
			}
		}

		public static bool IsAmendmentMessageSentOrAccepted(string code)
		{
			return code == Codes.AmendmentSent
				|| code == Codes.AmendmentAccepted;
		}

		bool IStatusList.ShouldUsersBeWarnedPriorToPrintingDocument(string code)
		{
			return code == Codes.OriginalRejected
				|| code == Codes.AmendmentRejected
				|| code == Codes.CancellationRejected;
		}

		string IStatusList.GetDocumentPrintingWarningMessage()
		{
			return Res.GetString("8A7613E0-93B9-4F07-A5E2-C0EBBA1EDD9B", "This document has been rejected by Customs.");
		}

		public static string GetErrorStatus(string status)
		{
			var result = string.Empty;
			switch (status)
			{
				case CustomsMessageStatusTypeList.Codes.OriginalSent:
					result = CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal;
					break;
				case CustomsMessageStatusTypeList.Codes.AmendmentSent:
					result = CustomsMessageStatusTypeList.Codes.ErrorSendingAmendment;
					break;
				case CustomsMessageStatusTypeList.Codes.CancellationSent:
					result = CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation;
					break;
			}
			return result;
		}
		public static CodeDescriptionPairList GetStatusListForRefundDeclaration(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CustomsMessageStatusTypeList_GetStatusListForRefundDeclaration", () =>
			{
				var result = new UntranslatableCodeDescriptionPairList((NoResString)Constants.Lists.KrOnlyTest);
				result.AddPair(Codes.ErrorSendingOriginal, Descriptions.ErrorSendingOriginal);
				result.AddPair(Codes.OriginalSent, Descriptions.OriginalSent);
				result.AddPair(Codes.OriginalRejected, Descriptions.OriginalRejected);
				result.AddPair(Codes.OriginalAccepted, Descriptions.OriginalAccepted);
				result.AddPair(Codes.CancellationByCustoms, Descriptions.CancellationByCustoms);
				return result;
			});
		}
	}
}
