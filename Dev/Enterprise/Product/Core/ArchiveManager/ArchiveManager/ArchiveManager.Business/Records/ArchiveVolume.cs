using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Enterprise.ArchiveManager.Business.Records
{
	public enum VolumeState { Open, Closed }

	public class ArchiveVolume
	{
		ArchiveVolume(string volumeZipFilePath)
		{
			VolumePath = volumeZipFilePath.ToLower().Replace(".zip", "");
			ZString volumeFilePathZString = Path.GetFileName(VolumePath);
			VolumeNo = Convert.ToInt32(volumeFilePathZString.Substring(VolumeFilePrefix.Length));
		}

		ArchiveVolume(long volumeNo, string volumePath, decimal freeSpaceInMB)
		{
			VolumePath = volumePath;
			VolumeNo = volumeNo;
			this.freeSpaceInMB = freeSpaceInMB;
		}

		public VolumeState VolumeState { get; private set; }
		public string VolumePath { get; private set; }
		public string VolumeZipPath { get { return VolumePath + ".zip"; } }
		public long VolumeNo { get; private set; }
		public IArchiveLogger Logger { get; private set; }
		public const string VolumeFilePrefix = "Vol-";

		public static ArchiveVolume CreateNew(string rootDirectory, decimal freeSpaceInMB, IArchiveLogger logger, IArchiveSystemDescriptor descriptor)
		{
			var factory = new BusinessObjectFactory();
			long currentVolumeNo;
			using (var transactionManager = ((ITransactionParticipant)factory).BeginTransactionWithManager())
			{
				currentVolumeNo = Env.NumberFountains.ArchiveVolumeNo.GetNext(factory);
				transactionManager.CommitTransaction();
			}

			var volumePath = GetVolumePath(rootDirectory, currentVolumeNo, VolumeState.Open);
			if (!Directory.Exists(volumePath) && !File.Exists(volumePath + ".zip"))
			{
				_ = Directory.CreateDirectory(volumePath);
			}
			else
			{
				var retries = 0;
				var maxRetries = 100;
				while ((Directory.Exists(volumePath) || File.Exists(volumePath + ".zip")) && retries < maxRetries)
				{
					logger.LogInfo(descriptor.Code, Res.GetString("9C4F8A18-D58E-4F4C-8425-497091E1FA82", "Either the volume path {0} or zip file {1} already exists. The system will create a new volume.", volumePath, $"{volumePath}.zip"));

					using (var transactionManager = ((ITransactionParticipant)factory).BeginTransactionWithManager())
					{
						currentVolumeNo = Env.NumberFountains.ArchiveVolumeNo.GetNext(factory);
						transactionManager.CommitTransaction();
					}

					volumePath = GetVolumePath(rootDirectory, currentVolumeNo, VolumeState.Open);
					if (!Directory.Exists(volumePath) && !File.Exists(volumePath + ".zip"))
					{
						_ = Directory.CreateDirectory(volumePath);
						break;
					}

					++retries;
				}
			}

			var volume = new ArchiveVolume(currentVolumeNo, volumePath, freeSpaceInMB)
			{
				VolumeState = VolumeState.Open,
				Logger = logger
			};
			return volume;
		}

		public static ArchiveVolume LoadVolume(string rootDirectory, long volumeNo, VolumeState state)
		{
			if (VolumeExists(rootDirectory, volumeNo, state))
			{
				var volume = new ArchiveVolume(GetVolumePath(rootDirectory, volumeNo, state))
				{
					VolumeState = state
				};
				return volume;
			}
			else
			{
				throw new InvalidOperationException("Volume does not exist");
			}
		}

		public static ArchiveVolume LoadVolume(string volumeFilePath)
		{
			if (File.Exists(volumeFilePath) && volumeFilePath.ToLower().EndsWith(".zip"))
			{
				var volume = new ArchiveVolume(volumeFilePath)
				{
					VolumeState = VolumeState.Closed
				};
				return volume;
			}
			else
			{
				throw new InvalidOperationException("Volume does not exist or is invalid");
			}
		}

		public static bool VolumeExists(string rootDirectory, long volumeNo, VolumeState state)
		{
			var volumePath = GetVolumePath(rootDirectory, volumeNo, state);
			return state == VolumeState.Open ? Directory.Exists(volumePath) : File.Exists(volumePath);
		}

		public bool ExceededCapacity
			=> volumeSizeInBytes / 1024 / 1024 >= freeSpaceInMB;

		public void Add(string fileName, Stream fileStream)
			=> Add(fileName, fileStream, null, null);

		public delegate void SaveToStream(Stream streamToSaveTo);

		public void Add(string fileName, SaveToStream saveToStreamDelegate, StorageDocsBase originalStorageDoc)
			=> Add(fileName, null, saveToStreamDelegate, originalStorageDoc);

		public bool IsVolumeCrypted()
		{
			using var fileStreamIn = new FileStream(VolumeZipPath, FileMode.Open, FileAccess.Read);
			using var zipInStream = new ZipInputStream(fileStreamIn);
			var entry = zipInStream.GetNextEntry();
			return entry.IsCrypted;
		}

		public bool HasEntry(string fileName)
		{
			using var zipFile = new ZipFileCore(VolumeZipPath);
			return zipFile.HasEntry(fileName);
		}

		public void Extract(string fileName, string targetDirectory)
		{
			if (VolumeState == VolumeState.Open)
			{
				throw new InvalidOperationException("Cannot extract from an open volume");
			}

			using var zipFile = new ZipFileCore(VolumeZipPath);
			if (IsVolumeCrypted())
			{
				zipFile.SetPassword(ZipPassword);
			}

			if (!HasEntry(fileName))
			{
				throw new InvalidOperationException("Unable to find the file in this volume.");
			}
			else
			{
				zipFile.ExtractEntry(fileName, targetDirectory);
			}
		}

		public void Close()
		{
			if (VolumeState == VolumeState.Closed)
			{
				throw new InvalidOperationException("volume is already closed");
			}

			var creator = new ZipCreator()
			{
				CompressionLevel = 9
			};
			using (FileStream zipOutputStream = new FileStream(VolumeZipPath, FileMode.CreateNew))
			{
				creator.ZipStream(GetZipStreams(), zipOutputStream);
			}

			VolumeState = VolumeState.Closed;
			Directory.Delete(VolumePath, recursive: true);
		}

		public IEnumerable<Guid> GetAllStorageMainPKs()
		{
			if (VolumeState == VolumeState.Open)
			{
				throw new InvalidOperationException("Cannot get PKs from an open volume");
			}

			using var zipFile = new ZipFileCore(VolumeZipPath);
			foreach (ZipEntry entry in zipFile)
			{
				if (entry.Name.ToLower().EndsWith(".meta"))
				{
					yield return new Guid(Path.GetFileName(entry.Name).Substring(0, 36));
				}
			}
		}

		const string ZipPassword = "CoG0W1sE";
		long volumeSizeInBytes;
		readonly decimal freeSpaceInMB;

		static string GetVolumePath(string rootDirectory, long volumeNo, VolumeState state)
		{
			var extension = state == VolumeState.Open ? "" : ".zip";
			return Path.Combine(rootDirectory, VolumeFilePrefix + volumeNo.ToString(CultureInfo.InvariantCulture).PadLeft(8, '0') + extension);
		}

		void Add(string fileName, Stream fileStream, SaveToStream saveToStreamDelegate, StorageDocsBase originalStorageDoc)
		{
			if (VolumeState == VolumeState.Closed)
			{
				throw new InvalidOperationException("Cannot add to a closed volume");
			}

			var safeFileName = MakeFilenameSafe.MakeSafe(fileName, '_');

			try
			{
				using var outputFileStream = GetFileStream(safeFileName, originalStorageDoc);
				if (fileStream != null)
				{
					CopyStream(fileStream, outputFileStream);
				}
				else if (saveToStreamDelegate != null)
				{
					saveToStreamDelegate(outputFileStream);
				}
				else
				{
					throw new InvalidOperationException("fileStream and saveToStreamDelegate cannot both be null.");
				}

				volumeSizeInBytes += outputFileStream.Length;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ex.Data.Add("ArchiveOfflineFilePathInfo", $"The file path was: {Path.Combine(VolumePath, safeFileName)}");
				throw;
			}
		}

		FileStream GetFileStream(string fileName, StorageDocsBase originalStorageDoc)
		{
			var arcTruncatedStringTag = "[TruncatedLongName]";
			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
			var extension = Path.GetExtension(fileName);
			const int maxPath = 259;
			var combinedPath = Path.Combine(VolumePath, fileName);

			if (combinedPath.Length <= maxPath)
			{
				var duplicateFileNameCount = 1;

				while (File.Exists(combinedPath))
				{
					var duplicateTag = $"({duplicateFileNameCount})";

					combinedPath = Path.Combine(VolumePath, fileNameWithoutExtension + duplicateTag + extension);
					duplicateFileNameCount++;
				}
			}

			if (combinedPath.Length > maxPath)
			{
				var duplicateFileNameCount = 1;
				combinedPath = Path.Combine(VolumePath, fileNameWithoutExtension.Substring(0, maxPath - 1 - arcTruncatedStringTag.Length - extension.Length - VolumePath.Length) + arcTruncatedStringTag + extension);

				while (File.Exists(combinedPath))
				{
					var duplicateTag = $"({duplicateFileNameCount})";

					combinedPath = Path.Combine(VolumePath, fileNameWithoutExtension.Substring(0, maxPath - 1 - arcTruncatedStringTag.Length - extension.Length - duplicateTag.Length - VolumePath.Length) + arcTruncatedStringTag + duplicateTag + extension);
					duplicateFileNameCount++;
				}

				Logger.LogInfo($"File {fileName} was truncated to {Path.GetFileName(combinedPath)} because its path was too long");
			}

			if (originalStorageDoc != null)
			{
				originalStorageDoc.SC_FileName = Path.GetFileNameWithoutExtension(combinedPath);
			}

			var result = new FileStream(combinedPath, FileMode.Create);
			return result;
		}

		IEnumerable<ZipStream> GetZipStreams()
		{
			foreach (var file in Directory.GetFiles(VolumePath))
			{
				var filename = Path.GetFileName(file);
				var stream = new FileStream(file, FileMode.Open);
				yield return new ZipStream(filename, stream);
			}
		}

		void CopyStream(Stream source, Stream target)
		{
			const int bufSize = 0x8000;
			var buf = new byte[bufSize];
			source.Position = 0;
			int bytesRead;
			while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
			{
				target.Write(buf, 0, bytesRead);
			}
		}
	}
}
