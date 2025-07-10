using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business
{
	public class ExitNotificationMessageSendingAction : MessageSendingAction, IObsoleteValidation
	{
		public ExitNotificationMessageSendingAction(CusExitDetail exitDetail) : base(exitDetail, (x) => ((CusExitDetail)x).CED_MovementReferenceNumber)
		{
		}

		public new CusExitDetail MessagingObject => (CusExitDetail)base.MessagingObject;

		[ResourceStringData("a1de50e2-b807-4fa9-bab7-9105ad07df2f", Caption = "Reference Number")]
		public ZString ReferenceNumber => MessagingObject.Header.CEH_ReferenceNumber;

		[ResourceStringData("2e35935b-cc80-465c-bdc0-85cb54055417", Caption = "Status")]
		public ZString Status => MessagingObject.CED_Status;

		[ResourceStringData("1299a44f-8b65-4ae2-8b6a-a5f7a8919906", Caption = "Status Description")]
		public ZString StatusDescription => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, Status, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, ZDate.Today)?.ZZD_Description ?? ZString.Empty;

		[ResourceStringData("db2cca13-0b8c-430a-8cd9-dc92acea6f98", Caption = "Message Type")]
		[List(nameof(Lookups) + "." + nameof(ExitNotificationMessageSendingActionLookups.MessageTypeList))]
		[MaxLength(3)]
		public ZString MessageType
		{
			get { return messageType; }
			set
			{
				SetNonPersistentPropertyValue(MessageTypeInfo, ref messageType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMessageType();
				}
			}
		}
		ZString messageType;

		public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(nameof(MessageType));

		[ResourceStringData("481AEA0B-5D53-4001-BFE1-00F186113121", Caption = "Missing Quantity")]
		public ZBool MissingQuantity
		{
			get => missingQuantity;
			set
			{
				SetNonPersistentPropertyValue(MissingQuantityInfo, ref missingQuantity, value);
			}
		}
		ZBool missingQuantity;
		public ZPropertyInfo MissingQuantityInfo => GetZPropertyInfo(nameof(MissingQuantity));

		[ResourceStringData("26A5A380-1CFB-47DE-B4FF-CA73FE433E25", Caption = "Finalization")]
		public ZBool Finalization
		{
			get => finalization;
			set
			{
				SetNonPersistentPropertyValue(FinalizationInfo, ref finalization, value);
			}
		}
		ZBool finalization;
		public ZPropertyInfo FinalizationInfo => GetZPropertyInfo(nameof(Finalization));

		[ResourceStringData("93EF3893-5653-4F3A-81B6-64606AF382B0", Caption = "Redirection")]
		public ZBool Redirection
		{
			get => redirection;
			set
			{
				SetNonPersistentPropertyValue(RedirectionInfo, ref redirection, value);
				if (!IsValidationSuspended && value)
				{
					Validation.ValidateIntendedExitCustomsOffice();
				}

				if (!value)
				{
					IntendedExitCustomsOffice = ZString.Empty;
				}
			}
		}
		ZBool redirection;
		public ZPropertyInfo RedirectionInfo => GetZPropertyInfo(nameof(Redirection));

		[ResourceStringData("28BD3668-56A5-4199-B8C9-0255CFC3F0BB", Caption = "Intended Exit Customs Office")]
		[ReadOnlyMember(nameof(IntendedExitCustomsOffice_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(ExitNotificationMessageSendingActionLookups.IntendedExitCustomsOfficeList))]
		public ZString IntendedExitCustomsOffice
		{
			get => intendedExitCustomsOffice;
			set
			{
				SetNonPersistentPropertyValue(IntendedExitCustomsOfficeInfo, ref intendedExitCustomsOffice, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateIntendedExitCustomsOffice();
				}
			}
		}
		ZString intendedExitCustomsOffice;

		bool IntendedExitCustomsOffice_ReadOnly => !Redirection;

		public ZPropertyInfo IntendedExitCustomsOfficeInfo => GetZPropertyInfo(nameof(IntendedExitCustomsOffice));

		public ExitNotificationItemCollection Items => items ?? (items = new ExitNotificationItemCollection(MessagingObject));
		ExitNotificationItemCollection items;
		public ExitNotificationPackageCollection Packages => packages ?? (packages = new ExitNotificationPackageCollection(MessagingObject));
		ExitNotificationPackageCollection packages;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			MessageType = ExitSummaryMessageTypeList.Codes.Notification;
		}

		#region Lookups

		public ExitNotificationMessageSendingActionLookups Lookups => fLookups ?? (fLookups = new ExitNotificationMessageSendingActionLookups(this));
		ExitNotificationMessageSendingActionLookups fLookups;

		#endregion

		#region Validation

		public ExitNotificationMessageSendingActionValidation Validation => new ExitNotificationMessageSendingActionValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion
	}
}
