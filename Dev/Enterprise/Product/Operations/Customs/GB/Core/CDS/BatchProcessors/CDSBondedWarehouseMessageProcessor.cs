using System;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSBondedWarehouseMessageProcessor : Customs.Business.MessageProcessors.BondedWarehouseEntryMessageProcessor
	{
		protected internal CDSBondedWarehouseMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<EmailDef, Enterprise.Messaging.Business.EDIMessage> sendMail)
			: base(messagePK, emailReportThatHasBeenDelayed, sendMail)
		{
		}

		bool IsCleared => entryHeader.CH_EntryStatus == EntryStatusList.Codes.Clear;

		protected override bool HasBeenWithdrawn => IsCleared && message.EM_MessageSubType == CDSEDIMessageTypeList.Codes.CancelDeclaration;
		protected override bool IsAmendmentError => message.EM_MessageSubType == CDSEDIMessageTypeList.Codes.AmendDeclaration && ((entryHeader.CH_Status == EDIMessageStatusList.Codes.Error) || IsStatusRejected);
		protected override bool IsAmendmentClear => IsCleared && message.EM_MessageSubType == CDSEDIMessageTypeList.Codes.AmendDeclaration;
		protected override bool IsOriginalError => message.EM_MessageSubType == CDSEDIMessageTypeList.Codes.NewDeclaration && (IsStatusRejected || (entryHeader.CH_Status == EDIMessageStatusList.Codes.Error));
		protected override bool IsWithdrawalError => message.EM_MessageSubType == CDSEDIMessageTypeList.Codes.CancelDeclaration && (IsStatusRejected || (entryHeader.CH_Status == EDIMessageStatusList.Codes.Error));

		protected override string CountryCode => GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

		protected override ZDateTime AssessmentDate
		{
			get
			{
				var result = entryHeader.EntryInstruction?.CEI_DateForDuty ?? ZDateTime.Invalid;
				return result.IsValid ? result : ZDateTime.Today;
			}
		}

		protected new CusEntryHeader entryHeader
		{
			get { return (CusEntryHeader)supporter; }
		}
	}
}
