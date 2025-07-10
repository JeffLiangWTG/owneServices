using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Client
{
	public static class JobStatusFileWriter
	{
		public const char Splitter = '|';
		public const string FileName = "StatusData.txt";
		public const string DirectoryName = "JobStatus";
		public static readonly string FilePath = Path.Combine(Constants.WebPrintClientDataPath, DirectoryName, FileName);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Invoke")]
		public static void Append(IEnumerable<JobDetails> jobsDetails, Action<string> log = null)
		{
			try
			{
				log?.Invoke(jobsDetails.Count() + " processed jobs statuses to be saved.");

				var directoryPath = Path.GetDirectoryName(FilePath);
				if (directoryPath != null && !Directory.Exists(directoryPath))
				{
					log?.Invoke("Creating directory for client temporary local data storage: " + directoryPath);
					Directory.CreateDirectory(directoryPath);
				}

				log?.Invoke("Saving processed jobs statuses to local file.");
				File.AppendAllLines(FilePath, jobsDetails.Select(JobDetailsToString));

				if (File.Exists(FilePath))
				{
					log?.Invoke("Processed job statuses successfully saved to local file.");
				}
			}
			catch (Exception ex)
			{
				log?.Invoke("An error occured while saving processed jobs statuses.\r\nException Message: " + ex.Message);
				throw;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Invoke")]
		public static IEnumerable<JobDetails> ReadAll(Action<string> log = null)
		{
			try
			{
				if (File.Exists(FilePath))
				{
					var dataLines = File.ReadAllLines(FilePath);
					log?.Invoke("Reading " + dataLines.Length + " processed jobs statuses from local file.");

					var result = dataLines
						.Where(dataLine => !string.IsNullOrWhiteSpace(dataLine))
						.Select(dataLine => StringToJobDetails(dataLine, log));

					return result;
				}

				return null;
			}
			catch (Exception ex)
			{
				log?.Invoke("An error occured while reading processed jobs statuses.\r\nException Message: " + ex.Message);
				throw;
			}
		}

		static string JobDetailsToString(JobDetails jobDetails)
		{
			var part1 = jobDetails.PK.ToString();
			var part2 = jobDetails.Status.ToString();
			var part3 = (jobDetails.FailureReason ?? string.Empty).Replace('\n', ' ').Replace('\r', ' ');

			return part1 + Splitter + part2 + Splitter + part3;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Invoke")]
		static JobDetails StringToJobDetails(string content, Action<string> log = null)
		{
			var parts = content.Split(new[] { Splitter }, 3);
			if (parts.Length < 2)
			{
				log?.Invoke("Invalid data stored in processed jobs status file: " + content);
			}

			var pk = Guid.Empty;
			if (parts.Length > 0 && !Guid.TryParse(parts[0], out pk))
			{
				log?.Invoke("Invalid guid value in processed jobs status file: " + content);
			}

			var status = ProcessedStatus.Unprocessed;
			if (parts.Length > 1 && !Enum.TryParse(parts[1], out status))
			{
				log?.Invoke("Invalid status value in processed jobs status file: " + content);
			}

			return new JobDetails(pk, parts.Length > 2 ? parts[2] : string.Empty, status);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Invoke")]
		public static void Delete(Action<string> onShowInformation = null)
		{
			if (File.Exists(FilePath))
			{
				onShowInformation?.Invoke("Deleting old local file of processed jobs statuses.");
				File.Delete(FilePath);
			}
		}
	}
}
