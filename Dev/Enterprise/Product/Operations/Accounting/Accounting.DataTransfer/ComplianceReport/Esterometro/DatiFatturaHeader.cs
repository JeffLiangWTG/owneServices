using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro
{
	internal class DatiFatturaHeader : ComplianceReportXmlBuilder
	{
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			var ivaNumber = EsterometroDataHelper.SelectBestOrgProxyOrNull(GlbBranch.CurrentBranch, GlbCompany.CurrentCompany).IvaNumberForItaly();
			var nextSequenceNumber = GetNextSequenceNumber();
			return BuildDatiFatturaHeaderXml(ivaNumber, nextSequenceNumber);
		}

		static internal int GetNextSequenceNumber() => AccountingMasterFilesRegistry.Instance.ComplianceReportFileNextSequenceNumber.Value;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded xml node name")]
		XStreamingElement BuildDatiFatturaHeaderXml(ZString ivaNumber, int nextSequenceNumber)
		{
			var xmlCompliantIvaNumberOrNull = EnsureIvaNumberXmlCompliance(ivaNumber);
			var formattedSequenceNumber = nextSequenceNumber.ProgressiveNumberToBase36();

			return new XStreamingElement("DatiFatturaHeader",
					new XElement("ProgressivoInvio", formattedSequenceNumber),
					new XElement("Dichiarante",
							new XElement("CodiceFiscale", xmlCompliantIvaNumberOrNull),
							new XElement("Carica", "1")
					)
				);
		}

		string EnsureIvaNumberXmlCompliance(ZString ivaNumber)
		{
			var result = ivaNumber.EnsureComplianceWithBasicLatinAndLatin1Supplement();
			if (result == null)
			{
				return null;
			}

			if (result.Length > 16)
			{
				result = result.Substring(0, 16);
			}
			else if (result.Length < 11)
			{
				result = new string(' ', 11 - result.Length) + result;
			}
			return result.ToUpper(CultureInfo.InvariantCulture);     // Uppercase requirement defined in XSD
		}

		#region Unit Test Helpers
#if DEBUG
		internal XStreamingElement BuildBuildDatiFatturaHeaderXmlForUnitTest(string ivaNumber, int nextSequenceNumber) => BuildDatiFatturaHeaderXml(ivaNumber, nextSequenceNumber);
#endif
		#endregion
	}
}
