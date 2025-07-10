using CargoWise.Integration;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IMessageInstructions
	{
		string DocumentName { get; }
		string TranslatedDocumentName { get; }
		string DataContext { get; }
		string Recipient { get; }
		string EHubClientID { get; }
		string DirectXTClientID { get; }
		string XmlNamespace { get; }
		bool AllowSendMessage { get; }
		bool AllowSendMessageAmendment { get; }
		bool AllowSendMessageWithdrawal { get; }
		bool AllowResetToOriginal { get; }
		bool OrderLogsByLocalTime { get; }
		bool RequireMessageAmendmentReason { get; }
		ICodeDescriptionPairList AmendmentOptions { get; }
		ICodeDescriptionPairList WidthdrawalOptions { get; }
	}
}
