using System;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class BondedWarehouseMessageProcessor : BondedWarehouseEntryMessageProcessor
	{
		public BondedWarehouseMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, EDIMessage> sendMail) : base(messagePK, emailReportThatHasBeenDelayed, sendMail)
		{
		}

		protected new FREDIMessage message => (FREDIMessage)base.message;

		protected override ZDateTime AssessmentDate
		{
			get
			{
				var result = entryHeader.EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Invalid;
				return result.IsValid ? result : ZDateTime.Today;
			}
		}

		protected override string CountryCode => entryHeader.CountryCode;

		protected new CusEntryHeader entryHeader => (CusEntryHeader)base.entryHeader;

		DeltaGStatusResolver BondedWarehouseStatusResolver => bondedWarehouseStatusResolver ?? (bondedWarehouseStatusResolver = new DeltaGStatusResolver(entryHeader, message));
		DeltaGStatusResolver bondedWarehouseStatusResolver;

		protected override bool IsOriginalError => BondedWarehouseStatusResolver.CheckIsOriginalError();

		protected override bool IsAmendmentError => BondedWarehouseStatusResolver.CheckIsModificationRejected() || BondedWarehouseStatusResolver.CheckIsRectificationError();

		protected override bool IsAmendmentClear => BondedWarehouseStatusResolver.CheckIsModificationAccepted() || BondedWarehouseStatusResolver.CheckIsRectificationClear();

		protected override bool HasBeenWithdrawn => BondedWarehouseStatusResolver.CheckHasBeenWithdrawn();

		protected override bool IsWithdrawalError => BondedWarehouseStatusResolver.CheckIsWithdrawalError();
	}
}
