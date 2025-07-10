using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	sealed class CusEntryHeaderOutboundMessageHeaderProvider : IJPOutboundMessageHeader
	{
		public CusEntryHeaderOutboundMessageHeaderProvider(JobDeclarationMessageSendingObject baseSendingObject)
		{
			var header = Argument.NotNull(baseSendingObject?.Header, nameof(JobDeclarationMessageSendingObject.Header));
			var declaration = Argument.NotNull((JobDeclaration)header.Declaration, nameof(CusEntryHeader.Declaration));

			InputReference = ((IInputReferenceProvider)header).InputReference;
			MessageTag = EDIMessage.MessageNumberPlaceHolder;

			var credential = declaration.Credential;
			if (credential != null)
			{
				UserCode = credential.GP_MailBoxID;
				UserId = credential.GP_UserID;
				UserPassword = EDIMessage.PasswordPlaceHolder;
			}

			if (baseSendingObject is MessageSendingObject sendingObject)
			{
				ProcedureCode = sendingObject.ProcedureCode;
			}
			else if (baseSendingObject is MSXMessageSendingObject)
			{
				ProcedureCode = JPProcedureCodeList.Codes.MSX;
			}

			if (declaration.IsAir)
			{
				SystemType = "1";
			}
			else if (declaration.IsSea)
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
