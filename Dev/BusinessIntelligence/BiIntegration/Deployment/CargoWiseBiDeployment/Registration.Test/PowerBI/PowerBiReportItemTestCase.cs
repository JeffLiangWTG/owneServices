using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CargoWise.Bi.Registration.PowerBi.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	abstract class PowerBiItemTestCase : TestCase
	{
		public abstract string PowerBiReportFilePath { get; }

		public const string PowerBiReportsDirectory = @"BusinessIntelligence\CargoWiseBi\CargoWiseBi.PBIRS\Reports";
		public const string PbixExtension = ".pbix";
		public const string DatasetExtension = ".rsd";
		const string spaceEscapeCharacter = "%20";
		const string slashCharacter = "/";

		public string PowerBiAuditSubdirectory = Path.Combine(TestCase.BaseSourcePath, PowerBiReportsDirectory, "Audit");
		public string PowerBiFinanceSubdirectory = Path.Combine(TestCase.BaseSourcePath, PowerBiReportsDirectory, "Finance");
		public string PowerBiLogisticsSubdirectory = Path.Combine(TestCase.BaseSourcePath, PowerBiReportsDirectory, "Logistics");
		public string PowerBiTelematicsSubdirectory = Path.Combine(TestCase.BaseSourcePath, PowerBiReportsDirectory, "Telematics");
		public string PowerBiWarehouseSubdirectory = Path.Combine(TestCase.BaseSourcePath, PowerBiReportsDirectory, "Warehouse");
		public string PowerBiSalesSubdirectory = Path.Combine(TestCase.BaseSourcePath, PowerBiReportsDirectory, "Sales");
		public string PowerBiProductivitySubdirectory = Path.Combine(TestCase.BaseSourcePath, PowerBiReportsDirectory, "Productivity");

		public virtual void TestPbiComplimentingFolderMatchesPbixContent()
		{
			if (!ReportItem.IsDataset && ReportItem.ReportType.Equals(BiReportType.NonPaginated))
			{
				ComparePbiData(PowerBiReportFilePath);
			}
			Assert("Remove empty test error for rdl reports", true);
		}

		void ComparePbiData(string reportFile)
		{
			var reportPath = reportFile.Replace(PbixExtension, string.Empty);

			var errors = new ZStringBuilder(Environment.NewLine);
			if (!new Uri(reportPath).IsUnc)
			{
				var currentDirectory = new Uri(Directory.GetCurrentDirectory() + slashCharacter);
				reportPath = currentDirectory.MakeRelativeUri(new Uri(reportPath)).OriginalString.Replace(spaceEscapeCharacter, " ");
			}

			using (ZipArchive archive = ZipFile.OpenRead(reportFile))
			{
				var visualGuidDict = new Dictionary<string, string>();

				foreach (var packageInfoFile in Directory.EnumerateFiles(reportPath, "package.json", SearchOption.AllDirectories))
				{
					using (var fileStream = File.OpenRead(packageInfoFile))
					using (var reader = new StreamReader(fileStream))
					{
						string json = reader.ReadToEnd();
						var packageObj = JsonConvert.DeserializeObject<JObject>(json);

						var guid = packageObj.SelectToken("visual.guid").ToString();
						var displayName = packageObj.SelectToken("visual.name").ToString();

						visualGuidDict[guid] = displayName;
					}
				}

				foreach (ZipArchiveEntry entry in archive.Entries)
				{
					var complimentingFilePath = string.Join("/", reportPath, entry.FullName);
					var parent = string.IsNullOrEmpty(entry.Name) ? complimentingFilePath : complimentingFilePath.Replace(entry.Name, string.Empty);
					foreach (var visualGuid in visualGuidDict)
					{
						if (parent.Contains(visualGuid.Key))
						{
							complimentingFilePath = Path.Combine(parent.Replace(visualGuid.Key, visualGuid.Value), entry.Name);
						}
					}

					if (entry.Length == 0)
					{
						if (!Directory.Exists(complimentingFilePath))
						{
							errors.Append($"A complimenting directory contained in a pbix archive is missing: {complimentingFilePath}");
						}
					}
					else
					{
						if (!File.Exists(complimentingFilePath))
						{
							errors.Append($"A complimenting file contained in a pbix archive is missing: {complimentingFilePath}");
						}
						else if (!AreFilesEqual(entry, complimentingFilePath))
						{
							errors.Append($"A complimenting pbix file mismatch occurred: {complimentingFilePath}");
						}
					}
				}

				var archiveEntryNames = new List<string>();
				foreach (ZipArchiveEntry entry in archive.Entries)
				{
					var entryName = entry.FullName;
					var parent = string.IsNullOrEmpty(entry.Name) ? string.Empty : entry.FullName.Replace(entry.Name, "");
					foreach (var visualGuid in visualGuidDict)
					{
						if (parent.Contains(visualGuid.Key))
						{
							entryName = Path.Combine(parent.Replace(visualGuid.Key, visualGuid.Value), entry.Name);
						}
					}
					archiveEntryNames.Add(entryName);
				}

				var excessPbixFiles = Directory.EnumerateFiles(reportPath, "*", SearchOption.AllDirectories)
					.Where(f => !archiveEntryNames.Any(e => string.Equals(e, f.Replace(reportPath, string.Empty).TrimStart(new[] { '\\' }).Replace("\\", slashCharacter))));
				if (excessPbixFiles.Any())
				{
					errors.Append($"Excess pbix files found:\r\n'{string.Join("'\r\n'", excessPbixFiles)}'");
				}
			}
			Assert($"The following errors occurred while comparing PBIX files with their respective complimenting folders: {errors.ToStringWithNewLineBetweenAppends()}", errors.ToString().Trim().Length == 0);
		}

		bool AreFilesEqual(ZipArchiveEntry entry, string filePath)
		{
			const int chunkSize = sizeof(long);
			var isMatch = false;
			var segmentsToRead = (int)Math.Ceiling((double)entry.Length / chunkSize);
			try
			{
				using (var fileStream = File.OpenRead(filePath))
				using (var entryStream = entry.Open())
				{
					byte[] segment1 = new byte[chunkSize];
					byte[] segment2 = new byte[chunkSize];

					for (var i = 0; i < segmentsToRead; i++)
					{
						fileStream.Read(segment1, 0, chunkSize);
						entryStream.Read(segment2, 0, chunkSize);
						isMatch = BitConverter.ToInt64(segment1, 0) == BitConverter.ToInt64(segment2, 0);
						if (!isMatch)
						{
							break;
						}
					}
				}
				return isMatch;
			}
			catch (FileNotFoundException)
			{
				return false;
			}
		}

		public void TestReportNameShouldNotBeNullOrEmpty()
		{
			Assert("Report name should not be null nor empty.", !string.IsNullOrEmpty(ReportItem.Name));
		}

		public void TestBusinessAreaIsValid()
		{
			Assert("Business area should not be null nor empty.", !string.IsNullOrEmpty(ReportItem.BusinessArea));
			Assert($"Business area [{ReportItem.BusinessArea}] should not contain '.'.", !ReportItem.BusinessArea.Contains("."));
		}

		public void TestResourceShouldExist()
		{
			AssertNoExceptionThrown($"Resource for power BI item does not exist. '{ReportItem.ResourceName}'", () =>
			{
				var resource = new DeploymentFileLoader().LoadPowerBiEmbeddedResource(ReportItem.ResourceName);
			});
		}

		public void TestResourceExtensionIsCorrect()
		{
			var extension = ReportItem.ResourceName.Split('.').Last();
			if (ReportItem.IsDataset)
			{
				Assert("Extension should be .rsd", extension.Equals("rsd"));
			}
			else if (ReportItem.ReportType.Equals(BiReportType.NonPaginated))
			{
				Assert("Extension should be .pbix", extension.Equals("pbix"));
			}
			else
			{
				Assert("Extension should be .rdl", extension.Equals("rdl"));
			}
		}

		public void TestDataSourceModelIsValid()
		{
			var ssasModel = BiAutomationConfigLoader.Instance.ConfigData.SsasCubes.FirstOrDefault(c => c.SsasModelLogicalName == ReportItem.DataSourceModel);
			Assert($"Data source [{ReportItem.DataSourceModel}] is not a registered model for CW1.", ssasModel != null);
		}

		public void TestPowerBiReportIsInCollection()
		{
			var reportCollection = new DeploymentFileLoader().GetAllPowerBiReports().Select(r => r.GetType().FullName);

			AssertCollectionContains("Power BI Reports collection is missing some classes.", ReportItem.GetType().FullName, reportCollection);
		}

		protected abstract PowerBiItem ReportItem { get; }
	}
}
