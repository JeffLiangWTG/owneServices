using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.JP.Common;
using static Enterprise.Customs.JP.Common.JPMessageActionList;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class ManifestMessageSendingObject : Customs.Business.BaseMessageSendingObject
	{
		public ManifestMessageSendingObject(AsycudaBill bill, ManifestMessageSendingObjectParent parent = null) : base(bill.Factory)
		{
			Parent = parent;
			Bill = Argument.NotNull(bill, nameof(bill));
			Header = Bill.Header;
			DefaultValues();
		}

		public ManifestMessageSendingObjectParent Parent { get; }

		public AsycudaBill Bill { get; }

		public AsycudaManifestHeader Header { get; }

		[ReadOnlyMember(nameof(ActionReadOnly))]
		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(ManifestMessageSendingObjectLookups.ActionList))]
		[ResourceStringData("580DD01F-C606-4B20-8CF4-AED5D640C7CC", Caption = "Action")]
		public ZString Action
		{
			get => action;
			set
			{
				SetNonPersistentPropertyValue(ActionInfo, ref action, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAction();
					Validation.ValidateShouldSend();
				}
				ShouldSendInfo.RefreshBinding();
			}
		}

		ZString action;

		public ZPropertyInfo ActionInfo => GetZPropertyInfo(nameof(Action));

		[ResourceStringData("66435F09-8B2F-4B86-AD63-9AF59D4638DC", Caption = "Action Description")]
		public ZString ActionDescription => Lookups.ActionList.GetDescriptionFromCode(action);

		public ZPropertyInfo ActionDescriptionInfo => GetZPropertyInfo(nameof(ActionDescription));

		ZBool ActionReadOnly => MessageType != JPProcedureCodeList.Codes.HDF01 && MessageType != JPProcedureCodeList.Codes.NVC01;

		[List(nameof(Lookups) + "." + nameof(ManifestMessageSendingObjectLookups.MessageTypeList))]
		[ResourceStringData("78516EBF-1AEC-4B24-BC0B-AE27824E454A", Caption = "Message Type")]
		public ZString MessageType
		{
			get => messageType;
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

		public override ZBool ShouldSend
		{
			get => base.ShouldSend;
			set
			{
				var oldValue = base.ShouldSend;
				base.ShouldSend = value;

				if (!IsValidationSuspended && ShouldSend != oldValue)
				{
					Validation.ValidateShouldSend();
					Validation.ValidateReason();
				}

				if (Parent != null && !Parent.IsValidationSuspended)
				{
					Parent.Validation.ValidateEndSendMessage();
				}
			}
		}

		public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(nameof(MessageType));

		#region CHA

		[List(nameof(Lookups) + "." + nameof(ManifestMessageSendingObjectLookups.ReasonList))]
		[ResourceStringData("JP|ManifestMessageSendingObject|Reason", Caption = "Reason")]
		public ZString Reason
		{
			get => reason;
			set
			{
				SetNonPersistentPropertyValue(ReasonInfo, ref reason, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReason();
				}
			}
		}

		ZString reason;

		public ZPropertyInfo ReasonInfo => GetZPropertyInfo(nameof(Reason));

		#endregion

		void DefaultValues()
		{
			if (Parent != null)
			{
				MessageType = Parent.CurrentProcedureCode;
				DefaultAction();
			}
		}

		void DefaultAction()
		{
			if (Parent.IsHDF01)
			{
				switch (Bill.ABL_BillStatus)
				{
					case JPCustomsStatusList.Codes.AWR:
					case JPCustomsStatusList.Codes.AWC:
					case JPCustomsStatusList.Codes.AWD:
						Action = HDF01MessageActionList.Codes.X;
						break;
				}
			}
			else if (MessageType == JPProcedureCodeList.Codes.NVC01)
			{
				Action = Bill.ABL_BillStatus.ToString() switch
				{
					"" or JPCustomsStatusList.Codes.AWR or JPCustomsStatusList.Codes.AWA or JPCustomsStatusList.Codes.AWD => NVC01MessageActionList.Codes.Nine,
					JPCustomsStatusList.Codes.REG or JPCustomsStatusList.Codes.AMD => NVC01MessageActionList.Codes.Five,
					_ => string.Empty,
				};
			}
		}

		public ManifestMessageSendingObjectLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = new ManifestMessageSendingObjectLookups(this);
				}
				return lookups;
			}
		}
		ManifestMessageSendingObjectLookups lookups;

		public ManifestMessageSendingObjectValidation Validation => GetNewValidation();

		protected ManifestMessageSendingObjectValidation GetNewValidation() => new (this);
	}
}
