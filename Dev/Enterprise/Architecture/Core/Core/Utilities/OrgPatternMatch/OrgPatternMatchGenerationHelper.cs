using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Organizations.PatternMatching;

namespace Enterprise.ZArchitecture.Core
{
	/// <summary>
	/// The methods used to create PatternMatch Data.
	/// </summary>
	public class OrgPatternMatchGenerationHelper
	{
		#region Singleton Instance

		[ThreadStatic]
		static OrgPatternMatchGenerationHelper cachedHelper;
		public static OrgPatternMatchGenerationHelper Get(string languageCode, string city, MultilingualString country)
		{
			if (cachedHelper == null)
			{
				cachedHelper = new OrgPatternMatchGenerationHelper();
			}
			//Language Settings are cached in OrgPatternLanguageSetting
			cachedHelper.SetLanguageSettings(languageCode, city, country);
			return cachedHelper;
		}

		#endregion

		void SetLanguageSettings(string languageCode, string city, MultilingualString country)
		{
			languageSetting = OrgPatternLanguageSetting.Get(languageCode, city, country);
		}

		#region Company Name

		/// <summary>
		/// Gets the Company Name with common words removed (EG: Inc, Pty Limited, etc...)
		/// </summary>
		/// <param name="companyName">The original Company Name</param>
		/// <param name="hyphensSplitWords">Should Hyphens be used as a word splitting delimiter</param>
		/// <returns>The stripped company name</returns>
		public string SuccinctCompanyName(string companyName, bool hyphensSplitWords)
		{
			return languageSetting.SuccinctCompanyName(companyName, hyphensSplitWords);
		}

		/// <summary>
		/// Gets the breakdown of the company name into its soundex values
		/// </summary>
		/// <param name="companyName">The original Company Name (NOT SuccinctCompanyName)</param>
		/// <param name="hyphensSplitWords">Should Hyphens be used as a word splitting delimiter</param>
		/// <returns>A 4-element array consisting of the soundex values for each word of the company name</returns>
		public string[] CompanyNameSoundexWords(string companyName, bool hyphensSplitWords)
		{
			return CompanyNameSoundexWords(SuccinctCompanyName(companyName, hyphensSplitWords));
		}

		/// <summary>
		/// Gets the breakdown of the company name into its soundex values
		/// </summary>
		/// <param name="succinctCompanyName">The clear Company Name (i.e. SuccinctCompanyName with common words removed)</param>
		/// <returns>A 4-element array consisting of the soundex values for each word of the company name</returns>
		public string[] CompanyNameSoundexWords(string succinctCompanyName)
		{
			return ExtractSoundexWords(succinctCompanyName, 4);
		}

		#endregion

		#region Corporation

		/// <summary>
		/// Determines if the organisation is a corporation
		/// </summary>
		/// <param name="companyName">The original Company Name (NOT the SuccinctCompanyName)</param>
		/// <returns>True if the organisation is a corporation</returns>
		public bool IsOrganisationACorporation(string companyName)
		{
			return languageSetting.IsOrgCorporation(companyName);
		}

		#endregion

		#region PO Box

		/// <summary>
		/// Is the specified address a PO-Box Address
		/// </summary>
		/// <param name="addressLine1">The first line of the address</param>
		/// <param name="addressLine2">The second line of the address</param>
		/// <returns>True if the address is a PO-Box Address</returns>
		public bool IsPOBoxAddress(string addressLine1, string addressLine2)
		{
			return languageSetting.IsAddressAPOBox(addressLine1, addressLine2);
		}

		/// <summary>
		/// Extracts the PO-Box Number from the address
		/// </summary>
		/// <param name="addressLine1">The first line of the address</param>
		/// <param name="addressLine2">The second line of the address</param>
		/// <param name="pOBoxMaxLength">The maximum number of characters for the PO-Box</param>
		/// <returns>Returns the number of  the PO-Box</returns>
		public string POBoxNumber(string addressLine1, string addressLine2, int pOBoxMaxLength)
		{
			string result = "";

			if (IsPOBoxAddress(addressLine1, addressLine2))
			{
				List<string> addressWords = GetAddressWords(addressLine1);
				languageSetting.RemoveIgnoredAddressWords(addressWords);

				int pOBoxLocation = -1;
				foreach (string pOBoxWord in languageSetting.NewPOBoxWords)
				{
					int wordLocation = addressWords.IndexOf(pOBoxWord);
					if (wordLocation > pOBoxLocation)
					{
						pOBoxLocation = wordLocation;
					}
				}

				if (pOBoxLocation > -1 && ((pOBoxLocation + 1) < addressWords.Count))
				{
					int pOBoxNumberLocation = pOBoxLocation + 1;
					string pOBoxNumber = addressWords[pOBoxNumberLocation];

					// Search through the words to get the next word after the PO BOX text
					while (string.IsNullOrEmpty(pOBoxNumber) && ((pOBoxNumberLocation + 1) < addressWords.Count))
					{
						pOBoxNumberLocation++;
						pOBoxNumber = addressWords[pOBoxNumberLocation];
					}

					if (pOBoxNumber.StartsWith("NO") && (pOBoxNumber.Length > 2))
					{
						pOBoxNumber = pOBoxNumber.Substring(2);
					}

					if (!string.IsNullOrEmpty(pOBoxNumber) && char.IsNumber(pOBoxNumber[0]))
					{
						result = TrimString(pOBoxNumber, pOBoxMaxLength);
					}
				}
			}

			return result;
		}

