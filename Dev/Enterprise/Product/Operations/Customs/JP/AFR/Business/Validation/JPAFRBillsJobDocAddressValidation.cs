using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRBillsJobDocAddressValidation : JobDocAddressValidation
	{
		public JPAFRBillsJobDocAddressValidation(AutoJobDocAddress parent) : base(parent) { }

		protected new JobDocAddress Parent
		{
			get { return base.Parent; }
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (!Parent.E2_AddressOverride && Parent.HasRealAddress)
			{
				var targetInfo = Parent.E2_OA_AddressInfo;

				var sourceCompanyNameValue = Parent.E2_CompanyName;
				var sourceCompanyNameInfo = Parent.E2_CompanyNameInfo;
				AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(sourceCompanyNameInfo, targetInfo, sourceCompanyNameValue);
				AddWarningIfInappropriate(sourceCompanyNameInfo, targetInfo, sourceCompanyNameValue);

				var sourceAddress1Value = Parent.E2_Address1;
				var sourceAddress1Info = Parent.E2_Address1Info;
				AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(sourceAddress1Info, targetInfo, sourceAddress1Value);
				AddWarningIfInappropriate(sourceAddress1Info, targetInfo, sourceAddress1Value);

				var sourceCityValue = Parent.E2_City;
				var sourceCityInfo = Parent.E2_CityInfo;
				if (sourceCityValue.IsEmpty)
				{
					targetInfo.AddMessageError(ValidationConstants.JobDocAddress.AddressFieldIsRequired(sourceCityInfo.HumanReadableName));
				}
				else
				{
					AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(sourceCityInfo, targetInfo, sourceCityValue);
				}

				var sourcePhoneValue = Parent.E2_Phone;
				var sourcePhoneInfo = Parent.E2_PhoneInfo;
				if (sourcePhoneValue.IsEmpty)
				{
					targetInfo.AddMessageError(ValidationConstants.JobDocAddress.AddressFieldIsRequired(sourcePhoneInfo.HumanReadableName));
				}

				var sourceCountryValue = Parent.E2_RN_NKCountryCode;
				var sourceCountryInfo = Parent.E2_RN_NKCountryCodeInfo;
				if (sourceCountryValue.IsEmpty || !sourceCountryValue.IsValid)
				{
					targetInfo.AddMessageError(ValidationConstants.JobDocAddress.AddressFieldIsRequired(sourceCountryInfo.HumanReadableName));
				}
				else
				{
					AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(sourceCountryInfo, targetInfo, sourceCountryValue);
				}
				var sourceAddress2Value = Parent.E2_Address2;
				var sourceAddress2Info = Parent.E2_Address2Info;
				AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(sourceAddress2Info, targetInfo, sourceAddress2Value);
				AddWarningIfAddressFieldReachMaxAllowed(sourceAddress2Info, targetInfo, ValidationConstants.Constants.Address2MaxLenth);
				var sourceStateValue = Parent.E2_State;
				var sourceStateInfo = Parent.E2_StateInfo;
				AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(sourceStateInfo, targetInfo, sourceStateValue);
				var sourcePostCodeValue = Parent.E2_Postcode;
				var sourcePostCodeInfo = Parent.E2_PostcodeInfo;
				AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(sourcePostCodeInfo, targetInfo, sourcePostCodeValue);
				AddWarningIfAddressFieldReachMaxAllowed(sourcePostCodeInfo, targetInfo, ValidationConstants.Constants.PostCodeMaxLenth);
				ValidateE2_Contact();
			}
		}

		protected override void CheckE2_City()
		{
			base.CheckE2_City();
			if (Parent.E2_AddressOverride)
			{
				var targetInfo = Parent.E2_CityInfo;
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(targetInfo, Parent.E2_City);
			}
		}

		protected override void CheckE2_State()
		{
			base.CheckE2_State();
			if (Parent.E2_AddressOverride)
			{
				AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(Parent.E2_StateInfo, Parent.E2_State);
			}
		}

		protected override void CheckE2_RN_NKCountryCode()
		{
			base.CheckE2_RN_NKCountryCode();
			if (Parent.E2_AddressOverride)
			{
				var targetInfo = Parent.E2_RN_NKCountryCodeInfo;
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(targetInfo, Parent.E2_RN_NKCountryCode);
			}
		}

		protected override void CheckE2_CompanyName()
		{
			base.CheckE2_CompanyName();
			if (Parent.E2_AddressOverride)
			{
				var targetInfo = Parent.E2_CompanyNameInfo;
				var sourceValue = Parent.E2_CompanyName;
				AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(targetInfo, sourceValue);
				AddWarningIfInappropriate(targetInfo, sourceValue);
			}
		}

		protected override void CheckE2_Address1()
		{
			base.CheckE2_Address1();
			if (Parent.E2_AddressOverride)
			{
				var targetInfo = Parent.E2_Address1Info;
				var sourceValue = Parent.E2_Address1;
				AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(targetInfo, sourceValue);
				AddWarningIfInappropriate(targetInfo, sourceValue);
			}
		}

		protected override void CheckE2_Address2()
		{
			base.CheckE2_Address2();
			if (Parent.E2_AddressOverride)
			{
				var targetInfo = Parent.E2_Address2Info;
				AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(targetInfo, Parent.E2_Address2);
				AddWarningIfAddressFieldReachMaxAllowed(targetInfo, ValidationConstants.Constants.Address2MaxLenth);
			}
		}

		protected override void CheckE2_Postcode()
		{
			base.CheckE2_Postcode();
			if (Parent.E2_AddressOverride)
			{
				var targetInfo = Parent.E2_PostcodeInfo;
				AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(targetInfo, Parent.E2_Postcode);
				AddWarningIfAddressFieldReachMaxAllowed(targetInfo, ValidationConstants.Constants.PostCodeMaxLenth);
			}
		}

		protected override void CheckE2_Contact()
		{
			base.CheckE2_Contact();
			if (!Parent.E2_AddressOverride)
			{
				var sourceInfo = Parent.E2_PhoneInfo;
				var targetInfo = Parent.E2_ContactInfo;
				var sourceValue = Parent.E2_Phone;
				var limit = ValidationConstants.Constants.PhoneMaxLength;
				AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(sourceInfo, targetInfo, sourceValue);
				if (sourceValue.ToString().Length > limit)
				{
					targetInfo.AddWarning(ValidationConstants.JobDocAddress.PhoneNumberLengthReachedMaxAllowed(sourceInfo.HumanReadableName, limit));
				}
			}
		}

		protected override void CheckE2_Phone()
		{
			base.CheckE2_Phone();
			if (Parent.E2_AddressOverride)
			{
				var targetInfo = Parent.E2_PhoneInfo;
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(targetInfo, Parent.E2_Phone);
				AddWarningIfAddressFieldReachMaxAllowed(targetInfo, ValidationConstants.Constants.PhoneMaxLength);
			}
		}

		#region Implementation

		#region AddWarningIfAddressFieldReachMaxAllowed

		void AddWarningIfAddressFieldReachMaxAllowed(ZPropertyInfo targetInfo, int lengthLimit)
		{
			AddWarningIfAddressFieldReachMaxAllowed(targetInfo, targetInfo, lengthLimit);
		}

		void AddWarningIfAddressFieldReachMaxAllowed(ZPropertyInfo sourceInfo, ZPropertyInfo targetInfo, int lengthLimit)
		{
			var newsourceField = sourceInfo.Value.ToString();
			if (newsourceField.Length > lengthLimit)
			{
				targetInfo.AddWarning(ValidationConstants.Shared.FieldLengthReachedMaxAllowedWillBeTruncated(sourceInfo.HumanReadableName, lengthLimit));
			}
		}

		#endregion

		#region  AddMessageErrorIfAddressFieldContainsInvalidNACCSChar

		void AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(ZPropertyInfo targetInfo, ZString sourceValue)
		{
			AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(targetInfo, targetInfo, sourceValue);
		}

		void AddMessageErrorIfAddressFieldContainsInvalidNACCSChar(ZPropertyInfo sourceInfo, ZPropertyInfo targetInfo, ZString sourceValue)
		{
			if (sourceValue.ContainsInvalidNACCSCharacters())
			{
				targetInfo.AddMessageError(ValidationConstants.Shared.InvalidNACCSChar(sourceInfo.HumanReadableName));
			}
		}

		#endregion

		#region AddWarningIfInappropriate

		void AddWarningIfInappropriate(ZPropertyInfo targetInfo, ZString sourceValue)
		{
			AddWarningIfInappropriate(targetInfo, targetInfo, sourceValue);
		}

		void AddWarningIfInappropriate(ZPropertyInfo sourceInfo, ZPropertyInfo targetInfo, ZString sourceValue)
		{
			if (sourceValue.KeepAlphabeticCharacters().Length < 3)
			{
				targetInfo.AddWarning(ValidationConstants.JobDocAddress.InappropriateCompanyInfo(sourceInfo.HumanReadableName));
			}
		}

		#endregion

		#endregion
	}
}
