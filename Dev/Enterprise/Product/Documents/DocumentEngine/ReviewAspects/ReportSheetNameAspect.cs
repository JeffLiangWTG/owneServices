using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Dat.Integration.AspectData;
using Dat.Integration.VersionControl;
using Enterprise.ZArchitecture.Core;
using FlexCel.XlsAdapter;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentEngine
{
	[CodeAlive("This class will be called by AspectHandler")]
	public class ReportSheetNameAspect : IAspectDataExtractor
	{
		//We put all of our report templates in Dev\Enterprise\Product\Documents\ExcelTemplates\Reports and file names cannot contain \/:*?"<>|
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "regular expression")]
		const string ReportTemplateFilePathPattern = @"(?<![\\/:\*\?""<>\|])\\Enterprise\\Product\\Documents\\ExcelTemplates\\Reports\\[^\\/:\*\?""<>\|]+\.xl(sx|s)$";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "regular expression")]
		const string DataTransformationPathPattern = @"(?<![\\/:\*\?""<>\|])\\Enterprise\\Architecture\\Core\\Core\\ServiceManager\\ServiceManager.Tasks\\OnlineDataTransformation\\Reports\\[^\\/:\*\?""<>\|]+\.cs$";

		Lazy<Regex> ReportTemplateFilePathRegex { get; } = new Lazy<Regex>(() => new Regex(ReportTemplateFilePathPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));

		Lazy<Regex> DataTransformationPathRegex { get; } = new Lazy<Regex>(() => new Regex(DataTransformationPathPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));

		public IEnumerable<AspectBit> GetAspectsForAllFiles(string sourcePath, string binPath)
		{
			return Enumerable.Empty<AspectBit>();
		}

		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		public IEnumerable<AspectBit> GetAspectsForShelf(string sourcePath, string binPath, IPendingChange[] changes)
		{
			var reportTemplatesIncludingSheetNameChange = new List<IPendingChange>();
			var isDataTransformationIncludedInChange = false;
			if (changes.Any(p => IsReportTemplateFile(p)))
			{
				foreach (var change in changes)
				{
					if (IsReportTemplateFile(change))
					{
						var serverSheetNames = RetrieveSheetNames(change, true);
						var localSheetNames = RetrieveSheetNames(change, false);
						if (serverSheetNames.Except(localSheetNames).Any())
						{
							reportTemplatesIncludingSheetNameChange.Add(change);
						}
					}
					else if (IsReportSheetDataTransformation(change))
					{
						var isNameSpaceCorrect = false;
						var isImplementingCorrectClass = false;
						using (var fileReader = new StreamReader(change.LocalItem))
						{
							string line;
							int lineNum = 0;
							while ((line = fileReader.ReadLine()) != null)
							{
								lineNum++;
								if (line.Contains((NoResString)"namespace Enterprise.ServiceManager.Tasks.OnlineDataTransformation.Reports"))
								{
									isNameSpaceCorrect = true;
								}
								else if (line.Replace(" ", "").Contains(":UpdateReportSheetNameConfigurationBase"))
								{
									isImplementingCorrectClass = true;
								}

								if (isNameSpaceCorrect && isImplementingCorrectClass)
								{
									break;
								}
							}
						}
						isDataTransformationIncludedInChange = isNameSpaceCorrect && isImplementingCorrectClass;
					}
				}
			}

			if (reportTemplatesIncludingSheetNameChange.Count > 0 && !isDataTransformationIncludedInChange)
			{
				foreach (var change in reportTemplatesIncludingSheetNameChange)
				{
					yield return new AspectBit(change.ServerItem, GetMD5(change.LocalItem), change.ServerItem, 0, 0);
				}
			}
		}

		public bool IsReportTemplateFile(IPendingChange change)
		{
			return ReportTemplateFilePathRegex.Value.IsMatch(change.ServerItem) &&
			(change.ChangeType & TfsChangeType.Add) != TfsChangeType.Add && (change.ChangeType & TfsChangeType.Branch) != TfsChangeType.Branch;
		}

		public bool IsReportSheetDataTransformation(IPendingChange change)
		{
			return DataTransformationPathRegex.Value.IsMatch(change.ServerItem) &&
			((change.ChangeType & TfsChangeType.Add) == TfsChangeType.Add || (change.ChangeType & TfsChangeType.Branch) == TfsChangeType.Branch);
		}

		[SuppressMessage("Enterprise", "EDI011:TempPathRule")]
		List<string> RetrieveSheetNames(IPendingChange change, bool isDownloadBaseFile)
		{
			List<string> result;
			var tempFile = Temp.GetTempFileName();
			try
			{
				if (isDownloadBaseFile)
				{
					change.DownloadBaseFile(tempFile);
				}
				else
				{
					File.Copy(change.LocalItem, tempFile, true);
				}
				result = GetTemplateSheetNamesFromFile(tempFile);
			}
			finally
			{
				TempFile.TryDeleteHandleAllExceptions(tempFile);
			}
			return result;
		}

		List<string> GetTemplateSheetNamesFromFile(string fileName)
		{
			var result = new List<string>();
			File.SetAttributes(fileName, FileAttributes.Normal);
			using (var templateFileStream = new FileStream(fileName, FileMode.Open))
			{
				var xls = new XlsFile();
				xls.Open(templateFileStream);
				for (int i = 1; i <= xls.SheetCount; i++)
				{
					xls.ActiveSheet = i;
					if (Report.IsTemplateSheet(xls.SheetName))
					{
						result.Add(xls.SheetName);
					}
				}
			}
			return result;
		}

		static string GetMD5(string filename)
		{
			using (var stream = File.OpenRead(filename))
			{
				return HashCalculator.CalculateMD5Hash(stream.ToByteArray());
			}
		}
	}
}
