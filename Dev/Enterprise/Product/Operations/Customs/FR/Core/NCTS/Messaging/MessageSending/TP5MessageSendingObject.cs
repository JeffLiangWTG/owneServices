using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.NCTS.Messaging.MessageSending;
using NctsDepartureMovementHeader = Enterprise.Customs.FR.Business.NCTS.NctsDepartureMovementHeader;
using NctsHeader = Enterprise.Customs.FR.Business.NCTS.NctsHeader;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class TP5MessageSendingObject : NctsHeaderMessageSendingObject, ITP5MessageSendingObject
	{
		public TP5MessageSendingObject(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public new class Schema : NctsHeaderMessageSendingObject.Schema
		{
			public const string JustificationCode = nameof(TP5MessageSendingObject.JustificationCode);
			public const string QueryIdentifier = nameof(TP5MessageSendingObject.QueryIdentifier);
			public const string PeriodFrom = nameof(TP5MessageSendingObject.PeriodFrom);
			public const string PeriodTo = nameof(TP5MessageSendingObject.PeriodTo);
			public const string RequesterRole = nameof(TP5MessageSendingObject.RequesterRole);
		}

		[ReadOnlyMember(nameof(TP5MessageSendingObject.JustificationCode_ReadOnly))]
		[ResourceStringData("FR.NCTS.Messaging|JustificationCode", Caption = "Regular Justification Code")]
		[List(nameof(Lookups) + "." + nameof(TP5MessageSendingObjectLookups.RegularJustificationCodesList))]
		public ZString JustificationCode
		{
			get
			{
				return justificationCode;
			}
			set
			{
				SetNonPersistentPropertyValue(JustificationCodeInfo, ref justificationCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJustificationCode();
				}
			}
		}
		ZString justificationCode;

		public ZPropertyInfo JustificationCodeInfo => this.GetZPropertyInfo(Schema.JustificationCode);

		protected bool JustificationCode_ReadOnly => NctsHeader.IsPhase5Departure && !MessageType.EqualsIgnoringCase(NCTS5DeparturePhaseList.Codes.Cancellation);

		public override ZString MessageType
		{
			get => base.MessageType;
			set
			{
				var oldValue = MessageType;
				base.MessageType = value.ToUpperInvariant();
				if (!IsCopying && oldValue != MessageType)
				{
					ClearJustificationCode();
				}
			}
		}

		void ClearJustificationCode()
		{
			if (!JustificationCode.IsEmpty && JustificationCode_ReadOnly)
			{
				JustificationCode = ZString.Empty;
			}
		}

		public IFRMessagesOwner MessagesOwner
		{
			get
			{
				if (NctsHeader.IsDepartureMovement)
				{
					return (NctsDepartureMovementHeader)NctsHeader.MovementHeader;
				}
				return (NctsHeader)NctsHeader;
			}
		}

		public object DataSource => NctsHeader;

		protected override bool Justification_ReadOnly => !ListOfMessageNeededJustification.Contains(MessageType);

		public ZString[] ListOfMessageNeededJustification => new ZString[] { TP5MessageTypeList.Codes.CC013C, TP5MessageTypeList.Codes.CC014C };

		[List(nameof(Lookups) + "." + nameof(TP5MessageSendingObjectLookups.QueryIdentifierCodesList))]
		public ZString QueryIdentifier
		{
			get
			{
				return queryIdentifier;
			}
			set
			{
				SetNonPersistentPropertyValue(QueryIdentifierInfo, ref queryIdentifier, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateQueryIdentifier();
				}

				if (Period_ReadOnly)
				{
					PeriodFrom = ZDateTime.Empty;
					PeriodTo = ZDateTime.Empty;
				}
			}
		}
		ZString queryIdentifier;

		public ZPropertyInfo QueryIdentifierInfo => GetZPropertyInfo(Schema.QueryIdentifier);

		[ReadOnlyMember(nameof(Period_ReadOnly))]
		public ZDateTime PeriodFrom
		{
			get
			{
				return periodFrom;
			}
			set
			{
				SetNonPersistentPropertyValue(PeriodFromInfo, ref periodFrom, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePeriodFrom();
				}
			}
		}
		ZDateTime periodFrom = ZDateTime.Empty;

		public ZPropertyInfo PeriodFromInfo => GetZPropertyInfo(Schema.PeriodFrom);

		[ReadOnlyMember(nameof(Period_ReadOnly))]
		public ZDateTime PeriodTo
		{
			get
			{
				return periodTo;
			}
			set
			{
				SetNonPersistentPropertyValue(PeriodToInfo, ref periodTo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePeriodTo();
				}
			}
		}
		ZDateTime periodTo = ZDateTime.Empty;

		public ZPropertyInfo PeriodToInfo => GetZPropertyInfo(Schema.PeriodTo);

		bool Period_ReadOnly
		{
			get
			{
				var bondType = NctsHeader.MovementHeader?.Guarantees[0]?.PW_BondType ?? ZString.Empty;
				var periodEditable = (QueryIdentifier == "1" || QueryIdentifier == "3") && (bondType == "0" || bondType == "1" || bondType == "9");
				return !periodEditable;
			}
		}

		public ZString RequesterId => NctsHeader.MovementHeader.Representative.Organisation?.GetEORI() ?? NctsHeader.Principal.Organisation?.GetEORI() ?? ZString.Empty;

		[List(nameof(Lookups) + "." + nameof(TP5MessageSendingObjectLookups.RequesterRoleCodesList))]
		public ZString RequesterRole
		{
			get
			{
				return requesterRole;
			}
			set
			{
				SetNonPersistentPropertyValue(RequesterRoleInfo, ref requesterRole, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateRequesterRole();
				}
			}
		}
		ZString requesterRole;

		public ZPropertyInfo RequesterRoleInfo => GetZPropertyInfo(Schema.RequesterRole);

		public new TP5MessageSendingObjectLookups Lookups => (TP5MessageSendingObjectLookups)base.Lookups;

		protected override NctsHeaderMessageSendingObjectLookups GetNewLookups() => new TP5MessageSendingObjectLookups(this);

		protected override NctsHeaderMessageSendingObjectValidation GetNewValidation()
		{
			return new TP5MessageSendingObjectValidation(this);
		}

		protected override void SetDefaultDataCore()
		{
			base.SetDefaultDataCore();
			JustificationCode = ZString.Empty;
			var bondType = NctsHeader.MovementHeader?.Guarantees[0]?.PW_BondType ?? ZString.Empty;
			if (bondType == "2" || bondType == "4")
			{
				QueryIdentifier = bondType;
			}
			else
			{
				QueryIdentifier = ZString.Empty;
			}
			RequesterRole = "1";
		}

		public new TP5MessageSendingObjectValidation Validation => (TP5MessageSendingObjectValidation)base.Validation;
	}
}
