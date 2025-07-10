using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.SAFT;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT
{
	class SAFTXMLWriter1_30 : SAFTXMLWriterBase
	{
		public SAFTXMLWriter1_30(AccComplianceReport report, ReportModeAndCreditorSelector reportModeAndCreditorSelector, Action<string, int?> updateProgressStatus) : base(report, reportModeAndCreditorSelector, updateProgressStatus) { }

		public SAFTXMLWriter1_30(IEnumerable<AccComplianceReport> reports, ReportModeAndCreditorSelector reportModeAndCreditorSelector, Action<string, int?> updateProgressStatus) : base(reports, reportModeAndCreditorSelector, updateProgressStatus) { }

		protected override IEnumerable<ComplianceReportXmlBuilder> MainBodyElements => new ComplianceReportXmlBuilder[] { new Header1_30(), new MasterFiles1_30(), new GeneralLedgerEntries1_30() };

		protected override string XSDName => "Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT._1_30.SAFT1.30.xsd";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "This is not a frontend facing string")]
		protected override string NameSpace => "urn:StandardAuditFile-Taxation-Financial:NO";

		protected override Encoding FileEncoding => Encoding.UTF8;

		protected override bool IsSupportSalesInvoice => false;

		protected override void WriteTransactions(XmlWriter writer, AccComplianceReport report = null)
		{
		}

		protected override IEnumerable<XStreamingElement> BuildMultiReportsXmlCore(IEnumerable<AccComplianceReport> reports, ComplianceReportAdditionalDataCollector additionalData)
		{
			return MainBodyElements.Select(x => x.BuildAnnualXml(reports, additionalData));
		}

		protected override (IEnumerable<string> FileNames, ZStringBuilder Messages) WriteSingleReportXmlToStreamCore(Stream stream, Func<Stream> openXmlFile, string unmappedFileName)
		{
			base.WriteSingleReportXmlToStreamCore(stream, openXmlFile, unmappedFileName);
			return CompressAndCopyToRealStream(stream, openXmlFile, unmappedFileName);
		}

		protected override (IEnumerable<string> FileNames, ZStringBuilder Messages) WriteMultipleReportsXmlToStreamCore(Stream stream, Func<Stream> openXmlFile, string unmappedFileName)
		{
			base.WriteMultipleReportsXmlToStreamCore(stream, openXmlFile, unmappedFileName);
			return CompressAndCopyToRealStream(stream, openXmlFile, unmappedFileName);
		}

		(IEnumerable<string> FileName, ZStringBuilder Messages) CompressAndCopyToRealStream(Stream tempStream, Func<Stream> openXmlFile, string fileName)
		{
			tempStream.Position = 0;
			var companyCode = Report != null ? Report.Company.GC_RN_NKCountryCode : Reports.FirstOrDefault().Company.GC_RN_NKCountryCode;
			var provider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(companyCode) as IInstanceProvider<IComplianceReportGUIActionProvider>)?.Get();
			var compressionInfo = provider.GetSAFTFileCompressionInfo();
			var messages = new ZStringBuilder();
			IEnumerable<string> attachFileNames = new[] { fileName };
			var successfulMessage = Report != null
				? Res.GetString("b71b5099-e860-476d-a63b-b2574ad97156", "The SAFT XML file was generated successfully.")
				: Res.GetString("a939df3a-9334-4cdc-9486-208d9a19322a", "The Annual SAFT XML file was generated successfully.");
			messages.AppendLine(successfulMessage);

			if (compressionInfo.IsNeedCompression && tempStream.Length > compressionInfo.AllowedMaxSize)
			{
				messages.AppendLine(Res.GetString("cbbc0a8d-9d32-4658-aec8-f3adf0c984a2", "As the XML exceeded the maximum file size accepted by the Tax Authority, the XML is compressed into a ZIP file."));

				var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
				WriteFileToZippedTempFolder(fileNameWithoutExtension + ".xml", tempStream.WriteToString());
				var tempZipFile = Path.Combine(FolderName, fileNameWithoutExtension + ".zip");
				ZArchitecture.Core.ZipCompression.Zip(ZippedFolderName, tempZipFile);
				DeleteAllFilesInZippedTempFolder();
				IEnumerable<string> attatchZipFilesInTempFolder = new[] { tempZipFile };
				var zipFile = new FileInfo(tempZipFile);

				if (zipFile.Length > compressionInfo.AllowedMaxSize)
				{
					if (Report == null)
					{
						attatchZipFilesInTempFolder = WriteReportToSeparateXMLAndZip(tempZipFile);
					}
					if (Report != null || CheckSplitZipFileSize(tempZipFile, compressionInfo.AllowedMaxSize))
					{
						messages.AppendLine(Res.GetString("d70813e6-0af7-4c41-b5d3-81d724768b52", "The exported file exceed the maximum file size accepted by the Tax Authority, please raise an eRequest for assistance."));
					}
				}

				attachFileNames = CopyFileFromClientToServer(attatchZipFilesInTempFolder, Path.GetDirectoryName(fileName));

				DeleteAllFilesInTempFolder();
				DisposeTempFolder();
			}
			else
			{
				using (var fileStream = openXmlFile())
				{
					tempStream.Position = 0;
					tempStream.CopyTo(fileStream);
				}
			}

			return (attachFileNames, messages);
		}

		bool CheckSplitZipFileSize(string originalFileName, int allowedMaxSize)
		{
			var result = false;
			for (var index = 1; index <= 2; index++)
			{
				var fileInfo = new FileInfo(Path.Combine(Path.GetDirectoryName(originalFileName), Path.GetFileNameWithoutExtension(originalFileName) + $"_{index}.zip"));
				if (fileInfo.Length > allowedMaxSize)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		IEnumerable<string> WriteReportToSeparateXMLAndZip(string fileName)
		{
			var zipIndex = 1;
			var xmlFileIndex = 1;
			var xmlTotalCount = Reports.Count() + 1;
			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
			IEnumerable<string> attachFileNames = Array.Empty<string>();

			var result = WriteMasterFilesXMLToZipFile(zipIndex, xmlFileIndex, xmlTotalCount, fileName, fileNameWithoutExtension);
			attachFileNames = attachFileNames.Append(result.attachFileName);

			var generalLedgerEntriesFileNames = WriteGeneralLedgerEntriesXMLToZipFile(zipIndex, result.xmlFileIndex, xmlTotalCount, fileName, fileNameWithoutExtension);
			attachFileNames = attachFileNames.Union(generalLedgerEntriesFileNames);

			return attachFileNames;
		}

		(string attachFileName, int xmlFileIndex) WriteMasterFilesXMLToZipFile(int zipIndex, int xmlFileIndex, int xmlTotalCount, string fileName, string fileNameWithoutExtension)
		{
			var zipFileName = Path.Combine(Directory.GetParent(fileName).ToString(), fileNameWithoutExtension + $"_{zipIndex}.zip");
			var xmlName = fileNameWithoutExtension + $"_{xmlFileIndex++}_{xmlTotalCount}.xml";
			WriteReportXmlToZipFile(Reports, new ComplianceReportXmlBuilder[] { new Header1_30(), new MasterFiles1_30() }, xmlName);
			ZArchitecture.Core.ZipCompression.Zip(ZippedFolderName, zipFileName);
			DeleteAllFilesInZippedTempFolder();

			return (zipFileName, xmlFileIndex);
		}

		IEnumerable<string> WriteGeneralLedgerEntriesXMLToZipFile(int zipIndex, int xmlFileIndex, int xmlTotalCount, string fileName, string fileNameWithoutExtension)
		{
			IEnumerable<string> attachFileNames = Array.Empty<string>();
			var reportIndex = 0;
			while (zipIndex <= Reports.Count() / 2)
			{
				zipIndex++;
				var zipFileName = Path.Combine(Directory.GetParent(fileName).ToString(), fileNameWithoutExtension + $"_{zipIndex}.zip");
				var reportsInZip = Reports.Skip(reportIndex).Take(2);
				foreach (var report in reportsInZip)
				{
					AdditionalData = new SAFTAdditionalDataCollector(report, ReportMode, CreditorPK, CreditorCode);

					var name = fileNameWithoutExtension + $"_{xmlFileIndex++}_{xmlTotalCount}.xml";
					WriteReportXmlToZipFile(new AccComplianceReport[] { report }, new ComplianceReportXmlBuilder[] { new Header1_30(), new GeneralLedgerEntries1_30() }, name);
				}

				ZArchitecture.Core.ZipCompression.Zip(ZippedFolderName, zipFileName);
				DeleteAllFilesInZippedTempFolder();

				if (!attachFileNames.Contains(zipFileName))
				{
					attachFileNames = attachFileNames.Append(zipFileName);
				}

				reportIndex += 2;
			}

			return attachFileNames;
		}

		void WriteReportXmlToZipFile(IEnumerable<AccComplianceReport> reports, ComplianceReportXmlBuilder[] complianceReportXmlBuilders, string xmlFileName)
		{
			var elements = complianceReportXmlBuilders.Select(x => x.BuildAnnualXml(reports, AdditionalData));
			var element = AddTopLevelElement(elements);

			using (var mainStream = new VirtualMemoryStream())
			using (var invoicesStream = new VirtualMemoryStream())
			{
				WriteMainBody(element, mainStream);
				var zipStream = new VirtualMemoryStream();
				MergeIntoResultStream(mainStream, invoicesStream, zipStream);
				WriteFileToZippedTempFolder(xmlFileName, zipStream.WriteToString());
			}
		}

		void WriteFileToZippedTempFolder(string filename, string data)
		{
			File.AppendAllText(Path.Combine(ZippedFolderName, filename), data, System.Text.Encoding.UTF8);
		}

		void DeleteAllFilesInZippedTempFolder()
		{
			if (ZippedTempDir != null)
			{
				var directoryInfo = new DirectoryInfo(ZippedFolderName);
				foreach (var file in directoryInfo.GetFiles())
				{
					file.Delete();
				}
			}
		}

		void DeleteAllFilesInTempFolder()
		{
			DeleteAllFilesInZippedTempFolder();

			if (TempDir != null)
			{
				var directoryInfo = new DirectoryInfo(FolderName);
				foreach (var file in directoryInfo.GetFiles())
				{
					file.Delete();
				}
			}
		}

		IEnumerable<string> CopyFileFromClientToServer(IEnumerable<string> fileNames, string directoryName)
		{
			IEnumerable<string> attachmentFiles = Array.Empty<string>();
			foreach (var fileName in fileNames)
			{
				var tempStream = new VirtualMemoryStream();
				using (var clientStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					clientStream.CopyTo(tempStream);
				}
				tempStream.Position = 0;
				var targetFileName = Path.Combine(directoryName, Path.GetFileName(fileName));
				attachmentFiles = attachmentFiles.Append(targetFileName);
				using (var targetStream = ObjectFactory.Get<IFileMapper>().OpenWrite(targetFileName))
				{
					tempStream.CopyTo(targetStream);
				}
			}

			return attachmentFiles;
		}

		void DisposeTempFolder()
		{
			TempDir?.Dispose();
			TempDir = null;

			ZippedTempDir?.Dispose();
			ZippedTempDir = null;
		}

		string ZippedFolderName
		{
			get
			{
				if (ZippedTempDir == null)
				{
					ZippedTempDir = new TempDirectory();
				}
				return ZippedTempDir;
			}
		}
		TempDirectory ZippedTempDir;

		string FolderName
		{
			get
			{
				if (TempDir == null)
				{
					TempDir = new TempDirectory();
				}
				return TempDir;
			}
		}

		TempDirectory TempDir;
	}
}
