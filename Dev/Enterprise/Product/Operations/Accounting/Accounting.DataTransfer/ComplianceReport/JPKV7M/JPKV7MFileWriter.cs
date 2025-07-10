using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Integration;
using static System.FormattableString;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M
{
	public class JPKV7MFileWriter : ComplianceReportXmlWriter
	{
		/// <summary>
		/// Generates an XML file in Poland's JPK_V7M file format
		/// </summary>
		public JPKV7MFileWriter(AccComplianceReport report, ILogger logger)
			: base(report, ComplianceReportDataCollectionMode.JPKV7M, (msg, step) => logger.Log(LogType.Debug, msg))
		{
			Logger = Argument.NotNull(logger, nameof(logger));
		}

		ILogger Logger { get; }

		/// <summary>
		/// Writes JPK_V7M data to an XML file
		/// </summary>
		public string WriteOutputFile(TempDirectory tempDirectory)
		{
			Argument.NotNull(tempDirectory, nameof(tempDirectory));

			var fileName = Invariant($"{ZDateTime.Now:yyyy-MM-ddTHHmmss}_JPK_VAT.xml");
			var filePath = Path.Combine(tempDirectory, fileName);
			using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				WriteSingleReportXmlToStreamCore(fileStream, null, filePath);

				Logger.Log(LogType.Debug, Invariant($"Generated JPK_V7M file: {filePath}"));
			}

			return filePath;
		}

		#region Overrides

		protected override string ReportName => "JPKV7M";

		protected override string MultipleReportsName => throw new NotImplementedException();

		protected override IEnumerable<ComplianceReportXmlBuilder> MainBodyElements => new ComplianceReportXmlBuilder[] { new Naglowek(), new Podmiot1(), new Deklaracja(), new Ewidencja() };

		protected override string TopLevelElementName => "JPK";

		protected override string TransactionElementName => "";

		protected override string NameSpace => "http://crd.gov.pl/wzor/2021/12/27/11148/";

		protected override string NameSpaceAttributeXsi => "http://www.w3.org/2001/XMLSchema-instance";

		protected override string NameSpaceAttributeEtd => "http://crd.gov.pl/xml/schematy/dziedzinowe/mf/2021/06/08/eD/DefinicjeTypy/";

		protected override Encoding FileEncoding => System.Text.Encoding.UTF8;

		protected override string XSDName => "Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M.schemat.xsd";

		protected override IEnumerable<XStreamingElement> BuildMultiReportsXmlCore(IEnumerable<AccComplianceReport> reports, ComplianceReportAdditionalDataCollector additionalData)
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<XStreamingElement> BuildTransactionsXml(AccComplianceReport report, ComplianceReportAdditionalDataCollector additionalData)
		{
			return Array.Empty<XStreamingElement>();
		}

		#endregion
	}
}
