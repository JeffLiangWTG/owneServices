using System;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business
{
	public class BondedWarehouseMessageProcessor : BondedWarehouseEntryMessageProcessor
	{
		public BondedWarehouseMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, EDIMessage> sendMail) : base(messagePK, emailReportThatHasBeenDelayed, sendMail)
		{
		}

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
	}
}
