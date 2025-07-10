using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.JAS.Business
{
	public class ValidationHelper
	{
		#region CharacterTypes

		enum CharacterTypes
		{
			Free,
			Numeric,
			LettersOnly,
			Alphanumeric
		}

		#endregion

		#region ValidateNumeric

		public void ValidateNumericTextFieldWithExactLength(ZPropertyInfo propertyInfo, int expectedLength)
		{
			ValidateTextFieldWithExactLength(propertyInfo, CharacterTypes.Numeric, expectedLength);
		}

		public void ValidateNumericTextField(ZPropertyInfo propertyInfo, int maxLength)
		{
			ValidateTextField(propertyInfo, CharacterTypes.Numeric, maxLength);
		}

		public void ValidateNumericTextField(ZPropertyInfo propertyInfo, int minLength, int maxLength)
		{
			ValidateTextField(propertyInfo, CharacterTypes.Numeric, minLength, maxLength);
		}

		#endregion

		#region ValidateAlphabetic

		public void ValidateAlphabeticTextFieldWithExactLength(ZPropertyInfo propertyInfo, int expectedLength)
		{
			ValidateTextFieldWithExactLength(propertyInfo, CharacterTypes.LettersOnly, expectedLength);
		}

		public void ValidateAlphabeticTextField(ZPropertyInfo propertyInfo, int maxLength)
		{
			ValidateTextField(propertyInfo, CharacterTypes.LettersOnly, maxLength);
		}

		public void ValidateAlphabeticTextField(ZPropertyInfo propertyInfo, int minLength, int maxLength)
		{
			ValidateTextField(propertyInfo, CharacterTypes.LettersOnly, minLength, maxLength);
		}

		#endregion

		#region ValidateAlphanumeric

		public void ValidateAlphanumericTextFieldWithExactLength(ZPropertyInfo propertyInfo, int expectedLength)
		{
			ValidateTextFieldWithExactLength(propertyInfo, CharacterTypes.Alphanumeric, expectedLength);
		}

		public void ValidateAlphanumericTextField(ZPropertyInfo propertyInfo, int maxLength)
		{
			ValidateTextField(propertyInfo, CharacterTypes.Alphanumeric, maxLength);
		}

		public void ValidateAlphanumericTextField(ZPropertyInfo propertyInfo, int minLength, int maxLength)
		{
			ValidateTextField(propertyInfo, CharacterTypes.Alphanumeric, minLength, maxLength);
		}

		#endregion

		#region ValidateFreeText

		public void ValidateFreeTextFieldWithExactLength(ZPropertyInfo propertyInfo, int expectedLength)
		{
			ValidateTextFieldWithExactLength(propertyInfo, CharacterTypes.Free, expectedLength);
		}

		public void ValidateFreeTextField(ZPropertyInfo propertyInfo, int maxLength)
		{
			ValidateTextField(propertyInfo, CharacterTypes.Free, maxLength);
		}

		public void ValidateFreeTextField(ZPropertyInfo propertyInfo, int minLength, int maxLength)
		{
			ValidateTextField(propertyInfo, CharacterTypes.Free, minLength, maxLength);
		}

		#endregion

		#region ValidateRegex

		public void ValidateRegexField(ZPropertyInfo propertyInfo, ZString value, string pattern, string message)
		{
			if (!Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase))
			{
				AddJXCWarning(propertyInfo, message);
			}
		}

		public void ValidateRegexField(ZPropertyInfo propertyInfo, string pattern, string message)
		{
			ValidateRegexField(propertyInfo, propertyInfo.Value.ToString(), pattern, message);
		}

		#endregion

		#region Validate JXC Netting and Office Code

		public void ValidateJXCForwarder(ZPropertyInfo propertyInfo, JASOrgHeader forwarder)
		{
			ValidateJXCOrganisation(propertyInfo, forwarder, "Forwarder");
		}

		public void ValidateJXCDebtor(ZPropertyInfo propertyInfo, JASOrgHeader debtor)
		{
			ValidateJXCOrganisation(propertyInfo, debtor, "Debtor");
		}

		public void ValidateJXCBranchProxy(ZPropertyInfo propertyInfo, JASOrgHeader orgProxy)
		{
			ValidateJXCOrganisation(propertyInfo, orgProxy, "Proxy Organisation for the current Branch");
		}

		void ValidateJXCOrganisation(ZPropertyInfo propertyInfo, JASOrgHeader organisation, ZString organisationType)
		{
			if (organisation != null && propertyInfo != null)
			{
				if (organisation.NettingCode.IsEmpty)
				{
					AddJXCWarning(propertyInfo, organisationType + " does not have JAS Netting Code");
				}

				if (organisation.OfficeCode.IsEmpty)
				{
					AddJXCWarning(propertyInfo, organisationType + " does not have JAS Office Code");
				}
			}
		}

		#endregion

		public void ValidateCurrencyCode(BusinessObjectFactory factory, ZPropertyInfo propertyInfo, ZString currencyCode)
		{
			if (propertyInfo != null)
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);
				if (currency == null)
				{
					AddJXCWarning(propertyInfo, "Invalid currency code");
				}
			}
		}

		public void AddJXCWarningIfNotEntered(ZPropertyInfo propertyInfo)
		{
			JXCMandatoryValidation.WarnIfNotEntered(propertyInfo);
		}

		public void AddJXCWarningIfInvalidCode(ZPropertyInfo propertyInfo, ICodeDescriptionPairList list)
		{
			JXCListValidation.WarnIfInvalidCode(propertyInfo, list);
		}

		public void AddJXCWarningIfInvalidCode(ZPropertyInfo propertyInfo, IBusinessObjectCollection list)
		{
			JXCListValidation.WarnIfInvalidCode(propertyInfo, list, (NoResString)(JXCConstants.JXCWarningPrefix + ListValidation.InvalidCodeMessage));
		}

		// Warning is used to indicate 'JAS-specific message error' rather than message error to avoid conflicts with other messaging relying on message error validations
		public void AddJXCWarning(ZPropertyInfo propertyInfo, string errorMessage)
		{
			propertyInfo.AddWarning(JXCConstants.JXCWarningPrefix + errorMessage);
		}

		public bool HasJXCWarnings(BusinessObject bizO)
		{
			foreach (INotification warning in GetNotificationsIncludingChildren(bizO).GetWarnings())
			{
				if (warning.Message.IndexOf(JXCConstants.JXCWarningPrefix) > -1)
				{
					return true;
				}
			}

			return false;
		}

		#region Implementation

		void ValidateTextFieldWithExactLength(ZPropertyInfo propertyInfo, CharacterTypes characterType, int length)
		{
			ValidateTextField(propertyInfo, characterType, length, length);
		}

		void ValidateTextField(ZPropertyInfo propertyInfo, CharacterTypes characterType, int maxLength)
		{
			ValidateTextField(propertyInfo, characterType, 0, maxLength);
		}

		void ValidateTextField(ZPropertyInfo propertyInfo, CharacterTypes characterType, int minLength, int maxLength)
		{
			ZString value = propertyInfo.Value.ToString();
			if (maxLength < minLength)
			{
				int temp = maxLength;
				maxLength = minLength;
				minLength = temp;
			}

			if (!IsFieldValueValid(value, characterType) || value.Length < minLength || value.Length > maxLength)
			{
				string warningMessage = ConstructWarningMessage(propertyInfo.HumanReadableName, characterType, minLength, maxLength);
				AddJXCWarning(propertyInfo, warningMessage);
			}
		}

		string ConstructWarningMessage(ZString fieldName, CharacterTypes characterType, int minLength, int maxLength)
		{
			StringBuilder result = new StringBuilder();

			result.Append(fieldName);

			ZString characterTypeString = GetCharacterTypeString(characterType);
			if (!characterTypeString.IsEmpty)
			{
				result.AppendFormat(" must only contain {0} and", characterTypeString);
			}
			result.Append(" must be ");

			if (minLength == maxLength)
			{
				result.AppendFormat("{0} ", minLength);
			}
			else
			{
				if (minLength > 0)
				{
					result.AppendFormat("at least {0} and ", minLength);
				}
				result.AppendFormat("at most {0} ", maxLength);
			}
			result.Append("characters in length");

			return result.ToString();
		}

		string GetCharacterTypeString(CharacterTypes characterType)
		{
			string result = "";

			switch (characterType)
			{
				case CharacterTypes.Alphanumeric: result = "alphanumeric characters \"a-zA-Z0-9_\""; break;
				case CharacterTypes.Numeric: result = "numeric characters \"0-9\""; break;
				case CharacterTypes.LettersOnly: result = "alphabetic characters \"a-zA-Z\""; break;
			}

			return result;
		}

		bool IsFieldValueValid(ZString fieldValue, CharacterTypes characterType)
		{
			bool result = true;

			switch (characterType)
			{
				case CharacterTypes.Alphanumeric: result = !Regex.IsMatch(fieldValue, @"[^\w]", RegexOptions.IgnoreCase); break;
				case CharacterTypes.Numeric: result = fieldValue.IsNumbersOnlyOrEmpty; break;
				case CharacterTypes.LettersOnly: result = fieldValue.IsLettersOnlyOrEmpty; break;
			}

			return result;
		}

		IEnumerable<INotification> GetNotificationsIncludingChildren(BusinessObject bizO)
		{
			return new ZNotificationCollector(bizO, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
		}

		#endregion
	}
}
