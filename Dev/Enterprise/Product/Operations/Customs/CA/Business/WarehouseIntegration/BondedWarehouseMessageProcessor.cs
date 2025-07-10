using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class BondedWarehouseMessageProcessor : BondedWarehouseDeclarationMessageProcessor
	{
		internal BondedWarehouseMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef> sendMail)
			: base(messagePK, emailReportThatHasBeenDelayed)
		{
			this.sendMail = Argument.NotNull(sendMail, "sendMail");
		}
		readonly Action<EmailDef> sendMail;

		protected CusEntryHeader entryHeader
		{
			get { return (CusEntryHeader)message.EM_LinkedObject; }
		}

		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		protected override bool HasBeenWithdrawn
		{
			get { return entryHeader.CH_Status == MessageStatusList.Codes.ClearDelete; }
		}

		protected override bool IsAmendmentError
		{
			get { return entryHeader.CH_Status == MessageStatusList.Codes.ErrorChange; }
		}

		protected override bool IsAmendmentClear
		{
			get { return entryHeader.CH_Status == MessageStatusList.Codes.ClearChange; }
		}

		protected override bool IsOriginalError
		{
			get { return entryHeader.CH_Status == MessageStatusList.Codes.ErrorOriginal; }
		}

		protected override bool IsWithdrawalError
		{
			get { return entryHeader.CH_Status == MessageStatusList.Codes.ErrorDelete; }
		}

		protected override void SendEmailCore(EmailDef email)
		{
			if (sendMail != null)
			{
				sendMail(email);
			}
		}

		protected override string GetReferenceDetail()
		{
			return Res.GetString("27c22880-d99a-4733-9af8-4c32ac6057a1", @" <strong>Job Number : {0}<br />
Reference Number : {1}<br />
<br />", EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference), entryHeader.CH_BGMReference);
		}
	}
}
