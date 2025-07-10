using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.IO;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentVisualizer.Build;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ExcelTemplates.ExcelTemplateReadFromExcelTemplatesSolution;

#region SuppressResourceStringsCheckRegion

namespace Enterprise.DocumentEngine.Build
{
	public static class TemplateSerializationHelper
	{
		static ConcurrentBag<string> reportSheetNames;
		static ConcurrentBag<string> reportTitlesInConfigArea;

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Out parameter required by design")]
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
		public static bool Execute(string enterpriseBaseDirectory, string documentXmlPath, out List<LogMessage> messages)
		{
			ZrsFile.ZrsFileDirectoryLocator = AssemblyLoader.GetBinPath;
			var hadErrors = false;
			messages = new List<LogMessage>();

			var dataSet = new DataSet();

			try
			{
				dataSet.ReadXml(enterpriseBaseDirectory + documentXmlPath, XmlReadMode.ReadSchema);

				var stmTemplate = dataSet.Tables[StmTemplateSchema.Constants.TableName];

				var reportMenuPKs = new HashSet<Guid>(dataSet.Tables[StmMenuItemSchema.Constants.TableName].Select(string.Format(CultureInfo.InvariantCulture, "{0} like 'REP%'", StmMenuItemSchema.Constants.SU_BusinessContext))
					.Select(row => (Guid)row[StmMenuItemSchema.Constants.PK]));

				var reportTemplatePKs = new HashSet<Guid>();
				foreach (var reportTemplatePk in dataSet.Tables[StmMenuTemplatePivotSchema.Constants.TableName].Select().Where(row => reportMenuPKs.Contains((Guid)row[StmMenuTemplatePivotSchema.Constants.SI_SU]))
					.Select(row => (Guid)row[StmMenuTemplatePivotSchema.Constants.SI_SO]))
				{
					reportTemplatePKs.Add(reportTemplatePk);
				}

				var clientSpecificTemplates = new Dictionary<string, byte[]>();

				var templatePath = "";
				foreach (DataRow row in stmTemplate.Rows)
				{
					var excelTemplateName = row[StmTemplateSchema.SO_ExcelTemplatePath.Name].ToString();
					var templateName = row[StmTemplateSchema.SO_Name.Name].ToString();

					var isLegacyDocument = !(templateName == Enterprise.Core.Constants.SectionRepositoryTemplateNames.System || reportTemplatePKs.Contains((Guid)row[StmTemplateSchema.Constants.PK]));
					if (isLegacyDocument)
					{
						var fileInfo = Compressor.Uncompress(row[StmTemplateSchema.Constants.SO_Template] as byte[]);
						clientSpecificTemplates.Add(excelTemplateName, fileInfo);

						if (string.IsNullOrEmpty(templatePath))
						{
							templatePath = excelTemplateName;
						}
					}
				}

				messages.Add(new LogMessage('M', "Analyzing Legacy Document Label Resource Strings..."));

				var legacyDocumentLabelAsmid = GetAsmidByTemplatePath(templatePath);
				AnalyzeLegacyTemplatesResourceStrings(new LegacyDocumentTemplateTranslationHelper(), clientSpecificTemplates, legacyDocumentLabelAsmid);
			}
			catch (Exception ex)
			{
				messages.Add(new LogMessage('E', ex.ToString()));
				hadErrors = true;
			}

			return !hadErrors;
		}

