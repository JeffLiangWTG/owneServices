using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CAAddressValidator : RelatedObjectValidation
	{
		CAAddressValidator(IDocAddress address, ZPropertyInfo notificationInfo, string addressCaption)
			: base(notificationInfo)
		{
			this.address = address;
			this.addressCaption = addressCaption;
		}

		#region Validate

		/// <summary>
		/// Standard Canadian address validation.
		/// </summary>
		/// <param name="address">e.g. JobDocAddress or OrgAddress.</param>
		/// <param name="notificationInfo">Notification property.</param>
		public static void Validate(IDocAddress address, ZPropertyInfo notificationInfo)
		{
			Validate(address, notificationInfo, notificationInfo.Description);
		}

		/// <summary>
		/// Standard Canadian address validation.
		/// </summary>
		/// <param name="address">e.g. JobDocAddress or OrgAddress.</param>
		/// <param name="notificationInfo">Notification property.</param>
		/// <param name="addressCaption">e.g. Importer Documentary or Supplier Main.</param>
		public static void Validate(IDocAddress address, ZPropertyInfo notificationInfo, string addressCaption)
		{
			new CAAddressValidator(address, notificationInfo, addressCaption).Validate();
		}

		#endregion

		#region Validate Mandatory

		/// <summary>
		/// Standard Canadian address validation. CompanyName, Address1, City are mandatory.
		/// </summary>
		/// <param name="address">e.g. JobDocAddress or OrgAddress.</param>
		/// <param name="notificationInfo">Notification property.</param>
		public static void ValidateMandatory(IDocAddress address, ZPropertyInfo notificationInfo)
		{
			ValidateMandatory(address, notificationInfo, notificationInfo.Description);
		}

		/// <summary>
		/// Standard Canadian address validation. CompanyName, Address1, City are mandatory.
		/// </summary>
		/// <param name="address">e.g. JobDocAddress or OrgAddress.</param>
		/// <param name="notificationInfo">Notification property.</param>
		/// <param name="addressCaption">e.g. Importer Documentary or Supplier Main.</param>
		public static void ValidateMandatory(IDocAddress address, ZPropertyInfo notificationInfo, string addressCaption)
		{
			new CAAddressValidator(address, notificationInfo, addressCaption).ValidateMandatory();
		}

		public static void ValidateMandatoryCityNameOnly(IDocAddress address, ZPropertyInfo notificationInfo, string addressCaption)
		{
			new CAAddressValidator(address, notificationInfo, addressCaption).ValidateMandatoryCityNameOnly();
		}

		public static void ValidateMandatoryName30Only(IDocAddress address, ZPropertyInfo notificationInfo, string addressCaption)
		{
			new CAAddressValidator(address, notificationInfo, addressCaption).ValidateMandatoryName30Only();
		}

		public static void ValidateMHHouseValidationMandatory(IDocAddress address, ZPropertyInfo notificationInfo, string addressCaption)
		{
			new CAAddressValidator(address, notificationInfo, addressCaption).ValidateMHHouseValidationMandatory();
		}

		#endregion

		#region Post Code Validation

		internal static void PostCodeValidation(IZPropertyInfo postCodeInfo, ZString country)
		{
			PostCodeValidation((ZString)postCodeInfo.Value, postCodeInfo, country);
		}

		internal static void PostCodeValidation(ZString postCode, IZPropertyInfo notificationInfo, ZString country)
		{
			if (country == Core.Constants.CountryCodes.Canada || country == Core.Constants.CountryCodes.UnitedStates)
			{
				postCode = postCode.Replace(" ", "");
				if (postCode.IsEmpty)
				{
					notificationInfo.AddMessageError(ExporterPostCodeIsRequiredCountryCanadaOrUnitedStates);
				}
				else if (country == Core.Constants.CountryCodes.Canada && !CanadaPostCodeRegex.IsMatch(postCode))
				{
					notificationInfo.AddMessageError(CanadianExporterPostCodeFormat);
				}
				else if (country == Core.Constants.CountryCodes.UnitedStates && !USPostCodeRegex.IsMatch(postCode))
				{
					notificationInfo.AddMessageError(UnitedStatesPostCodeFormat);
				}
			}
		}

		static Regex CanadaPostCodeRegex
		{
			get { return canadaPostCodeRegex ?? (canadaPostCodeRegex = new Regex(@"^[a-z][0-9][a-z][0-9][a-z][0-9]$", RegexOptions.Compiled | RegexOptions.IgnoreCase)); }
		}
		[ThreadStatic]
		static Regex canadaPostCodeRegex;

		static Regex USPostCodeRegex
		{
			get { return uSPostCodeRegex ?? (uSPostCodeRegex = new Regex(@"^[0-9]{5}(-[0-9]{4})?$", RegexOptions.Compiled | RegexOptions.IgnoreCase)); }
		}
		[ThreadStatic]
		static Regex uSPostCodeRegex;

		#endregion

		#region State Validation

		internal static void StateValidation(IZPropertyInfo stateInfo, ZString country)
		{
			StateValidation((ZString)stateInfo.Value, stateInfo, country);
		}

		internal static void StateValidation(ZString state, IZPropertyInfo notificationInfo, ZString country)
		{
			if (country == Core.Constants.CountryCodes.Canada || country == Core.Constants.CountryCodes.UnitedStates)
			{
				if (state.IsEmpty)
				{
					notificationInfo.AddMessageError(ProvinceStateIsRequiredCountryCanadaOrUnitedStates);
				}
				else
				{
					if (country == Core.Constants.CountryCodes.Canada)
					{
						var canadianProvinceList = new CanadianProvinceList();
						if (!canadianProvinceList.ContainsCode(state))
						{
							notificationInfo.AddMessageError(invalidCAProvince);
						}
					}
					else if (country == Core.Constants.CountryCodes.UnitedStates)
					{
						var statesList = new USStatesList();
						if (!statesList.ContainsCode(state))
						{
							notificationInfo.AddMessageError(invalidUSState);
						}
					}
				}
			}
		}

		#endregion

		#region

		public static void ValidateAddressInCanada(JobDeclaration declaration, IDocAddress address, ZPropertyInfo notificationInfo, string addressCaption)
		{
			if (address != null && address.CountryCode != Core.Constants.CountryCodes.Canada)
			{
				bool invoiceWithoutCanadianAddressFound = false;
				foreach (JobComInvoiceHeader invoice in declaration.Invoices)
				{
					if ((!invoice.BuyerDocumentaryAddress.HasRealAddress || invoice.BuyerDocumentaryAddress.Address.OA_RL_NKRelatedPortCode.Left(2) != Core.Constants.CountryCodes.Canada) &&
						(!invoice.FinalConsigneeAddress.HasRealAddress || invoice.FinalConsigneeAddress.Address.OA_RL_NKRelatedPortCode.Left(2) != Core.Constants.CountryCodes.Canada))
					{
						invoiceWithoutCanadianAddressFound = true;
						break;
					}
				}
				if (invoiceWithoutCanadianAddressFound)
				{
					notificationInfo.AddMessageError(GetAddressNotInCanadaMessageError(addressCaption));
				}
			}
		}

		#endregion

		#region Strings

		#region Messages

		internal static string ProvinceStateIsRequiredCountryCanadaOrUnitedStates
		{
			get { return Res.GetString("a910fef1-6d6b-407c-b76c-9967b84671cf", "A province/state is required when the country/region is Canada or United States. Please add a province/state or go to the organization and add to organization address details."); }
		}

		internal static string invalidCAProvince
		{
			get { return Res.GetString("bac923e1-5069-43e9-82b4-6cc0e6f5c2b6", "Invalid Canadian province code"); }
		}

		internal static string invalidUSState
		{
			get { return Res.GetString("43e56bae-6d31-4aea-8704-082c6e2254a9", "Invalid US state code"); }
		}

		internal static string ExporterPostCodeIsRequiredCountryCanadaOrUnitedStates
		{
			get { return Res.GetString("39daafdb-73a2-44ae-bbb2-a3a03086169e", "A postal/zip code is required when the country/region is Canada or United States. Please add a postal/zip code or go to the organization and add to organization address details."); }
		}

		internal static string CanadianExporterPostCodeFormat
		{
			get { return Res.GetString("b3d4103d-b760-4c33-b303-cac4af9ac24e", "This postal code is invalid. A Canadian postal code should be in the following format: A9A9A9, where A is a letter and 9 is a digit."); }
		}

		internal static string UnitedStatesPostCodeFormat
		{
			get { return Res.GetString("2C6492A3-04E9-4C18-B59D-A4D84BC16C30", "This US zip code is invalid. A US Zip code should be in the following format: 99999 or 99999-9999, where 9 is a digit."); }
		}

		internal static string GetAddressNotConfiguredMessageError(string addressCaption)
		{
			return Res.GetString("d6445637-93a8-4137-8642-45d5d78806c4", "Please configure {0} Address.", addressCaption);
		}

		internal static string GetAddressIsInvalidMessageError(string addressCaption)
		{
			return ErrorStringBuilder.GetFieldIsInvalidMessageError(addressCaption + AddressSuffix);
		}

		internal static string GetAddressNotInCanadaMessageError(string addressCaption)
		{
			return Res.GetString("E227561C-ECCE-4BB4-9C99-A9283BD5AAB2", "The {0} Address is not in Canada, and you have not specified a Canadian Purchaser or Consignee on the Invoice Header tab.", addressCaption);
		}

		#endregion

		#region Captions

		internal static string NameCaption
		{
			get { return Res.GetString("abd46c54-2ad5-474a-b9a8-03e5c6b7efe9", "name"); }
		}

		internal static string Address1Caption
		{
			get { return Res.GetString("a9f14f2c-10f8-43ce-b074-818cd27195f8", "address line 1"); }
		}

		internal static string Address2Caption
		{
			get { return Res.GetString("9f169bcd-a1da-42ee-b5e3-29bd0d42bcd2", "address line 2"); }
		}

		internal static string CityCaption
		{
			get { return Res.GetString("707d80f7-c542-4e29-b3bb-aea2f3f0dfcf", "city"); }
		}

		internal static string CountryCodeCaption
		{
			get { return Res.GetString("0d2357af-fb05-420c-8b71-289194bc3025", "country/region code"); }
		}

		internal static string StateCaption
		{
			get { return Res.GetString("e0bfcac4-f9cc-411b-bb84-515b7e2eb6e0", "province / state"); }
		}

		internal static string PostCodeCaption
		{
			get { return Res.GetString("1e67d6de-1169-4add-b397-80b21b06e56f", "postal / zip code"); }
		}

		static string AddressSuffix
		{
			get { return " " + Res.GetString("027BF37C-5641-4285-B8B3-2087B7D4F151", "Address"); }
		}

		#endregion

		#endregion

		#region Implementation

		void Validate()
		{
			if (CheckDocAddressIsNotEmpty())
			{
				var errorBuilder = new ErrorStringBuilder(notificationInfo, addressCaption + AddressSuffix);
				SplitMaxLengthValidation(address.E2_CompanyName, 70, 35, errorBuilder, NameCaption, false);
				if (address.E2_Address2.IsEmpty)
				{
					SplitMaxLengthValidation(address.E2_Address1, 70, 35, errorBuilder, Address1Caption, false);
				}
				else
				{
					MaxLengthValidation(address.E2_Address1, 35, errorBuilder, Address1Caption);
					MaxLengthValidation(address.E2_Address2, 35, errorBuilder, Address2Caption);
				}
				MaxLengthValidation(address.E2_City, 35, errorBuilder, CityCaption);
				MaxLengthValidation(address.E2_State, 9, errorBuilder, StateCaption);
				StateValidation(address.E2_State, errorBuilder, address.CountryCode);
				MaxLengthValidation(address.E2_Postcode, 9, errorBuilder, PostCodeCaption);
				PostCodeValidation(address.E2_Postcode, errorBuilder, address.CountryCode);
				errorBuilder.FillErrorMessages();
			}
		}

		void ValidateMHHouseValidationMandatory()
		{
			if (CheckDocAddressIsNotEmpty())
			{
				var errorBuilder = new ErrorStringBuilder(notificationInfo, addressCaption + AddressSuffix);
				MaxLengthValidation(address.E2_CompanyName, 70, errorBuilder, NameCaption, true);
				SplitMaxLengthValidation(address.E2_Address1, 105, 35, errorBuilder, Address1Caption, true);
				if (!address.E2_Address2.IsEmpty)
				{
					SplitMaxLengthValidation(address.E2_Address2, 70, 35, errorBuilder, Address2Caption, false);
				}

				MaxLengthValidation(address.E2_City, 35, errorBuilder, CityCaption, true);
				MaxLengthValidation(address.E2_State, 9, errorBuilder, StateCaption);
				StateValidation(address.E2_State, errorBuilder, address.CountryCode);
				MaxLengthValidation(address.E2_Postcode, 9, errorBuilder, PostCodeCaption);
				PostCodeValidation(address.E2_Postcode, errorBuilder, address.CountryCode);
				MaxLengthValidation(address.E2_RN_NKCountryCode, 2, errorBuilder, CountryCodeCaption, true);
				errorBuilder.FillErrorMessages();
			}
		}

		void ValidateMandatory()
		{
			if (CheckDocAddressIsNotEmpty())
			{
				var errorBuilder = new ErrorStringBuilder(notificationInfo, addressCaption + AddressSuffix);
				SplitMaxLengthValidation(address.E2_CompanyName, 70, 35, errorBuilder, NameCaption, true);
				if (address.E2_Address2.IsEmpty)
				{
					SplitMaxLengthValidation(address.E2_Address1, 70, 35, errorBuilder, Address1Caption, true);
				}
				else
				{
					MaxLengthValidation(address.E2_Address1, 35, errorBuilder, Address1Caption, true);
					MaxLengthValidation(address.E2_Address2, 35, errorBuilder, Address2Caption);
				}
				MaxLengthValidation(address.E2_City, 35, errorBuilder, CityCaption, true);
				MaxLengthValidation(address.E2_State, 9, errorBuilder, StateCaption);
				StateValidation(address.E2_State, errorBuilder, address.CountryCode);
				MaxLengthValidation(address.E2_Postcode, 9, errorBuilder, PostCodeCaption);
				PostCodeValidation(address.E2_Postcode, errorBuilder, address.CountryCode);
				MaxLengthValidation(address.E2_RN_NKCountryCode, 2, errorBuilder, CountryCodeCaption, true);
				errorBuilder.FillErrorMessages();
			}
		}

		void ValidateMandatoryCityNameOnly()
		{
			if (CheckDocAddressIsNotEmpty())
			{
				var errorBuilder = new ErrorStringBuilder(notificationInfo, addressCaption + AddressSuffix);
				MaxLengthValidation(address.E2_City, 35, errorBuilder, CityCaption, true);
				errorBuilder.FillErrorMessages();
			}
		}

		void ValidateMandatoryName30Only()
		{
			if (CheckDocAddressIsNotEmpty())
			{
				var errorBuilder = new ErrorStringBuilder(notificationInfo, addressCaption + AddressSuffix);
				MaxLengthValidation(address.E2_CompanyName, 30, errorBuilder, NameCaption, true);
				MaxLengthValidation(address.E2_City, 35, errorBuilder, CityCaption);
				MaxLengthValidation(address.E2_State, 9, errorBuilder, StateCaption);
				StateValidation(address.E2_State, errorBuilder, address.CountryCode);
				MaxLengthValidation(address.E2_Postcode, 9, errorBuilder, PostCodeCaption);
				PostCodeValidation(address.E2_Postcode, errorBuilder, address.CountryCode);
				errorBuilder.FillErrorMessages();
			}
		}

		bool CheckDocAddressIsNotEmpty()
		{
			var isEmpty = !address.E2_OA_Address.IsValid && !address.E2_AddressOverride;
			if (isEmpty)
			{
				notificationInfo.AddMessageError(GetAddressNotConfiguredMessageError(addressCaption));
			}
			return !isEmpty;
		}

		readonly IDocAddress address;
		readonly string addressCaption;

		#endregion
	}
}
