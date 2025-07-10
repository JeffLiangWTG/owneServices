using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.H7.Business;

public class MessageSendingObject : EU.H7.Business.MessageSendingObject
{
	public MessageSendingObject(EU.H7.Business.AsycudaBill bill)
		: base(bill)
	{
	}

	public new class Schema : EU.H7.Business.AutoMessageSendingObject.Schema
	{
		public const string LegislativeReference = "LegislativeReference";
		public const int LegislativeReferenceMaxLength = 35;

		public const string DutyAmount = "DutyAmount";
		public const int DutyAmountMaxLength = 17;
		public const int DutyAmountDecimalPrecision = 16;
		public const int DutyAmountDecimalPlaces = 2;

		public const string Currency = "Currency";
	}

	protected override ZString DefaultAction => ITH7MessageTypes.Codes.H7D;

	public override ZString Action
	{
		get => base.Action;
		set
		{
			var oldValue = base.Action;
			base.Action = value;

			if (base.Action != oldValue)
			{
				AmendmentReasonCode = string.Empty;
			}
		}
	}

	[List(nameof(AmendmentReasonCodeList))]
	[ResourceStringData("Enterprise.Customs.IT.H7.Business.MessageSendingObject|AmendmentReasonCode", Caption = "Amendment/Cancellation Reason")]
	public override ZString AmendmentReasonCode
	{
		get => base.AmendmentReasonCode;
		set => base.AmendmentReasonCode = value;
	}

	protected override bool AmendmentReasonCode_ReadOnly => !ActionIsH7MOrH7C;

	[ReadOnlyMember(nameof(LegislativeReference_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.IT.H7.Business.MessageSendingObject|LegislativeReference", Caption = "Legislative Reference")]
	[List(nameof(LegislativeReferenceCodeList))]
	[MaxLength(Schema.LegislativeReferenceMaxLength)]
	public ZString LegislativeReference
	{
		get
		{
			return legislativeReference;
		}
		set
		{
			CheckMaximumLength(LegislativeReferenceInfo, value);
			SetNonPersistentPropertyValue(LegislativeReferenceInfo, ref legislativeReference, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateLegislativeReference();
			}
		}
	}

	public ZPropertyInfo LegislativeReferenceInfo => GetZPropertyInfo(Schema.LegislativeReference);

	ZString legislativeReference;

	protected bool LegislativeReference_ReadOnly => !ActionIsH7MOrH7C;

	[ReadOnlyMember(nameof(DutyAmount_ReadOnly))]
	[MaxLength(Schema.DutyAmountMaxLength)]
	[DecimalPrecision(Schema.DutyAmountDecimalPrecision)]
	[DecimalPlaces(Schema.DutyAmountDecimalPlaces)]
	[ResourceStringData("Enterprise.Customs.IT.H7.Business.MessageSendingObject|DutyAmount", Caption = "Duty Amount (Difference)", ShortCaption = "Duty Amount")]
	public ZDecimal DutyAmount
	{
		get => dutyAmount;
		set
		{
			SetNonPersistentPropertyValue(DutyAmountInfo, ref dutyAmount, value);
			DutyAmountInfo.RefreshBinding();
			if (!IsValidationSuspended)
			{
				Validation.ValidateDutyAmount();
			}
		}
	}

	public ZPropertyInfo DutyAmountInfo => GetZPropertyInfo(Schema.DutyAmount);

	ZDecimal dutyAmount;

	protected bool DutyAmount_ReadOnly => Action != ITH7MessageTypes.Codes.H7C;

	[ResourceStringData("Enterprise.Customs.IT.H7.Business.MessageSendingObject|Currency", Caption = "Currency")]
	public ZString Currency => Core.Constants.CurrencyCodes.EuropeanUnion;

	public CodeDescriptionPairList LegislativeReferenceCodeList => GetLegislativeReferenceCodeList();

	protected override CodeDescriptionPairList GetActionList() => Factory.GetCachedValue<ITH7MessageTypes>();

	protected override CodeDescriptionPairList GetAmendmentReasonCodeList()
	{
		if (Action == ITH7MessageTypes.Codes.H7M)
		{
			return Factory.GetCachedValue<AmendmentReasonList>();
		}
		else if (Action == ITH7MessageTypes.Codes.H7C)
		{
			return Factory.GetCachedValue<Ucc6ImportCancellationReasonList>();
		}

		return new CodeDescriptionPairList();
	}

	protected CodeDescriptionPairList GetLegislativeReferenceCodeList() => Factory.GetCachedValue<Ucc6ImportCancellationAndAmendmentLegislativeReferenceList>();

	protected override EU.H7.Business.MessageSendingObjectValidation GetNewValidation() => new MessageSendingObjectValidation(this);

	public new MessageSendingObjectValidation Validation => (MessageSendingObjectValidation)base.Validation;

	public override bool IsAmendmentAction => Action == ITH7MessageTypes.Codes.H7M;

	public override bool IsCancellationAction => Action == ITH7MessageTypes.Codes.H7C;

	public bool ActionIsH7MOrH7C => IsAmendmentAction || IsCancellationAction;

	public override EU.H7.Business.MessageSender CreateSender() => new MessageSender(this);
}
