using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Common
{
	public class JobDocAddressProvider : IJapaneseAddress, IWesternAddress
	{
		public JobDocAddressProvider(JobDocAddress docAddress, bool trimPhoneNumberIfNeeded = false)
		{
			Argument.NotNull(docAddress, nameof(docAddress));
			jobDocAddress = docAddress;
			this.trimPhoneNumberIfNeeded = trimPhoneNumberIfNeeded;
			customsCodes = jobDocAddress.Organisation?.CustomsCodes;
			isJapanese = jobDocAddress.E2_RN_NKCountryCode == Core.Constants.CountryCodes.Japan;
			isEnglishAddress = jobDocAddress.Address?.IsEnglish ?? false;
			englishTranslatedAddress = jobDocAddress.Address?.TranslatedAddresses.FirstOrDefault(x => x.IsEnglish);
		}

		readonly JobDocAddress jobDocAddress;
		readonly bool trimPhoneNumberIfNeeded;
		readonly OrgCusCodeCollection customsCodes;
		readonly bool isJapanese;
		readonly bool isEnglishAddress;
		readonly OrgTranslatedAddress englishTranslatedAddress;

		public string Code
		{
			get
			{
				var result = string.Empty;
				switch (jobDocAddress.DocAddressType)
				{
					case MasterFiles.Integration.DocAddressType.AdministratorOfCustomsWork:
					case MasterFiles.Integration.DocAddressType.ImporterDocumentaryAddress:
					case MasterFiles.Integration.DocAddressType.SupplierDocumentaryAddress:
					case MasterFiles.Integration.DocAddressType.ConsigneeAddress:
						result = isJapanese ? GetJapaneseImporterExporterCode() : GetForeignConsignorConsigneeCode();
						break;
					case MasterFiles.Integration.DocAddressType.CustomsDepotAddress:
						result = customsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID);
						break;
					case MasterFiles.Integration.DocAddressType.CustomsContainerYardAddress:
						result = customsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID);
						if (string.IsNullOrWhiteSpace(result))
						{
							result = GetJapaneseImporterExporterCode();
						}
						break;
					case MasterFiles.Integration.DocAddressType.VanningLocationAddress:
						result = GetVanningLocationCode();
						break;
					default:
						break;
				}
				return result;
			}
		}

		public string NACCSUserCode
		{
			get
			{
				var result = customsCodes?.GetCustomsRegNo(OrgCusCode.JapanCodeTypes.NUC, Core.Constants.CountryCodes.Japan, jobDocAddress.PK);
				if (string.IsNullOrEmpty(result))
				{
					result = customsCodes?.GetCustomsRegNo(OrgCusCode.JapanCodeTypes.NUC);
				}
				return result;
			}
		}

		string GetJapaneseImporterExporterCode()
		{
			var result = jobDocAddress.E2_AddressOverride ? jobDocAddress.E2_GovRegNum : (jobDocAddress.Address?.CustomsCodes.GetCustomsRegNoMatching(OrgCusCode.JapanCodeTypes.LPC, OrgCusCode.JapanCodeTypes.CIE, OrgCusCode.JapanCodeTypes.JAS) ?? ZString.Empty);
			if (result.Length == LengthOfJASPROCodeMainPart || result.Length == LengthOfLegalPersonCodeMainPart)
			{
				result += DefaultBranchNumber;
			}
			return result;
		}

		string GetForeignConsignorConsigneeCode() => customsCodes?.GetCustomsRegNo(OrgCusCode.JapanCodeTypes.FSB);

		public string Name => GetValueFromEnglishAddress(x => x.E2_CompanyName, x => x.OTA_CompanyName);

		public string PostCode => isJapanese ? jobDocAddress.E2_Postcode.Split("-").ToStringWithSeparator("") : jobDocAddress.E2_Postcode.ToString();

		string IJapaneseAddress.Street
		{
			get
			{
				if (isJapanese)
				{
					var streetBuilder = new ZStringBuilder();
					streetBuilder.AppendIfNotEmpty(EffectiveAddress1);
					streetBuilder.AppendIfNotEmpty(EffectiveAddress2);
					return streetBuilder.ToStringWithDelimiterBetweenAppends(" ");
				}

				return string.Empty;
			}
		}

		string IJapaneseAddress.AdditionalInformation => isJapanese
			? jobDocAddress.E2_AddressOverride
				? jobDocAddress.UnrestrictedAdditionalAddressInformation.ToString()
				: (jobDocAddress.Address?.AdditionalInfos?.Select(a => a.OAI_AdditionalInfo) ?? Array.Empty<ZString>()).ToStringWithSeparator(" ")
			: string.Empty;

		public string City => jobDocAddress.E2_City;

		string IJapaneseAddress.Prefecture => isJapanese ? jobDocAddress.State : ZString.Empty;

		string IJapaneseAddress.Phone
		{
			get
			{
				ZString phone = isJapanese ? jobDocAddress.PhoneNumber.FormattedForBinding.Split("-").ToStringWithSeparator("") : string.Empty;
				return trimPhoneNumberIfNeeded ? phone.RemoveNonNumCharFromPhoneNumber(11, true) : phone;
			}
		}

		string IWesternAddress.Street1 => isJapanese ? string.Empty : EffectiveAddress1;

		string IWesternAddress.Street2 => isJapanese ? string.Empty : EffectiveAddress2;

		string EffectiveAddress1 => GetValueFromEnglishAddress(x => x.E2_Address1, x => x.OTA_Address1);

		string EffectiveAddress2 => GetValueFromEnglishAddress(x => x.E2_Address2, x => x.OTA_Address2);

		string GetValueFromEnglishAddress(Func<JobDocAddress, string> docAddressSelector, Func<OrgTranslatedAddress, string> translatedAddressSelector)
		{
			string result;
			if (!jobDocAddress.E2_AddressOverride && !isEnglishAddress && englishTranslatedAddress != null)
			{
				result = translatedAddressSelector.Invoke(englishTranslatedAddress);
			}
			else
			{
				result = docAddressSelector.Invoke(jobDocAddress);
			}

			return result;
		}

		string IWesternAddress.State => isJapanese ? ZString.Empty : jobDocAddress.State;

		string IWesternAddress.CountryCode => isJapanese ? ZString.Empty : jobDocAddress.E2_RN_NKCountryCode;

		public string Address => null;

		public string Phone => null;

		const string DefaultBranchNumber = "0000";
		const int LengthOfJASPROCodeMainPart = 8;
		const int LengthOfLegalPersonCodeMainPart = 13;

		string GetVanningLocationCode()
		{
			if (string.IsNullOrEmpty(jobDocAddress.E2_GovRegNum))
			{
				return customsCodes?.GetCustomsRegNoMatching(OrgCusCode.CodeTypes.ControlledPremisesID, OrgCusCode.JapanCodeTypes.LPC, OrgCusCode.JapanCodeTypes.CIE) ?? ZString.Empty;
			}
			else
			{
				return jobDocAddress.E2_GovRegNum;
			}
		}
	}
}
