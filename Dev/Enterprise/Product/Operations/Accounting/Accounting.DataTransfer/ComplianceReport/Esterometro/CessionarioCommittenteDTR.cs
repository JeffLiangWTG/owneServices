using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro
{
	internal class CessionarioCommittenteDTR : ComplianceReportXmlBuilder
	{
		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			var orgProxy = EsterometroDataHelper.SelectBestOrgProxyOrNull(GlbBranch.CurrentBranch, GlbCompany.CurrentCompany);

			return new XStreamingElement("CessionarioCommittenteDTR",   // Hard-coded xml node name
					BuildIdentificativiFiscaliXml(orgProxy),   // Hard-coded xml node name
					BuildAltriDatiIdentificativiXml(orgProxy)   // Hard-coded xml node name
				);
		}

		XStreamingElement BuildIdentificativiFiscaliXml(OrgHeader orgProxy)
		{
			return new XStreamingElement("IdentificativiFiscali", FatturaElettronicaXmlElementCreator.CreateIdFiscaleType(Core.Constants.CountryCodes.Italy, orgProxy.IvaNumberForItaly()));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildAltriDatiIdentificativiXml(OrgHeader orgProxy)
		{
			var orgFullName = orgProxy?.OH_FullName ?? ZString.Empty;
			var denominazione = !orgFullName.IsEmpty && orgFullName.Length > 80 ? orgFullName.Substring(0, 80) : orgFullName;

			var address1 = orgProxy?.MainAddress?.OA_Address1 ?? ZString.Empty;
			var address2 = orgProxy?.MainAddress?.OA_Address2 ?? ZString.Empty;
			var combinedAddress = address1 + " " + address2;
			var postCode = FatturaElettronicaXmlValueFormatter.GetItalyPostCode(orgProxy?.MainAddress?.OA_PostCode);
			var city = orgProxy?.MainAddress?.OA_City ?? ZString.Empty;
			var state = orgProxy?.MainAddress?.OA_State;
			var country = orgProxy?.MainAddress?.OA_RN_NKCountryCode ?? ZString.Empty;

			return new XStreamingElement("AltriDatiIdentificativi",
				new XElement("Denominazione", denominazione.EnsureComplianceWithBasicLatinAndLatin1Supplement()),
				FatturaElettronicaXmlElementCreator.CreateIndirizzoType(combinedAddress.Trim(), null, postCode, city, state, country));
		}
	}
}
