using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M
{
	internal class Naglowek : ComplianceReportXmlBuilder
	{
		/// <summary>
		/// Builds XStreamingElement for Naglowek node
		/// </summary>
		/// <param name="report">AccComplianceReport</param>
		/// <param name="line">AccComplianceReportLineBase</param>
		/// <param name="additionalData">ComplianceReportAdditionalDataCollector</param>
		/// <returns></returns>
		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			#region SuppressResourceStringsCheckRegion

			return new XStreamingElement("Naglowek",
					getFormCodeElement(),
					new XElement("WariantFormularza", "2"),
					new XElement("DataWytworzeniaJPK", ZDateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)),
					new XElement("NazwaSystemu", BrandingFactory.Instance.ProductName),
					getPurposeOfLodgementElement(),
					new XElement("KodUrzedu", getTaxOfficeCode()),
					new XElement("Rok", report.ACR_DateFrom.Year),
					new XElement("Miesiac", report.ACR_DateFrom.Month)
				);

			XElement getFormCodeElement()
			{
				var result = new XElement("KodFormularza", "JPK_VAT");
				result.SetAttributeValue("kodSystemowy", "JPK_V7M (2)");
				result.SetAttributeValue("wersjaSchemy", "1-0E");
				return result; // <KodFormularza kodSystemowy="JPK_V7M (2)" wersjaSchemy="1-0E">JPK_VAT</KodFormularza>
			}

			XElement getPurposeOfLodgementElement()
			{
				var result = new XElement("CelZlozenia", "1");
				result.SetAttributeValue("poz", "P_7");
				return result;
			}

			string getTaxOfficeCode()
			{
				var reportCompany = report.Company;
				var polandCusCodes = reportCompany.OrgProxy.CustomsCodes.Cast<OrgCusCode>().Where(x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Poland);
				return polandCusCodes.FirstOrDefault(x => x.OK_CodeType == "GCR")?.OK_CustomsRegNo.Replace(" ", "") ?? "0000";
			}

			#endregion
		}
	}
}
