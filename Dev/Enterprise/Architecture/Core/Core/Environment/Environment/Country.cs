using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.Integration.ZArchitecture;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	#region Date / Time Format

	public enum CountryDateTimeFormat
	{
		US,
		Japan,
		Other
	}

	#endregion

	#region ICountry Interface

	public interface ICountry
	{
		string Code { get; }
		CultureInfo Culture { get; }
		string Description { get; }
		ICurrency Currency { get; }
		bool IsGSTRegistered { get; }
		Guid PK { get; }
		string ConsumptionTaxDescription { get; }

#if DEBUG
		IDisposable SetCultureForTest(CultureInfo culture);
#endif
	}

	#endregion

	public static class Country
	{
		#region Tax Logic

		#region Tax Code

		public static string GetConsumptionTaxRegistrationOrgCusCode(string countryCode)
		{
			var result = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(countryCode)?.GetConsumptionTaxRegistrationCode();
			if (result != null)
			{
				return result;
			}

			throw new NotSupportedException($"Country {countryCode} not supported for GetConsumptionTaxRegistrationOrgCusCode");
		}

		public static string GetConsumptionTaxDescription(string countryCode)
		{
			var result = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(countryCode)?.GetConsumptionTaxCode();
			if (result != null)
			{
				return result;
			}

			throw new NotSupportedException($"Country {countryCode} not supported for GetConsumptionTaxDescription");
		}

		public static string GetDomesticNameofTaxCode(string countryCode)
		{
			switch (countryCode)
			{
				case Constants.CountryCodes.Germany:
					return "MWST";
				case Constants.CountryCodes.Denmark:
				case Constants.CountryCodes.Sweden:
					return "MOMS";
				default:
					return GetConsumptionTaxDescription(countryCode);
			}
		}

		public static string GetDefaultTaxCodeDescription(string taxRegistrationOrgCusCode)
		{
			return string.IsNullOrEmpty(taxRegistrationOrgCusCode) ?
				Res.GetString("Organisation|CustomsCodes|GSTCodeTemplateGeneric", "Government Code") :
				Res.GetString("Organisation|CustomsCodes|GSTCodeTemplate", "Government {0} Code", taxRegistrationOrgCusCode);
		}

		public static class TaxCodeDescriptions
		{
			public static string VATBusinessRegistrationNumber
				=> Res.GetString("Organisation|CustomsCodes|VATBusinessRegistrationNumber", "VAT Business Registration Number");

			public static string VATCodeTemplate(string code)
				=> Res.GetString("Organisation|CustomsCodes|VATCodeTemplate", "VAT ({0}) Business Registration Number", code);
		}

		#region Tax Codes for Organisation Country

		public static string[] GetConsumptionTaxRegistrationCodesForOrgCountry(string orgCountryCode)
		{
			var result = new List<string>();
			switch (orgCountryCode)
			{
				case Constants.CountryCodes.Argentina:
					result.Add("IVA");
					result.Add("CUI");
					break;
				case Constants.CountryCodes.Brazil:
					result.Add("CMT");
					result.Add("CJN");
					break;
				case Constants.CountryCodes.Chile:
					result.Add("IVA");
					result.Add("RUT");
					break;
				case Constants.CountryCodes.India:
					result.Add("SER");
					result.Add("PAN");
					break;
				case Constants.CountryCodes.Spain:
					result.Add("NIF");
					result.Add("DNI");
					result.Add("IGC");
					break;
				default:
					result.Add(GetConsumptionTaxRegistrationOrgCusCode(orgCountryCode));
					break;
			}

			result.Add("GBR");
			result.Add("GCR");

			return result.ToArray();
		}

		#endregion

		#endregion

		#region GST / Withholding Registration

		public static bool GetIsGSTRegistered(string countryCode)
		{
			return GetConsumptionTaxDescription(countryCode).Length > 0;
		}

		#endregion

		#endregion

		#region Licence Key Builder Supported Country Codes

#if DEBUG
		public
#endif
		static string[] LicenceKeyBuilderSupportedCountryCodes
		{
			get
			{
				// Please keep the list below in an alphabetical order.
				List<string> list = new List<string>();
				list.Add(Constants.CountryCodes.Afghanistan);
				list.Add(Constants.CountryCodes.Albania);
				list.Add(Constants.CountryCodes.Algeria);
				list.Add(Constants.CountryCodes.Andorra);
				list.Add(Constants.CountryCodes.Angola);
				list.Add(Constants.CountryCodes.Argentina);
				list.Add(Constants.CountryCodes.Aruba);
				list.Add(Constants.CountryCodes.Australia);
				list.Add(Constants.CountryCodes.Austria);
				list.Add(Constants.CountryCodes.Azerbaijan);
				list.Add(Constants.CountryCodes.Bahamas);
				list.Add(Constants.CountryCodes.Bahrain);
				list.Add(Constants.CountryCodes.Bangladesh);
				list.Add(Constants.CountryCodes.Barbados);
				list.Add(Constants.CountryCodes.Belarus);
				list.Add(Constants.CountryCodes.Belgium);
				list.Add(Constants.CountryCodes.Belize);
				list.Add(Constants.CountryCodes.Benin);
				list.Add(Constants.CountryCodes.Bermuda);
				list.Add(Constants.CountryCodes.Bolivia);
				list.Add(Constants.CountryCodes.BonaireSintEustatiusAndSaba);
				list.Add(Constants.CountryCodes.BosniaAndHerzegovina);
				list.Add(Constants.CountryCodes.Botswana);
				list.Add(Constants.CountryCodes.Brazil);
				list.Add(Constants.CountryCodes.Brunei);
				list.Add(Constants.CountryCodes.Bulgaria);
				list.Add(Constants.CountryCodes.BurkinaFaso);
				list.Add(Constants.CountryCodes.Burundi);
				list.Add(Constants.CountryCodes.Cambodia);
				list.Add(Constants.CountryCodes.Cameroon);
				list.Add(Constants.CountryCodes.Canada);
				list.Add(Constants.CountryCodes.CapeVerde);
				list.Add(Constants.CountryCodes.CaymanIslands);
				list.Add(Constants.CountryCodes.Chad);
				list.Add(Constants.CountryCodes.Chile);
				list.Add(Constants.CountryCodes.China);
				list.Add(Constants.CountryCodes.Congo);
				list.Add(Constants.CountryCodes.CookIslands);
				list.Add(Constants.CountryCodes.Croatia);
				list.Add(Constants.CountryCodes.Cuba);
				list.Add(Constants.CountryCodes.Curacao);
				list.Add(Constants.CountryCodes.CzechRepublic);
				list.Add(Constants.CountryCodes.Colombia);
				list.Add(Constants.CountryCodes.CostaRica);
				list.Add(Constants.CountryCodes.CoteDivoire);
				list.Add(Constants.CountryCodes.Cyprus);
				list.Add(Constants.CountryCodes.DemocraticRepublicOfCongo);
				list.Add(Constants.CountryCodes.Denmark);
				list.Add(Constants.CountryCodes.Djibouti);
				list.Add(Constants.CountryCodes.DominicanRepublic);
				list.Add(Constants.CountryCodes.Ecuador);
				list.Add(Constants.CountryCodes.Egypt);
				list.Add(Constants.CountryCodes.ElSalvador);
				list.Add(Constants.CountryCodes.Estonia);
				list.Add(Constants.CountryCodes.Ethiopia);
				list.Add(Constants.CountryCodes.EquatorialGuinea);
				list.Add(Constants.CountryCodes.FaeroeIslands);
				list.Add(Constants.CountryCodes.Fiji);
				list.Add(Constants.CountryCodes.Finland);
				list.Add(Constants.CountryCodes.France);
				list.Add(Constants.CountryCodes.FrenchGuyana);
				list.Add(Constants.CountryCodes.FrenchPolynesia);
				list.Add(Constants.CountryCodes.Gabon);
				list.Add(Constants.CountryCodes.Gambia);
				list.Add(Constants.CountryCodes.Georgia);
				list.Add(Constants.CountryCodes.Germany);
				list.Add(Constants.CountryCodes.Ghana);
				list.Add(Constants.CountryCodes.Gibraltar);
				list.Add(Constants.CountryCodes.Greece);
				list.Add(Constants.CountryCodes.Guadeloupe);
				list.Add(Constants.CountryCodes.Guatemala);
				list.Add(Constants.CountryCodes.Guinea);
				list.Add(Constants.CountryCodes.Guyana);
				list.Add(Constants.CountryCodes.Haiti);
				list.Add(Constants.CountryCodes.Honduras);
				list.Add(Constants.CountryCodes.HongKong);
				list.Add(Constants.CountryCodes.Hungary);
				list.Add(Constants.CountryCodes.Iceland);
				list.Add(Constants.CountryCodes.India);
				list.Add(Constants.CountryCodes.Indonesia);
				list.Add(Constants.CountryCodes.Iran);
				list.Add(Constants.CountryCodes.Iraq);
				list.Add(Constants.CountryCodes.Ireland);
				list.Add(Constants.CountryCodes.Israel);
				list.Add(Constants.CountryCodes.Italy);
				list.Add(Constants.CountryCodes.Jamaica);
				list.Add(Constants.CountryCodes.Japan);
				list.Add(Constants.CountryCodes.Jordan);
				list.Add(Constants.CountryCodes.Kazakhstan);
				list.Add(Constants.CountryCodes.Kenya);
				list.Add(Constants.CountryCodes.Kiribati);
				list.Add(Constants.CountryCodes.KoreaSouth);
				list.Add(Constants.CountryCodes.Kosovo);
				list.Add(Constants.CountryCodes.Kuwait);
				list.Add(Constants.CountryCodes.Kyrgyzstan);
				list.Add(Constants.CountryCodes.LaoPeoplesDemocraticRepublic);
				list.Add(Constants.CountryCodes.Latvia);
				list.Add(Constants.CountryCodes.Lebanon);
				list.Add(Constants.CountryCodes.Lesotho);
				list.Add(Constants.CountryCodes.LibyanArabJamahiriya);
				list.Add(Constants.CountryCodes.Lithuania);
				list.Add(Constants.CountryCodes.Luxembourg);
				list.Add(Constants.CountryCodes.Macau);
				list.Add(Constants.CountryCodes.Macedonia);
				list.Add(Constants.CountryCodes.Madagascar);
				list.Add(Constants.CountryCodes.Malawi);
				list.Add(Constants.CountryCodes.Malaysia);
				list.Add(Constants.CountryCodes.Mali);
				list.Add(Constants.CountryCodes.MarshallIslands);
				list.Add(Constants.CountryCodes.Martinique);
				list.Add(Constants.CountryCodes.Mauritania);
				list.Add(Constants.CountryCodes.Maldives);
				list.Add(Constants.CountryCodes.Malta);
				list.Add(Constants.CountryCodes.Mauritius);
				list.Add(Constants.CountryCodes.Mayotte);
				list.Add(Constants.CountryCodes.Mexico);
				list.Add(Constants.CountryCodes.Micronesia);
				list.Add(Constants.CountryCodes.Moldova);
				list.Add(Constants.CountryCodes.Mongolia);
				list.Add(Constants.CountryCodes.Montenegro);
				list.Add(Constants.CountryCodes.Morocco);
				list.Add(Constants.CountryCodes.Mozambique);
				list.Add(Constants.CountryCodes.Myanmar);
				list.Add(Constants.CountryCodes.Namibia);
				list.Add(Constants.CountryCodes.Netherlands);
				list.Add(Constants.CountryCodes.NewCaledonia);
				list.Add(Constants.CountryCodes.NewZealand);
				list.Add(Constants.CountryCodes.Nepal);
				list.Add(Constants.CountryCodes.Nicaragua);
				list.Add(Constants.CountryCodes.Niger);
				list.Add(Constants.CountryCodes.Nigeria);
				list.Add(Constants.CountryCodes.NorfolkIsland);
				list.Add(Constants.CountryCodes.Norway);
				list.Add(Constants.CountryCodes.Oman);
				list.Add(Constants.CountryCodes.Pakistan);
				list.Add(Constants.CountryCodes.Palau);
				list.Add(Constants.CountryCodes.PalestinianTerritory);
				list.Add(Constants.CountryCodes.Panama);
				list.Add(Constants.CountryCodes.PapuaNewGuinea);
				list.Add(Constants.CountryCodes.Paraguay);
				list.Add(Constants.CountryCodes.Peru);
				list.Add(Constants.CountryCodes.Philippines);
				list.Add(Constants.CountryCodes.Poland);
				list.Add(Constants.CountryCodes.Portugal);
				list.Add(Constants.CountryCodes.Qatar);
				list.Add(Constants.CountryCodes.Reunion);
				list.Add(Constants.CountryCodes.Romania);
				list.Add(Constants.CountryCodes.Russia);
				list.Add(Constants.CountryCodes.Rwanda);
				list.Add(Constants.CountryCodes.SaintKittsAndNevis);
				list.Add(Constants.CountryCodes.SaintMartin);
				list.Add(Constants.CountryCodes.SaudiArabia);
				list.Add(Constants.CountryCodes.Senegal);
				list.Add(Constants.CountryCodes.Serbia);
				list.Add(Constants.CountryCodes.SierraLeone);
				list.Add(Constants.CountryCodes.Singapore);
				list.Add(Constants.CountryCodes.SintMaarten);
				list.Add(Constants.CountryCodes.Slovakia);
				list.Add(Constants.CountryCodes.Slovenia);
				list.Add(Constants.CountryCodes.SolomonIslands);
				list.Add(Constants.CountryCodes.Somalia);
				list.Add(Constants.CountryCodes.SouthAfrica);
				list.Add(Constants.CountryCodes.SouthSudan);
				list.Add(Constants.CountryCodes.Spain);
				list.Add(Constants.CountryCodes.SriLanka);
				list.Add(Constants.CountryCodes.StPierreEtMiquelon);
				list.Add(Constants.CountryCodes.Sudan);
				list.Add(Constants.CountryCodes.Suriname);
				list.Add(Constants.CountryCodes.Swaziland);
				list.Add(Constants.CountryCodes.Sweden);
				list.Add(Constants.CountryCodes.Switzerland);
				list.Add(Constants.CountryCodes.Taiwan);
				list.Add(Constants.CountryCodes.Tanzania);
				list.Add(Constants.CountryCodes.Thailand);
				list.Add(Constants.CountryCodes.TimorLeste);
				list.Add(Constants.CountryCodes.Togo);
				list.Add(Constants.CountryCodes.Tonga);
				list.Add(Constants.CountryCodes.TrinidadAndTobago);
				list.Add(Constants.CountryCodes.Tunisia);
				list.Add(Constants.CountryCodes.Turkey);
				list.Add(Constants.CountryCodes.Turkmenistan);
				list.Add(Constants.CountryCodes.TurksAndCaicosIslands);
				list.Add(Constants.CountryCodes.Tuvalu);
				list.Add(Constants.CountryCodes.Uganda);
				list.Add(Constants.CountryCodes.Ukraine);
				list.Add(Constants.CountryCodes.UnitedArabEmirates);
				list.Add(Constants.CountryCodes.UnitedKingdom);
				list.Add(Constants.CountryCodes.Uruguay);
				list.AddRange(Constants.CountryCodes.UsaAndTerritoriesList);
				list.Add(Constants.CountryCodes.Uzbekistan);
				list.Add(Constants.CountryCodes.Vanuatu);
				list.Add(Constants.CountryCodes.Venezuela);
				list.Add(Constants.CountryCodes.VietNam);
				list.Add(Constants.CountryCodes.WesternSamoa);
				list.Add(Constants.CountryCodes.Yemen);
				list.Add(Constants.CountryCodes.Zambia);
				list.Add(Constants.CountryCodes.Zimbabwe);
				// Please keep the list above in an alphabetical order.

				return list.ToArray();
			}
		}

		public static bool IsSupportedForLicenceBuilder(string countryCode)
		{
			return ((IList)LicenceKeyBuilderSupportedCountryCodes).Contains(countryCode);
		}

		#endregion

		#region Currency

		public static bool IsReciprocal(string countryCode)
		{
			var result = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(countryCode)?.GetIsReciprocal();
			if (result != null && result.HasValue)
			{
				return result.Value;
			}

			throw new NotSupportedException($"Country {countryCode} not supported for IsReciprocal");
		}

		#endregion

		#region Cash Basis GST
		public static bool IsGSTCashBasis(string countryCode)
		{
			switch (countryCode)
			{
				case Constants.CountryCodes.Chad:
				case Constants.CountryCodes.CoteDivoire:
				case Constants.CountryCodes.Cameroon:
				case Constants.CountryCodes.EquatorialGuinea:
					return true;

				default:
					return false;
			}
		}
		#endregion
	}
}
