using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCMessageType : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Amend = EDIMessageSubTypeList.Codes.NEXDOCAmend;
			public const string CancelEDN = EDIMessageSubTypeList.Codes.NEXDOCCancelEDN;
			public const string Cancellation = EDIMessageSubTypeList.Codes.NEXDOCCancellation;
			public const string Lodge = EDIMessageSubTypeList.Codes.NEXDOCLodge;
			public const string ManualAmend = EDIMessageSubTypeList.Codes.NEXDOCManualAmend;
			public const string Order = EDIMessageSubTypeList.Codes.NEXDOCOrder;
			public const string PreviewCertificate = EDIMessageSubTypeList.Codes.NEXDOCPreviewCertificate;
			public const string ReadREX = EDIMessageSubTypeList.Codes.NEXDOCReadRex;
			public const string ReissueCertificate = EDIMessageSubTypeList.Codes.NEXDOCReissueCertificate;
			public const string ReplacementCertificate = EDIMessageSubTypeList.Codes.NEXDOCReplacementCertificate;
			public const string REXForward = EDIMessageSubTypeList.Codes.NEXDOCREXForward;
			public const string REXTransfer = EDIMessageSubTypeList.Codes.NEXDOCREXTransfer;
			public const string TransferEDN = EDIMessageSubTypeList.Codes.NEXDOCTransferEDN;
			public const string Withdrawal = EDIMessageSubTypeList.Codes.NEXDOCWithdrawal;
			public const string WithdrawalOwnership = EDIMessageSubTypeList.Codes.NEXDOCWithdrawalOwnership;
		}

		public static class Descriptions
		{
			public static MultilingualString Amend => EDIMessageSubTypeList.Descriptions.NEXDOCAmend;
			public static MultilingualString CancelEDN => EDIMessageSubTypeList.Descriptions.NEXDOCCancelEDN;
			public static MultilingualString Cancellation => EDIMessageSubTypeList.Descriptions.NEXDOCCancellation;
			public static MultilingualString Lodge => EDIMessageSubTypeList.Descriptions.NEXDOCLodge;
			public static MultilingualString ManualAmend => EDIMessageSubTypeList.Descriptions.NEXDOCManualAmend;
			public static MultilingualString Order => EDIMessageSubTypeList.Descriptions.NEXDOCOrder;
			public static MultilingualString PreviewCertificate => EDIMessageSubTypeList.Descriptions.NEXDOCPreviewCertificate;
			public static MultilingualString ReadREX => EDIMessageSubTypeList.Descriptions.NEXDOCReadRex;
			public static MultilingualString ReissueCertificate => EDIMessageSubTypeList.Descriptions.NEXDOCReissueCertificate;
			public static MultilingualString ReplacementCertificate => EDIMessageSubTypeList.Descriptions.NEXDOCReplacementCertificate;
			public static MultilingualString REXForward => EDIMessageSubTypeList.Descriptions.NEXDOCREXForward;
			public static MultilingualString REXTransfer => EDIMessageSubTypeList.Descriptions.NEXDOCREXTransfer;
			public static MultilingualString TransferEDN => EDIMessageSubTypeList.Descriptions.NEXDOCTransferEDN;
			public static MultilingualString Withdrawal => EDIMessageSubTypeList.Descriptions.NEXDOCWithdrawal;
			public static MultilingualString WithdrawalOwnership => EDIMessageSubTypeList.Descriptions.NEXDOCWithdrawalOwnership;
		}

		public NEXDOCMessageType()
		{
			AddPair(Codes.Amend, Descriptions.Amend);
			AddPair(Codes.CancelEDN, Descriptions.CancelEDN);
			AddPair(Codes.Cancellation, Descriptions.Cancellation);
			AddPair(Codes.Lodge, Descriptions.Lodge);
			AddPair(Codes.ManualAmend, Descriptions.ManualAmend);
			AddPair(Codes.Order, Descriptions.Order);
			AddPair(Codes.PreviewCertificate, Descriptions.PreviewCertificate);
			AddPair(Codes.ReadREX, Descriptions.ReadREX);
			AddPair(Codes.ReissueCertificate, Descriptions.ReissueCertificate);
			AddPair(Codes.ReplacementCertificate, Descriptions.ReplacementCertificate);
			AddPair(Codes.REXForward, Descriptions.REXForward);
			AddPair(Codes.REXTransfer, Descriptions.REXTransfer);
			AddPair(Codes.TransferEDN, Descriptions.TransferEDN);
			AddPair(Codes.Withdrawal, Descriptions.Withdrawal);
			AddPair(Codes.WithdrawalOwnership, Descriptions.WithdrawalOwnership);
		}

		public static bool IsSoapMessageType(string messageType) =>
			messageType == Codes.REXForward ||
			messageType == Codes.REXTransfer ||
			messageType == Codes.WithdrawalOwnership;
	}
}
