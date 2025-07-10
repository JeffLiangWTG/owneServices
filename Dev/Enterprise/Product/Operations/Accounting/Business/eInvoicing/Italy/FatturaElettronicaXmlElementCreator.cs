using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public static class FatturaElettronicaXmlElementCreator
	{
		/// <summary> Tax info of orgnization company </summary>
		public static XElement CreateIdFiscaleType(string idPaese, string idCodice, string elementName = "IdFiscaleIVA")
		{
			return new XElement(elementName,
				new XElement("IdPaese", idPaese),
				new XElement("IdCodice", idCodice));
		}

		/// <summary> Tax info of idividule company </summary>
		public static XElement CreateCodiceFiscale(string codiceFiscale, string elementName = "CodiceFiscale")
		{
			return new XElement(elementName, codiceFiscale);
		}

		/// <summary> Orgnization/Idividule company info </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No localization required.")]
		public static XElement CreateAnagraficaType(string companyName, bool isNAT, string title, string codEORI, string elementName = "Anagrafica")
		{
			var result = new XElement(elementName);
			if (isNAT)
			{
				var names = GetIndividualNames(companyName);
				result.Add(new XElement("Nome", names.Item1.EnsureComplianceWithBasicLatinAndLatin1Supplement()));
				result.Add(new XElement("Cognome", names.Item2.EnsureComplianceWithBasicLatinAndLatin1Supplement()));
			}
			else
			{
				result.Add(new XElement("Denominazione", companyName.EnsureComplianceWithBasicLatinAndLatin1Supplement()));
			}

			if (title != null)
			{
				result.Add(new XElement("Titolo", title.EnsureComplianceWithBasicLatin()));
			}
			if (codEORI != null)
			{
				result.Add(new XElement("CodEORI", codEORI));
			}

			return result;
		}

		/// <summary> Address info </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No localization required.")]
		public static XElement CreateIndirizzoType(string address, string number, string postCode, string city, string state, string country, string elementName = "Sede")
		{
			var result = new XElement(elementName);

			var fixedAddress = ((address?.Length ?? 0) > 60 ? address.Substring(0, 60) : address ?? string.Empty).TrimEnd();
			result.Add(new XElement("Indirizzo", fixedAddress.EnsureComplianceWithBasicLatinAndLatin1Supplement()));

			if (number != null)
			{
				result.Add(new XElement("NumeroCivico", number));
			}

			result.Add(new XElement("CAP", postCode));
			result.Add(new XElement("Comune", city.EnsureComplianceWithBasicLatinAndLatin1Supplement()));

			if (country == Core.Constants.CountryCodes.Italy && state != null && new Regex("^[A-Z]{2}$").IsMatch(state))
			{
				result.Add(new XElement("Provincia", state));
			}

			result.Add(new XElement("Nazione", country ?? "IT"));

			return result;
		}

		#region Implementation

		static Tuple<string, string> GetIndividualNames(ZString? source)
		{
			var subStrings = Array.Empty<string>();
			string firstName = string.Empty;
			string lastName = string.Empty;

			if (source.HasValue)
			{
				subStrings = source.Value.Trim().
					Split(new[] { ' ' }, 2).
					Select(x => x.ToString().Trim()).ToArray();
			}

			if (subStrings.Length == 1)
			{
				firstName = lastName = subStrings[0];
			}
			else if (subStrings.Length == 2)
			{
				firstName = subStrings[0];
				lastName = subStrings[1];
			}

			return new Tuple<string, string>(firstName, lastName);
		}

		#endregion
	}
}
