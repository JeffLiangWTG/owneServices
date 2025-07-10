using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro
{
	internal class DTR : ComplianceReportXmlBuilder
	{
		/// <param name="fileNumber">One based file number, for pagination.</param>
		/// <param name="maxTransactionsPerFile">Number of transactions, for pagination</param>
		internal DTR(int fileNumber, int maxTransactionsPerFile)
		{
			FileNumber = Math.Max(1, fileNumber);
			MaxTransactionsPerFile = Math.Max(1, maxTransactionsPerFile);
		}

		readonly int FileNumber;
		readonly int MaxTransactionsPerFile;

		internal override XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			Argument.NotNull(additionalData, nameof(additionalData));

			return new XStreamingElement("DTR",   // Hard-coded xml node name
					new CessionarioCommittenteDTR().BuildXml(report, null, additionalData),
					BuildCedentePrestatoreDTRXml(report, additionalData));   // Hard-coded xml node name
		}

		List<XStreamingElement> BuildCedentePrestatoreDTRXml(AccComplianceReport report, ComplianceReportAdditionalDataCollector additionalData)
		{
			var complianceReportLinesGroupedByTransactionPK = GetInvoiceLines(additionalData);
			var result = new List<XStreamingElement>();
			foreach (var reportLineGroup in complianceReportLinesGroupedByTransactionPK)
			{
				result.Add(new CedentePrestatoreDTR(reportLineGroup.OrderBy(x => x.ACL_ReportSequence).ToList()).BuildXml(report, null, additionalData));
			}
			return result;
		}

		IEnumerable<IGrouping<ZGuid, AccComplianceReportLine>> GetInvoiceLines(ComplianceReportAdditionalDataCollector additionalData)
		{
			var toSkip = (FileNumber - 1) * MaxTransactionsPerFile;
			return additionalData.InvoiceLines.Skip(toSkip).Take(MaxTransactionsPerFile).ToArray();
		}
	}
}
