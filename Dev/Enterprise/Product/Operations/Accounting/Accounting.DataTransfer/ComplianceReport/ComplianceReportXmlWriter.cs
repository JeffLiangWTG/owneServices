using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.SAFT;
using Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	/// <summary>
	/// This class should be used as a base class for the new JPKV7MFileWriter as well as the SAFTXMLWriterBase
	/// </summary>
	public abstract class ComplianceReportXmlWriter
	{
		protected ComplianceReportXmlWriter(AccComplianceReport report, ComplianceReportDataCollectionMode reportMode, Action<string, int?> updateProgressStatusAction, OrgHeader organisation = null)
			: this(reportMode, updateProgressStatusAction, organisation)
		{
			Report = Argument.NotNull(report, nameof(report));
		}

		protected ComplianceReportXmlWriter(IEnumerable<AccComplianceReport> reports, ComplianceReportDataCollectionMode reportMode, Action<string, int?> updateProgressStatusAction, OrgHeader organisation = null)
			: this(reportMode, updateProgressStatusAction, organisation)
		{
			Reports = Argument.NotNull(reports, nameof(reports));
			Argument.GreaterThanZero(reports.Count(), nameof(reports) + ".Length");
		}

		protected ComplianceReportXmlWriter(ComplianceReportDataCollectionMode reportMode, Action<string, int?> updateProgressStatusAction, OrgHeader organisation = null)
		{
			ReportMode = reportMode;

			CreditorPK = organisation?.PK ?? ZGuid.Empty;
			CreditorCode = organisation?.OH_Code ?? ZString.Empty;

			UpdateProgressStatusAction = updateProgressStatusAction;
		}

		protected static ComplianceReportDataCollectionMode GetReportMode(ReportModeAndCreditorSelector reportModeAndCreditorSelector)
		{
			if (reportModeAndCreditorSelector.GenerateAllInvoices)
			{
				var isSAFTv130FeatureEnabled = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.AccountingSAFT13Report) != null;

				return isSAFTv130FeatureEnabled ?
					ComplianceReportDataCollectionMode.SAFT1_30 :
					ComplianceReportDataCollectionMode.SAFT1_10;
			}
			else
			{
				return reportModeAndCreditorSelector.GenerateSalesInvoices ?
					ComplianceReportDataCollectionMode.SAFT :
					ComplianceReportDataCollectionMode.SAFTSelfBilling;
			}
		}

		void WriteTransactionsCore(XmlWriter writer, AccComplianceReport report, ComplianceReportAdditionalDataCollector additionalData)
		{
			foreach (var transactionXml in BuildTransactionsXml(report, additionalData))
			{
				transactionXml.WriteTo(writer);
			}
		}

		protected AccComplianceReport Report { get; }

		protected IEnumerable<AccComplianceReport> Reports { get; }

		protected ComplianceReportDataCollectionMode ReportMode { get; }

		protected ZGuid CreditorPK { get; }

		protected ZString CreditorCode { get; }

		protected ComplianceReportAdditionalDataCollector AdditionalData;

		protected abstract string ReportName { get; }

		protected abstract string MultipleReportsName { get; }

		protected abstract string XSDName { get; }

		protected abstract string TopLevelElementName { get; }

		protected abstract string TransactionElementName { get; }

		protected abstract string NameSpace { get; }

		protected virtual string NameSpaceAttributeEtd => null;

		protected abstract string NameSpaceAttributeXsi { get; }

		protected virtual Encoding FileEncoding => System.Text.Encoding.GetEncoding("Windows-1252");

		protected abstract IEnumerable<ComplianceReportXmlBuilder> MainBodyElements { get; }

		#region Building XStreamingElements

		protected XStreamingElement BuildXml(AccComplianceReport report, AccComplianceReportLineBase line = null, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			var elements = MainBodyElements.Select(x => x.BuildXml(report, null, additionalData));
			return AddTopLevelElement(elements);
		}

		protected XStreamingElement BuildMultiReportsXml(IEnumerable<AccComplianceReport> reports, ComplianceReportAdditionalDataCollector additionalData = null)
		{
			var elements = BuildMultiReportsXmlCore(reports, additionalData);
			return AddTopLevelElement(elements);
		}

		protected virtual XStreamingElement AddTopLevelElement(IEnumerable<XStreamingElement> elements) => new XStreamingElement(TopLevelElementName, elements);

		#endregion

		protected abstract IEnumerable<XStreamingElement> BuildTransactionsXml(AccComplianceReport report, ComplianceReportAdditionalDataCollector additionalData);

		protected abstract IEnumerable<XStreamingElement> BuildMultiReportsXmlCore(IEnumerable<AccComplianceReport> reports, ComplianceReportAdditionalDataCollector additionalData);

		protected virtual (IEnumerable<string> FileNames, ZStringBuilder Messages) WriteSingleReportXmlToStreamCore(Stream stream, Func<Stream> openXmlFile, string unmappedFileName)
		{
			const int progressStepsForSavingSingleReport = 12;
			UpdateProgressStatus(Res.GetString("0db4a6aa-eb27-43ee-88d2-e34080c902e3", "Loading Compliance Report Data..."), progressStepsForSavingSingleReport + 1);

			AdditionalData = ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(Report, ReportMode, CreditorPK, CreditorCode);

			UpdateProgressStatus(Res.GetString("3f35b228-8d69-48d0-b558-0b0daeddab1a", "Building {0} XML...", ReportName));

			var xml = BuildXml(Report, null, AdditionalData);

			using (var mainStream = new VirtualMemoryStream())
			using (var transactionsStream = new VirtualMemoryStream())
			using (var transactionsWriter = GetPartialWriter(transactionsStream))
			{
				WriteMainBody(xml, mainStream);
				WriteTransactions(transactionsWriter);

				MergeIntoResultStream(mainStream, transactionsStream, stream);
			}

			var compressionInfo = GetCompressionInfo();
			if ((!compressionInfo.IsNeedCompression || stream.Length <= compressionInfo.AllowedMaxSize)
				&& openXmlFile != null)
			{
				using (var fileStream = openXmlFile())
				{
					stream.Position = 0;
					stream.CopyTo(fileStream);
				}
			}

			UpdateProgressStatus(Res.GetString("0e39aff9-c359-4b9e-9011-ccb92af4151b", "{0} XML File Generation Completed.", ReportName));

			var messages = new ZStringBuilder();
			messages.AppendLine(Res.GetString("1f90e02e-9614-4a30-a9ce-97805449a21c", "The {0} XML file was generated successfully.", ReportName));

			return (new[] { unmappedFileName }, messages);
		}

		protected virtual (IEnumerable<string> FileNames, ZStringBuilder Messages) WriteMultipleReportsXmlToStreamCore(Stream stream, Func<Stream> openXmlFile, string unmappedFileName)
		{
			const int progressStepsPerEveryReport = 3;
			const int progressStepsForCommonPartOfAnnualReport = 10;
			UpdateProgressStatus(Res.GetString("c6fbde8c-27b6-430d-93ab-2d263f7654c1", "Generating {0} XML...", MultipleReportsName), Reports.Count() * progressStepsPerEveryReport + progressStepsForCommonPartOfAnnualReport + 1);
			AdditionalData = null;

			using (var invoicesStream = new VirtualMemoryStream())
			{
				using (var invoiceWriter = GetPartialWriter(invoicesStream))
				{
					foreach (var report in Reports)
					{
						UpdateProgressStatus(Res.GetString("744137fe-dce1-4350-ba28-c81df23508ed", "Loading Compliance Report Data..."));

						if (AdditionalData == null)
						{
							AdditionalData = ComplianceReportAdditionalDataCollectorFactory.GetComplianceReportAdditionalDataCollector(report, ReportMode, CreditorPK, CreditorCode);
						}
						else
						{
							AdditionalData.MergeDataFromAnotherReport(report);
						}

						WriteTransactions(invoiceWriter, report ?? Report);
					}
				}

				UpdateProgressStatus(Res.GetString("ab3dd561-e994-4b7a-a863-dafdc6f0820b", "Building {0} XML...", ReportName));

				AdditionalData.SetOrgBalance(Reports.ToArray());

				var xml = BuildMultiReportsXml(Reports, AdditionalData);

				using (var bodyStream = new VirtualMemoryStream())
				{
					WriteMainBody(xml, bodyStream);
					MergeIntoResultStream(bodyStream, invoicesStream, stream);
				}
			}

			var compressionInfo = GetCompressionInfo();
			if (!compressionInfo.IsNeedCompression || stream.Length <= compressionInfo.AllowedMaxSize)
			{
				using (var fileStream = openXmlFile())
				{
					stream.Position = 0;
					stream.CopyTo(fileStream);
				}
			}

			UpdateProgressStatus(Res.GetString("7f5db534-265b-47bb-83dc-7564f06bf4c8", "{0} XML File Generation Completed.", ReportName));

			var messages = new ZStringBuilder();
			messages.AppendLine(Res.GetString("c5372a59-01ef-4036-a8b8-73640cda7d13", "The {0} XML file was generated successfully.", MultipleReportsName));

			return (new[] { unmappedFileName }, messages);
		}

		protected void WriteMainBody(XStreamingElement xml, Stream stream)
		{
			var settings = new XmlWriterSettings();
			settings.ConformanceLevel = ConformanceLevel.Document;
			settings.NamespaceHandling = NamespaceHandling.OmitDuplicates;
			settings.Encoding = FileEncoding;

			using (var writer = XmlWriter.Create(stream, settings))
			{
				UpdateProgressStatus(Res.GetString("22fb7a64-5435-4bc6-a6fd-420018100ea8", "Starting generation main part of {0} XML...", ReportName));
				writer.WriteStartDocument();

				UpdateProgressStatus(Res.GetString("60025081-2d93-49c0-bfd4-178868b1c1b8", "Generating main part {0} XML...", ReportName));
				xml.WriteTo(writer);

				UpdateProgressStatus(Res.GetString("301e7995-7abc-4ca5-8af8-884056f731c9", "Completing generation main part of {0} XML...", ReportName));
				writer.WriteEndDocument();
				writer.Flush();
			}
		}

		protected virtual void WriteTransactions(XmlWriter writer, AccComplianceReport report = null)
		{
			UpdateProgressStatus(Res.GetString("80842e0e-e02d-45a3-8668-961e93f80a93", "Generating Transactions part of {0} XML...", ReportName));
			WriteTransactionsCore(writer, report ?? Report, AdditionalData);
			writer.Flush();
			UpdateProgressStatus(Res.GetString("38b3f83b-9d12-4783-b6ed-cb029cfd70dc", "Completed generation Transactions part of {0} XML...", ReportName));
		}

		protected void MergeIntoResultStream(Stream mainStream, Stream transactionsStream, Stream resultStream)
		{
			var writerSettings = new XmlWriterSettings();
			writerSettings.ConformanceLevel = ConformanceLevel.Fragment;
			writerSettings.NamespaceHandling = NamespaceHandling.OmitDuplicates;
			writerSettings.Encoding = FileEncoding;

			var readerSettings = new XmlReaderSettings();
			readerSettings.ConformanceLevel = ConformanceLevel.Document;

			using (var resultWriter = XmlWriter.Create(resultStream, writerSettings))
			{
				var processingStatus = Res.GetString("a545e92d-f2d5-457c-bfe2-22ae293bc839", "Merging combined {0} XML file...", ReportName);
				UpdateProgressStatus(processingStatus);
				mainStream.Position = 0;

				using (var mainReader = XmlReader.Create(new StreamReader(mainStream, FileEncoding), readerSettings))
				{
					while (mainReader.Read())
					{
						switch (mainReader.NodeType)
						{
							case XmlNodeType.Element:
								if (mainReader.Name == TopLevelElementName)
								{
									resultWriter.WriteStartElement(mainReader.Name, NameSpace);
									if (!string.IsNullOrEmpty(NameSpaceAttributeEtd))
									{
										resultWriter.WriteAttributeString("xmlns", "etd", null, NameSpaceAttributeEtd);
									}
									resultWriter.WriteAttributeString("xmlns", "xsi", null, NameSpaceAttributeXsi);
									break;
								}
								if (mainReader.Name == TransactionElementName && mainReader.IsEmptyElement)
								{
									UpdateProgressStatus(Res.GetString("f12a2ee4-b644-4b14-b1cf-56fc11ab5c4b", "Merging Transactions into combined {0} XML file...", ReportName));
									CopyElementsFromPartialStream(transactionsStream, resultWriter);
									UpdateProgressStatus(processingStatus);
									break;
								}

								resultWriter.WriteStartElement(mainReader.Name);
								if (mainReader.HasAttributes)
								{
									resultWriter.WriteAttributes(mainReader, true);
								}
								if (mainReader.IsEmptyElement)
								{
									resultWriter.WriteEndElement();
								}
								break;
							case XmlNodeType.Text:
								resultWriter.WriteString(mainReader.Value);
								break;
							case XmlNodeType.XmlDeclaration:
							case XmlNodeType.ProcessingInstruction:
								resultWriter.WriteProcessingInstruction(mainReader.Name, mainReader.Value);
								break;
							case XmlNodeType.Comment:
								resultWriter.WriteComment(mainReader.Value);
								break;
							case XmlNodeType.EndElement:
								resultWriter.WriteFullEndElement();
								break;
							default:
								var message = FormattableString.Invariant($"Unsupported XmlNodeType: {mainReader.NodeType}");   // Developer error message
								ExceptionReporter.Instance.ReportDeveloperException(mainReader.NodeType.ToString(), message, new ArgumentException(message));
								break;
						}
					}
				}
				resultWriter.Flush();
				UpdateProgressStatus(Res.GetString("95b57c9c-768f-40ad-8377-27b91658ce79", "Completed merging combined {0} XML file...", ReportName));
			}
		}

		void CopyElementsFromPartialStream(Stream partialStream, XmlWriter writer)
		{
			var partialReaderSettings = new XmlReaderSettings();
			partialReaderSettings.ConformanceLevel = ConformanceLevel.Fragment;
			XNamespace ns = NameSpace;

			partialStream.Position = 0;

			using (var reader = XmlReader.Create(new StreamReader(partialStream, FileEncoding), partialReaderSettings))
			{
				reader.Read();
				while (!reader.EOF)
				{
					var element = XElement.ReadFrom(reader) as XElement;
					foreach (var e in element.DescendantsAndSelf())
					{
						e.Name = ns + e.Name.LocalName;
					}
					element.WriteTo(writer);
				}
			}
		}

		XmlWriter GetPartialWriter(Stream stream)
		{
			var partialWriterSettings = new XmlWriterSettings();
			partialWriterSettings.ConformanceLevel = ConformanceLevel.Fragment;
			partialWriterSettings.NamespaceHandling = NamespaceHandling.OmitDuplicates;
			partialWriterSettings.Encoding = FileEncoding;

			return XmlWriter.Create(stream, partialWriterSettings);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1092:DoNotUseXmlSchema", Justification = "Baseline")]
		protected void ValidateXml(Stream stream, INotifications notifications)
		{
			UpdateProgressStatus(Res.GetString("76521bf5-beee-4a77-a344-c1edcc51ce28", "Validating the {0} XML generated file...", ReportName));
			stream.Position = 0;

			var helper = new ValidationHelper(notifications);
			var settings = new XmlReaderSettings();
			settings.ValidationType = System.Xml.ValidationType.Schema;
			settings.ValidationEventHandler += new ValidationEventHandler(helper.ValidationEventHandler);

			// Validate XSD stored as an embedded resource
			using (var xsdStream = GetType().Assembly.GetManifestResourceStream(XSDName))
			{
				settings.Schemas.Add(ZXmlSchema.Read(xsdStream, helper.ValidationEventHandler));
			}

			// Validate XML using XmlReader
			using (var reader = XmlReader.Create(new StreamReader(stream, FileEncoding), settings))
			{
				while (reader.Read())
				{ }
			}
		}

		#region Progress/Logging

		readonly Action<string, int?> UpdateProgressStatusAction;

		void UpdateProgressStatus(string statusText, int? itemsToComplete = null) => UpdateProgressStatusAction?.Invoke(statusText, itemsToComplete);

		#endregion

		#region Output Compression

		protected virtual (bool IsNeedCompression, int AllowedMaxSize) GetCompressionInfo() => (false, 0);

		#endregion
	}
}
