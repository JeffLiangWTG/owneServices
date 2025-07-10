using System.Linq;
using System.Xml.Linq;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M
{
	internal class Podmiot1 : ComplianceReportXmlBuilder
	{
		/// <summary>
		/// Builds XStreamingElement for Podmiot1 node
		/// </summary>
		/// <param name="report">AccComplianceReport</param>
		/// <param name="line">AccComplianceReportLineBase</param>
		/// <param name="additionalData">ComplianceReportAdditionalDataCollector</param>
		/// <returns></returns>
		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			#region SuppressResourceStringsCheckRegion

			var reportCompany = report.Company;

			var result = new XStreamingElement("Podmiot1",
					new XAttribute("rola", "Podatnik"),
					new XElement("OsobaNiefizyczna",
						new XElement("NIP", getNIP()),
						new XElement("PelnaNazwa", reportCompany.GC_Name),
						new XElement("Email", reportCompany.GC_Email),
						!reportCompany.GC_Phone.IsEmpty ? new XElement("Telefon", reportCompany.GC_Phone) : null
					)
				);

			return result;

			string getNIP()
				=> (reportCompany.GC_BusinessRegNo.IsEmpty
					? reportCompany.OrgProxy.CustomsCodes.Cast<OrgCusCode>()
						.FirstOrDefault(x => x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Poland && x.OK_CodeType == OrgCusCode.PolandCodeTypes.PTU)?.OK_CustomsRegNo ?? "0000000000"
					: reportCompany.GC_BusinessRegNo).Replace(" ", "");

			#endregion
		}
	}
}
