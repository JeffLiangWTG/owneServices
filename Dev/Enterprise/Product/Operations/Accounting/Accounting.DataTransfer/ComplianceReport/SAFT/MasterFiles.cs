using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using UniversalOrgAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	internal class MasterFiles : ComplianceReportXmlBuilder
	{
		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			var saftAdditionalData = (SAFTAdditionalDataCollector)additionalData;
			var customers = GetCustomers(saftAdditionalData);
			var chargeCodes = GetChargeCodes(saftAdditionalData);
			var glAccounts = GetGLAccounts(saftAdditionalData);

			return new XStreamingElement("MasterFiles",   // Hard-coded xml node name
					BuildCustomerXmls(customers, saftAdditionalData),
					chargeCodes.Select(x => BuildProductXml(x)).Concat(glAccounts.Select(x => BuildProductXml(x))),
					new TaxTable().BuildXml(report, null, additionalData));
		}

		IEnumerable<XStreamingElement> BuildCustomerXmls(IEnumerable<UniversalOrgAddress> customers, SAFTAdditionalDataCollector additionalData)
		{
			var customerNodes = new List<XStreamingElement>();
			foreach (var customer in customers)
			{
				additionalData.CustomerSalesInvoices.TryGetValue(customer.OrganizationCode.Value, out var salesInvoicePKs);
				if (salesInvoicePKs == null || salesInvoicePKs.Any(x => !additionalData.PostedWithoutIVASalesInvoicePKs.Contains(x)))
				{
					customerNodes.Add(BuildCustomerXml(customer, additionalData));
				}
			}

			if (additionalData.PostedWithoutIVASalesInvoicePKs.Count > 0)
			{
				var desconhecido = PortugalComplianceInfo.UnknownData;
				var consumidorFinalCustomerXml = BuildCustomerXml(PortugalComplianceInfo.PortugalAccountCodeForMissingRegistrationNumber, desconhecido,
					AccountingCountrySpecificValidationHelper.EmptyPortugalIVA, desconhecido,
					BuildBillingAddress(desconhecido, desconhecido, desconhecido, desconhecido), 0);
				customerNodes.Add(consumidorFinalCustomerXml);
			}
			return customerNodes;
		}

		XStreamingElement BuildCustomerXml(UniversalOrgAddress orgAddress, SAFTAdditionalDataCollector additionalData)
		{
			return BuildCustomerXml(orgAddress.OrganizationCode,
				PortugalComplianceInfo.UnknownData,
				GetTaxID(orgAddress, additionalData),
				orgAddress.CompanyName,
				BuildBillingAddress(orgAddress),
				additionalData.OrgHeaderDetails.TryGetValue(orgAddress.OrganizationCode.Value, out var details) && details.ARCustomerSelfBillsRevenue ? 1 : 0);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildCustomerXml(ZCodeMappedZString? organizationCode, string accountID, string customerTaxID, ZString? companyName, XStreamingElement billingAddressElement, int selfBillingIndicator)
		{
			return new XStreamingElement("Customer",
					new XElement("CustomerID", organizationCode),
					new XElement("AccountID", accountID),
					new XElement("CustomerTaxID", customerTaxID),
					new XElement("CompanyName", companyName),
					billingAddressElement,
					new XElement("SelfBillingIndicator", selfBillingIndicator)
				);
		}

		IEnumerable<UniversalOrgAddress> GetCustomers(SAFTAdditionalDataCollector additionalData)
		{
			return additionalData.OrgAddresses.Where(x => additionalData.CustomerCodes.Contains(x.Key)).Select(y => y.Value).OrderBy(z => z.OrganizationCode.Value.ToString());
		}

		XStreamingElement BuildBillingAddress(UniversalOrgAddress orgAddress)
		{
			return BuildBillingAddress(orgAddress.Address1, orgAddress.City,GetPostalCode(orgAddress), orgAddress.Country.Code);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildBillingAddress(ZString? addressDetail, ZString? city, string postalCode, ZString? country)
		{
			return new XStreamingElement("BillingAddress",
					new XElement("AddressDetail", addressDetail),
					new XElement("City", city),
					new XElement("PostalCode", postalCode),
					new XElement("Country", country)
				);
		}

		string GetTaxID(UniversalOrgAddress orgAddress, ComplianceReportAdditionalDataCollector additionalData)
		{
			var result = AccountingCountrySpecificValidationHelper.EmptyPortugalIVA;

			var ptIvaTaxRegistrationNumber = orgAddress.RegistrationNumberCollection?.Where(x => PortugalComplianceInfo.IsTaxRegistrationNumber(x.CountryOfIssue?.Code.Value, x.Type?.Code.Value)).FirstOrDefault();
			var countryCode = string.Empty;
			var prefixForTaxRegistrationNumber = Constants.CountryCodes.Portugal;
			if (ptIvaTaxRegistrationNumber != null)
			{
				countryCode = Constants.CountryCodes.Portugal;
				result = ptIvaTaxRegistrationNumber.Value.ToString();
			}
			else if (additionalData.OrgTaxRegistrationNumberDetails.TryGetValue(orgAddress.OrganizationCode.Value, out var homeCountryRegistration)
				&& !homeCountryRegistration.Item1.IsEmpty && !homeCountryRegistration.Item2.IsEmpty)
			{
				countryCode = homeCountryRegistration.Item1;
				prefixForTaxRegistrationNumber = RefCountry.GetPrefixForTaxRegistrationCode(homeCountryRegistration.Item1);
				result = homeCountryRegistration.Item2;
			}

			if (result.StartsWith(prefixForTaxRegistrationNumber, StringComparison.OrdinalIgnoreCase)
				&& additionalData.IsEUCountryCode(countryCode))
			{
				result = result.Substring(prefixForTaxRegistrationNumber.Length);
			}

			return result;
		}

		internal static string GetTaxID(UniversalOrgAddress orgAddress)
		{
			var ivaTaxID = orgAddress?.RegistrationNumberCollection?.Where(x => x.CountryOfIssue.Code.Value == Constants.CountryCodes.Portugal && x.Type.Code.Value == OrgCusCode.CodeTypes.IVA).FirstOrDefault();
			return ivaTaxID != null ? ivaTaxID.Value : orgAddress.RegistrationNumberCollection?.FirstOrDefault()?.Value ?? string.Empty;
		}

		internal static string GetPostalCode(UniversalOrgAddress orgAddress) => !string.IsNullOrEmpty(orgAddress.Postcode) ?
			(string)orgAddress.Postcode : PortugalComplianceInfo.UnknownData;

		IEnumerable<ChargeCodeDetails> GetChargeCodes(ComplianceReportAdditionalDataCollector additionalData)
		{
			return from chargeCode in additionalData.ChargeCodes
				   join usedCode in additionalData.UsedChargeCodes on chargeCode.Key equals usedCode
				   orderby chargeCode.Key
				   select chargeCode.Value;
		}

		IEnumerable<GLAccount> GetGLAccounts(SAFTAdditionalDataCollector additionalData)
		{
			return from account in additionalData.GLAccounts
				   join usedAccount in additionalData.UsedGLAccounts on account.Key equals usedAccount
				   orderby account.Key
				   select account.Value;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name, Hard-coded xml node name and values")]
		XStreamingElement BuildProductXml(ChargeCodeDetails chargeCodeDetails)
		{
			return new XStreamingElement("Product",
					new XElement("ProductType", chargeCodeDetails.GoodsServiceType == GoodServiceTypes.Codes.SRV ? "S" : "P"),
					new XElement("ProductCode", chargeCodeDetails.ChargeCode.Code),
					new XElement("ProductDescription", chargeCodeDetails.ChargeCode.Description),
					new XElement("ProductNumberCode", chargeCodeDetails.ChargeCode.Code)
				);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name, Hard-coded xml node name and values")]
		XStreamingElement BuildProductXml(GLAccount glAccount)
		{
			return new XStreamingElement("Product",
					new XElement("ProductType", "S"),
					new XElement("ProductCode", glAccount.AccountCode),
					new XElement("ProductDescription", glAccount.Description),
					new XElement("ProductNumberCode", glAccount.AccountCode)
				);
		}
	}
}
