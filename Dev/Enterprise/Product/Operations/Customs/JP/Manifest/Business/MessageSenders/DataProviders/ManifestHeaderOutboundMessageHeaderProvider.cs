using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Manifest.Business
{
	sealed class ManifestHeaderOutboundMessageHeaderProvider : IJPOutboundMessageHeader
	{
		public ManifestHeaderOutboundMessageHeaderProvider(ManifestMessageSendingObject sendingObject)
		{
			var header = Argument.NotNull(sendingObject?.Bill?.Header, nameof(AsycudaBill.Header));

			InputReference = ((IInputReferenceProvider)header).InputReference;
			MessageTag = EDIMessage.MessageNumberPlaceHolder;
			ProcedureCode = sendingObject.MessageType;

			var credential = header.CustomsAgentCredential;
			if (credential != null)
			{
				UserCode = credential.GP_MailBoxID;
				UserId = credential.GP_UserID;
				UserPassword = EDIMessage.PasswordPlaceHolder;
			}
			if (header.IsAir)
			{
				SystemType = "1";
			}
			else if (header.IsSea)
			{
				SystemType = "2";
			}
		}

		public string ProcedureCode { get; set; }
		public string UserCode { get; set; }
		public string UserId { get; set; }
		public string UserPassword { get; set; }
		public string MessageTag { get; set; }
		public string InputReference { get; set; }
		public string SplitReference { get; set; }
		public string SystemType { get; set; }
	}
}