		List<string> GetAddressWords(string address)
		{
			List<string> resultList = new List<string>();
			var result = languageSetting.GetAddressWords(address, "");
			resultList.AddRange(result);
			return resultList;
		}

		#endregion

		#region Address

		/// <summary>
		/// Returns the street number of a specified address line
		/// </summary>
		/// <param name="addressLine1">The line of the address from which to get the street number</param>
		/// <param name="streetNumberMaxLength">The maximum number of characters for the street number</param>
		/// <returns>The string consisting of the street number</returns>
		public string StreetNumber(string addressLine1, int streetNumberMaxLength)
		{
			string result = "";
			string address = languageSetting.RemoveIgnoredAddressWords(addressLine1);
			address = languageSetting.ReplaceIgnoredCharacters(address, " ");

			if (!string.IsNullOrEmpty(address))
			{
				string[] addressWords = address.Trim().Split(' ');

				if (!string.IsNullOrEmpty(addressWords[0]) && char.IsNumber(addressWords[0], 0))
				{
					result = TrimString(addressWords[0], streetNumberMaxLength);
				}
			}

			return result;
		}

		/// <summary>
		/// Decomposes the address information into its soundex values
		/// </summary>
		/// <param name="addressLine1">The first line of the address</param>
		/// <param name="addressLine2">The second line of the address</param>
		/// <param name="streetNumber">The street number of the address</param>
		/// <param name="pOBoxNumber">The PO-Box number</param>
		/// <returns>A 4-element array containing the soundex values of the 4 components of the address</returns>
		public string[] AddressSoundexWords(string addressLine1, string addressLine2, string streetNumber, string pOBoxNumber)
		{
			string combinedAddress = GetEncodeableAddress(addressLine1, addressLine2, streetNumber, pOBoxNumber);
			return ExtractSoundexWords(combinedAddress, 4);
		}

		/// <summary>
		/// Prepares an address for encoding by removing ignored characters, ignored words and PO Box and street number information
		/// </summary>
		string GetEncodeableAddress(string address1, string address2, string streetNumber, string pOBoxNumber)
		{
			string encodeableAddress = languageSetting.RemoveIgnoredAddressWords(address1, address2);
			List<string> addressWordList = GetAddressWords(encodeableAddress);

			if (!string.IsNullOrEmpty(streetNumber))
			{
				addressWordList.Remove(streetNumber);
			}

			if (IsPOBoxAddress(address1, address2))
			{
				foreach (string pOBoxWord in languageSetting.NewPOBoxWords)
				{
					addressWordList.Remove(pOBoxWord);
				}

				if (!string.IsNullOrEmpty(pOBoxNumber))
				{
					addressWordList.Remove(pOBoxNumber);
				}
			}

			return string.Join(" ", addressWordList);
		}

		static string RemoveIgnoredPhoneCharacters(string phoneNumber)
		{
			return Regex.Replace(phoneNumber, "[^0-9]", "");
		}

		#endregion

		#region UNLOCO

		/// <summary>
		/// Sanitises the UNLOCO input data 
		/// </summary>
		/// <param name="headerUNLOCO">The UNLOCO of the Organisation</param>
		/// <param name="addressRelatedPortCode">The UNLOCO of the Address (Related Port Code)</param>
		/// <returns>The correct UNLOCO for the pattern match record</returns>
		public string UNLOCO(string headerUNLOCO, string addressRelatedPortCode)
		{
			return !string.IsNullOrEmpty(addressRelatedPortCode) ? addressRelatedPortCode : headerUNLOCO;
		}

		#endregion

		#region Business Registration Number

		/// <summary>
		/// Sanitises the Business Registration Number
		/// </summary>
		/// <param name="registrationNumber">The Registration Number</param>
		/// <param name="regNoMaxLength">The Max Length of the Reg No</param>
		/// <returns>The sanitised Business Registration Number</returns>
		public static string SanitisedBusinessRegistrationNumber(string registrationNumber, int regNoMaxLength)
		{
			string result = "";

			if (!string.IsNullOrEmpty(registrationNumber))
			{
				result = TrimString(registrationNumber.Replace(" ", string.Empty), regNoMaxLength);
			}

			return result;
		}

		#endregion

		#region Post Code

