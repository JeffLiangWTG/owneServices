using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.Integration.ZArchitecture;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public interface ICompany
	{
		string Code { get; }
		string Name { get; }
		Guid OrganisationPK { get; }
		Guid PK { get; }

		string Address1 { get; }
		string Address2 { get; }
		string City { get; }
		string Postcode { get; }
		string State { get; }
		ICountry Country { get; }
		string Fax { get; }

		string BusinessRegNo1 { get; }
		string BusinessRegNo2 { get; }

		CountryDateTimeFormat DateTimeFormat { get; }
		ICurrency LocalCurrency { get; }
		ExchangeRate ExchangeRate { get; }

		IEnumerable<IBranch> Branches { get; }
		IEnumerable<IBranch> ActiveBranches { get; }

		bool IsDemoCompany { get; }
		bool IsGSTCashBasis { get; }
		bool IsGSTRegistered { get; }
		bool IsReciprocal { get; }
		bool IsWHTCashBasis { get; }
		bool IsWHTRegistered { get; }
		int ExchangeRateDecimalPlaces { get; }

		string HumanReadableNameForRegistry { get; }

		int GetPeriod(DateTime date);

#if DEBUG
		string LicenceKeyIdentifier { get; }
#endif
		void UpdateLicenceKeyIdentifier();
	}

	public static class CompanyExtensions
	{
		public static string GetSummaryAndRegistrationText(this ICompany company)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			return Res.GetString("81bca968-e7c9-4ab2-bb6c-d028fd6a2920", "CLIENT NAME: {0}\r\nENTERPRISE CODE: {1}\r\nCOMPANY CODE: {2}\r\nSERVER CODE: {3}",
				company != null ? company.Name : "",
				registrationKey.EnterpriseCode,
				company != null ? company.Code : "",
				registrationKey.ServerCode) + "\r\n";
		}

		public static string GetLicenceCode(this ICompany company, string separator = "")
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			return registrationKey.EnterpriseCode + separator + (company != null ? company.Code : "") + separator + registrationKey.ServerCode;
		}

		public static string GetEHubAuthValue(this ICompany company)
		{
			var clientId = company.GetLicenceCode();
			var password = ObjectFactory.Get<IProductRegistration>().Key.Password;
			return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0}:{1}", clientId, password))); // Constant
		}
	}
}
