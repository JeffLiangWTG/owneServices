using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BD;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class EarlyReleaseMiscMessageSendingObject : JobDeclarationMiscMessageSendingObjectCore
	{
		public EarlyReleaseMiscMessageSendingObject(CusEntryHeader entry)
			: base(entry, ElectronicDocumentTypeList.Codes._5BD)
		{
		}

		/// <summary>
		/// Please do not use this option as it is used only for the binding purpose.
		/// </summary>
		/// <param name="factory"></param>
		public EarlyReleaseMiscMessageSendingObject(BusinessObjectFactory factory) : base(null, ElectronicDocumentTypeList.Codes._5BD)
		{
		}

		public new class Schema : JobDeclarationMiscMessageSendingObjectCore.Schema
		{
			public const int RequestReasonMaxLength = 50;
			public const string OtherSecurityType = "OtherSecurityType";
			public const int OtherSecurityTypeMaxLength = 20;
			public const string SecurityType = "SecurityType";
			public const int SecurityTypeMaxLength = 2;
			public const string SecurityTypeName = "SecurityTypeName";
			public const string SecurityStartDate = "SecurityStartDate";
			public const string SecurityEndDate = "SecurityEndDate";
			public const string SecurityAmount = "SecurityAmount";
			public const string ReasonForEarlyRemoval = "ReasonForEarlyRemoval";
			public const int ReasonForEarlyRemovalMaxLength = 2;
			public const string ReasonForEarlyRemovalName = "ReasonForEarlyRemovalName";
			public const string RequestDate = "RequestDate";
		}

		[MaxLength(Schema.RequestReasonMaxLength)]
		[ResourceStringData("EarlyReleaseMiscMessageSendingObject|AmendmentReason", Caption = "Request Reason")]
		public override ZString AmendmentReason
		{
			get => base.AmendmentReason;
			set
			{
				base.AmendmentReason = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateAmendmentReason();
				}
			}
		}

		[MaxLength(Schema.OtherSecurityTypeMaxLength)]
		[ResourceStringData("EarlyReleaseMiscMessageSendingObject|OtherSecurityType", Caption = "Other Security Type")]
		public ZString OtherSecurityType
		{
			get => otherSecurityType;
			set
			{
				SetNonPersistentPropertyValue(OtherSecurityTypeInfo, ref otherSecurityType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOtherSecurityType();
				}
			}
		}
		ZString otherSecurityType;
		public ZPropertyInfo OtherSecurityTypeInfo => GetZPropertyInfo(nameof(OtherSecurityType));

		[MaxLength(Schema.SecurityTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(EarlyReleaseMiscMessageSendingObjectLookups.SecurityTypeList))]
		[ResourceStringData("EarlyReleaseMiscMessageSendingObject|SecurityType", Caption = "Security Type")]
		public ZString SecurityType
		{
			get => securityType;
			set
			{
				SetNonPersistentPropertyValue(SecurityTypeInfo, ref securityType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSecurityType();
				}
			}
		}
		ZString securityType;
		public ZPropertyInfo SecurityTypeInfo => GetZPropertyInfo(nameof(SecurityType));
		public ZString SecurityTypeName => Lookups.SecurityTypeList.GetDescriptionFromCode(SecurityType);

		[ResourceStringData("EarlyReleaseMiscMessageSendingObject|SecurityStartDate", Caption = "Security Start Date")]
		public ZDateTime SecurityStartDate
		{
			get => securityStartDate;
			set
			{
				SetNonPersistentPropertyValue(SecurityStartDateInfo, ref securityStartDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSecurityStartDate();
				}
			}
		}
		ZDateTime securityStartDate;
		public ZPropertyInfo SecurityStartDateInfo => GetZPropertyInfo(nameof(SecurityStartDate));

		[ResourceStringData("EarlyReleaseMiscMessageSendingObject|SecurityEndDate", Caption = "Security End Date")]
		public ZDateTime SecurityEndDate
		{
			get => securityEndDate;
			set
			{
				SetNonPersistentPropertyValue(SecurityEndDateInfo, ref securityEndDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSecurityEndDate();
				}
			}
		}
		ZDateTime securityEndDate;
		public ZPropertyInfo SecurityEndDateInfo => GetZPropertyInfo(nameof(SecurityEndDate));

		[DecimalPlaces(DecimalPlacesConstants.AmountInKRW)]
		[ResourceStringData("EarlyReleaseMiscMessageSendingObject|SecurityAmount", Caption = "Security Amount")]
		public ZDecimal SecurityAmount
		{
			get => securityAmount;
			set
			{
				SetNonPersistentPropertyValue(SecurityAmountInfo, ref securityAmount, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSecurityAmount();
				}
			}
		}
		ZDecimal securityAmount;
		public ZPropertyInfo SecurityAmountInfo => GetZPropertyInfo(nameof(SecurityAmount));

		[MaxLength(Schema.ReasonForEarlyRemovalMaxLength)]
		[List(nameof(Lookups) + "." + nameof(EarlyReleaseMiscMessageSendingObjectLookups.ReasonForEarLyRemovalList))]
		[ResourceStringData("EarlyReleaseMiscMessageSendingObject|ReasonForEarlyRemoval", Caption = "Reason For Early Removal")]
		public ZString ReasonForEarlyRemoval
		{
			get => reasonForEarlyRemoval;
			set
			{
				SetNonPersistentPropertyValue(ReasonForEarlyRemovalInfo, ref reasonForEarlyRemoval, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReasonForEarlyRemoval();
				}
			}
		}
		ZString reasonForEarlyRemoval;
		public ZPropertyInfo ReasonForEarlyRemovalInfo => GetZPropertyInfo(nameof(ReasonForEarlyRemoval));
		public ZString ReasonForEarlyRemovalName => Lookups.ReasonForEarLyRemovalList.GetDescriptionFromCode(ReasonForEarlyRemoval);

		public ZDateTime RequestDate
		{
			get => requestDate;
			set
			{
				SetNonPersistentPropertyValue(RequestDateInfo, ref requestDate, value);
			}
		}
		ZDateTime requestDate;
		public ZPropertyInfo RequestDateInfo => GetZPropertyInfo(nameof(RequestDate));

		public void Populate(EDIMessage message)
		{
			if (message.EM_MessageType != ElectronicDocumentTypeList.Codes._5BD)
			{
				throw new ArgumentException("expected to receive an EDIMessage of 5BD");
			}

			Declaration declaration = null;

			using (var reader = message.GetEM_MessageTextReader())
			{
				declaration = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<Declaration>(reader);
			}

			using (GetValidationSuspender())
			{
				ReasonForEarlyRemoval = declaration.ReasonCode.Value;
				AmendmentReason = declaration.Reason.Value;
				SecurityType = declaration.ObligationGuarantee.SecurityDetailsCode.Value;
				OtherSecurityType = declaration.AdditionalInformation.Content.Value;

				if (DateTime.TryParseExact(declaration.ObligationGuarantee.SecurityEffectiveDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
				{
					SecurityStartDate = dt;
				}

				if (DateTime.TryParseExact(declaration.ObligationGuarantee.LpcoExpirationDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
				{
					SecurityEndDate = dt2;
				}

				if (DateTime.TryParseExact(declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt3))
				{
					RequestDate = dt3;
				}

				SecurityAmount = declaration.ObligationGuarantee.SecurityAmount.Value;
				HasChanges = false;
			}
		}

		public new EarlyReleaseMiscMessageSendingObjectLookups Lookups => new EarlyReleaseMiscMessageSendingObjectLookups(this);
		public new EarlyReleaseMiscMessageSendingObjectValidation Validation => (EarlyReleaseMiscMessageSendingObjectValidation)base.Validation;
		protected override Customs.Business.JobDeclarationMessageSendingObjectValidation GetNewValidation() => new EarlyReleaseMiscMessageSendingObjectValidation(this);

		public override ZString AmendmentType => throw new NotImplementedException();

		public override ZString AmendmentTypeDescription => throw new NotImplementedException();
	}
}
