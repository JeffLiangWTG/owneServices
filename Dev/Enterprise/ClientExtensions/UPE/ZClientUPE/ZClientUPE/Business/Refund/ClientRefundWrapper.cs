using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public class ClientRefundWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ClientRefundWrapper(BusinessObjectFactory factory) : base(factory) { }

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateContact();
			ValidatePhoneNumber();
			ValidateEnquiryDetails();
			ValidateEnquiryRaisedBy();
		}

		#region Contact

		[MaxLength(ClientRefund.Schema.T10_EnquiryContactMaxLength)]
		public ZString Contact
		{
			get { return contact; }
			set
			{
				if (contact != value)
				{
					CheckMaximumLength(ContactInfo, value);
					SetNonPersistentPropertyValue(ContactInfo, ref contact, value);
					if (!IsValidationSuspended)
					{
						ValidateContact();
					}
				}
			}
		}
		ZString contact;

		public ZPropertyInfo ContactInfo
		{
			get { return GetZPropertyInfo(nameof(Contact)); }
		}

		public void ValidateContact()
		{
			ContactInfo.ClearAllNotifications();
			CheckEntered(ContactInfo);
		}

		#endregion

		#region Phone Number

		[MaxLength(ClientRefund.Schema.T10_EnquiryPhoneNumberMaxLength)]
		public ZString PhoneNumber
		{
			get { return phoneNumber; }
			set
			{
				if (phoneNumber != value)
				{
					CheckMaximumLength(PhoneNumberInfo, value);
					SetNonPersistentPropertyValue(PhoneNumberInfo, ref phoneNumber, value);
					if (!IsValidationSuspended)
					{
						ValidatePhoneNumber();
					}
				}
			}
		}
		ZString phoneNumber;

		public ZPropertyInfo PhoneNumberInfo
		{
			get { return GetZPropertyInfo(nameof(PhoneNumber)); }
		}

		public void ValidatePhoneNumber()
		{
			PhoneNumberInfo.ClearAllNotifications();
			CheckEntered(PhoneNumberInfo);
			if (PhoneNumber.ContainsAnyLetters)
			{
				PhoneNumberInfo.AddError("The Phone number must be Digits only.");
			}
		}

		#endregion

		#region Enquiry Details

		[MaxLength(ClientRefund.Schema.T10_EnquiryDetailsMaxLength)]
		public ZString EnquiryDetails
		{
			get { return enquiryDetails; }
			set
			{
				if (enquiryDetails != value)
				{
					CheckMaximumLength(EnquiryDetailsInfo, value);
					SetNonPersistentPropertyValue(EnquiryDetailsInfo, ref enquiryDetails, value);
					if (!IsValidationSuspended)
					{
						ValidateEnquiryDetails();
					}
				}
			}
		}
		ZString enquiryDetails;

		public ZPropertyInfo EnquiryDetailsInfo
		{
			get { return GetZPropertyInfo(nameof(EnquiryDetails)); }
		}

		public void ValidateEnquiryDetails()
		{
			EnquiryDetailsInfo.ClearAllNotifications();
			CheckEntered(EnquiryDetailsInfo);
		}

		#endregion

		#region Enquiry RaisedBy

		[MaxLength(ClientRefund.Schema.T10_EnquiryRaisedByMaxLength)]
		public ZString EnquiryRaisedBy
		{
			get { return enquiryRaisedBy; }
			set
			{
				if (enquiryRaisedBy != value)
				{
					CheckMaximumLength(EnquiryRaisedByInfo, value);
					SetNonPersistentPropertyValue(EnquiryRaisedByInfo, ref enquiryRaisedBy, value);
					if (!IsValidationSuspended)
					{
						ValidateEnquiryRaisedBy();
					}
				}
			}
		}
		ZString enquiryRaisedBy;

		public ZPropertyInfo EnquiryRaisedByInfo
		{
			get { return GetZPropertyInfo(nameof(EnquiryRaisedBy)); }
		}

		public void ValidateEnquiryRaisedBy()
		{
			EnquiryRaisedByInfo.ClearAllNotifications();
			CheckEntered(EnquiryRaisedByInfo);
			ListValidation.ErrorIfInvalidCode(EnquiryRaisedByInfo, RaisedByList);
		}

		#endregion

		public bool EnquiryDetailsValid
		{
			get
			{
				RunPreSaveValidationCore();
				return
					!ContactInfo.HasErrors() &&
					!PhoneNumberInfo.HasErrors() &&
					!EnquiryDetailsInfo.HasErrors() &&
					!EnquiryRaisedByInfo.HasErrors();
			}
		}

		void CheckEntered(ZPropertyInfo propertyInfo)
		{
			MandatoryValidation.CheckEntered(propertyInfo);
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(propertyInfo);
		}

		public RaisedByPairList RaisedByList
		{
			get { return raisedByList ?? (raisedByList = new RaisedByPairList()); }
		}
		RaisedByPairList raisedByList;

		public IRefundEnquiry RefundOwner { get; set; }

		internal void Syncronise(ClientRefund refund)
		{
			refund.T10_EnquiryContact = Contact;
			refund.T10_EnquiryPhoneNumber = PhoneNumber;
			refund.T10_EnquiryDetails = EnquiryDetails;
			refund.T10_EnquiryRaisedBy = EnquiryRaisedBy;
		}
	}
}
