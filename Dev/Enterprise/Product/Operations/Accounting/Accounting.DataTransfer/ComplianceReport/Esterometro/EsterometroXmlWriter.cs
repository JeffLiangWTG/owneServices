using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro
{
	public class EsterometroXmlWriter : IProgressFormSupportable
	{
		public EsterometroXmlWriter(AccComplianceReport report)
			: this(report, false)
		{ }

		public EsterometroXmlWriter(AccComplianceReport report, bool indentXmlOutput)
		{
			Report = report;
			IndentXmlOutput = indentXmlOutput;
		}

		readonly AccComplianceReport Report;
		readonly bool IndentXmlOutput;

		public const int MaximumTransactionsPerFileDefinedByXSD = 1000;
		protected virtual int MaximumTransactionsPerFile => MaximumTransactionsPerFileDefinedByXSD;

		ComplianceReportAdditionalDataCollector AdditionalData => additionalData ?? (additionalData = ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(Report, ComplianceReportDataCollectionMode.Esterometro));
		ComplianceReportAdditionalDataCollector additionalData;

		#region IProgressFormSupportable

		public string CurrentStatusText { get; private set; }

		public string Log => string.Empty;

		public int CompletedItems { get; private set; }

		public int TotaItemsToComplete { get; private set; }

		public event Action<IProgressFormSupportable, bool> RaiseProgressUpdateEvent;

		void UpdateProgressStatus(string statusText)
		{
			CurrentStatusText = statusText;
			CompletedItems++;

			RaiseProgressUpdateEvent?.Invoke(this, CompletedItems == TotaItemsToComplete);
		}

		const int ProgressStepsPerFile = 6;

		#endregion

		/// <summary>
		/// Determine the number of XML files to be written, based on number of transactions in this compliance report.
		/// </summary>
		public int CalculateTotalFilesToBeWritten() => Math.Max(1, (int)Math.Ceiling(AdditionalData.InvoiceLines.Count() / (double)MaximumTransactionsPerFile));

		/// <summary>
		/// The filename required for submitting Esterometro xml, based on government spec.
		/// The same progressive number is used by the filename and ProgressivoInvio element.
		/// </summary>
		public string GetNextFilename()
		{
			var ivaNumber = EsterometroDataHelper.SelectBestOrgProxyOrNull(GlbBranch.CurrentBranch, GlbCompany.CurrentCompany).IvaNumberForItaly();
			var nextSequenceNumberFormatted = DatiFatturaHeader.GetNextSequenceNumber().ProgressiveNumberToBase36();
			return FormattableString.Invariant($"IT{ivaNumber}_DF_{nextSequenceNumberFormatted}.xml");        // Format dictated by external specification
		}

		/// <summary>
		/// Create a per-company ZGlobalMutex, which should be held while XML file(s) are generated.
		/// </summary>
		/// <returns></returns>
		public ZGlobalMutex CreateMutexForWriting() => new ZGlobalMutex(MutexIDs.ComplianceReportGeneratingXml, GlbCompany.CurrentCompany.GC_Code);

		/// <summary>
		/// Runs the ComplianceReportAdditionalDataCollector. Should only be called once for as many files as need to be processed.
		/// </summary>
		public void InitializeProgressForm()
		{
			TotaItemsToComplete = ProgressStepsPerFile * CalculateTotalFilesToBeWritten();
			if (TotaItemsToComplete > 0)
			{
				RaiseProgressUpdateEvent?.Invoke(this, CompletedItems == TotaItemsToComplete);
			}
		}

		/// <summary>
		/// Writes the report content as XML to the supplied stream.
		/// </summary>
		/// <param name="stream">Stream to write XML to.</param>
		/// <param name="fileNumber">One based number of the file to create. Used to paginate into the list of transactions.</param>
		public void WriteXmlToStream(Stream stream, int fileNumber)
		{
			var totalFileCount = CalculateTotalFilesToBeWritten();
			UpdateProgressStatus(Res.GetString("e5c9f4ab-da8f-425a-b066-b85f30a355fa", "Building Esterometro XML... (file {0} of {1})", fileNumber, totalFileCount));

			var xml = BuildXml(fileNumber);

			var settings = new XmlWriterSettings();
			settings.ConformanceLevel = ConformanceLevel.Document;
			settings.OmitXmlDeclaration = false;
			settings.NamespaceHandling = NamespaceHandling.OmitDuplicates;
			settings.Encoding = Encoding.GetEncoding("UTF-8");   // Hard-coded encoding name
			settings.Indent = IndentXmlOutput;

			using (var writer = XmlWriter.Create(stream, settings))
			{
				UpdateProgressStatus(Res.GetString("a45323ae-edff-4434-b7d7-f3c5464d7629", "Starting Esterometro XML Generation... (file {0} of {1})", fileNumber, totalFileCount));

				writer.WriteStartDocument();

				UpdateProgressStatus(Res.GetString("e0657778-2c01-433e-9a9c-959915441ef6", "Generating Esterometro XML... (file {0} of {1})", fileNumber, totalFileCount));

				xml.WriteTo(writer);

				UpdateProgressStatus(Res.GetString("23d35f0b-90a2-466d-a018-70ee54265f40", "Terminating Esterometro XML Generation... (file {0} of {1})", fileNumber, totalFileCount));

				writer.WriteEndDocument();
				writer.Flush();
			}

			UpdateProgressStatus(Res.GetString("940a7725-8b6b-4aae-aa93-186158156a96", "Esterometro XML Generation Completed (file {0} of {1})", fileNumber, totalFileCount));
		}

		/// <summary>
		/// Validates the generated XML against the DatiFattura XSD schema.
		/// </summary>
		/// <param name="stream">XML to validate.</param>
		/// <param name="notifications">Notification buffer where errors and warnings are added.</param>
		public void ValidateXml(Stream stream, INotifications notifications, int fileNumber)
		{
			var totalFileCount = CalculateTotalFilesToBeWritten();
			UpdateProgressStatus(Res.GetString("fb87b375-f9ae-4290-adea-00696dba3ee5", "Validating the Esterometro XML... (file {0} of {1})", fileNumber, totalFileCount));
			stream.Position = 0;

			var helper = new ValidationHelper(notifications);
			var settings = new XmlReaderSettings();
			settings.ValidationType = ValidationType.Schema;
			settings.ValidationEventHandler += new ValidationEventHandler(helper.ValidationEventHandler);
			settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

			// Validate XSD stored as an embedded resource
			ZXmlSchema datiFatturaSchema, xmldsigSchema;
			using (var xsdStream = typeof(EsterometroXmlWriter).Assembly.GetManifestResourceStream("Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro.DatiFattura_v2.1.xsd"))   // Hard-coded xsd file name
			{
				datiFatturaSchema = ZXmlSchema.Read(xsdStream, helper.ValidationEventHandler);
			}
			using (var xsdStream = typeof(EsterometroXmlWriter).Assembly.GetManifestResourceStream("Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro.xmldsig-core-schema.xsd"))   // Hard-coded xsd file name
			{
				xmldsigSchema = ZXmlSchema.Read(xsdStream, helper.ValidationEventHandler);
			}
			datiFatturaSchema.Includes.Add(new XmlSchemaImport() { Schema = xmldsigSchema, Namespace = "http://www.w3.org/2000/09/xmldsig#" });     // External XML namespace
			settings.Schemas.Add(datiFatturaSchema);
			settings.Schemas.Compile();

			// Validate XML using XmlReader
			using (var reader = XmlReader.Create(stream, settings))
			{
				while (reader.Read()) { }
			}
		}

		/// <summary>
		/// Increments the sequence number used within the ProgressivoInvio element, and for the file name.
		/// </summary>
		public void IncrementSequenceNumber()
		{
			var nextSequenceNumber = AccountingMasterFilesRegistry.Instance.ComplianceReportFileNextSequenceNumber.Value;
			AccountingMasterFilesRegistry.Instance.ComplianceReportFileNextSequenceNumber.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, nextSequenceNumber + 1);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Arbitrary xml namespace, Hard-coded xml attribute & value")]
		internal XStreamingElement BuildXml(int fileNumber)
		{
			XNamespace datiFatturaNamespace = "http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v2.0";
			return new XStreamingElement(datiFatturaNamespace + HeaderElementName,
					new XAttribute(XNamespace.Xmlns + "cw1dfns", datiFatturaNamespace.NamespaceName),
					new XAttribute("versione", "DAT20"),

					new DatiFatturaHeader().BuildXml(Report, null, AdditionalData),
					new DTR(fileNumber, MaximumTransactionsPerFile).BuildXml(Report, null, AdditionalData)
				);
		}

		protected virtual string HeaderElementName => "DatiFattura";   // Hard-coded xml node name
	}
}