		internal static ushort GetAsmidByTemplatePath(string templatePath)
		{
			var regex = new Regex(@"\\Documents\\(?<ClientCode>[a-z0-9]{3})\\", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			var match = regex.Match(templatePath);
			var clientCode = match.Success ? match.Groups["ClientCode"].Value.ToUpper() : "";
			switch (clientCode)
			{
				case "EDI":
					return DocBuilderResourceStrings.EDILegacyDocumentLabelAsmid;
				default:
					return DocBuilderResourceStrings.LegacyDocumentLabelAsmid;
			}
		}

		public static bool SystemRefDocTypeHasErrors(string xmlFilePath, out string messages)
		{
			var hasErrors = false;
			IEnumerable<XElement> allRefDocType;
			var newLine = System.Environment.NewLine;

			messages = newLine;
			var docTypeWhiteList = new List<string>
			{
					"TSC", "POF", "POC", "LDL", "TSR", "EXA", "RQS", "CAR", "IRT", "SAN", "WSH", "CSD",
					"DOR", "PRV", "GLJ", "WTE", "BDR", "CUS", "DAN", "AGI", "POA", "CUP", "PPO", "LID",
					"CDA", "TAD", "COM", "REQ", "HBL", "JRJ", "MAD", "MUL", "PER", "MBL", "SOA", "WPO",
					"WWO", "CLI", "MAN", "BKC", "PPL", "CAT", "WIN", "CHG", "WPB", "NOT", "ATD", "ECA",
					"WIC", "MCD", "KCA", "COR", "WSV", "GTP", "PAL", "FUM", "VGM", "CAD", "CER", "LBL",
					"MDC", "DOO", "PIN", "DEM", "POD", "ECM", "SLI", "WNP", "WSS", "BCT", "CST", "WDL",
					"HCC", "RSB", "OBL", "CRR", "QPP", "ECR", "DEC", "1RM", "BOD", "COO", "BRC", "VET",
					"CLL", "CDR", "PRS", "PAY", "WPL", "HXD", "2RM", "CSS", "COL", "ERA", "PKD", "SCR",
					"INV", "ECD", "COA", "RRA", "EMN", "MOD", "DGF", "IEP", "UMR", "ILR", "WBL", "EXM",
					"BOE", "SDN", "DCF", "QPK", "LET", "DRC", "WPA", "CLS", "WPU", "TDM", "QRA", "SEC",
					"SAD", "EFT", "WAC", "ICA", "CIV", "BKG", "ICT", "ARN", "PMP", "DNO", "WMR", "QRP",
					"MVC", "DDR", "BCA", "LCO", "ACV", "ARE", "QRC", "PUB", "INS", "WTR", "EPR", "HAR",
					"CAU", "RES", "EXV", "PKL", "FDC", "BOA", "CLR", "DBL", "ICM", "COT", "CTR", "AIN",
					"OUT", "T2F", "FWI", "MSH", "WCA", "ARC", "SHI", "SHO", "ICR", "WSI", "WID", "DIS",
					"DLB", "MSD", "DAL", "EAD", "CRP", "PSB", "MFD", "FCR", "MSC", "WPS", "MCF", "PMR",
					"NAF", "EXD", "QUO", "SEN", "IDC"
			}; //DO NOT add any new elements to the list.

			try
			{
				allRefDocType = XElement.Load(xmlFilePath).Elements("RefDocType");
			}
			catch (Exception ex)
			{
				messages = ex.ToString();
				return true;
			}

			#region CheckDuplication
			var repeatRefDocType = allRefDocType.GroupBy(node => new { docType = node.Element("RT_DocType")?.Value, referenceType = node.Element("RT_ReferenceType")?.Value }).Where(g => g.Count() > 1);
			if (repeatRefDocType.Any())
			{
				var duplicateDocType = repeatRefDocType.First().Key;
				var repeatMessage = $"Duplicate RefDocType records: (RT_ReferenceType: {duplicateDocType.referenceType}, RT_DocType: {duplicateDocType.docType}){newLine}";
				messages += repeatMessage;
				hasErrors = true;
			}
			#endregion

			#region CheckDocType
			var nowPK = string.Empty;
			var nowDocType = string.Empty;
			var nullDocType = new List<string>();
			var notPassSysDocType = new List<string>();
			var notPassCusDocType = new List<string>();
			foreach (var node in allRefDocType)
			{
				nowPK = node.Element("RT_PK").Value;
				nowDocType = node.Element("RT_DocType").Value;
				bool.TryParse(node.Element("RT_IsSystem").Value, out var nowIsSystem);

				if (nowDocType.IsNullOrEmpty())
				{
					nullDocType.Add(nowPK);
					continue;
				}
				if (nowIsSystem)
				{
					if (!docTypeWhiteList.Contains(nowDocType) && (nowDocType.Length != 4 || !nowDocType.StartsWith("S")))
					{
						notPassSysDocType.Add(nowPK);
					}
				}
				else
				{
					notPassCusDocType.Add(nowPK);
				}
			}
			#endregion

			if (nullDocType.Count != 0)
			{
				messages += $"RT_DocType can not be null. PK: {string.Join(", ", nullDocType)}{newLine}";
				hasErrors = true;
			}
			if (notPassCusDocType.Count != 0)
			{
				messages += $"RT_IsSystem can not be false. PK: {string.Join(", ", notPassCusDocType)}{newLine}";
				hasErrors = true;
			}
			if (notPassSysDocType.Count != 0)
			{
				messages += $"System-defined doc type must be 4 characters and begin with 'S'. PK: {string.Join(", ", notPassSysDocType)}{newLine}";
				hasErrors = true;
			}

			if (hasErrors)
			{
				messages += "(Please check the changes in Documents.xml and see WI00332700 for details)";
			}
			return hasErrors;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static bool Execute(string enterpriseBaseDirectory, string documentsXmlPath, string documentsWithBlobsXmlPath, out List<LogMessage> messages)
		{
			ZrsFile.ZrsFileDirectoryLocator = AssemblyLoader.GetBinPath;
			bool hadErrors = false;
			messages = new List<LogMessage>();

			messages.Add(new LogMessage('M', string.Format("Serializing templates from ExcelTemplates solution into {0}{1}...", enterpriseBaseDirectory, documentsWithBlobsXmlPath)));

			DataSet dataSet = new DataSet();
			dataSet.ReadXml(enterpriseBaseDirectory + documentsXmlPath, XmlReadMode.ReadSchema);
			DataTable stmTemplate = dataSet.Tables[StmTemplateSchema.Constants.TableName];

			var files = new Dictionary<Guid, string>();
			foreach (DataRow row in stmTemplate.Rows)
			{
				string excelTemplateName = (string)row[StmTemplateSchema.SO_ExcelTemplatePath.Name];
				if (!string.IsNullOrEmpty(excelTemplateName))
				{
					string fileName = enterpriseBaseDirectory + excelTemplateName;
					FileInfo fileInfo = new FileInfo(fileName);
					if (fileInfo.Exists)
					{
						try
						{
							files.Add((Guid)row[StmTemplateSchema.PK.Name], fileName);
						}
						catch (ArgumentException)
						{
							messages.Add(new LogMessage('W', String.Format("Error adding template {0} -- duplicate path.", excelTemplateName)));
							hadErrors = true;
						}
					}
					else
					{
						messages.Add(new LogMessage('W', String.Format("File {0} does not exist.", fileName)));
						//						hadErrors = true;
					}
				}
				else
				{
					messages.Add(new LogMessage('W', String.Format("No path was specified for template {0}.", (string)row[StmTemplateSchema.SO_Name.Name])));
					//					hadErrors = true;
				}
			}

			if (hadErrors = SystemRefDocTypeHasErrors(enterpriseBaseDirectory + documentsXmlPath, out var errorMessage))
			{
				messages.Add(new LogMessage('E', errorMessage));
			}

			var legacyDocumentTemplatesNeedTranslate = new HashSet<Guid>();

			if (!hadErrors)
			{
				try
				{
					byte[] template;

					var legacyDocumentTemplates = new Dictionary<string, byte[]>();
					var coverSheetTemplates = new Dictionary<string, byte[]>();

					var reportTemplates = new Dictionary<string, byte[]>();
					var reportMenuPKs = new HashSet<Guid>(dataSet.Tables[StmMenuItemSchema.Constants.TableName].Select($"{StmMenuItemSchema.Constants.SU_BusinessContext} like 'REP%'")
						.Select(row => (Guid)row[StmMenuItemSchema.Constants.PK]));

					var reportTemplatePKs = new HashSet<Guid>();
					foreach (var reportTemplatePk in dataSet.Tables[StmMenuTemplatePivotSchema.Constants.TableName].Select().Where(row => reportMenuPKs.Contains((Guid)row[StmMenuTemplatePivotSchema.Constants.SI_SU]))
						.Select(row => (Guid)row[StmMenuTemplatePivotSchema.Constants.SI_SO]))
					{
						reportTemplatePKs.Add(reportTemplatePk);
					}

					var formBuilderTemplates = new Dictionary<Guid, byte[]>();
					var legacyDocumentTranslationHelper = new LegacyDocumentTemplateTranslationHelper();

					foreach (DataRow row in stmTemplate.Rows)
					{
						var excelTemplateName = (string)row[StmTemplateSchema.SO_ExcelTemplatePath.Name];
						var pK = (Guid)row[StmTemplateSchema.PK.Name];

						if (!files.ContainsKey(pK))
						{
							continue;
						}

						messages.Add(new LogMessage('M', String.Format("Serializing {0}...", files[pK])));
						template = GetFileContent(files[pK]);
						row[StmTemplateSchema.SO_Template.Name] = Compressor.Compress(template);

						var templateName = row[StmTemplateSchema.SO_Name.Name].ToString();
						var templateType = row[StmTemplateSchema.SO_TemplateType.Name].ToString();

						if (templateName == Enterprise.Core.Constants.SectionRepositoryTemplateNames.System)
						{
							messages.Add(new LogMessage('M', "Analyzing DocLabel Resource Strings..."));
							ZrsFile.Save(ZrsFile.GetDefaultFilePath(Res.DefaultLanguage, string.Empty), DocBuilderResourceStrings.DocLabelAsmid, Validate(Array.ConvertAll(new DocBuilderTemplateTranslationHelper().GetUniqueLabels(null, excelTemplateName, template), item => item.Data)));
						}
						else if (reportTemplatePKs.Contains(pK))
						{
							reportTemplates.Add(excelTemplateName, template);
						}
						else if (string.CompareOrdinal(templateType, Core.Constants.StmMenuItemTypes.Forms) == 0)
						{
							formBuilderTemplates.Add(pK, template);
						}
						else
						{
							legacyDocumentTemplates.Add(excelTemplateName, template);

							if (legacyDocumentTranslationHelper.NeedTranslate(excelTemplateName, template))
							{
								legacyDocumentTemplatesNeedTranslate.Add((Guid)row[StmTemplateSchema.PK.Name]);
							}
						}
					}

					var coverSheetNames = new string[] { TemplateNames.EmailCoverSheet, TemplateNames.FaxCoverSheet, TemplateNames.PrintCoverSheet };
					coverSheetNames.Select(name => Path.Combine(MenuCustomisation.TemplatesDirectory, name + Extension))
						.ForEach(path => coverSheetTemplates.Add(path, GetFileContent(enterpriseBaseDirectory + path)));

					#region Form Builder

					messages.Add(new LogMessage('M', "Analyzing Form Builder Label Resource Strings..."));
					AnalyzeFormBuilderTemplatesResourceStrings(formBuilderTemplates);

					#endregion

					#region Legacy Document

					messages.Add(new LogMessage('M', "Analyzing Legacy Document Label Resource Strings..."));
					AnalyzeLegacyTemplatesResourceStrings(legacyDocumentTranslationHelper, legacyDocumentTemplates, DocBuilderResourceStrings.LegacyDocumentLabelAsmid);

					#endregion

					#region Cover Sheet

					messages.Add(new LogMessage('M', "Analyzing Cover Sheet Label Resource Strings..."));
					AnalyzeCoverSheetTemplatesResourceStrings(coverSheetTemplates);

					#endregion

					#region Report

					messages.Add(new LogMessage('M', "Analyzing Report Label Resource Strings..."));
					AnalyzeReportTemplatesResourceStrings(reportTemplates);

					#endregion
				}
				catch (Exception ex)
				{
					messages.Add(new LogMessage('E', ex.ToString()));
					hadErrors = true;
				}
			}

			string documentsWithBlobsXmlFullPath = enterpriseBaseDirectory + documentsWithBlobsXmlPath;
			if (File.Exists(documentsWithBlobsXmlFullPath))
			{
				File.SetAttributes(documentsWithBlobsXmlFullPath, FileAttributes.Normal);
				File.Delete(documentsWithBlobsXmlFullPath);
			}

			try
			{
				dataSet.WriteXml(documentsWithBlobsXmlFullPath, XmlWriteMode.WriteSchema);
			}
			catch (Exception ex)
			{
				messages.Add(new LogMessage('E', ex.ToString()));
			}

			messages.Add(new LogMessage('M', "Analyzing Report Title Resource Strings..."));
			try
			{
				AnalyzeReportTitleResourceStrings(dataSet, legacyDocumentTemplatesNeedTranslate);
			}
			catch (Exception ex)
			{
				messages.Add(new LogMessage('E', ex.ToString()));
				hadErrors = true;
			}

			messages.Add(new LogMessage('M', "Finished serializing templates."));

			return !hadErrors;
		}

		static byte[] GetFileContent(string path)
		{
			using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				var fileSize = fileStream.Length;
				if (fileSize > int.MaxValue)
				{
					throw new OverflowException("File is too large");
				}

				var template = new byte[(int)fileSize];
				if (fileStream.Read(template, 0, (int)fileSize) != (int)fileSize)
				{
					throw new IOException(String.Format("Error reading from {0}.", path));
				}
				return template;
			}
		}

		static void AnalyzeFormBuilderTemplatesResourceStrings(IReadOnlyDictionary<Guid, byte[]> formBuilderTemplates)
		{
			if (formBuilderTemplates.Count == 0)
			{
				return;
			}

			var filePath = ZrsFile.GetDefaultFilePath(Res.DefaultLanguage, string.Empty);
			var analyzer = new ResourceStringAnalyzer();
			var resourceStrings = analyzer.ExtractResStringData(formBuilderTemplates.Values.ToArray());

			if (resourceStrings.Count > 0)
			{
				ZrsFile.Save(filePath, analyzer.Asmid, resourceStrings);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule", Justification = "ConcurrentBag is only converted to array once threads have completed")]
		static void AnalyzeLegacyTemplatesResourceStrings(LegacyDocumentTemplateTranslationHelper legacyDocumentTranslationHelper, Dictionary<string, byte[]> legacyDocumentTemplates, UInt16 legacyDocumentLabelAsmid)
		{
			var legacyDocumentContentResourceString = new ConcurrentBag<ResourceStringData>();

			Parallel.ForEach(legacyDocumentTemplates,
				legacyDocumentTemplate =>
				{
					var resourceStrings = Array.ConvertAll(legacyDocumentTranslationHelper.GetUniqueLabels(null, new Dictionary<string, byte[]>() { { legacyDocumentTemplate.Key, legacyDocumentTemplate.Value } }), item => item.Data);
					foreach (var resourceString in resourceStrings)
					{
						legacyDocumentContentResourceString.Add(resourceString);
					}
				}
			);

			reportSheetNames = legacyDocumentTranslationHelper.SheetNames;
			var filePath = ZrsFile.GetDefaultFilePath(Res.DefaultLanguage, string.Empty);
			ZrsFile.Save(filePath, legacyDocumentLabelAsmid, Validate(legacyDocumentContentResourceString.ToArray()));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule", Justification = "ConcurrentBag is only converted to array once threads have completed")]
		static void AnalyzeCoverSheetTemplatesResourceStrings(Dictionary<string, byte[]> coverSheetTemplates)
		{
			var coverSheetTranslationHelper = new CoverSheetTemplateTranslationHelper();
			var coverSheetContentResourceString = new ConcurrentBag<ResourceStringData>();

			Parallel.ForEach(coverSheetTemplates,
				coverSheetTemplate =>
				{
					var resourceStrings = Array.ConvertAll(coverSheetTranslationHelper.GetUniqueLabels(null, new Dictionary<string, byte[]>() { { coverSheetTemplate.Key, coverSheetTemplate.Value } }), item => item.Data);
					foreach (var resourceString in resourceStrings)
					{
						coverSheetContentResourceString.Add(resourceString);
					}
				}
			);

			coverSheetTranslationHelper.SheetNames.ForEach(x => reportSheetNames.Add(x));

			var filePath = ZrsFile.GetDefaultFilePath(Res.DefaultLanguage, string.Empty);
			ZrsFile.Save(filePath, DocBuilderResourceStrings.CoverSheetLabelAsmid, Validate(coverSheetContentResourceString.ToArray()));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule", Justification = "ConcurrentBag is only converted to array once threads have completed")]
		static void AnalyzeReportTemplatesResourceStrings(Dictionary<string, byte[]> reportTemplates)
		{
			var reportTranslationHelper = new ReportTemplateTranslationHelper();
			var reportContentResourceString = new ConcurrentBag<ResourceStringData>();

			Parallel.ForEach(reportTemplates,
				reportTemplate =>
				{
					var resourceStrings = Array.ConvertAll(reportTranslationHelper.GetUniqueLabels(null, new Dictionary<string, byte[]>() { { reportTemplate.Key, reportTemplate.Value } }), item => item.Data);
					foreach (var resourceString in resourceStrings)
					{
						reportContentResourceString.Add(resourceString);
					}
				}
			);

			reportTranslationHelper.SheetNames.ForEach(x => reportSheetNames.Add(x));
			reportTitlesInConfigArea = reportTranslationHelper.ReportTitles;

			ZrsFile.Save(ZrsFile.GetDefaultFilePath(Res.DefaultLanguage, string.Empty), DocBuilderResourceStrings.ReportLabelAsmid, Validate(reportContentResourceString.ToArray()));
		}

		static void AnalyzeReportTitleResourceStrings(DataSet dataSet, HashSet<Guid> legacyDocumentTemplatesNeedTranslate)
		{
			var pivots = new HashSet<Guid>();
			foreach (DataRow row in dataSet.Tables[StmMenuDocumentConfigSchema.Constants.TableName].Rows)
			{
				pivots.Add((Guid)row[StmMenuDocumentConfigSchema.S3_SI.Name]);
			}

			var reportMenuPKs = new HashSet<Guid>(dataSet.Tables[StmMenuItemSchema.Constants.TableName].Select(string.Format(CultureInfo.InvariantCulture, "{0} like 'REP%'", StmMenuItemSchema.Constants.SU_BusinessContext))
				.Select(row => (Guid)row[StmMenuItemSchema.Constants.PK]));

			var reportNames = new HashSet<string>();
			var reportTitles = new HashSet<string>();
			foreach (DataRow row in dataSet.Tables[StmMenuTemplatePivotSchema.Constants.TableName].Rows)
			{
				if (pivots.Contains((Guid)row[StmMenuTemplatePivotSchema.PK.Name]) || legacyDocumentTemplatesNeedTranslate.Contains((Guid)row[StmMenuTemplatePivotSchema.SI_SO.Name]))
				{
					reportNames.Add((string)row[StmMenuTemplatePivotSchema.SI_DocumentTitle.Name]);
				}
				else if (reportMenuPKs.Contains((Guid)row[StmMenuTemplatePivotSchema.SI_SU.Name]))
				{
					reportTitles.Add((string)row[StmMenuTemplatePivotSchema.SI_DocumentTitle.Name]);
				}
			}

			reportSheetNames?.ForEach(s => reportNames.Add(s));
			reportTitlesInConfigArea?.ForEach(t => reportTitles.Add(t));

			ZrsFile.Save(ZrsFile.GetDefaultFilePath(Res.DefaultLanguage, string.Empty), DocBuilderResourceStrings.ReportNamesAsmid, Validate(DocBuilderTemplateTranslationHelper.GetReportNames(DocBuilderResourceStrings.ReportNameKeyPrefix, reportNames.ToArray())));
			ZrsFile.Save(ZrsFile.GetDefaultFilePath(Res.DefaultLanguage, string.Empty), DocBuilderResourceStrings.ReportTitleAsmid, Validate(DocBuilderTemplateTranslationHelper.GetReportNames(DocBuilderResourceStrings.ReportTitleKeyPrefix, reportTitles.ToArray())));
		}

		static ResourceStringData[] Validate(ResourceStringData[] resourceStrings)
		{
			var keys = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var item in resourceStrings)
			{
				if (keys.Contains(item.Key))
				{
					throw new Exception("Duplicate resource string key: " + item.Key + ":" + item.Caption);
				}

				keys.Add(item.Key);
			}
			return resourceStrings;
		}
	}

	public class LogMessage
	{
		public LogMessage(char type, string text)
		{
			Type = type;
			Text = text;
		}

		public LogMessage(Exception ex)
		{
			Type = 'X';
			Exception = ex;
		}

		public char Type { get; set; }
		public string Text { get; set; }
		public Exception Exception { get; set; }
	}
}

#endregion
