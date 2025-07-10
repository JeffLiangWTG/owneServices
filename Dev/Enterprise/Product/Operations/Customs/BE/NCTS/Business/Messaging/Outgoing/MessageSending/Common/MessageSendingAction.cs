using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class MessageSendingAction : NctsHeaderMessageSendingObject
	{
		public MessageSendingAction(NctsCommonMovementHeader movementHeader)
			: base(Argument.NotNull(movementHeader, nameof(movementHeader)).Header)
		{
			using (GetValidationDataSuspender())
			using (SuspendSettingHasChanges())
			{
				MovementHeader = movementHeader;
				Header = (NctsHeader)MovementHeader.Header;
				SetDefaultEntryType();
			}
		}
		public readonly NctsCommonMovementHeader MovementHeader;
		public readonly NctsHeader Header;

		public new class Schema : NctsHeaderMessageSendingObject.Schema
		{
			public const string IsTestDeclaration = nameof(MessageSendingAction.IsTestDeclaration);
			public const string AdditionalDeclarationType = nameof(MessageSendingAction.AdditionalDeclarationType);
			public const string PresentationDateTime = nameof(MessageSendingAction.PresentationDateTime);
			public const string EntryType = nameof(MessageSendingAction.EntryType);
			public const int EntryTypeMaxLength = 3;
			public const string EntryStatus = nameof(MessageSendingAction.EntryStatus);
			public const string TCI11 = nameof(MessageSendingAction.TCI11);
			public const string AgreeWithMinorDiscrepancies = nameof(MessageSendingAction.AgreeWithMinorDiscrepancies);
			public const string RepresentativeCBRNumber = nameof(MessageSendingAction.RepresentativeCBRNumber);
		}

		#region Lookups

		public new MessageSendingActionLookups Lookups => fLookups ?? (fLookups = new MessageSendingActionLookups(this));

		MessageSendingActionLookups fLookups;

		#endregion

		#region Validation

		protected override NctsHeaderMessageSendingObjectValidation GetNewValidation() => new MessageSendingActionValidation(this);

		public new MessageSendingActionValidation Validation => (MessageSendingActionValidation)GetNewValidation();

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		protected override bool ShouldSend_ReadOnly => EntryTypeInfo.Notifications.HasNotifications();

		protected override ZValidation GetActualConsigneeJobDocAddressAdditionalValidation(JobDocAddress actualConsigneeJobDocAddress) => new NctsActualConsigneeJobDocAddressValidation(actualConsigneeJobDocAddress, this);

		public ZString SubApplicationCode => MovementHeader.BM_SubApplicationCode;

		[ResourceStringData("Enterprise.Customs.BE.NCTS.Business.MessageSendingAction|EntryStatus", Caption = "Entry Status")]
		[ReadOnly(true)]
		public ZString EntryStatus => MovementHeader.BM_CustomsStatus;

		public ZPropertyInfo EntryStatusInfo => GetZPropertyInfo(Schema.EntryStatus);

		[ResourceStringData("Enterprise.Customs.BE.NCTS.Business.MessageSendingAction|AdditionalDeclarationType", Caption = "Type")]
		[ReadOnly(true)]
		public ZString AdditionalDeclarationType => MovementHeader.BM_AdditionalDeclarationType;

		public ZPropertyInfo AdditionalDeclarationTypeInfo => GetZPropertyInfo(Schema.AdditionalDeclarationType);

		[ResourceStringData("Enterprise.Customs.BE.NCTS.Business.MessageSendingAction|ReferenceNumber", Caption = "Reference Number")]
		[ReadOnly(true)]
		public override ZString LRN => base.LRN;

		[ResourceStringData("Enterprise.Customs.BE.NCTS.Business.MessageSendingAction|PresentationDateTime", Caption = "Presentation Date And Time")]
		[ReadOnlyMember(nameof(PresentationDateTimeReadOnly))]
		public ZDateTime PresentationDateTime
		{
			get => MovementHeader.BM_ArrivalDate;
			set
			{
				MovementHeader.BM_ArrivalDate = value;
				PresentationDateTimeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PresentationDateTimeInfo => GetZPropertyInfo(Schema.PresentationDateTime);

		[ResourceStringData("Enterprise.Customs.BE.NCTS.Business.MessageSendingAction|IsTestDeclaration", Caption = "Test?")]
		public ZBool IsTestDeclaration
		{
			get => isTestDeclaration;
			set => SetNonPersistentPropertyValue(IsTestDeclarationInfo, ref isTestDeclaration, value);
		}
		ZBool isTestDeclaration;

		public ZPropertyInfo IsTestDeclarationInfo => GetZPropertyInfo(Schema.IsTestDeclaration);

		[ResourceStringData("Enterprise.Customs.BE.NCTS.Business.MessageSendingAction|AgreeWithMinorDiscrepancies", Caption = "Agree with minor discrepancies?")]
		public ZBool AgreeWithMinorDiscrepancies
		{
			get => agreeWithMinorDiscrepancies;
			set => SetNonPersistentPropertyValue(AgreeWithMinorDiscrepanciesInfo, ref agreeWithMinorDiscrepancies, value);
		}
		ZBool agreeWithMinorDiscrepancies;

		public ZPropertyInfo AgreeWithMinorDiscrepanciesInfo => GetZPropertyInfo(Schema.AgreeWithMinorDiscrepancies);

		public bool PresentationDateTimeReadOnly => Header.IsDepartureMovement
			? EntryType == NctsMessageTypeList.Codes.Amendment
				? HasLogWithGIVReference
				: !IsPresentationDateTimeEnabledForCustomsStatusAndPhase
			: MovementHeader.BM_AdditionalDeclarationType != NctsTypeOfAdditionalDeclarationList.Codes.D;

		bool HasLogWithGIVReference => Header.Logs.HasLogWith(l => l.SL_Reference == NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid);

		bool IsPresentationDateTimeEnabledForCustomsStatusAndPhase
		{
			get
			{
				var enabledForCustomsStatus = false;
				var enabledForPhase = true;

				switch (MovementHeader.BM_CustomsStatus)
				{
					case NctsTransitStatusList.Codes.DeclarationAccepted:
					case NctsTransitStatusList.Codes.DeclarationRejected:
					case NctsTransitStatusList.Codes.RequestForAmendment:
					case NctsMessageTypeList.Codes.Amendment:
					case NctsTransitStatusList.Codes.Unknown:
						enabledForCustomsStatus = true;
						break;
				}

				if (enabledForCustomsStatus)
				{
					switch (MovementHeader.BM_Phase)
					{
						case NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent:
						case NctsMovementHeaderTransactionStatusList.Codes.InvalidationSent:
						case NctsMovementHeaderTransactionStatusList.Codes.DeclarationSent:
						case NctsMovementHeaderTransactionStatusList.Codes.RequestForReleaseSent:
						case NctsMovementHeaderTransactionStatusList.Codes.InformationNonArrivedMovementSent:
						case NctsMovementHeaderTransactionStatusList.Codes.PresentationNotificationSent:
						case NctsMovementHeaderTransactionStatusList.Codes.ArrivalNotificationSent:
						case NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarksSent:
							enabledForPhase = false;
							break;
					}
				}

				return enabledForCustomsStatus && enabledForPhase;
			}
		}

		public bool ShowValidationErrors
		{
			get
			{
				switch (EntryType)
				{
					case NctsMessageTypeList.Codes.InvalidationCancellation:
					case NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement:
					case NctsMessageTypeList.Codes.RequestARelease:
						return false;
					default:
						return true;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.BE.NCTS.Business.MessageSendingAction|TCI11", Caption = "TCI11 Date")]
		public ZDateTime TCI11
		{
			get => tci11Date;
			set => SetNonPersistentPropertyValue(TCI11Info, ref tci11Date, value);
		}

		ZDateTime tci11Date;

		public ZPropertyInfo TCI11Info => GetZPropertyInfo(Schema.TCI11);

		[ResourceStringData("Enterprise.Customs.BE.NCTS.Business.MessageSendingAction|EntryType", Caption = "Entry Type")]
		[List(nameof(Lookups) + "." + nameof(MessageSendingActionLookups.EntryTypeList))]
		[MaxLength(Schema.EntryTypeMaxLength)]
		public ZString EntryType
		{
			get => entryType;
			set
			{
				var isChanged = entryType != value;
				SetNonPersistentPropertyValue(EntryTypeInfo, ref entryType, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateEntryType();
				}

				if (isChanged && ShouldSend_ReadOnly)
				{
					ShouldSend = false;
				}
			}
		}
		ZString entryType;

		public ZPropertyInfo EntryTypeInfo => GetZPropertyInfo(Schema.EntryType);

		protected void SetDefaultEntryType()
		{
			EntryType = ZString.Empty;
			var list = Lookups.EntryTypeList;
			if (list.Count == 1)
			{
				EntryType = list[0].Code;
			}
			else
			{
				ShouldSend = false;
			}
		}

		protected override bool Justification_ReadOnly => false;

		public bool HasRepresentative => MovementHeader.Representative.Organisation != null;

		[ReadOnly(true)]
		public ZString RepresentativeCBRNumber => HasRepresentative ? MovementHeader.Representative.Organisation.GetCBR() : ZString.Empty;

		public ZPropertyInfo RepresentativeCBRNumberInfo => GetZPropertyInfo(Schema.RepresentativeCBRNumber);
	}
}
