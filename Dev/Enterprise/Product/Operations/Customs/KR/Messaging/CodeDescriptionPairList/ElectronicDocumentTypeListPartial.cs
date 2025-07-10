using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Messaging
{
	partial class ElectronicDocumentTypeList
	{
		public static string[] GetOriginalFTAMessageTypes() => new string[] { Codes._5SC, Codes._DHR };

		public static bool IsMiscDeclaration(string code)
		{
			return code == Codes._D87
				|| code == Codes._008
				|| code == Codes._5SM;
		}

		public static bool SupportsAmendment(string code)
		{
			return code == Codes._830
				|| code == Codes._929
				|| code == Codes._5BA
				|| code == Codes._5SC
				|| code == Codes._DHR
				|| code == Codes._5DQ
				|| code == Codes._5DP;
		}

		public static bool SupportsCancellation(string code)
		{
			return code == Codes._830
				|| code == Codes._929
				|| code == Codes._5DQ
				|| code == Codes._5DP;
		}

		/// <summary>
		/// A subsequent message should not be sent until a mandatory review message arrives
		/// </summary>
		public static string MandatoryCustomsApprovalMessageFor(string outgoingMessageType)
		{
			switch (outgoingMessageType)
			{
				case Codes._5DP:
				case Codes._5DQ:
				case Codes._5DR:
				case Codes._5DS:
					return Codes._RR3;
				case Codes._5AS:
				case Codes._DKJ:
					return Codes._5DT;
				case Codes._5FE:
					return Codes._5FK;
				case Codes._105:
				case Codes._DHS:
					return Codes._106;
				case Codes._5BF:
					return Codes._5BG;
				case Codes._5SG:
					return Codes._5SH;
				case Codes._5BB:
					return Codes._5BC;
				case Codes._5TE:
					return Codes._5TF;
				case Codes._5UL:
					return Codes._RCA;
				case Codes._D72:
					return Codes._R43;
				case Codes._5UA:
					return Codes._5UB;
				default:
					return string.Empty;
			}
		}

		public static ZString[] GetOutgoingMessagesToReceiveCustomsReviewMessage(string customsReviewMessage)
		{
			switch (customsReviewMessage)
			{
				case Codes._RR3:
					return new ZString[] { Codes._5DP, Codes._5DQ, Codes._5DR, Codes._5DS };
				case Codes._5DT:
					return new ZString[] { Codes._5AS, Codes._DKJ };
				case Codes._5FK:
					return new ZString[] { Codes._5FE };
				case Codes._106:
					return new ZString[] { Codes._105, Codes._DHS };
				case Codes._5BG:
					return new ZString[] { Codes._5BF };
				case Codes._5SH:
					return new ZString[] { Codes._5SG };
				case Codes._5BC:
					return new ZString[] { Codes._5BB };
				case Codes._5TF:
					return new ZString[] { Codes._5TE };
				case Codes._RCA:
					return new ZString[] { Codes._5UL };
				case Codes._R43:
					return new ZString[] { Codes._D72 };
				default:
					return System.Array.Empty<ZString>();
			}
		}

		public static bool IsCustomsReviewMessage(string code)
		{
			return code == Codes._RR3
				|| code == Codes._5DT
				|| code == Codes._5FK
				|| code == Codes._106
				|| code == Codes._5BG
				|| code == Codes._5SH
				|| code == Codes._5BC
				|| code == Codes._5TF
				|| code == Codes._RCA
				|| code == Codes._R43;
		}

		public static bool IsAmendment(string code)
		{
			return code == Codes._5AS
				|| code == Codes._5FE
				|| code == Codes._5BB
				|| code == Codes._105
				|| code == Codes._DHS
				|| code == Codes._5DS
				|| code == Codes._5DR;
		}

		public static bool IsCancellation(string code)
		{
			return code == Codes._DKJ
				|| code == Codes._5BF
				|| code == Codes._5DS
				|| code == Codes._5DR;
		}

		/// <summary>
		/// Supplementary means a message additional to a main declaration or its amendment/cancellation messages
		/// </summary>
		public static bool IsSupplementaryOutgoingMessage(string code)
		{
			return code == Codes._5AC
				|| code == Codes._DF3
				|| code == Codes._934
				|| code == Codes._5FN
				|| code == Codes._5SC
				|| code == Codes._105
				|| code == Codes._DHR
				|| code == Codes._DHS
				|| code == Codes._5GW
				|| code == Codes._5BD
				|| code == Codes._5SG
				|| code == Codes._5SI
				|| code == Codes._5BA
				|| code == Codes._5BB
				|| code == Codes._5TE
				|| code == Codes._5TM
				|| code == Codes._5UA
				|| code == Codes._5UL
				|| code == Codes._D72;
		}

		public static bool GenerateSnapshot(string code)
		{
			return SupportsAmendment(code) || code == ElectronicDocumentTypeList.Codes._5UL;
		}

		public static bool IsOriginalOrSupplementaryOriginalMessage(string code)
		{
			return SupportsAmendment(code)
				|| code == Codes._008
				|| code == Codes._5SM
				|| code == Codes._D87
				|| IsSupplementaryOutgoingMessage(code) && !IsAmendment(code);
		}

		public static bool CanSendBeforeDeclarationIsAccepted(string supplementaryOriginalMessageType)
		{
			return supplementaryOriginalMessageType == Codes._934;
		}

		public static string GetMainOriginalMessageTypeFor(string supplementaryOriginalMessageType)
		{
			if (IsSupplementaryOutgoingMessage(supplementaryOriginalMessageType) && !IsAmendment(supplementaryOriginalMessageType) && !IsCancellation(supplementaryOriginalMessageType))
			{
				switch (supplementaryOriginalMessageType)
				{
					case Codes._DF3:
						return Codes._5DQ;
					case Codes._5AC:
						return Codes._830;
					default:
						return Codes._929;
				}
			}
			return string.Empty;
		}

		public static string GetOriginalType(string code)
		{
			string result = string.Empty;
			if (IsAmendment(code) || IsCancellation(code))
			{
				switch (code)
				{
					case Codes._5AS:
					case Codes._DKJ:
						result = Codes._830;
						break;
					case Codes._5FE:
					case Codes._5BF:
						result = Codes._929;
						break;
					case Codes._5BB:
						result = Codes._5BA;
						break;
					case Codes._105:
						result = Codes._5SC;
						break;
					case Codes._DHS:
						result = Codes._DHR;
						break;
					case Codes._5DS:
						result = Codes._5DQ;
						break;
					case Codes._5DR:
						result = Codes._5DP;
						break;
				}
			}
			return result;
		}

		public static string[] GetMainMessageTypeLeadingToCustomsReviewMessage(string messageType)
		{
			switch (messageType)
			{
				case ElectronicDocumentTypeList.Codes._5DR:
					return new string[] { Codes._5DP };
				case ElectronicDocumentTypeList.Codes._5DS:
					return new string[] { Codes._5DQ };
				case ElectronicDocumentTypeList.Codes._DF3:
					return new string[] { Codes._5DP, Codes._5DQ, Codes._5DS, Codes._5DR, };
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
					return new string[] { Codes._5FE, Codes._5BF, };
				case ElectronicDocumentTypeList.Codes._5BF:
					return new string[] { Codes._5FE };
				case ElectronicDocumentTypeList.Codes._5FE:
					return new string[] { Codes._5BF };
				case ElectronicDocumentTypeList.Codes._5AS:
					return new string[] { Codes._DKJ };
				case ElectronicDocumentTypeList.Codes._DKJ:
					return new string[] { Codes._5AS };
				default:
					return System.Array.Empty<string>();
			}
		}

		public static bool IsExportOrLocalExportOutgoingMessage(string messageType)
		{
			switch (messageType)
			{
				case Codes._830:
				case Codes._5AS:
				case Codes._DKJ:
				case Codes._5DP:
				case Codes._5DQ:
				case Codes._5DR:
				case Codes._5DS:
				case Codes._5AC:
				case Codes._DF3:
					return true;
				default:
					return false;
			}
		}
		public static bool IsLocalExportAmendmentOrCancellationMessage(string code)
		{
			return code == Codes._5DS
				|| code == Codes._5DR;
		}

		public static string GetMessageTypeSettingEntryToCancellationApprovedByCustoms(string entryType)
		{
			switch (entryType)
			{
				case Codes._5DP:
				case Codes._5DQ:
					return Codes._RR3;
				case JobMessageTypeList.Codes.Export:
					return Codes._5DT;
				case JobMessageTypeList.Codes.Import:
					return Codes._5BG;
				case Codes._5SM:
					return Codes._5SN;
				default:
					return string.Empty;
			}
		}

		public static string GetMessageTypeSettingEntryToCancellationByCustoms(string entryType)
		{
			switch (entryType)
			{
				case Codes._5DP:
				case Codes._5DQ:
					return Codes._RR3;
				case JobMessageTypeList.Codes.Import:
					return Codes._023;
				default:
					return string.Empty;
			}
		}
	}
}
