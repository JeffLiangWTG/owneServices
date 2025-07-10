using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public abstract class JobDeclarationMiscMessageSendingObjectCore : JobDeclarationMessageSendingObject
		, IAmendmentDetails
	{
		protected JobDeclarationMiscMessageSendingObjectCore(CusEntryHeader entry, ZString messageType)
			: base(entry, messageType)
		{
		}

		public new class Schema : JobDeclarationMessageSendingObject.Schema
		{
			public const string AmendmentVersion = "AmendmentVersion";
			public const string AmendmentTypeDescription = "AmendmentTypeDescription";
			public const string AmendmentReason = "AmendmentReason";
			public const int AmendmentReasonMaxLength = 500;
			public const string ReasonCode = "ReasonCode";
			public const int ReasonCodeMaxLength = 2;
			public const string FaultParty = "FaultParty";
			public const int FaultPartyMaxLength = 2;
			public const string CustomsReceiptNumber = "CustomsReceiptNumber";
			public const int CustomsReceiptNumberMaxLength = 14;
			public const string FaultPartyOtherDescription = "FaultPartyOtherDescription";
			public const int FaultPartyOtherDescriptionMaxLength = 100;
		}

		#region IAmendmentDetails

		[ResourceStringData("JobDeclarationMiscMessageSendingObject|AmendmentVersion", Caption = "Version No.")]
		public virtual ZShort AmendmentVersion => Header.CH_VersionID;

		public abstract ZString AmendmentType { get; }

		[ResourceStringData("JobDeclarationMiscMessageSendingObject|AmendmentTypeDescription", Caption = "Amendment Type")]
		[ResourceStringData("6BAC9F1F-1840-4A0D-92AC-C7797C4AF9C2", Caption = "Amendment Type", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public abstract ZString AmendmentTypeDescription { get; }

		[MaxLength(Schema.AmendmentReasonMaxLength)]
		[ResourceStringData("JobDeclarationMiscMessageSendingObject|AmendmentReason", Caption = "Amendment Reason")]
		[ResourceStringData("8D9CE457-3E6C-4EED-8675-6230F9FFDBDC", Caption = "Amendment Reason", MultipleKey = KRJobMessageTypeList.Codes.Import)]
		public virtual ZString AmendmentReason
		{
			get { return amendmentReason; }
			set
			{
				CheckMaximumLength(AmendmentReasonInfo, value);
				SetNonPersistentPropertyValue(AmendmentReasonInfo, ref amendmentReason, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAmendmentReason();
				}
			}
		}
		ZString amendmentReason;
		public ZPropertyInfo AmendmentReasonInfo => GetZPropertyInfo(Schema.AmendmentReason);

		[MaxLength(Schema.ReasonCodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationMiscMessageSendingObjectCoreLookups.AmendmentReasonCodeList))]
		[ResourceStringData("JobDeclarationMiscMessageSendingObject|ReasonCode", Caption = "Reason Code")]
		public virtual ZString ReasonCode
		{
			get { return reasonCode; }
			set
			{
				CheckMaximumLength(ReasonCodeInfo, value);
				SetNonPersistentPropertyValue(ReasonCodeInfo, ref reasonCode, value);
				if (DateOfFinalPrice_ReadOnly)
				{
					DateOfFinalPrice = ZDate.Empty;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateReasonCode();
				}
			}
		}
		ZString reasonCode;
		public ZPropertyInfo ReasonCodeInfo => GetZPropertyInfo(Schema.ReasonCode);

		[MaxLength(Schema.FaultPartyMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationMiscMessageSendingObjectCoreLookups.FaultPartyList))]
		[ResourceStringData("JobDeclarationMiscMessageSendingObject|FaultParty", Caption = "Fault Party")]
		public ZString FaultParty
		{
			get { return faultParty; }
			set
			{
				CheckMaximumLength(FaultPartyInfo, value);
				SetNonPersistentPropertyValue(FaultPartyInfo, ref faultParty, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateFaultParty();
				}
			}
		}
		ZString faultParty;
		public ZPropertyInfo FaultPartyInfo => GetZPropertyInfo(Schema.FaultParty);

		[MaxLength(Schema.FaultPartyOtherDescriptionMaxLength)]
		[ResourceStringData("JobDeclarationMiscMessageSendingObject|FaultPartyOtherDescription", Caption = "Fault Reason")]
		public ZString FaultPartyOtherDescription
		{
			get { return faultPartyOtherDescription; }
			set
			{
				CheckMaximumLength(FaultPartyOtherDescriptionInfo, value);
				SetNonPersistentPropertyValue(FaultPartyOtherDescriptionInfo, ref faultPartyOtherDescription, value);
			}
		}
		ZString faultPartyOtherDescription;
		public ZPropertyInfo FaultPartyOtherDescriptionInfo => GetZPropertyInfo(Schema.FaultPartyOtherDescription);

		[ResourceStringData("JobDeclarationMiscMessageSendingObject|DateOfFinalPrice", Caption = "Date Of Final Price")]
		[ReadOnlyMember(nameof(DateOfFinalPrice_ReadOnly))]
		[BusinessObjectTestExclude]
		public ZDate DateOfFinalPrice
		{
			get { return dateOfFinalPrice; }
			set
			{
				SetNonPersistentPropertyValue(DateOfFinalPriceInfo, ref dateOfFinalPrice, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDateOfFinalPrice();
				}
			}
		}
		ZDate dateOfFinalPrice;
		public ZPropertyInfo DateOfFinalPriceInfo => GetZPropertyInfo(nameof(DateOfFinalPrice));
		public ZBool DateOfFinalPrice_ReadOnly => !(TransactionTypeCodeList.IsDateOfFinalPriceValidation(Header.Declaration.JE_ExportGoodsType) && ReasonCode == ExportAmendmentReasonCodeList.Codes._28);

		[ResourceStringData("JobDeclarationMiscMessageSendingObject|CustomsReceiptNumber", Caption = "Customs Receipt Number")]
		public ZString CustomsReceiptNumber => MessageFunctions.GetCustomsReceiptNumber(Header.CH_BGMReference);

		ZInt IAmendmentDetails.AmendmentVersionNo => AmendmentVersion;
		ZString IAmendmentDetails.AmendmentType => AmendmentType;
		ZString IAmendmentDetails.AmendReasonDescription => AmendmentReason;
		ZString IAmendmentDetails.ReasonCode => ReasonCode;
		ZString IAmendmentDetails.FaultParty => FaultParty;
		ZString IAmendmentDetails.FaultPartyOtherDescription => FaultPartyOtherDescription;
		ZString IAmendmentDetails.PenaltyPaymentReasonCode => ZString.Empty;
		ZDate IAmendmentDetails.DateOfFinalPrice => ZDate.Empty;
		#endregion
		public JobDeclarationMiscMessageSendingObjectCoreLookups Lookups => new JobDeclarationMiscMessageSendingObjectCoreLookups(this);
		public new JobDeclarationMiscMessageSendingObjectCoreValidation Validation => (JobDeclarationMiscMessageSendingObjectCoreValidation)base.Validation;
		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() => new JobDeclarationMiscMessageSendingObjectCoreValidation(this);
	}
}
