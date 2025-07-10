using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	internal class Header1_30 : ComplianceReportXmlBuilder
	{
		internal override XStreamingElement BuildAnnualXml(IEnumerable<AccComplianceReport> reports, ComplianceReportAdditionalDataCollector additionalData)
		{
			Argument.NotNull(additionalData, nameof(additionalData));

			var lastReport = reports.Last();
			return BuildXmlCore(lastReport, (SAFTAdditionalDataCollector)additionalData);
		}

		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			Argument.NotNull(additionalData, nameof(additionalData));

			return BuildXmlCore(report, (SAFTAdditionalDataCollector)additionalData);
		}

		XStreamingElement BuildXmlCore(AccComplianceReport report, SAFTAdditionalDataCollector additionalData)
		{
			var reportCompany = report.Company;
			var companyOrgAddress = additionalData.CompanyOrgAddress;
			var countryCusCodes = reportCompany.OrgProxy.CustomsCodes.Cast<OrgCusCode>().Where(x => x.OK_RN_NKCodeCountry.Equals(report.Company.GC_RN_NKCountryCode));
			var govRegistrationNumber = countryCusCodes.FirstOrDefault(x => x.OK_CodeType == additionalData.BusinessRegistrationCode)?.OK_CustomsRegNo;
			var taxRegistrationNumber = countryCusCodes.FirstOrDefault(x => x.OK_CodeType == additionalData.TaxRegistrationCode)?.OK_CustomsRegNo ?? ComplianceReportAdditionalDataProviderHelper.NoneTaxRegistrationNumber;
			var countryCode = companyOrgAddress?.Country?.Code ?? ZString.Empty;
			var contactStaff = additionalData.ContactStaff;

			var companyElement = BuildCompany(companyOrgAddress, contactStaff, additionalData.TaxAuthority, govRegistrationNumber, taxRegistrationNumber, additionalData.DefaultReceiptAccountNumber);
			var selectionCriteriaElement = BuildSelectionCriteria(additionalData.DateFrom, additionalData.DateTo);
			var header = BuildHeader(companyElement, selectionCriteriaElement, countryCode, reportCompany.LocalCurrency.Code, additionalData.TaxAccountingBasis);

			return header;
		}

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildHeader(XElement companyElement, XElement selectionCriteriaElement, string countryCode, string localCurrencyCode, string taxAccountingBasis)
		{
			var header = new XStreamingElement("Header",
							new XElement("AuditFileVersion", "1.30"),
							new XElement("AuditFileCountry", countryCode),
							new XElement("AuditFileDateCreated", ZDateTime.Today.ToString(SAFTXmlBuilder.DateFormat, CultureInfo.InvariantCulture)),
							new XElement("SoftwareCompanyName", "Wisetech Global Limited"),
							new XElement("SoftwareID", "CargoWise"),
							new XElement("SoftwareVersion", ReleaseInfo.Instance.VersionNumber.ToString())
							);

			header.Add(companyElement);
			header.Add(new XElement("DefaultCurrencyCode", localCurrencyCode));
			header.Add(selectionCriteriaElement);
			header.Add(new XElement("TaxAccountingBasis", taxAccountingBasis));

			return header;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XElement BuildCompany(OrganizationAddress companyOrgAddress, GlbStaff contactStaff, string taxAuthority, string govRegistrationNumber, string taxRegistrationNumber, string defaultReceiptAccountNumber)
		{
			var result = new XElement("Company");
			result.Add(new XElement("RegistrationNumber", govRegistrationNumber));
			result.Add(new XElement("Name", companyOrgAddress?.CompanyName));
			result.Add(BuildAddress(companyOrgAddress));
			result.Add(BuildContact(contactStaff));
			result.Add(BuildTaxRegistration(taxRegistrationNumber, taxAuthority));
			result.Add(BuildBankAccount(defaultReceiptAccountNumber));

			return result;
		}

		XElement BuildSelectionCriteria(ZDate dateFrom, ZDate dateTo)
		{
			return new XElement("SelectionCriteria",   // Hard-coded xml node name
						new XElement("SelectionStartDate", dateFrom.ToString(SAFTXmlBuilder.DateFormat, CultureInfo.InvariantCulture)),   // Hard-coded xml node name
						new XElement("SelectionEndDate", dateTo.ToString(SAFTXmlBuilder.DateFormat, CultureInfo.InvariantCulture))   // Hard-coded xml node name
						);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XElement BuildAddress(OrganizationAddress companyOrgAddress)
		{
			var address1 = companyOrgAddress?.Address1 ?? ZString.Empty;
			var address2 = companyOrgAddress?.Address2 ?? ZString.Empty;
			var city = companyOrgAddress?.City ?? ZString.Empty;
			var postcode = companyOrgAddress?.Postcode ?? ZString.Empty;
			var countryCode = companyOrgAddress?.Country?.Code ?? ZString.Empty;

			return new XElement("Address",
						address1.IsEmpty ? null : new XElement("StreetName", address1),
						address2.IsEmpty ? null : new XElement("AdditionalAddressDetail", address2),
						city.IsEmpty ? null : new XElement("City", city),
						postcode.IsEmpty ? null : new XElement("PostalCode", postcode),
						countryCode.IsEmpty ? null : new XElement("Country", countryCode),
						new XElement("AddressType", "PostalAddress")
						);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XElement BuildContact(GlbStaff contactStaff)
		{
			return new XElement("Contact",
							new XElement("ContactPerson",
								new XElement("FirstName", "NotUsed"),
								new XElement("LastName", contactStaff?.GS_FullName)
								),
							new XElement("Telephone", contactStaff?.GS_WorkPhone),
							!(contactStaff?.GS_EmailAddress ?? ZString.Empty).IsEmpty ? new XElement("Email", contactStaff?.GS_EmailAddress) : null
							);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XElement BuildTaxRegistration(string taxRegistrationNumber, string taxAuthority)
		{
			return new XElement("TaxRegistration",
							new XElement("TaxRegistrationNumber", taxRegistrationNumber),
							new XElement("TaxAuthority", taxAuthority)
							);
		}

		XElement BuildBankAccount(string defaultReceiptAccountNumber)
		{
			return new XElement("BankAccount",   // Hard-coded xml node name
							new XElement("BankAccountNumber", defaultReceiptAccountNumber)   // Hard-coded xml node name
							);
		}
	}
}
