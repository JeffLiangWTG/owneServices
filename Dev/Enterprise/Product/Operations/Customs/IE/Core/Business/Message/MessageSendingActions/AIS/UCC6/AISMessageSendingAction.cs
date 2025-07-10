using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class AISMessageSendingAction : CusEntryHeaderMessageSendingAction, IFallbackProcedureSendingObject
	{
		public AISMessageSendingAction(CusEntryHeader cusEntryHeader) : base(cusEntryHeader) { }

		protected override Type SenderType => typeof(AISMessageSender);

		protected override CusEntryHeaderMessageSendingActionLookups GetNewLookups()
		{
			return new AISMessageSendingActionLookups(this);
		}

		public new AISMessageSendingActionValidation Validation => (AISMessageSendingActionValidation)base.Validation;

		protected override CusEntryHeaderMessageSendingActionValidation GetNewValidation()
		{
			return new AISMessageSendingActionValidation(this);
		}

		[ResourceStringData("3369F19D-10CE-4075-B5B2-22616ABAF0CC", Caption = "Alternative Date of Acceptance")]
		public ZDate AlternativeDateOfAcceptance
		{
			get => alternativeDateOfAcceptance;
			set => SetNonPersistentPropertyValue(AlternativeDateOfAcceptanceInfo, ref alternativeDateOfAcceptance, value);
		}
		ZDate alternativeDateOfAcceptance;
		public ZPropertyInfo AlternativeDateOfAcceptanceInfo => GetZPropertyInfo(nameof(AlternativeDateOfAcceptance));

		[ResourceStringData("4DCD4121-65DD-4462-9653-8CCEB7AC7025", Caption = "Customs Reference")]
		public ZString CustomsReferenceNumber
		{
			get => customsReferenceNumber;
			set => SetNonPersistentPropertyValue(CustomsReferenceNumberInfo, ref customsReferenceNumber, value);
		}
		ZString customsReferenceNumber;
		public ZPropertyInfo CustomsReferenceNumberInfo => GetZPropertyInfo(nameof(CustomsReferenceNumber));

		[ResourceStringData("7AD21ABE-8868-4F2D-9BF1-ACDF3D2C8F03", Caption = "Customs Justification")]
		public ZString CustomsJustification
		{
			get => customsJustification;
			set => SetNonPersistentPropertyValue(CustomsJustificationInfo, ref customsJustification, value);
		}
		ZString customsJustification;
		public ZPropertyInfo CustomsJustificationInfo => GetZPropertyInfo(nameof(CustomsJustification));
	}
}
