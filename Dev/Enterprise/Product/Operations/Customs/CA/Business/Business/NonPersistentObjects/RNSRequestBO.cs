//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	using System;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Business.MessageBuilders;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.CA.Registry;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Customs.Universal;
	using Enterprise.Messaging.Business;

	public class RNSRequestBO : NonPersistentBusinessObject, IObsoleteValidation, IRNSRequest
	{
		public RNSRequestBO(string messageSubType, BusinessObjectFactory factory)
			: this(messageSubType, factory, true, true)
		{
		}
		public RNSRequestBO(string messageSubType, BusinessObjectFactory factory, bool isCargoControlNumberEditable, bool isTransactionNumberApplicable)
			: this(null, messageSubType, factory, isCargoControlNumberEditable, isTransactionNumberApplicable)
		{
		}

		public RNSRequestBO(RNSMessagingBO rnsMessaging, string messageSubType, BusinessObjectFactory factory, bool isCargoControlNumberEditable, bool isTransactionNumberApplicable)
			: base(factory)
		{
			if (messageSubType != RNSMessageTypes.Codes.StatusQuery && messageSubType != RNSMessageTypes.Codes.ArrivalCertification)
			{
				throw new ArgumentException("Invalid value", nameof(messageSubType));
			}

			this.rnsMessaging = rnsMessaging;

			using (SuspendSettingHasChanges())
			{
				this.messageSubType = messageSubType;
				this.IsCargoControlNumberEditable = isCargoControlNumberEditable;
				this.IsTransactionNumberApplicable = isTransactionNumberApplicable;
				DateOfArrival = ZDateTime.Now;
				OfficeCode = CACustomsDataRegistry.Instance.DefaultRNSOffice.Value;

				if (this.rnsMessaging != null)
				{
					this.CargoControlNumber = this.rnsMessaging.PlugInSupport.CargoControlNumber;
					this.HouseBillNumber = this.rnsMessaging.PlugInSupport.HouseBillNumber;
				}
			}
		}

		readonly RNSMessagingBO rnsMessaging;

		protected override ZString HumanReadableNameCore => string.Empty;

		#region Properties

		public string MessageDescription => Factory.GetCachedValue<RNSMessageTypes>().GetDescriptionFromCode(messageSubType);

		public bool IsArrivalCertification => messageSubType == RNSMessageTypes.Codes.ArrivalCertification;

		public bool IsTransactionNumberApplicable { get; }

		public bool IsCargoControlNumberEditable { get; }

		#region DateOfArrival

		[ReadOnlyMember(nameof(IsStatusQuery))]
		public ZDateTime DateOfArrival
		{
			get { return dateOfArrival; }
			set
			{
				SetNonPersistentPropertyValue(DateOfArrivalInfo, ref dateOfArrival, value);
				ValidateDateOfArrival();
			}
		}

		ZDateTime dateOfArrival;

		public ZPropertyInfo DateOfArrivalInfo => GetZPropertyInfo(nameof(DateOfArrival));

		protected bool IsStatusQuery => messageSubType == RNSMessageTypes.Codes.StatusQuery;

		#endregion

		#region HouseBillNumber

		[ReadOnlyMember(nameof(HouseBillNumber_ReadOnly))]
		public ZString HouseBillNumber
		{
			get { return houseBillNumber; }
			set { SetNonPersistentPropertyValue(HouseBillNumberInfo, ref houseBillNumber, value); }
		}
		ZString houseBillNumber;

		public ZPropertyInfo HouseBillNumberInfo => GetZPropertyInfo(nameof(HouseBillNumber));

		bool HouseBillNumber_ReadOnly => true;

		#endregion

		#region Selected

		public ZBool Selected
		{
			get { return fSelected; }
			set { SetNonPersistentPropertyValue(SelectedInfo, ref fSelected, value); }
		}
		ZBool fSelected;

		public ZPropertyInfo SelectedInfo => GetZPropertyInfo(nameof(Selected));

		#endregion

		#region CargoControlNumber

		[MaxLength(35)]
		[ReadOnlyMember(nameof(CargoControlNumber_ReadOnly))]
		public ZString CargoControlNumber
		{
			get { return cargoControlNumber; }
			set
			{
				SetNonPersistentPropertyValue(CargoControlNumberInfo, ref cargoControlNumber, value);
				ValidateCargoControlNumber();
				ValidateTransactionNumber();
			}
		}

		ZString cargoControlNumber;

		public ZPropertyInfo CargoControlNumberInfo => GetZPropertyInfo(nameof(CargoControlNumber));

		bool CargoControlNumber_ReadOnly => !IsCargoControlNumberEditable;

		#endregion

		#region TransactionNumber

		[MaxLength(14)]
		[ReadOnlyMember(nameof(TransactionNumber_ReadOnly))]
		public ZString TransactionNumber
		{
			get => transactionNumber;
			set
			{
				if (IsTransactionNumberApplicable)
				{
					SetNonPersistentPropertyValue(TransactionNumberInfo, ref transactionNumber, value);
					ValidateTransactionNumber();
				}
			}
		}

		ZString transactionNumber;

		public ZPropertyInfo TransactionNumberInfo => GetZPropertyInfo(nameof(TransactionNumber));

		bool TransactionNumber_ReadOnly => !IsTransactionNumberApplicable;

		#endregion

		#region OfficeCode

		[List(nameof(CBSAOffices))]
		[ReadOnlyMember(nameof(OfficeCode_ReadOnly))]
		[MaxLength(4)]
		public ZString OfficeCode
		{
			get => officeCode;
			set
			{
				SetNonPersistentPropertyValue(OfficeCodeInfo, ref officeCode, value.IsEmpty ? value : value.PadLeft(4, '0'));
				ValidateOfficeCode();
				if (OfficeCode.IsEmpty && !SubLocationCode.IsEmpty)
				{
					SubLocationCode = ZString.Empty;
				}
				ValidateSubLocationCode();
			}
		}
		ZString officeCode;

		public ZPropertyInfo OfficeCodeInfo => GetZPropertyInfo(nameof(OfficeCode));

		bool OfficeCode_ReadOnly => !IsArrivalCertification;

		public ZZRefCusCodeListCombinedCollection CBSAOffices => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);

		#endregion

		#region Sub-Location Code

		[List(nameof(SubLocationCodes))]
		[ReadOnlyMember(nameof(SubLocationCode_ReadOnly))]
		[MaxLength(4)]
		public ZString SubLocationCode
		{
			get => subLocationCode;
			set
			{
				SetNonPersistentPropertyValue(SubLocationCodeInfo, ref subLocationCode, value.IsEmpty ? value : value.PadLeft(4, '0'));
				ValidateSubLocationCode();
			}
		}
		ZString subLocationCode;

		public ZPropertyInfo SubLocationCodeInfo => GetZPropertyInfo(nameof(SubLocationCode));

		bool SubLocationCode_ReadOnly => IsStatusQuery || OfficeCode.IsEmpty;

		public CACSubLocationCollection SubLocationCodes
		{
			get
			{
				var result = new CACSubLocationCollection(Factory);
				result.FilterBusinessObjectDefaults.RemoveAll();
				if (!SubLocationCode.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Sub-Location Code", "Property", SubLocationCode, true));
				}
				if (!OfficeCode.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Port", "Property", OfficeCode, false));
				}
				return result;
			}
		}
		#endregion

		#endregion

		#region ICAEDIFACTMessageAttachee Members

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory
		{
			get { return Factory; }
		}

		void IEDIFACTMessageAttachee.AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			if (this.rnsMessaging == null)
			{
				message.EM_LinkUniqueID = ZGuid.Empty;
			}
			else
			{
				((IEDIFACTMessageAttachee)this.rnsMessaging).AddMessage(message);
			}
		}

		Enterprise.Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages
		{
			get
			{
				if (this.rnsMessaging == null)
				{
					return new EDIMessageCollection(this);
				}
				else
				{
					return ((IEDIFACTMessageAttachee)this.rnsMessaging).Messages;
				}
			}
		}

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get
			{
				if (this.rnsMessaging == null)
				{
					return ZString.Empty;
				}
				else
				{
					return ((IEDIFACTMessageAttachee)this.rnsMessaging).MessageStatus;
				}
			}
			set { throw new NotSupportedException(); }
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		bool IEDIFACTMessageAttachee.HasChanges
		{
			get
			{
				if (this.rnsMessaging == null)
				{
					return false;
				}
				else
				{
					return ((IEDIFACTMessageAttachee)this.rnsMessaging).HasChanges;
				}
			}
		}

		ZString IEDIFACTMessageAttachee.JobIdentification
		{
			get { return ZString.Empty; }
		}

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject
		{
			get
			{
				if (this.rnsMessaging == null)
				{
					return this;
				}
				else
				{
					return ((IEDIFACTMessageAttachee)this.rnsMessaging).TopLevelBusinessObject;
				}
			}
		}

		bool ICAEDIFACTMessageAttachee.IsCancelled
		{
			get
			{
				if (this.rnsMessaging == null)
				{
					return false;
				}
				else
				{
					return ((ICAEDIFACTMessageAttachee)this.rnsMessaging).IsCancelled;
				}
			}
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage
		{
			get { return true; }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDateOfArrival();
			ValidateCargoControlNumber();
			ValidateTransactionNumber();
			ValidateOfficeCode();
			ValidateSubLocationCode();
		}

		public void ValidateDateOfArrival()
		{
			if (!IsValidationSuspended)
			{
				DateOfArrivalInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(DateOfArrivalInfo);
				TypeValidation.CheckValidZDateTimeWithoutRange(DateOfArrivalInfo);
			}
		}

		internal static string NumberRequiredErrorText
		{
			get { return Res.GetString("698f4ffd-ef73-4c82-a9ce-53a53870607d", "Either a CCN or Transaction # must be entered, but both may not be entered at the same time"); }
		}

		internal static string CCNRequiredErrorText
		{
			get { return Res.GetString("58754960-8765-45fb-a291-cd9d943dcfb4", "CCN must be entered"); }
		}

		public void ValidateCargoControlNumber()
		{
			if (IsCargoControlNumberEditable && !IsValidationSuspended)
			{
				CargoControlNumberInfo.ClearAllNotifications();
				if (CargoControlNumber.IsEmpty)
				{
					CargoControlNumberInfo.AddError(CCNRequiredErrorText);
				}
			}
		}

		public void ValidateTransactionNumber()
		{
			if (IsTransactionNumberApplicable && !IsValidationSuspended)
			{
				TransactionNumberInfo.ClearAllNotifications();
				if (CargoControlNumber.IsEmpty && TransactionNumber.IsEmpty)
				{
					TransactionNumberInfo.AddError(NumberRequiredErrorText);
				}
			}
		}

		public void ValidateOfficeCode()
		{
			if (!IsValidationSuspended && IsArrivalCertification)
			{
				OfficeCodeInfo.ClearAllNotifications();
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(OfficeCodeInfo, CBSAOffices, Res.GetString("6434ECE4-DEB9-49E4-A0AE-947813D67848", "Office Code"));
			}
		}

		public void ValidateSubLocationCode()
		{
			if (!IsValidationSuspended && IsArrivalCertification)
			{
				SubLocationCodeInfo.ClearAllNotifications();
				MandatoryValidation.MessageErrorIfNotEntered(SubLocationCodeInfo);
				if (!subLocationCode.IsEmpty)
				{
					var subLocation = CACSubLocation.Load(Factory, subLocationCode);
					if (subLocation == null || subLocation != null && subLocation.SL_Port.TrimStart('0') != OfficeCode.TrimStart('0'))
					{
						SubLocationCodeInfo.AddMessageError(Res.GetString("E05FEB8D-296B-4D97-ADFC-0B6E5AE9792E", "The Sub-Location entered is not valid for the CBSA office entered."));
					}
				}
			}
		}

		#endregion

		readonly string messageSubType;

		#region IRNSRequest

		void IRNSRequest.RefreshMessagesForDisplay()
		{
			rnsMessaging?.MessagesForDisplay.Reload(true);
		}

		#endregion
	}
}
