using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CA.Business
{
	public class TradeChainPartnerSendingObject : NonPersistentBusinessObject, ITradeChainPartner
	{
		public TradeChainPartnerSendingObject(TradeChainPartner sendingObject, OrgHeader parent) : base(sendingObject.Factory)
		{
			SendOption = ZBool.False;
			if (sendingObject.CA_CSAStatus == CSAStatusList.Codes.Added)
			{
				ActionType = CSAStatusList.Codes.Deleted;
			}
			else
			{
				ActionType = CSAStatusList.Codes.Added;
			}
			this.tradeChainPartner = sendingObject;
			this.parent = parent;
		}

		public static class Schema
		{
			public const string SendOption = "SendOption";
			public const string ActionType = "ActionType";
		}

		public bool ColumnReadOnly => true;

		public ZBool SendOption
		{
			get
			{
				return sendOption;
			}
			set
			{
				SetNonPersistentPropertyValue(SendOptionInfo, ref sendOption, value);
			}
		}
		ZBool sendOption;

		public ZPropertyInfo SendOptionInfo
		{
			get { return GetZPropertyInfo(Schema.SendOption); }
		}

		public ZString ActionType
		{
			get
			{
				return actionType;
			}
			set
			{
				SetNonPersistentPropertyValue(ActionTypeInfo, ref actionType, value);
			}
		}
		ZString actionType;

		public ZPropertyInfo ActionTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ActionType); }
		}

		[ReadOnlyMember(nameof(ColumnReadOnly))]
		public ZGuid CA_Org
		{
			get
			{
				return TradeChainPartner.CA_Org;
			}
			set
			{
				var oldValue = TradeChainPartner.CA_Org;
				TradeChainPartner.CA_Org = value;
				if (!IsCopying && oldValue != value)
				{
					CA_OrgInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo CA_OrgInfo
		{
			get { return GetZPropertyInfo(nameof(CA_Org)); }
		}

		[ReadOnlyMember(nameof(ColumnReadOnly))]
		public ZGuid CA_Address
		{
			get
			{
				return TradeChainPartner.CA_Address;
			}
			set
			{
				var oldValue = TradeChainPartner.CA_Address;
				TradeChainPartner.CA_Address = value;
				if (!IsCopying && oldValue != value)
				{
					CA_AddressInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo CA_AddressInfo
		{
			get { return GetZPropertyInfo(nameof(CA_Address)); }
		}

		[ReadOnlyMember(nameof(ColumnReadOnly))]
		[MaxLength(1)]
		public ZString CA_Type
		{
			get
			{
				return TradeChainPartner.CA_Type;
			}
			set
			{
				var oldValue = TradeChainPartner.CA_Type;
				TradeChainPartner.CA_Type = value;
				if (!IsCopying && oldValue != value)
				{
					CA_TypeInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCA_Type();
				}
			}
		}

		public ZPropertyInfo CA_TypeInfo
		{
			get { return GetZPropertyInfo(nameof(CA_Type)); }
		}

		[ReadOnlyMember(nameof(ColumnReadOnly))]
		[MaxLength(TradeChainPartnerAddInfo.Schema.CA_ActionMaxLength)]
		public ZString CA_Action
		{
			get
			{
				return TradeChainPartner.CA_Action;
			}
			set
			{
				var oldValue = TradeChainPartner.CA_Action;
				TradeChainPartner.CA_Action = value;
				if (!IsCopying && oldValue != value)
				{
					CA_ActionInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo CA_ActionInfo
		{
			get { return GetZPropertyInfo(nameof(CA_Action)); }
		}

		[ReadOnlyMember(nameof(ColumnReadOnly))]
		[MaxLength(TradeChainPartnerAddInfo.Schema.CA_CSAIDTypeMaxLength)]
		public ZString CA_CSAIDType
		{
			get
			{
				return TradeChainPartner.CA_CSAIDType;
			}
			set
			{
				var oldValue = TradeChainPartner.CA_CSAIDType;
				TradeChainPartner.CA_CSAIDType = value;
				if (!IsCopying && oldValue != value)
				{
					CA_CSAIDTypeInfo.RefreshBinding(oldValue);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCA_CSAIDType();
				}
			}
		}

		public ZPropertyInfo CA_CSAIDTypeInfo
		{
			get { return GetZPropertyInfo(nameof(CA_CSAIDType)); }
		}

		[ReadOnlyMember(nameof(ColumnReadOnly))]
		[MaxLength(TradeChainPartnerAddInfo.Schema.CA_CSAIDMaxLength)]
		public ZString CA_CSAID
		{
			get
			{
				return TradeChainPartner.CA_CSAID;
			}
			set
			{
				var oldValue = TradeChainPartner.CA_CSAID;
				TradeChainPartner.CA_CSAID = value;
				if (!IsCopying && oldValue != value)
				{
					CA_CSAIDInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo CA_CSAIDInfo
		{
			get { return GetZPropertyInfo(nameof(CA_CSAID)); }
		}

		public TradeChainPartnerSendingObjectValidation Validation
		{
			get
			{
				return new TradeChainPartnerSendingObjectValidation(this);
			}
		}

		public TradeChainPartner TradeChainPartner
		{
			get => tradeChainPartner;
		}
		readonly TradeChainPartner tradeChainPartner;

		public OrgHeader Parent
		{
			get => parent;
		}
		readonly OrgHeader parent;

		#region ITradeChainPartner

		ZString ITradeChainPartner.MessageNumber
		{
			get
			{
				if (messageNumber.IsEmpty)
				{
					messageNumber = EDIMessage.MessageNumberPlaceHolder;
				}
				return messageNumber;
			}
		}
		ZString messageNumber;

		bool ITradeChainPartner.IsVendor => CA_Type == TradeChainPartnersTypeList.Codes.V;

		ZString ITradeChainPartner.ApplicationImporterNumber
		{
			get
			{
				var result = ZString.Empty;
				var regNumber = Parent.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, Core.Constants.CountryCodes.Canada);
				if (!regNumber.IsEmpty)
				{
					var regNumbers = regNumber.Split("R");
					result = regNumbers[0];
				}
				return result;
			}
		}
		ZString ITradeChainPartner.DivisionImporterNumber
		{
			get
			{
				return Parent.CustomsCodes.GetCustomsRegNo(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, Core.Constants.CountryCodes.Canada);
			}
		}

		ZDateTime ITradeChainPartner.TransactionDate => ZDateTime.Now;

		ZString ITradeChainPartner.OrgCodeType => CA_CSAIDType;

		ZString ITradeChainPartner.ReferenceIdentifier => CA_CSAID;

		IDocAddress ITradeChainPartner.VendorOrConsigneeAddress => Factory.Load<OrgAddress>(CA_Address);

		#endregion

		#region ICAEDIFACTMessageAttachee

		bool ICAEDIFACTMessageAttachee.IsCancelled => false;

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get
			{
				return messageStatus.IsValid ? messageStatus : ZString.Empty;
			}
			set
			{
				messageStatus = value;
			}
		}
		ZString messageStatus;

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get
			{
				return jobStatus.IsValid ? jobStatus : ZString.Empty;
			}
			set
			{
				jobStatus = value;
			}
		}
		ZString jobStatus;

		bool IEDIFACTMessageAttachee.HasChanges => TradeChainPartner.CA_Action == ActionType;

		ZString IEDIFACTMessageAttachee.JobIdentification => MessageTypeList.Codes.TradeChainPartner + "-" + messageNumber;

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject => TradeChainPartner;

		Enterprise.Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new Enterprise.Messaging.Business.EDIMessageCollection(TradeChainPartner, TradeChainPartner.Factory);
				}
				return messages;
			}
		}
		Enterprise.Messaging.Business.EDIMessageCollection messages;

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory => this.Factory;

		void IEDIFACTMessageAttachee.AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;

		#endregion
	}
}
