using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyMessageInstructions : IMessageInstructions
	{
		public string DocumentName { get; set; }
		public string TranslatedDocumentName { get; set; }
		public string DataContext { get; set; }
		public string Recipient { get; set; }
		public string EHubClientID { get; set; }
		public string DirectXTClientID { get; set; }
		public string XmlNamespace { get; set; }
		public bool AllowSendMessage { get; set; }
		public bool AllowSendMessageWithdrawal { get; set; }
		public bool AllowResetToOriginal { get; set; }
		public bool RequireMessageAmendmentReason { get; set; }
		public ICodeDescriptionPairList AmendmentOptions { get; set; }
		public ICodeDescriptionPairList WidthdrawalOptions { get; set; }
		public bool OrderLogsByLocalTime { get; set; }
		public bool AllowSendMessageAmendment { get; set; }
	}
}
