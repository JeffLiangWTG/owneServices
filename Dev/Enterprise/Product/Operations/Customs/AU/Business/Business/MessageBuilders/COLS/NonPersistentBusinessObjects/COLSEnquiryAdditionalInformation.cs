using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class COLSEnquiryAdditionalInformation : NonPersistentBusinessObject
	{
		public COLSEnquiryAdditionalInformation(QuarantineColsHeader colsHeader)
			: base(colsHeader.Factory)
		{
			this.colsHeader = Argument.NotNull(colsHeader, "colsHeader");
			DefaultContactDetailsInfo.ValueChanged += DefaultContactDetails_ValueChanged;
			DefaultContactDetails_ValueChanged(null, null);
		}
		readonly QuarantineColsHeader colsHeader;

		#region Properties

		[List(nameof(Lookups) + "." + nameof(COLSEnquiryAdditionalInformationLookups.EnquiryType))]
		public ZString EnquiryType
		{
			get { return enquiryType; }
			set
			{
				SetNonPersistentPropertyValue(EnquiryTypeInfo, ref enquiryType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateEnquiryType();
				}
			}
		}
		ZString enquiryType;

		public ZPropertyInfo EnquiryTypeInfo => GetZPropertyInfo(nameof(EnquiryType));

		public ZString ContactName
		{
			get { return contactName; }
			set
			{
				SetNonPersistentPropertyValue(ContactNameInfo, ref contactName, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateContactName();
				}
			}
		}
		ZString contactName;

		public ZPropertyInfo ContactNameInfo => GetZPropertyInfo(nameof(ContactName));

		public ZString ContactPhone
		{
			get { return contactPhone; }
			set
			{
				SetNonPersistentPropertyValue(ContactPhoneInfo, ref contactPhone, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateContactPhone();
				}
			}
		}
		ZString contactPhone;

		public ZPropertyInfo ContactPhoneInfo => GetZPropertyInfo(nameof(ContactPhone));

		public ZString ContactEmail
		{
			get { return contactEmail; }
			set
			{
				SetNonPersistentPropertyValue(ContactEmailInfo, ref contactEmail, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateContactEmail();
				}
			}
		}
		ZString contactEmail;

		public ZPropertyInfo ContactEmailInfo => GetZPropertyInfo(nameof(ContactEmail));

		public ZBool DefaultContactDetails
		{
			get { return defaultContactDetails; }
			set
			{
				SetNonPersistentPropertyValue(DefaultContactDetailsInfo, ref defaultContactDetails, value);
			}
		}
		ZBool defaultContactDetails;

		public ZPropertyInfo DefaultContactDetailsInfo => GetZPropertyInfo(nameof(DefaultContactDetails));

		public ZBool DocumentRequired
		{
			get { return documentRequired; }
			set
			{
				SetNonPersistentPropertyValue(DocumentRequiredInfo, ref documentRequired, value);
			}
		}
		ZBool documentRequired;

		public ZPropertyInfo DocumentRequiredInfo => GetZPropertyInfo(nameof(DocumentRequired));

		public COLSDeclarationAcceptance DeclarationAcceptance
		{
			get
			{
				if (declarationAcceptance == null)
				{
					declarationAcceptance = new COLSDeclarationAcceptance();
				}
				return declarationAcceptance;
			}
		}
		COLSDeclarationAcceptance declarationAcceptance;

		#endregion

		#region Lookups

		public COLSEnquiryAdditionalInformationLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new COLSEnquiryAdditionalInformationLookups(this);
				}

				return fLookups;
			}
		}
		COLSEnquiryAdditionalInformationLookups fLookups;

		#endregion

		#region Validation

		public COLSEnquiryAdditionalInformationValidation Validation => new COLSEnquiryAdditionalInformationValidation(this);

		#endregion

		void DefaultContactDetails_ValueChanged(object sender, System.EventArgs e)
		{
			var responsiblePartyAddress = colsHeader.ResponsibleParty;
			if (DefaultContactDetails && responsiblePartyAddress != null)
			{
				ContactName = responsiblePartyAddress.E2_Contact;
				ContactPhone = responsiblePartyAddress.E2_Phone_Formatted.IsEmpty ? responsiblePartyAddress.E2_Mobile_Formatted.KeepChars("+0123456789") : responsiblePartyAddress.E2_Phone_Formatted.KeepChars("+0123456789");
				ContactEmail = responsiblePartyAddress.E2_Email;
			}
			else
			{
				ContactName = string.Empty;
				ContactPhone = string.Empty;
				ContactEmail = string.Empty;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DefaultContactDetails = true;
		}
	}
}
