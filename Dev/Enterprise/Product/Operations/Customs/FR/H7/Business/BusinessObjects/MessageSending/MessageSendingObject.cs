using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.H7.Business
{
	public sealed class MessageSendingObject : EU.H7.Business.MessageSendingObject
	{
		public MessageSendingObject(H7Bill bill)
			: base(bill)
		{
		}

		public new class Schema : EU.H7.Business.AutoMessageSendingObject.Schema
		{
			public const string Motivation = "Motivation";
			public const string ManifestLodgementDateTime = "ManifestLodgementDateTime";
			public const string FallbackReferenceNumber = "FallbackReferenceNumber";

			public const int MotivationMaxLength = 5;
			public const int FallbackReferenceNumberMaxLength = 10;
		}

		public override ZString Action
		{
			get => base.Action;
			set
			{
				base.Action = value;
				if (base.Action != ZString.Empty)
				{
					AmendmentInvalidationReason = ZString.Empty;
					Motivation = ZString.Empty;
				}
			}
		}

		[MaxLength(350)]
		[ReadOnlyMember(nameof(AmendmentInvalidationProperties_ReadOnly))]
		public override ZString AmendmentInvalidationReason { get => base.AmendmentInvalidationReason; set => base.AmendmentInvalidationReason = value; }

		[List(nameof(MotivitationList))]
		[MaxLength(Schema.MotivationMaxLength)]
		[ReadOnlyMember(nameof(AmendmentInvalidationProperties_ReadOnly))]
		[ResourceStringData("FR.MessageSendingObject.Movitation", Caption = "Motivation")]
		public ZString Motivation
		{
			get
			{
				return motivation;
			}
			set
			{
				SetNonPersistentPropertyValue(MotivationInfo, ref motivation, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMotivation();
				}
			}
		}

		ZString motivation;

		public ZPropertyInfo MotivationInfo => GetZPropertyInfo(Schema.Motivation);

		public CodeDescriptionPairList MotivitationList => GetMotivationList();

		CodeDescriptionPairList GetMotivationList()
		{
			if (Action == H7MessageTypeList.Codes.H7Amendment)
			{
				return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.France, H7AmendmentMotivationCodeList, ZDate.Today, includeParentDataGrouping: false);
			}
			else if (Action == H7MessageTypeList.Codes.H7Invalidation)
			{
				return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.France, H7InvalidationMotivationCodeList, ZDate.Today, includeParentDataGrouping: false);
			}
			else
			{
				return new CodeDescriptionPairList();
			}
		}

		const string H7AmendmentMotivationCodeList = "H7AMD";
		const string H7InvalidationMotivationCodeList = "H7INV";

		bool AmendmentInvalidationProperties_ReadOnly => Action != H7MessageTypeList.Codes.H7Amendment && Action != H7MessageTypeList.Codes.H7Invalidation;

		[ReadOnlyMember(nameof(FallbackProcedureProperties_ReadOnly))]
		[ResourceStringData("FR.MessageSendingObject.ManifestLodgementDateTime", Caption = "Manifest Lodgement Date Time (Fallback Procedure)", ShortCaption = "Date Time (Fallback Proc.)", MediumCaption = "Man. Date Time (Fallback Proc.)")]
		public ZDateTime ManifestLodgementDateTime
		{
			get
			{
				return manifestLodgementDateTime;
			}
			set
			{
				SetNonPersistentPropertyValue(ManifestLodgementDateTimeInfo, ref manifestLodgementDateTime, value);
			}
		}

		ZDateTime manifestLodgementDateTime;

		public ZPropertyInfo ManifestLodgementDateTimeInfo => GetZPropertyInfo(Schema.ManifestLodgementDateTime);

		[MaxLength(Schema.FallbackReferenceNumberMaxLength)]
		[ReadOnlyMember(nameof(FallbackProcedureProperties_ReadOnly))]
		[ResourceStringData("FR.MessageSendingObject.FallbackReferenceNumber", Caption = "Reference Number (Fallback Procedure)", ShortCaption = "Ref. No. (Fallback Proc.)", MediumCaption = "Reference No. (Fallback Proc.)")]
		public ZString FallbackReferenceNumber
		{
			get
			{
				return fallbackReferenceNumber;
			}
			set
			{
				SetNonPersistentPropertyValue(FallbackReferenceNumberInfo, ref fallbackReferenceNumber, value);
			}
		}

		ZString fallbackReferenceNumber;

		public ZPropertyInfo FallbackReferenceNumberInfo => GetZPropertyInfo(Schema.FallbackReferenceNumber);

		bool FallbackProcedureProperties_ReadOnly => Action != H7MessageTypeList.Codes.H7Fallback;

		protected override CodeDescriptionPairList GetActionList() => new H7MessageTypeList();

		protected override ZString DefaultAction => H7MessageTypeList.Codes.H7Declaration;

		public new MessageSendingObjectValidation Validation => (MessageSendingObjectValidation)base.Validation;

		protected override EU.H7.Business.MessageSendingObjectValidation GetNewValidation() => new MessageSendingObjectValidation(this);
	}
}
