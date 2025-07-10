using System;
using System.Globalization;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	internal class Header : ComplianceReportXmlBuilder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			Argument.NotNull(additionalData, nameof(additionalData));

			var reportCompany = report.Company;
			var companyOrgAddress = ((SAFTAdditionalDataCollector)additionalData).CompanyOrgAddress;
			var companyRegistrationNumber = MasterFiles.GetTaxID(companyOrgAddress);
			var dateCreated = ZDateTime.Today;

			return new XStreamingElement("Header",
					new XElement("AuditFileVersion", "1.04_01"),
					new XElement("CompanyID", companyRegistrationNumber),
					new XElement("TaxRegistrationNumber", companyRegistrationNumber),
					new XElement("TaxAccountingBasis", GetTaxAccountingBasis(additionalData)),
					new XElement("CompanyName", companyOrgAddress?.CompanyName),
					new XElement("CompanyAddress",
						new XElement("AddressDetail", companyOrgAddress?.Address1),
						new XElement("City", companyOrgAddress?.City),
						new XElement("PostalCode", MasterFiles.GetPostalCode(companyOrgAddress)),
						!string.IsNullOrEmpty(companyOrgAddress?.State) && reportCompany.StateCodeList.ContainsCode((string)companyOrgAddress.State) ? new XElement("Region", reportCompany.StateCodeList[(string)companyOrgAddress.State, StringComparison.CurrentCultureIgnoreCase].Description) : null,
						new XElement("Country", companyOrgAddress?.Country?.Code)
					),
					new XElement("FiscalYear", report.AccountingPeriod / 100),
					new XElement("StartDate", additionalData.DateFrom.ToString(SAFTXmlBuilder.DateFormat, CultureInfo.InvariantCulture)),
					new XElement("EndDate", GetEndDate(additionalData, dateCreated)),
					new XElement("CurrencyCode", "EUR"),
					new XElement("DateCreated", dateCreated.ToString(SAFTXmlBuilder.DateFormat, CultureInfo.InvariantCulture)),
					new XElement("TaxEntity", "Global"),
					new XElement("ProductCompanyTaxID", "770010920"),
					new XElement("SoftwareCertificateNumber", AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.Value),
					new XElement("ProductID", AccountingMasterFilesRegistry.Instance.PTSoftwareName.Value),
					new XElement("ProductVersion", new EnterpriseInformationRetriever().ComplianceVersionNumber),
					!reportCompany.GC_Phone_Formatted.IsEmpty ? new XElement("Telephone", reportCompany.GC_Phone_Formatted) : null,
					!reportCompany.GC_Email.IsEmpty ? new XElement("Email", reportCompany.GC_Email) : null
				);
		}

		string GetEndDate(ComplianceReportAdditionalDataCollector additionalData, ZDateTime dateCreated)
		{
			var dateEnd = dateCreated >= additionalData.DateFrom && dateCreated <= additionalData.DateTo
						? dateCreated
						: additionalData.DateTo;

			return dateEnd.ToString(SAFTXmlBuilder.DateFormat, CultureInfo.InvariantCulture);
		}

		string GetTaxAccountingBasis(ComplianceReportAdditionalDataCollector additionalData) => additionalData.DataCollectionMode == ComplianceReportDataCollectionMode.SAFTSelfBilling
			? "S"
			: additionalData.IsMergedData ? "F" : "P";
	}
}
