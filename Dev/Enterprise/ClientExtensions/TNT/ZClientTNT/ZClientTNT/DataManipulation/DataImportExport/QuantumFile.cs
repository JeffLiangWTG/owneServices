using System.Collections.Specialized;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT
{
	public class QuantumFile
	{
		protected QuantumFile(string[] fileLines, QuantumSegment[] quantumSegments, string fileName, bool requiresConsolProcessing)
		{
			this.FileName = fileName;
			this.RequiresConsolProcessing = requiresConsolProcessing;
			this.QuantumSegments = quantumSegments;
		}

		public const string Extension = ".ok";

		#region Static Factory Methods

		public static QuantumFile GetFile(ZString fileName, INotifications notify)
		{
			return GetFile(new QuantumRecordFactory(), fileName, notify);
		}

		public static QuantumFile GetFile(QuantumRecordFactory recordFactory, ZString fileName, INotifications notify)
		{
			string fileData = File.ReadAllText(fileName);
			string[] fileLines = fileData.Split(new char[] { '\n' });
			fileLines = StringParser.TrimBlankLinesAndNewLines(fileLines);
			ZString fileNameOnly = Path.GetFileName(fileName);
			return QuantumFile.FromData(recordFactory, fileLines, fileNameOnly, notify);
		}

		public static QuantumFile FromData(string[] fileLines, ZString fileName, INotifications notify)
		{
			return FromData(new QuantumRecordFactory(), fileLines, fileName, notify);
		}

		public static QuantumFile FromData(QuantumRecordFactory recordFactory, string[] fileLines, ZString fileName, INotifications notify)
		{
			QuantumFile result = null;

			if (ValidateFileAndNotify(fileName, fileLines, notify))
			{
				ListDictionary segmentsList = new ListDictionary();

				for (int i = 0; i < fileLines.Length;)
				{
					int endLine;
					QuantumSegment segment = QuantumSegment.FromData(recordFactory, fileName.Left(3), fileLines, i, out endLine, notify);
					i = endLine;

					if (segment == null)
					{
						break;
					}

					// Checks if list contains a Segment with the same MasterBill for its Consol.
					// If so, merges segments into a new one, and removes previous one from list.
					if (segmentsList.Contains(segment.Consol.RecordKey))
					{
						QuantumSegment previousSegment = (QuantumSegment)segmentsList[segment.Consol.RecordKey];
						segment = QuantumSegment.FromMergingSegments(previousSegment, segment);
						segmentsList[segment.Consol.RecordKey] = segment;
					}
					else
					{
						segmentsList.Add(segment.Consol.RecordKey, segment);
					}
				}

				result = new QuantumFile(
					fileLines,
					QuantumSegmentsFromList(segmentsList),
					fileName,
					IsConsolProcessing(fileName));
			}

			return result;
		}

		static QuantumSegment[] QuantumSegmentsFromList(ListDictionary segmentsList)
		{
			QuantumSegment[] result = null;

			if (segmentsList.Count > 0)
			{
				result = new QuantumSegment[segmentsList.Count];
				segmentsList.Values.CopyTo(result, 0);
			}

			return result;
		}

		static bool ValidateFileAndNotify(ZString fileName, string[] fileLines, INotifications notify)
		{
			bool validFileName = IsValidFileName(fileName);
			bool validFileData = IsValidFileFormat(fileLines);

			if (!validFileName)
			{
				notify.Notify(new ErrorNotification(TNTErrorType.InvalidFileName, ""));
			}
			if (!validFileData)
			{
				notify.Notify(new ErrorNotification(TNTErrorType.InvalidFileFormat, ""));
			}

			return (validFileName && validFileData);
		}

		static bool IsValidFileName(ZString fileName)
		{
			var result = false;

			ZString[] splitFileName = fileName.Split('.');
			if (splitFileName.Length == 5)
			{
				ZString fileExtension = "." + splitFileName[4].ToLower();
				var branch = new BusinessObjectFactory().LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, splitFileName[0]);
				if (branch != null && fileExtension == QuantumFile.Extension)
				{
					ZString fileTypeCode = splitFileName[1].ToUpper();
					if (branch.Country.RN_Code == Core.Constants.CountryCodes.Australia)
					{
						result = (fileTypeCode == "X1" || fileTypeCode == "X2");
					}
					else if (branch.Country.RN_Code == Core.Constants.CountryCodes.NewZealand)
					{
						result = (fileTypeCode == "X2" || fileTypeCode == "IND");
					}
				}
			}

			return result;
		}

		static bool IsValidFileFormat(string[] fileLines)
		{
			bool result = false;

			if (fileLines.Length >= 4)
			{
				result = CheckLineFormat(fileLines[0], true) && CheckLineFormat(fileLines[fileLines.Length - 1], false);
			}

			return result;
		}

		static bool CheckLineFormat(ZString line, bool isFirstLine)
		{
			return (line.Length == 490 && line.Left(2) == (isFirstLine ? "01" : "04") && line.Right(1) == ".");
		}

		static bool IsConsolProcessing(string fileName)
		{
			bool result = false;

			if (fileName.Length >= 6 && fileName.Substring(4, 2) == "X2")
			{
				result = true;
			}

			return result;
		}

		#endregion

		public readonly string FileName;
		public readonly bool RequiresConsolProcessing;
		public readonly QuantumSegment[] QuantumSegments;
	}
}
