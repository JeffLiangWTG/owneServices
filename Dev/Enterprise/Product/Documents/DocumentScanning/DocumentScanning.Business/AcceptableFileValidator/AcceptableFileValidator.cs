using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Business
{
	public class AcceptableFileValidator
	{
		public AcceptableFileValidator(bool flagPDFFiles, params string[] fileNames)
		{
			this.flagPDFFiles = flagPDFFiles;
			allFileNames = fileNames;
			maximumLimitSizeInMB = SystemDataRegistry.Instance.eDocsMaximumFilesize.Value;
			ValidateFiles();
		}

		public string[] GetValidFiles()
		{
			return acceptedFiles.ToArray();
		}

		public ZString GetInvalidFilesMessage()
		{
			return FormatMessage(Res.GetString("3D8FD1B5-E685-4E32-AB6D-28DC48BB4536", "The following files were not added because they were not supported. Please check that they are present, valid and accessible:"), unsupportedFiles) +
					FormatMessage(Res.GetString("fb0ad34c-3c48-49f7-8c11-abf7b6063180", "The following files were not added because they are potentially dangerous file types:"), dangerousFiles) +
					FormatMessage(Res.GetString("f2930b05-a6ae-46bb-9e59-801e1997a290", "The following files were not added because they are empty:"), emptyFiles) +
					FormatMessage(Res.GetString("72b8a0b8-dc1e-4333-8ff7-566ddcd374ca", "The following files were not added because they are too long(The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters.):"), tooLongNameFiles) +
					((flagPDFFiles) ? FormatMessage(Res.GetString("6342785c-97fe-4f15-b9b7-ee25a3ce5bdb", "PDF Files are no longer supported for add. Please add the file to the eDocs tab of the appropriate form, or alternatively you can manually convert the file to TIF."), pdfFiles) : string.Empty) +
					FormatMessage(StorageDocsHelper.GetMaximumLimitSizeNotifications(SystemDataRegistry.Instance.eDocsMaximumFilesize), largeFiles);
		}

		#region Invalid File Message

		string FormatMessage(string prefix, ICollection<string> fileNames)
		{
			if (fileNames.Count > 0)
			{
				return prefix + System.Environment.NewLine + ListFormatter.FormatListToString(fileNames, showFileNamesOnly: true) + System.Environment.NewLine;
			}

			return string.Empty;
		}

		FileListFormatter ListFormatter => listFormatter ?? (listFormatter = new FileListFormatter());

		FileListFormatter listFormatter;

		#endregion

		#region ValidateFiles

		void ValidateFiles()
		{
			if (!validated && allFileNames != null)
			{
				var fileValidation = new FileTypeValidation();

				foreach (var fileName in allFileNames)
				{
					try
					{
						if (flagPDFFiles && IsPDFFile(fileName)) // display special msg for PDF files
						{
							pdfFiles.Add(fileName);
						}
						else if (fileValidation.IsDangerousFile(fileName))
						{
							dangerousFiles.Add(fileName);
						}
						else
						{
							var info = new FileInfo(fileName);

							if (info.Exists)
							{
								if (info.Length > 0)
								{
									using (var stream = info.OpenRead())
									{
										var isDangerous = fileValidation.IsDangerousFile(stream, out var actualExtension);
										if (flagPDFFiles && actualExtension.Equals("PDF", StringComparison.OrdinalIgnoreCase))
										{
											pdfFiles.Add(fileName);
										}
										else if (isDangerous)
										{
											dangerousFiles.Add(Res.GetString("4c5c2b39-0819-4615-bdde-05f04073311a", "{0} (Actually {1})", fileName, actualExtension));
										}
										else if (info.Length > maximumLimitSizeInMB * 1024 * 1024)
										{
											largeFiles.Add(fileName);
										}
										else
										{
											acceptedFiles.Add(fileName);
										}
									}
								}
								else
								{
									emptyFiles.Add(fileName);
								}
							}
						}
					}
					catch (NotSupportedException)
					{
						unsupportedFiles.Add(fileName);
					}
					catch (PathTooLongException)
					{
						tooLongNameFiles.Add(fileName);
					}
				}

				validated = true;
			}
		}

		bool IsPDFFile(string filename)
		{
			return Path.GetExtension(filename).Equals(".PDF", StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		bool validated;
		readonly int maximumLimitSizeInMB;
		readonly bool flagPDFFiles;
		readonly string[] allFileNames;
		readonly List<string> dangerousFiles = new List<string>();
		readonly List<string> acceptedFiles = new List<string>();
		readonly List<string> emptyFiles = new List<string>();
		readonly List<string> tooLongNameFiles = new List<string>();
		readonly List<string> pdfFiles = new List<string>();
		readonly List<string> largeFiles = new List<string>();
		readonly List<string> unsupportedFiles = new List<string>();
	}
}