		/// <summary>
		/// Sanitises the Post Code of an address
		/// </summary>
		/// <param name="postCode">The Post Code</param>
		/// <param name="postCodeMaxLength">The Maximum Length of the Post Code</param>
		/// <returns>The sanitised Post Code</returns>
		public string PostCode(string postCode, int postCodeMaxLength)
		{
			string result = "";

			if (!string.IsNullOrEmpty(postCode))
			{
				result = TrimString(postCode.Replace(" ", string.Empty), postCodeMaxLength);
			}

			return result;
		}

		#endregion

		#region City

		/// <summary>
		/// Returns the soundex value of the City
		/// </summary>
		/// <param name="cityName">The full name of the City</param>
		/// <returns>The 4-letter soundex code for the full city name</returns>
		public string CitySoundex(string cityName)
		{
			return !string.IsNullOrEmpty(cityName) ? languageSetting.GetSoundex(languageSetting.RemoveIgnoredCharacters(cityName)) : "";
		}

		#endregion

		#region State

		/// <summary>
		/// Returns the soundex value of the State
		/// </summary>
		/// <param name="stateName">The full name of the State</param>
		/// <returns>The 4-letter soundex code for the full state name</returns>
		public string StateSoundex(string stateName)
		{
			return !string.IsNullOrEmpty(stateName) ? languageSetting.GetSoundex(languageSetting.RemoveIgnoredCharacters(stateName)) : "";
		}

		#endregion

		#region Phone Fax Number

		/// <summary>
		/// Sanitises a Phone or Fax Number
		/// </summary>
		/// <param name="number">The Phone or Fax number to sanitise</param>
		/// <param name="maxLength">The maximum length of the number</param>
		/// <returns>The sanitised phone/fax number</returns>
		public static string SanitisedPhoneNumber(string number, int maxLength)
		{
			string result = RemoveIgnoredPhoneCharacters(number);

			if (result.Length > EncodedPhoneNumberLength)
			{
				result = result.Substring(result.Length - EncodedPhoneNumberLength);
			}

			return result;
		}

		#endregion

		#region Email And Domain

		/// <summary>
		/// Extracts the Email (username) part from a complete email address
		/// </summary>
		/// <param name="emailAddress">The complete email address to parse</param>
		/// <param name="emailMaxLength">The maximum length of the email address</param>
		/// <returns>The username portion of an email address</returns>
		public string Email(string emailAddress, int emailMaxLength)
		{
			return ExtractPartFromEmailAddress(EmailAddressPart.Email, emailAddress, emailMaxLength);
		}

		/// <summary>
		/// Extracts the Domain (company name) part from a complete email address
		/// </summary>
		/// <param name="emailAddress">The complete email address to parse</param>
		/// <param name="domainMaxLength">The maximum length of the domain</param>
		/// <returns>The company name portion of an email address</returns>
		public string Domain(string emailAddress, int domainMaxLength)
		{
			return ExtractPartFromEmailAddress(EmailAddressPart.Domain, emailAddress, domainMaxLength);
		}

		public IEnumerable<string> IgnoredAddressWords
		{
			get { return languageSetting.NewIgnoredAddressWords; }
		}

		enum EmailAddressPart
		{
			Email,
			Domain
		}

		string ExtractPartFromEmailAddress(EmailAddressPart part, string emailAddress, int maxLength)
		{
			string result = "";

			if (!string.IsNullOrEmpty(emailAddress))
			{
				string[] splitEmail = emailAddress.Split('@');
				if (part == EmailAddressPart.Email)
				{
					result = TrimString(splitEmail[0], maxLength).ToUpper();
				}
				else if (splitEmail.Length > 1)
				{
					result = TrimString(splitEmail[1], maxLength).ToUpper();
				}
			}

			return result;
		}

		#endregion

		#region Text Helper Functions

		string[] ExtractSoundexWords(string text, int minimumWordCount)
		{
			string[] result = ExtractWordsFromString(text, 4).ToArray();

			for (int i = 0; i < result.Length; i++)
			{
				result[i] = languageSetting.GetSoundex(result[i]);
			}

			return result;
		}

		List<string> ExtractWordsFromString(string text, int minimumWordCount)
		{
			List<string> strings = new List<string>(minimumWordCount);

			if (text.Length > 0)
			{
				var splitWords = languageSetting.GetWordsFromString(text);
				foreach (string word in splitWords)
				{
					if (!string.IsNullOrEmpty(word))
					{
						strings.Add(word);
					}
				}
			}

			while (strings.Count < minimumWordCount)
			{
				strings.Add(string.Empty);
			}

			return strings;
		}

		static string TrimString(string text, int maxLength)
		{
			return (text.Length <= maxLength) ? text : text.Substring(0, maxLength);
		}

		#endregion

		#region Implementation

		const int EncodedPhoneNumberLength = 7;
		internal IOrgPatternLanguageSetting languageSetting;

		#endregion
	}
}
