using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using UniversalOrgAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalRegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	internal class MasterFiles1_30 : ComplianceReportXmlBuilder
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildXmlCore(AccComplianceReport report, SAFTAdditionalDataCollector additionalData)
		{
			var customers = additionalData.OrgAddresses.Where(x => additionalData.CustomerCodes.Contains(x.Key)).Select(y => y.Value).OrderBy(z => z.OrganizationCode.Value.ToString());
			var suppliers = additionalData.OrgAddresses.Where(x => additionalData.SupplierCodes.Contains(x.Key)).Select(y => y.Value).OrderBy(z => z.OrganizationCode.Value.ToString());

			return new XStreamingElement("MasterFiles",
					BuildGLLedgerAccountXml(report, additionalData),
					customers.Any() ? new XStreamingElement("Customers",
						BuildCustomerSupplierXmls(report, customers, additionalData, ContactType.Receivables.Code)
					) : null,
					suppliers.Any() ? new XStreamingElement("Suppliers",
						BuildCustomerSupplierXmls(report, suppliers, additionalData, ContactType.Payables.Code)
					) : null,
					new XElement("TaxTable",
						new XElement("TaxTableEntry",
							new XElement("TaxType", additionalData.TaxRegistrationCode),
							new XElement("Description", additionalData.TaxTableDescription),
							BuildTaxCodeDetailXmls(report)
						)
					)
				);
		}

		IEnumerable<XStreamingElement> BuildCustomerSupplierXmls(AccComplianceReport report, IEnumerable<UniversalOrgAddress> orgAddresses, SAFTAdditionalDataCollector additionalData, string contactType)
		{
			var orgAddressNodes = new List<XStreamingElement>();
			foreach (var orgAddress in orgAddresses)
			{
				OrgContactDependentCollection orgContacts = null;
				var hasContacts = additionalData.OrgContacts?.TryGetValue(orgAddress.OrganizationCode ?? ZString.Empty, out orgContacts) ?? false;
				var contact = hasContacts
					? orgContacts?.FirstOrDefault(x => x is OrgContact orgContact && orgContact.Documents.Any(document => document is OrgDocument orgDocument && orgDocument.OD_DocumentGroup == contactType && orgDocument.OD_DefaultContact)) as OrgContact
					: null;

				orgAddressNodes.Add(BuildCustomerSupplierXml(report, orgAddress, contact, additionalData, contactType));
			}

			return orgAddressNodes;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1135: DoNotUseCountrySpecificBusinessRule", Justification = "Testing")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildCustomerSupplierXml(AccComplianceReport report, UniversalOrgAddress orgAddress, OrgContact contact, SAFTAdditionalDataCollector additionalData, string contactType)
		{
			var parentNodeName = contactType == ContactType.Receivables.Code ? "Customer" : "Supplier";
			var iDName = contactType == ContactType.Receivables.Code ? "CustomerID" : "SupplierID";
			var accountID = contactType == ContactType.Receivables.Code ? GLControlAccounts.Instance.ARControlAccount.AG_AccountNum : GLControlAccounts.Instance.APControlAccount.AG_AccountNum;
			var orgFullName = additionalData.OrgFullNames.TryGetValue(orgAddress.OrganizationCode.Value.ToString(), out var fullName) ? fullName : ZString.Empty;
			var countryCode = orgAddress?.Country?.Code ?? ZString.Empty;
			var countryRegistrationNumbers = orgAddress.RegistrationNumberCollection?.Where(x => x.CountryOfIssue.Code.Value == report.Company.GC_RN_NKCountryCode) ?? new List<UniversalRegistrationNumber>();
			var govRegistrationNumber = countryRegistrationNumbers.FirstOrDefault(x => x.Type.Code.Value == additionalData.BusinessRegistrationCode)?.Value;
			var taxRegistrationNumber = countryRegistrationNumbers.FirstOrDefault(x => x.Type.Code.Value == additionalData.TaxRegistrationCode)?.Value ?? ComplianceReportAdditionalDataProviderHelper.NoneTaxRegistrationNumber;
			var orgOpeningDebitBalance = additionalData.OrgOpeningDebitBalances.TryGetValue(orgAddress.OrganizationCode.Value.ToString(), out var openingDebitBalance) ? openingDebitBalance : 0;
			var orgOpeningCreditBalance = additionalData.OrgOpeningCreditBalances.TryGetValue(orgAddress.OrganizationCode.Value.ToString(), out var openingCreditBalance) ? openingCreditBalance : 0;
			var orgClosingDebitBalance = additionalData.OrgClosingDebitBalances.TryGetValue(orgAddress.OrganizationCode.Value.ToString(), out var closingDebitBalance) ? closingDebitBalance : 0;
			var orgClosingCreditBalance = additionalData.OrgClosingCreditBalances.TryGetValue(orgAddress.OrganizationCode.Value.ToString(), out var closingCreditBalance) ? closingCreditBalance : 0;

			return new XStreamingElement(parentNodeName,
					new XElement("RegistrationNumber", govRegistrationNumber ?? ZString.Empty),
					new XElement("Name", orgFullName),
					new XElement("Address",
							!(orgAddress?.Address1 ?? ZString.Empty).IsEmpty ? new XElement("StreetName", orgAddress?.Address1) : null,
							!(orgAddress?.Address2 ?? ZString.Empty).IsEmpty ? new XElement("AdditionalAddressDetail", orgAddress?.Address2) : null,
							!(orgAddress?.City ?? ZString.Empty).IsEmpty ? new XElement("City", orgAddress?.City) : null,
							!(orgAddress?.Postcode ?? ZString.Empty).IsEmpty ? new XElement("PostalCode", orgAddress?.Postcode) : null,
							!countryCode.IsEmpty ? new XElement("Country", countryCode) : null,
							new XElement("AddressType", "PostalAddress")
						),
					new XElement("Contact",
							new XElement("ContactPerson",
								new XElement("FirstName", "NotUsed"),
								new XElement("LastName", contact?.Name)
							),
							new XElement("Telephone", contact?.OC_Phone),
							new XElement("Email", contact?.OC_Email)
						),
					new XElement("TaxRegistration",
							new XElement("TaxRegistrationNumber", taxRegistrationNumber),
							new XElement("TaxAuthority", additionalData.TaxAuthority)
						),
					new XElement(iDName, orgAddress.OrganizationCode),
					new XElement("BalanceAccount",
						new XElement("AccountID", accountID),
						contactType == ContactType.Receivables.Code
						? new List<XElement>() {
							new XElement("OpeningDebitBalance", orgOpeningDebitBalance),
							new XElement("ClosingDebitBalance", orgClosingDebitBalance),
						}
						: new List<XElement>() {
							new XElement("OpeningCreditBalance", orgOpeningCreditBalance),
							new XElement("ClosingCreditBalance", orgClosingCreditBalance)
						}
					)
				);
		}

		IEnumerable<XStreamingElement> BuildTaxCodeDetailXmls(AccComplianceReport report)
		{
			var taxCodeDetailNodes = new List<XStreamingElement>();
			var taxIdAndTaxMessageCombinationRules = AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.GetFallBackValueAtAllLevels(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty).TaxIdAndTaxMessageCombinationRulesCollection
				.OfType<TaxIdAndTaxMessageCombinationRules>()
				.Where(x => !string.IsNullOrEmpty(x.GovernmentCode));
			foreach (var taxGroupCode in taxIdAndTaxMessageCombinationRules.DistinctBy(x => x.TaxGroupCode))
			{
				taxCodeDetailNodes.Add(BuildTaxCodeDetailXml(report, taxGroupCode));
			}
			return taxCodeDetailNodes;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildTaxCodeDetailXml(AccComplianceReport report, TaxIdAndTaxMessageCombinationRules taxIdAndTaxMessageCombinationRules)
		{
			var taxGroup = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty).OfType<CodeDescriptionBoolRelatedItem>().FirstOrDefault(x => x.Code == taxIdAndTaxMessageCombinationRules.TaxGroupCode);
			return new XStreamingElement("TaxCodeDetails",
				new XElement("TaxCode", taxGroup.Code),
				new XElement("Description", taxGroup.Description),
				new XElement("TaxPercentage", taxIdAndTaxMessageCombinationRules.TaxRateValue),
				new XElement("Country", GlbCompany.CurrentCompany.Country.Code),
				new XElement("StandardTaxCode", taxGroup.RelatedItemCode),
				new XElement("BaseRate", "100")
			);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildGLLedgerAccountXml(AccComplianceReport report, SAFTAdditionalDataCollector additionalData)
		{
			var decimals = report.Company.LocalCurrency.Decimals;
			var validGLAccounts = additionalData.ActiveGLAccounts.Values.Where(x => (x.GLAccount?.AccountType?.Code ?? ZString.Empty) != Core.Constants.AccountType.Note);
			var groupingCategory = AccountingMasterFilesRegistry.Instance.SAFTGroupingCategory.GetFallBackValueAtAllLevels(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty);

			return new XStreamingElement("GeneralLedgerAccounts",
				validGLAccounts.OrderBy(x => x.AccountNum).Select(x => new XStreamingElement("Account",
					new XElement("AccountID", x.GLAccount.AccountCode),
					new XElement("AccountDescription", x.GLAccount.Description),
					new XElement("AccountType", "GL"),
					new XElement("AccountCreationDate", x.GLAccount.CreationDate?.ToString("yyyy-MM-dd")),
					x.OpeningDebitBalance > 0
						? new XElement("OpeningDebitBalance", x.OpeningDebitBalance.ToString(decimals))
						: (x.OpeningCreditBalance > 0
							? new XElement("OpeningCreditBalance", x.OpeningCreditBalance.ToString(decimals))
							: new XElement("OpeningDebitBalance", x.OpeningDebitBalance.ToString(decimals))),
					x.ClosingDebitBalance > 0
						? new XElement("ClosingDebitBalance", x.ClosingDebitBalance.ToString(decimals))
						: (x.ClosingCreditBalance > 0
							? new XElement("ClosingCreditBalance", x.ClosingCreditBalance.ToString(decimals))
							: new XElement("ClosingDebitBalance", x.ClosingDebitBalance.ToString(decimals))),
					groupingCategory.IsNullOrEmpty() ? null : new XElement("GroupingCategory", groupingCategory),
					x.AlternateGLAccountNumber.IsNullOrEmpty() ? null : new XElement("GroupingCode", x.AlternateGLAccountNumber)
				))
			);
		}
	}
}
