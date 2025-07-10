using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by validation via reflection")]
	public sealed class COLSEnquiryAdditionalInformationValidation : ZValidation
	{
		public COLSEnquiryAdditionalInformationValidation(COLSEnquiryAdditionalInformation parent) : base(parent)
		{
			this.parent = parent;
			this.phoneNumberValidator = new PhoneNumberFormatterAndValidator();
		}
		readonly COLSEnquiryAdditionalInformation parent;
		readonly PhoneNumberFormatterAndValidator phoneNumberValidator;

		public override Type AutoValidationType => typeof(COLSEnquiryAdditionalInformationValidation);

		public override void ValidateAll()
		{
			ValidateEnquiryType();
			ValidateContactName();
			ValidateContactPhone();
			ValidateContactEmail();
		}

		public void ValidateEnquiryType()
		{
			ValidateCalculatedProperty(parent.EnquiryTypeInfo);
		}

		void CheckEnquiryType()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.EnquiryTypeInfo, "Enquiry Type");
		}

		public void ValidateContactName()
		{
			ValidateCalculatedProperty(parent.ContactNameInfo);
		}

		void CheckContactName()
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.ContactNameInfo, "Contact Name");
		}

		public void ValidateContactPhone()
		{
			ValidateCalculatedProperty(parent.ContactPhoneInfo);
		}

		void CheckContactPhone()
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.ContactPhoneInfo, "Contact Phone");
			var phoneNum = parent.ContactPhone;
			if (!phoneNum.IsEmpty)
			{
				var formattedPhoneNum = phoneNumberValidator.FormatLocal(phoneNum, Core.Constants.CountryCodes.Australia);
				if (formattedPhoneNum.IsEmpty)
				{
					parent.ContactPhoneInfo.AddMessageError(PhoneNumberFormatterAndValidator.InvalidPhoneNumberFormat);
				}
			}
		}

		public void ValidateContactEmail()
		{
			ValidateCalculatedProperty(parent.ContactEmailInfo);
		}

		void CheckContactEmail()
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.ContactEmailInfo, "Contact Email");
		}
	}
}
