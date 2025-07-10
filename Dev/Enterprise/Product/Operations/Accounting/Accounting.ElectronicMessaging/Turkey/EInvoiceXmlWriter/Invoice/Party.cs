using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	internal class Party
	{
		const string VKN = "VKN";
		const string SUBENO = "SUBENO";
		const string TICARETSICILNO = "TICARETSICILNO";
		const string MERSISNO = "MERSISNO";
		const string TCKN = "TCKN";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Turkish Language")]
		const string TurkeyInTurkish = "Türkiye";

		internal SupplierPartyType BuildAccountingSupplierParty(EInvoiceHelper helper)
		{
			var registrationNumberCollection = helper.UInvoice.BranchAddress.RegistrationNumberCollection;

			var accountingSupplierParty = new SupplierPartyType();
			accountingSupplierParty.Party = new PartyType();
			accountingSupplierParty.Party.WebsiteURI = new WebsiteURIType() { Value = helper.GetOrgHeader(helper.UInvoice.BranchAddress)?.MainWebURL?.PU_URL ?? string.Empty };

			var partyIdentification = new List<PartyIdentificationType>();
			partyIdentification.Add(PartyIdentificationFromCusCodes(helper, VKN, new ZString[] { OrgCusCode.CodeTypes.VATCode }));
			partyIdentification.Add(new PartyIdentificationType() { ID = new IDType() { Value = helper.UInvoice.Branch.Code, schemeID = SUBENO } });
			partyIdentification.Add(PartyIdentificationFromCusCodes(helper, TICARETSICILNO, new ZString[] { TurkeyOrgCusCodeInfo.OrgCusCodes.TradeRegistryNumber }));
			partyIdentification.Add(PartyIdentificationFromCusCodes(helper, MERSISNO, new ZString[] { TurkeyOrgCusCodeInfo.OrgCusCodes.MER }));
			accountingSupplierParty.Party.PartyIdentification = partyIdentification.ToArray();

			accountingSupplierParty.Party.PartyName = new PartyNameType();
			accountingSupplierParty.Party.PartyName.Name = new NameType1() { Value = helper.UInvoice.BranchAddress.CompanyName };

			accountingSupplierParty.Party.PostalAddress = new AddressType();
			accountingSupplierParty.Party.PostalAddress.StreetName = new StreetNameType() { Value = helper.CheckAddressForNull(helper.UInvoice.BranchAddress.Address1, helper.UInvoice.BranchAddress.Address2) };
			accountingSupplierParty.Party.PostalAddress.CitySubdivisionName = new CitySubdivisionNameType() { Value = helper.UInvoice.BranchAddress.City };
			accountingSupplierParty.Party.PostalAddress.CityName = new CityNameType();
			accountingSupplierParty.Party.PostalAddress.PostalZone = new PostalZoneType() { Value = helper.UInvoice.BranchAddress.Postcode };
			if (helper.UInvoice.BranchAddress.Country.Code.HasValue)
			{
				accountingSupplierParty.Party.PostalAddress.CityName.Value = helper.GetStateDescriptionByCountry(helper.UInvoice.BranchAddress.Country.Code.Value, ((ZString?)helper.UInvoice.BranchAddress.State).GetValueOrDefault());
			}
			accountingSupplierParty.Party.PostalAddress.Country = new CountryType()
			{
				Name = new NameType1()
				{
					Value = helper.UInvoice.BranchAddress.Country.Code.Value == CountryCodes.Turkey ? ((ZString)TurkeyInTurkish) : helper.UInvoice.BranchAddress.Country.Name.Value
				}
			};

			accountingSupplierParty.Party.PartyTaxScheme = new PartyTaxSchemeType();
			accountingSupplierParty.Party.PartyTaxScheme.TaxScheme = new TaxSchemeType()
			{
				Name = new NameType1()
				{
					Value = helper.GetOrgCusCodeValue(registrationNumberCollection, CountryCodes.Turkey, new ZString[] { TurkeyOrgCusCodeInfo.OrgCusCodes.VDM })?.Value.Value ?? ZString.Empty
				}
			};

			accountingSupplierParty.Party.Contact = new efatura.uyumsoft.com.tr.ContactType();
			accountingSupplierParty.Party.Contact.Telephone = new TelephoneType() { Value = helper.UInvoice.BranchAddress.Phone };
			accountingSupplierParty.Party.Contact.Telefax = new TelefaxType() { Value = helper.UInvoice.BranchAddress.Fax };
			accountingSupplierParty.Party.Contact.ElectronicMail = new ElectronicMailType() { Value = helper.UInvoice.BranchAddress.Email };

			return accountingSupplierParty;
		}

		internal CustomerPartyType BuildAccountingCustomerParty(EInvoiceHelper helper)
		{
			var accountingCustomerParty = new CustomerPartyType();
			var registrationNumberCollection = helper.UInvoice.OrganizationAddress.RegistrationNumberCollection;

			accountingCustomerParty.Party = new PartyType();
			accountingCustomerParty.Party.WebsiteURI = new WebsiteURIType() { Value = helper.Debtor?.MainWebURL?.PU_URL ?? string.Empty };  // TO DO: We don't have "MainWebURL.PU_URL" value in OrganisationAddress
			var partyIdentification = new List<PartyIdentificationType>();

			var vknTckn = helper.CustomerVATCode;
			var schemeID = VKN;
			if (helper.IsOrganizationCategoryNAT || string.IsNullOrEmpty(vknTckn))
			{
				vknTckn = helper.CustomerTCKN;
				schemeID = !string.IsNullOrEmpty(vknTckn) ? TCKN : VKN;
			}

			partyIdentification.Add(new PartyIdentificationType()
			{
				ID = new IDType()
				{
					Value = vknTckn,
					schemeID = schemeID
				}
			});

			accountingCustomerParty.Party.PartyIdentification = partyIdentification.ToArray();

			accountingCustomerParty.Party.PartyName = new PartyNameType();
			accountingCustomerParty.Party.PartyName.Name = new NameType1() { Value = helper.UInvoice.OrganizationAddress?.CompanyName };

			accountingCustomerParty.Party.PostalAddress = new AddressType();
			accountingCustomerParty.Party.PostalAddress.StreetName = new StreetNameType() { Value = helper.CheckAddressForNull(helper.UInvoice.OrganizationAddress.Address1, helper.UInvoice.OrganizationAddress.Address2) };
			accountingCustomerParty.Party.PostalAddress.CitySubdivisionName = new CitySubdivisionNameType() { Value = helper.UInvoice.OrganizationAddress.City };
			accountingCustomerParty.Party.PostalAddress.CityName = new CityNameType();
			if (helper.UInvoice.BranchAddress.Country.Code.HasValue)
			{
				accountingCustomerParty.Party.PostalAddress.CityName.Value = helper.GetStateDescriptionByCountry(helper.UInvoice.OrganizationAddress.Country.Code.Value, ((ZString?)helper.UInvoice.OrganizationAddress.State).GetValueOrDefault());
			}
			accountingCustomerParty.Party.PostalAddress.PostalZone = new PostalZoneType() { Value = helper.UInvoice.OrganizationAddress.Postcode };
			accountingCustomerParty.Party.PostalAddress.Country = new CountryType()
			{
				Name = new NameType1()
				{
					Value = helper.UInvoice.OrganizationAddress.Country.Code.Value == CountryCodes.Turkey ? (ZString)TurkeyInTurkish : helper.UInvoice.OrganizationAddress.Country.Name.Value
				}
			};

			accountingCustomerParty.Party.PartyTaxScheme = new PartyTaxSchemeType();
			accountingCustomerParty.Party.PartyTaxScheme.TaxScheme = new TaxSchemeType()
			{
				Name = new NameType1()
				{
					Value = helper.GetOrgCusCodeValue(registrationNumberCollection, CountryCodes.Turkey, new ZString[] { TurkeyOrgCusCodeInfo.OrgCusCodes.VDM })?.Value.Value ?? ZString.Empty
				}
			};

			accountingCustomerParty.Party.Contact = new efatura.uyumsoft.com.tr.ContactType();
			accountingCustomerParty.Party.Contact.Telephone = new TelephoneType() { Value = helper.UInvoice.OrganizationAddress.Phone };
			accountingCustomerParty.Party.Contact.Telefax = new TelefaxType() { Value = helper.UInvoice.OrganizationAddress.Fax };
			accountingCustomerParty.Party.Contact.ElectronicMail = new ElectronicMailType() { Value = helper.UInvoice.OrganizationAddress.Email };

			if (helper.IsOrganizationCategoryNAT)
			{
				var companyName = helper.UInvoice.OrganizationAddress.CompanyName.ToString();
				string[] stringSeparators = new string[] { companyName.Split(' ').Last() };
				var firstName = companyName.Split(stringSeparators, StringSplitOptions.None);
				accountingCustomerParty.Party.Person = new PersonType();
				accountingCustomerParty.Party.Person.FirstName = new FirstNameType() { Value = firstName[0].TrimEnd() };
				accountingCustomerParty.Party.Person.FamilyName = new FamilyNameType() { Value = companyName.Split(' ').Last() };
			}

			return accountingCustomerParty;
		}

		internal CustomerPartyType BuildBuyerCustomerParty(EInvoiceHelper helper)
		{
			if (!helper.IsOrganizationCategoryGOV || string.IsNullOrEmpty(helper.CustomerVATCode))
			{
				return null;
			}

			string vtpCode = helper.GetOrgCusCodeValue(CountryCodes.Turkey, new ZString[] { TurkeyOrgCusCodeInfo.OrgCusCodes.VTP })?.Value.Value ?? ZString.Empty;
			var buyerCustomerParty = new CustomerPartyType();
			buyerCustomerParty.Party = new PartyType();

			var partyIdentification = new List<PartyIdentificationType>();
			partyIdentification.Add(new PartyIdentificationType()
			{
				ID = new IDType()
				{
					schemeID = VKN,
					Value = string.IsNullOrEmpty(vtpCode) ? helper.CustomerVATCode : vtpCode
				}
			});
			buyerCustomerParty.Party.PartyIdentification = partyIdentification.ToArray();
			buyerCustomerParty.Party.PostalAddress = new AddressType()
			{
				CityName = new CityNameType(),
				CitySubdivisionName = new CitySubdivisionNameType(),
				PostalZone = new PostalZoneType(),
				StreetName = new StreetNameType(),
				Country = new CountryType()
				{
					Name = new NameType1()
				}
			};

			return buyerCustomerParty;
		}

		PartyIdentificationType PartyIdentificationFromCusCodes(EInvoiceHelper helper, string schemeIDValue, params ZString[] cusCodeTypes)
		{
			return new PartyIdentificationType()
			{
				ID = new IDType()
				{
					Value = helper.GetOrgCusCodeValue(helper.UInvoice.BranchAddress.RegistrationNumberCollection, CountryCodes.Turkey, cusCodeTypes)?.Value ?? ZString.Empty,
					schemeID = schemeIDValue
				}
			};
		}
	}
}
