using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore.Exceptions;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

#if DEBUG
using System.Data;
using CargoWise.Data.Testing;

#endif

[assembly: MailSubscriber(typeof(Enterprise.DocumentScanning.Business.BatchImportManager))]
namespace Enterprise.DocumentScanning.Business
{
	public class BatchImportManager : IBatchImportManagerInternals
	{
#if DEBUG
		public void ImportFilesAndEmails(string importFolder)
		{
			ImportFilesAndEmails(importFolder, CancellationToken.None);
		}
#endif
		public void ImportFilesAndEmails(string importFolder, CancellationToken token)
		{
			ImportFiles(importFolder, token);
			ImportEmails(token);
		}

		#region Files import

		enum ImportResult
		{
			Successful,
			UnsuccessfulWithFileAccessException = 1,
			UnsuccessfulWithFactorySaveFailure = 2,
			UnsuccessfulUnrecoverable = 3,
		}

		ImportResult ImportFileCore(string fileName, string importFolder, DocumentFactory factory, CancellationToken token, out Exception exception)
		{
			var filenameOnly = Path.GetFileName(fileName);
			exception = null;

			var parentFactory = factory.FactoryForEverythingExceptEDocs;
			var contextSwitchLogger = SystemDataRegistry.Instance.DocumentImportUserContextTracingEnabled.Value ? new UserContextSwitchLogger() : null;
			using (contextSwitchLogger != null ? Env.StartContextSwitchTrace(contextSwitchLogger) : null)
			using (factory.ServiceContainer.AddService(new WorkflowUserContextManager()).SetWorkflowUserContext(Env.CurrentUserContext, contextSwitchLogger))
			using (parentFactory.ServiceContainer.AddService(new WorkflowUserContextManager()).SetWorkflowUserContext(Env.CurrentUserContext, contextSwitchLogger))
			{
				token.ThrowIfCancellationRequested();
				try
				{
					if (!ImportFileFromFilesystem(factory, fileName))
					{
						MoveToUnsuccessfulImports(importFolder, fileName);
						return ImportResult.UnsuccessfulUnrecoverable;
					}
				}
				catch (FileAccessException ex)
				{
					exception = ex;
					return ImportResult.UnsuccessfulWithFileAccessException;
				}
				catch (Exception ex) when (ex is IncorrectVisibleCompanyBranchDepartmentException
				|| ex is ArgumentException
				|| ex is NonUniqueAllocationCodeException)
				{
					return LogErrorMessageAndMoveToUnsuccessfulImports(CouldNotBeImportedMessage(filenameOnly, ex.Message));
				}

				try
				{
					ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null, true, false, 1, new ImportEmailNotificationHandler(this));
					DeleteFile(fileName);
					return ImportResult.Successful;
				}
				catch (ZSaveException ex)
				{
					exception = ex;
					return ImportResult.UnsuccessfulWithFactorySaveFailure;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return LogErrorMessageAndMoveToUnsuccessfulImports(CouldNotBeImportedMessage(filenameOnly, ex.Message));
				}
			}

			ImportResult LogErrorMessageAndMoveToUnsuccessfulImports(string message)
			{
				LogMessage(TraceEventType.Error, message);
				MoveToUnsuccessfulImports(importFolder, fileName);
				return ImportResult.UnsuccessfulUnrecoverable;
			}
		}

		ImportResult ImportFile(string fileName, string importFolder, DocumentFactory factory, bool skipLockedFiles, CancellationToken token)
		{
			var filenameOnly = Path.GetFileName(fileName);
			LogMessage(TraceEventType.Information, Res.GetString("872d4fd7-c293-4a32-90dd-cdcae760e26b", "Importing file '{0}'", fileName));

			var attemptCountMax = 5;
			for (var attemptCount = 0; attemptCount < attemptCountMax; ++attemptCount)
			{
				var result = ImportFileCore(fileName, importFolder, factory, token, out Exception ex);
				if (result == ImportResult.Successful)
				{
					return result;
				}
				else if (result == ImportResult.UnsuccessfulUnrecoverable)
				{
					//already logged/moved to unsuccessful imports inside of ImportFileCore
					return result;
				}
				else if (result == ImportResult.UnsuccessfulWithFileAccessException)
				{
					if (skipLockedFiles)
					{
						return result;
					}

					//file might be temporarily locked or unavailable
					if (attemptCount < (attemptCountMax - 1))
					{
						Thread.Sleep(5000);
					}
					else
					{
						string message;
						if (ex.GetHResult() == FileIsInUseByAnotherProcess)
						{
							message = Res.GetString("529d943a-b681-47a2-a76e-8a2b8afcd2e5", "File is locked by another process.") + "\r\n";
						}
						else
						{
							message = Res.GetString("D439E969-E217-4E59-BE5F-0CCFE026CD3B", "Get I/O Exception when accessing file to import: {0}\r\nImport file folder: {1}", ex.Message, importFolder) + "\r\n";
						}

						LogMessage(TraceEventType.Warning, CouldNotBeImportedMessage(filenameOnly, message));

						return result;
					}
				}
				else if (result == ImportResult.UnsuccessfulWithFactorySaveFailure)
				{
					//factory saving might fail e.g. because StorageMain.SM_DB can't be merged, which isn't handled by ProcessWithSaveExceptionHandling/ConcurrencyResolved
					if (attemptCount < (attemptCountMax - 1))
					{
						//start fresh with a new factory to get new StorageMain values from DB
						//this is OK to do since we pass in new factories every so often anyway, and don't do anything with them in ImportFiles otherwise
						factory = GetDocumentFactory();
					}
					else
					{
						LogMessage(TraceEventType.Error, Res.GetString("9dc8feb9-b210-4894-8013-10ea80b4e812", "Error while saving document to database.") + " " +
						CouldNotBeImportedMessage(filenameOnly, ex.Message) + "\r\n" +
						ex.ToString());
						MoveToUnsuccessfulImports(importFolder, fileName);
						return result;
					}
				}
			}

			//this should be unreachable unless the code is modified in the future - last loop of the for loop returns in every branch
			throw new InvalidOperationException("HandleFile failed to handle HandleFileCore properly and for loop exited without returning");
		}

		string FileResString => Res.GetString("68DB7580-7779-4DB6-ADA9-52DAFD645761", "File");
		void ImportFiles(string importFolder, CancellationToken token)
		{
			if (Directory.Exists(importFolder))
			{
				if (CheckSecuritySettings(importFolder))
				{
					string[] files = GetFilesToImport(importFolder);
					List<string> skippedFiles = new List<string>();
					if (files != null && files.Length > 0)
					{
						LogMessage(TraceEventType.Information, Res.GetString("d8145677-00e8-4854-b196-97e89707debc",
							"Found {0} DocManager Files to import in {1}:\r\n{2}", files.Length, importFolder, string.Join("\r\n", files)));
						DocumentFactory factory = GetDocumentFactory();
						var importedWithFactory = 0;
						var ignoreFileNum = 0;
						foreach (string fileName in files)
						{
							token.ThrowIfCancellationRequested();
							if (++importedWithFactory > SystemDataRegistry.Instance.MaximumNumberOfDocManagerItemsToImportInABatch.Value)
							{
								factory = GetDocumentFactory();
								importedWithFactory = 0;
							}

							var lockedRunResult = Db.Connection.RunLocked(GetSafeLockKey(fileName), isFirstRun =>
							{
								var result = ImportFile(fileName, importFolder, factory, skipLockedFiles: true, token);
								if (result == ImportResult.UnsuccessfulWithFileAccessException)
								{
									skippedFiles.Add(fileName);
								}
							}, () => !File.Exists(fileName));

							if (lockedRunResult == LockedProcessResult.AlreadyBeingProcessed)
							{
								ignoreFileNum++;
								if (importedWithFactory > 0)
								{
									importedWithFactory--;
								}
							}

							LogRunLockedResultMessage(lockedRunResult, FileResString, fileName);
						}
						if (skippedFiles.Count > 0)
						{
							LogMessage(TraceEventType.Information, Res.GetString("83FCA049-AFD4-401B-893F-403FA7FEC170", "Importing skipped files."));
							foreach (string fileName in skippedFiles)
							{
								var lockedRunResultForSkippedFiles = Db.Connection.RunLocked(GetSafeLockKey(fileName), isFirstRun =>
								{
									ImportFile(fileName, importFolder, factory, skipLockedFiles: false, token);
								}, () => !File.Exists(fileName));

								LogRunLockedResultMessage(lockedRunResultForSkippedFiles, FileResString, fileName);
							}
						}
						LogMessage(TraceEventType.Information, Res.GetString("11809609-ddbc-4f96-a333-00aae0b78f27", "Import Run Completed, processed {0} file(s), ignored {1} file(s) processed by another process.", files.Length - ignoreFileNum, ignoreFileNum));
					}
				}
				else
				{
					LogMessage(TraceEventType.Information, Res.GetString("7A7496DF-73A3-4EB1-8E31-50A97B287DE2", "DMI Service Task doesn't have modification rights for {0} directory. Please check security settings and run service task again", importFolder));
				}
			}
		}

#if DEBUG
		internal
#endif
		string GetSafeLockKey(string fileName)
		{
			var sh1Bytes = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(fileName));
			return Convert.ToBase64String(sh1Bytes);
		}

		void LogRunLockedResultMessage(LockedProcessResult lockedRunResult, string emailOrFile, string name)
		{
			switch (lockedRunResult)
			{
				case LockedProcessResult.AlreadyBeingProcessed:
					LogMessage(TraceEventType.Verbose, Res.GetString("7D9A2EC3-BF52-4E75-8BF2-06B36850829A", "{0} '{1}' is being processed by another process, ignoring it.", emailOrFile, name));
					break;
				case LockedProcessResult.Error:
					LogMessage(TraceEventType.Warning, Res.GetString("5E81CDCE-BE61-4C7B-A8DE-5FF9733ED569", "{0} '{1}' could not be imported due to repeated connection issues, maximum retries exceeded. It has been removed from this list and will be retried in another run.", emailOrFile, name));
					break;
				case LockedProcessResult.Completed:
					LogMessage(TraceEventType.Information, Res.GetString("F2DB117E-455B-4192-930E-C34718829112", "{0} '{1}' is processed.", emailOrFile, name));
					break;
			}
		}

#if DEBUG
		protected virtual
#endif
		WindowsPrincipal Principal
		{
			get
			{
				if (fPrincipal == null)
				{
					fPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
				}
				return fPrincipal;
			}
		}
		WindowsPrincipal fPrincipal;

		public bool CheckSecuritySettings(string importFolder)
		{
			var isWritable = FileSystemAccessControlUtils.IsDirectoryWritable(importFolder, out var principalIsInRoleException);
			if (principalIsInRoleException != null)
			{
				LogMessage(TraceEventType.Error, Res.GetString("50E470AA-94DE-445C-9B12-98F9074084D9",
					"Exception while trying to import files: {0}. DMI Service Task doesn't have modification rights for {1} directory. Please check security settings and run service task again",
					principalIsInRoleException.Message, importFolder));
			}
			return isWritable;
		}

		internal DocumentFactory GetDocumentFactory(BusinessObjectFactory factory = null)
		{
			var documentFactory = new DocumentFactoryProvider().GetFactory(factory ?? new BusinessObjectFactory());
			documentFactory.Log += LogProgress;
			return documentFactory;
		}

		string[] GetFilesToImport(string importFolder)
		{
			int attemptCount = 5;
			string message = string.Empty;
			do
			{
				try
				{
					string[] files = Directory.GetFiles(importFolder);
					return files;
				}
				catch (DirectoryNotFoundException)
				{
					LogMessage(TraceEventType.Error, Res.GetString("6b9088b7-74db-4c3b-8ef0-f8553ebb9d82", "Process controller cannot find the import directory at {0}, please set correct path in Registry -> System -> DocManager -> DocManager Import Service Task Options.", importFolder));
					attemptCount = 0;
				}
				catch (IOException ex)
				{
					attemptCount--;
					if (ex.GetHResult() == FileIsInUseByAnotherProcess)
					{
						message += Res.GetString("529d943a-b681-47a2-a76e-8a2b8afcd2e5", "File is locked by another process.") + "\r\n";
					}
					else
					{
						message += Res.GetString("D439E969-E217-4E59-BE5F-0CCFE026CD3B", "Get I/O Exception when accessing file to import: {0}\r\nImport file folder: {1}", ex.Message, importFolder) + "\r\n";
					}

					Thread.Sleep(100);
					if (attemptCount == 0)
					{
						LogMessage(TraceEventType.Error, message);
					}
				}
				catch (UnauthorizedAccessException)
				{
					LogMessage(TraceEventType.Error, Res.GetString("fc59ff38-03f0-4140-a579-63254fbf2cab", "Process controller cannot access path {0}, please check permission settings for this directory.", importFolder));
					attemptCount = 0;
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}
					ErrorReporter.ReportOnce(string.Format("Unable to get files to import at {0}.", importFolder), ex);
					attemptCount = 0;
				}
			}
			while (attemptCount > 0);
			return null;
		}

		public const int FileIsInUseByAnotherProcess = -2147024864;

		string CouldNotBeImportedMessage(string fileName, string error)
		{
			return Res.GetString("8f40d9fe-b1a2-4c54-bbd7-be4debf80399", "File '{0}' could not be imported. {1}", fileName, error);
		}

		public bool HasPrefixInformation(ZString filenameOnly)
		{
			return filenameOnly.Trim().StartsWith(@"[") && filenameOnly.Contains(']');
		}

		int MaximumFileSizeInBytes
		{
			get { return MaxmimumFileSizeInMB * 1024 * 1024; }
		}

		int MaxmimumFileSizeInMB
		{
			get { return SystemDataRegistry.Instance.eDocsMaximumFilesize.Value; }
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description Debug info")]
		bool ImportFileFromFilesystem(DocumentFactory masterFactory, ZString filename)
		{
			if (string.IsNullOrEmpty(filename))
			{
				LogMessage(TraceEventType.Error, "Debug info: filename is empty.");
				return false;
			}

			ZString filenameOnly = Path.GetFileName(filename);
			LogMessage(TraceEventType.Information, Res.GetString("e7e3f7d2-0805-4247-84ca-bf51d0109adf", "Importing from File system.\r\nFull filename: {0};\r\nFilename only: {1}.", filename, filenameOnly));

			byte[] contents = DocumentUtilities.GetFileAsBytes(filename);
			return ImportFileContentCore(masterFactory, contents, filenameOnly, filename);
		}

		bool IBatchImportManagerInternals.ImportFileContent(DocumentFactory masterFactory, byte[] contents, ZString filenameOnly,
			ZString initialDocType, Guid initialCompanyPK, Guid initialBranchPK, Guid initialDepartmentPK, string fileFullPath)
		{
			var initialVisibleInfo = new VisibleCompanyBranchDepartmentInfo { VisibleCompanyPK = initialCompanyPK, VisibleBranchPK = initialBranchPK, VisibleDepartmentPK = initialDepartmentPK };
			return ImportFileContentCore(masterFactory, contents, filenameOnly, fileFullPath, initialDocType, initialVisibleInfo);
		}

		bool ImportFileContentCore(DocumentFactory masterFactory, byte[] contents, ZString filenameOnly, string fileFullPath = "", string initialDocType = "", VisibleCompanyBranchDepartmentInfo initialInfo = null)
		{
			AllocationCodeRetriever codeRetriever = null;
			if (HasPrefixInformation(filenameOnly))
			{
				var prefixInformation = filenameOnly.SubstringSafe(0, filenameOnly.IndexOf(']'));

				try
				{
					codeRetriever = new AllocationCodeRetriever(masterFactory, prefixInformation, true);
					if (!codeRetriever.VisibleInfo.IsEmpty())
					{
						LogMessage(TraceEventType.Warning, Res.GetString("6936CF2B-8C55-4ADF-B7A1-BC4D418B73A3", "The visibility parameters specified in the {0} element for file '{1}' will be used in place of the {2}, {3} and {4} elements.", "FileName", filenameOnly, "VisibleCompanyCode", "VisibleBranchCode", "VisibleDepartmentCode"));
					}
				}
				catch (AllocationCodeFormatException ex)
				{
					LogMessage(TraceEventType.Warning, Res.GetString("1734e60e-0aa7-447b-93eb-c527c57f7b18", "Allocation info for file '{0}' cannot be extracted from '{1}' prefix or specified Job not found. The file will be processed by barcodes if applicable. {2}", filenameOnly, prefixInformation, ex.Message));
				}
			}

			return ImportFile(masterFactory, contents, filenameOnly, codeRetriever, fileFullPath, initialDocType, initialInfo);
		}

		bool ImportFile(DocumentFactory masterFactory, byte[] contents, ZString filenameOnly, AllocationCodeRetriever codeRetriever, string fileFullPath = "", string initialDocType = "", VisibleCompanyBranchDepartmentInfo initialInfo = null)
		{
			var refType = codeRetriever?.RefType ?? ZString.Empty;
			var userSuppliedDocType = codeRetriever?.DocType ?? initialDocType;
			var userSuppliedDocSource = codeRetriever?.DocSource ?? ZString.Empty;
			var userSuppliedRefPK = codeRetriever?.RefPK ?? ZGuid.Empty;
			var bizoTobeImported = codeRetriever?.RefObject;
			var visibleInfo = (codeRetriever != null && !codeRetriever.VisibleInfo.IsEmpty()) ? codeRetriever.VisibleInfo : initialInfo ?? new VisibleCompanyBranchDepartmentInfo();

			var fileExtension = Path.GetExtension(filenameOnly);
			if (string.IsNullOrWhiteSpace(fileExtension))
			{
				LogMessage(TraceEventType.Error, CouldNotBeImportedMessage(filenameOnly, Res.GetString("9C3A1020-FA7F-44D5-8119-7F5593A40FC8", "Error message: File extension is missing.")));
				return false;
			}

			if (contents == null || contents.Length == 0)
			{
				LogMessage(TraceEventType.Error, CouldNotBeImportedMessage(filenameOnly, Res.GetString("226AE7A7-BF2D-43FA-A37E-D661158A08F7", "Error message: File is empty.")));
				return false;
			}

			var contentsToImport = contents;
			if (contentsToImport.Length > MaximumFileSizeInBytes && !SerializableEDocsTools.IsImage(fileExtension))
			{
				LogMessage(TraceEventType.Error, Res.GetString("737c08bf-6ab8-4ffc-853c-ee7d298635c5", "The file size of '{0}' exceeds the {1}MB maximum file size allowed for eDocs.", filenameOnly, MaxmimumFileSizeInMB));
				return false;
			}

			var filenameOnlyToImport = filenameOnly;
			if (FileImporter.IsSupportedImageFile(fileExtension) && !(AllocateDocumentsManager.IsJpegFile(filenameOnly) || AllocateDocumentsManager.IsGifFile(filenameOnly)))
			{
				try
				{
					contentsToImport = DocumentUtilities.ConvertFileToTiff(contents, filenameOnly);
					filenameOnlyToImport = Path.GetFileNameWithoutExtension(filenameOnly) + ".TIF";
				}
				catch (ImageFormatException ex)
				{
					LogMessage(TraceEventType.Warning, Res.GetString("09e25a99-c49b-42e6-8337-70a8fa0d210f", "File '{0}' could not be processed as an image due to invalid file content. {1}", filenameOnly, ex.Message));
				}
			}

			bool result;
			try
			{
				if (bizoTobeImported is IEDocsPluginHostDecider eDocsPluginHostDecider && eDocsPluginHostDecider.HostBusinessEntity is IDocManagerSupport docManagerSupport)
				{
					refType = docManagerSupport.DocManagerInfo.DocManagerCode;
				}

				result = masterFactory.Import(contentsToImport, filenameOnlyToImport, refType, userSuppliedRefPK, userSuppliedDocType, userSuppliedDocSource,
					visibleInfo.VisibleCompanyPK, visibleInfo.VisibleBranchPK, visibleInfo.VisibleDepartmentPK, fileFullPath, true, out var finalRestrictedDocType);
				if (result)
				{
					if (string.Compare(filenameOnlyToImport, filenameOnly, StringComparison.OrdinalIgnoreCase) == 0)
					{
						LogMessage(TraceEventType.Information, Res.GetString("01da1377-3771-4700-8bf8-bc36ecb6c2e5", "Imported file '{0}'.", filenameOnlyToImport));
					}
					else
					{
						LogMessage(TraceEventType.Information, Res.GetString("08f8e21f-2630-4d78-b3e0-b26dd61e71d8", "Imported file '{0}', converted to '{1}'.", filenameOnly, filenameOnlyToImport));
					}
				}
				else if (!string.IsNullOrEmpty(finalRestrictedDocType))
				{
					LogMessage(TraceEventType.Error, Res.GetString("913ad74b-3265-4172-a252-ca4c5375ad13", "Cannot import file '{0}' as it uses document type {1} which is denied in the registry setting 'Document Types Restricted For Import'.", filenameOnly, finalRestrictedDocType));
				}
				else
				{
					LogMessage(TraceEventType.Error, Res.GetString("d2cd1dff-c7e2-40f6-8bfb-968cc41abb52", "Couldn't import file '{0}'. Please make sure the allocation information is added to the beginning of the file name or the subject of the email (e.g. '[DocManager SHP CIV S00001000]')", filenameOnly));
				}
			}
			catch (VirusDetectedException ex)
			{
				LogMessage(TraceEventType.Error, ex.VirusDetectedFriendlyMessage);
				result = false;
			}
			catch (Exception ex) when (ex is UnsupportedDocumentException || ex is InvalidOperationException || ex is ExternalException)
			{
				LogMessage(TraceEventType.Error, ex.Message);
				result = false;
			}

			return result;
		}

		void DeleteFile(string filename)
		{
			try
			{
				File.SetAttributes(filename, FileAttributes.Normal);
				File.Delete(filename);
			}
			catch (Exception ex) // file in use, etc
			{
				LogMessage(TraceEventType.Information, Res.GetString("CE4488FE-730C-459F-860F-05AC1BE9B5A9", "Couldn't delete file '{0}'. Exception message: {1}", filename, ex.Message));
			}
		}

		void MoveToUnsuccessfulImports(string importFolder, string filename)
		{
			string unsuccessfulImportDirectory = Path.Combine(importFolder, UnsuccessfulDirectoryName);

			if (!Directory.Exists(unsuccessfulImportDirectory))
			{
				try
				{
					Directory.CreateDirectory(unsuccessfulImportDirectory);
				}
				catch // abort!
				{
					LogMessage(TraceEventType.Warning, Res.GetString("DEB18CF9-7A0D-4A30-AC2F-A27FF9599F04", "Could not create unsuccessful folder at {0}. Please check the permission setting for this folder or create it manually.", unsuccessfulImportDirectory));
					return;
				}
			}

			try
			{
				File.SetAttributes(filename, FileAttributes.Normal);
				UniqueFilenameGenerator nameGenerator = new UniqueFilenameGenerator();
				string newFilePath = nameGenerator.GetNewUniqueFilePath(unsuccessfulImportDirectory, Path.GetFileNameWithoutExtension(filename) + GetTimestamp().ToShortDateString() + Path.GetExtension(filename));
				File.Move(filename, newFilePath);
				LogMessage(TraceEventType.Information, Res.GetString("9782BF86-E185-4136-8F0C-9B98EAA96F65", "File '{0}' has been moved to unsuccessful folder.", filename));
			}
			catch (Exception ex) // file in use, etc
			{
				LogMessage(TraceEventType.Information, Res.GetString("05B74751-C14C-476E-BA24-FC7D074C972D", "Couldn't move file '{0}' to unsuccessful folder. Exception message: {1}", filename, ex.Message));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a folder name.")]
		internal const string UnsuccessfulDirectoryName = "Unsuccessful Imports";

		protected virtual ZDateTime GetTimestamp()
		{
			return ZDateTime.Now;
		}

		#endregion

		#region Email Import

		sealed class ImportEmailNotificationHandler : INotificationHandler
		{
			readonly BatchImportManager manager;
			public ImportEmailNotificationHandler(BatchImportManager manager)
				=> this.manager = manager;
			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				this.manager.LogMessage(TraceEventType.Error, Res.GetString("53a3bfc9-bdf0-4761-afd0-a51ccc936b4d", @"An error occurred while importing the email.
Caption: {0}
Message: {1}", caption, message));
			}

			public void ReportInformation(string message, string caption)
			{
				this.manager.LogMessage(TraceEventType.Information, Res.GetString("e5c57bb2-b2d1-4629-875d-729c49770434", @"Importing email information.
Caption: {0}
Message: {1}", caption, message));
			}
		}

		void ImportEmails(CancellationToken token)
		{
			var distinctMailItems = new HashSet<ZGuid>();

			var importableMailQueue = new DbOnlyBusinessObjectQueue<MailItem>(((QueryMailFilter)mailFilter).LoadQuery());
			importableMailQueue.ProcessBatch((importableMails, e) =>
			{
				var ignoreEmailNum = 0;
				if (token.IsCancellationRequested)
				{
					Thread.MemoryBarrier();
					e.Cancel = true;
				}
				else
				{
					if (importableMails.Length == 0)
					{
						return;
					}

					LogMessage(TraceEventType.Information, Res.GetString("56683E21-B8F3-433F-B6EF-9D1981045BB5", "Importing emails in batch of size {0}.", importableMails.Length));

					foreach (var email in importableMails)
					{
						if (token.IsCancellationRequested)
						{
							Thread.MemoryBarrier();
							e.Cancel = true;
						}

						LogMessage(TraceEventType.Information, Res.GetString("11F317B7-402F-4DA9-ADEB-8AF2332C3796", "Importing email from '{0}' with subject '{1}'.",
						email.MI_From, email.MI_Subject));

						var lockedRunResult = Db.Connection.RunLocked(FormattableString.Invariant($"DMI:ImportingEmail:{email.PK}"),
							isFirstRun => ImportEmailWithMutex(email, token),
							() => EmailHasAlreadyBeenImported(email));

						if (lockedRunResult == LockedProcessResult.Completed)
						{
							distinctMailItems.Add(email.PK);
						}
						if (lockedRunResult == LockedProcessResult.AlreadyBeingProcessed)
						{
							ignoreEmailNum++;
						}

						LogRunLockedResultMessage(lockedRunResult, Res.GetString("D34E3DEA-EB86-45E6-B54B-9AF9B3BCD964", "Email"), email.MI_Subject);
					}
				}

				LogMessage(TraceEventType.Information, Res.GetString("88747003-6003-4a23-92a2-6d56d64a069d", "Import Run Completed, processed {0} email(s), ignored {1} email(s) processed by another process.", distinctMailItems.Count, ignoreEmailNum));
			}, SystemDataRegistry.Instance.MaximumNumberOfDocManagerItemsToImportInABatch.Value, token, disableDataRefresh: true);
		}

		bool EmailHasAlreadyBeenImported(MailItem email)
		{
			email.Reload();
			return email.MI_Status != MailStatus.Queued;
		}

		void ImportEmailWithMutex(MailItem email, CancellationToken token)
		{
			email.Factory.SetContext(BusinessContext.NonAccountingCode);

			if (DocManagerRegistry.Instance.RestrictEmailAllocationForOrgContacts.Value
				&& IsEmailSubjectValidForDirectImport(email)
				&& !IsEmailAddressFromOrgContact(email)
				&& !IsEmailAddressAllowedForAllocation(email.MI_From))
			{
				LogMessage(TraceEventType.Warning, Res.GetString("FAC9C161-C86D-4FE8-869D-D6D8782798AC", "Unable to allocate document. Allocation has been restricted to only accept Organization Contact email addresses and {0} was not found in the Organization Contacts.", email.MI_From));
				MailBatchProcessor.MarkFailed(email);
				email.Factory.Save();
				return;
			}

			//have to have an extra layer of retry because changes to StorageMain.SM_DB can't be merged, and thus aren't handled by ProcessWithSaveExceptionHandling/ConcurrencyResolved
			var maxAttempts = 3;
			for (var i = 0; i < maxAttempts; ++i)
			{
				var documentFactory = GetDocumentFactory(email.Factory);
				documentFactory.SetContext(BusinessContext.NonAccountingCode);

				var staffMember = GetStaffMember(email);
				var branchToImportEmail = GlbBranch.CurrentBranch ?? staffMember?.HomeBranch;
				using (staffMember == null
					? null
					: Env.SetTemporaryUserContext(
						staffMember.PK.ToGuid(),
						branchToImportEmail?.PK.ToGuid() ?? Guid.Empty,
						GlbDepartment.CurrentDepartment?.PK.ToGuid() ?? staffMember.HomeDepartment?.PK.ToGuid() ?? Guid.Empty))
				{
					var success = ImportEmail(documentFactory, email, staffMember, token);
#if DEBUG
					ProcessingEmail = email;
					BranchUsedForImportEmail = branchToImportEmail;
					ImportEmailThrowExceptionForTest(email.MI_Body, documentFactory);
#endif
					if (success)
					{
						try
						{
							MailBatchProcessor.MarkSuccess(email);
							ZExceptionReporting.ProcessWithSaveExceptionHandling(documentFactory.Save, null, true, false, 1, new ImportEmailNotificationHandler(this));
							return;
						}
						catch (Exception ex) when (ex is ZCannotSaveException || ex is ZSaveException)
						{
							if (i >= maxAttempts - 1)
							{
								LogMessage(TraceEventType.Error, Res.GetString("d29b27fb-c4fa-45d3-9927-1ca4decc09df", "Error while saving email to database. Email '{0}' could not be imported. {1}", email.MI_Subject, ex.Message) + "\r\n" + ex);
							}
						}
					}
				}
			}

			var factory = new BusinessObjectFactory();
			var emailReloaded = factory.Load<MailItem>(email.PK);
			MailBatchProcessor.MarkFailed(emailReloaded);
			factory.Save();
			email.Reload();
		}

#if DEBUG
		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Exception thrown just for Unit Test")]
		void ImportEmailThrowExceptionForTest(string content, DocumentFactory factory)
		{
			Exception testEx = null;
			if (content == "Test unhandled ZSaveException")
			{
				testEx = new Exception();
			}
			else if (content == "Test deadlock exception")
			{
				testEx = SqlExceptionBuilder.CreateSqlException(1205, "There is a deadlock sql exception");
			}
			else if (content == "Email Factory Cannot Save")
			{
				testEx = new Exception();
				factory.FactoryForEverythingExceptEDocs.Saving += f => throw testEx;
			}

			if (testEx != null)
			{
				factory.Saving += (f) => ThrowZSaveExceptionForTest(testEx);
			}
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Exception thrown just for Unit Test")]
		[SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
		void ThrowZSaveExceptionForTest(Exception ex)
		{
			var table = new DataTable("BlahBlah");
			var col = new DataColumn("PK", typeof(Guid));
			table.Columns.Add(col);
			table.PrimaryKey = new DataColumn[] { col };
			var row = table.NewRow();
			var innerEx = new ZDataException(ex, row, Db.Connection);
			var factory1 = new BusinessObjectFactory();
			throw new ZSaveException(innerEx, factory1);
		}

		internal MailItem ProcessingEmail { get; private set; }

		internal GlbBranch BranchUsedForImportEmail { get; private set; }
#endif

		[SuppressMessage("CargoWiseOne", "CW1012:ProductNamingRule", Justification = "Part of email subject allocation information")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Windows Error Message")]
		bool ImportEmail(DocumentFactory masterFactory, MailItem email, GlbStaff staffMember, CancellationToken token)
		{
			var importSuccessful = true;
			var filesNotImported = new List<string>();

			try
			{
				var codeRetriever = IsEmailSubjectValidForDirectImport(email) ?
					new AllocationCodeRetriever(masterFactory, email.MI_Subject, true) :
					new AllocationCodeRetriever(masterFactory, string.Empty, true);

				if (staffMember != null && !staffMember.GS_IsSystemAccount)
				{
					var securityError = ValidateEDocsSecurity(codeRetriever.DocType);
					if (!string.IsNullOrEmpty(securityError))
					{
						LogMessage(TraceEventType.Error, securityError);
						return false;
					}
				}

				foreach (MailAttachment attachment in email.MailAttachments)
				{
					token.ThrowIfCancellationRequested();
					var filenameOnly = MakeFilenameSafe.MakeSafeAndFixFileExtension(attachment.MA_FileName, '_');

					try
					{
						if (!ImportFile(masterFactory, attachment.MA_Data, filenameOnly.Trim(), codeRetriever))
						{
							filesNotImported.Add(attachment.MA_FileName);
						}
					}
					catch (ArgumentException ex)
					{
						if (ex.Message != "Illegal characters in path.")
						{
							throw;
						}
						LogMessage(TraceEventType.Error, Res.GetString("814b1b0e-6200-4449-8f4e-8410698e3fc8", "There are illegal characters in attachment file name, the following attachment is not imported: {0}", attachment.MA_FileName));
					}
				}

				if (filesNotImported.Count > 0)
				{
					if (staffMember != null)
					{
						SendIncorrectFileFormatImportFailureEmail(email, staffMember, filesNotImported.ToArray());
						LogMessage(TraceEventType.Information, Res.GetString("97b4e85a-388a-4e71-a934-85144a368340", "An email has been sent to {0} explaining why the import failed.", staffMember.GS_FullName));
					}

					importSuccessful = false;
				}
			}
			catch (Exception ex)
			{
				if (ex is AllocationCodeFormatException || ex is IncorrectVisibleCompanyBranchDepartmentException || ex is NonUniqueAllocationCodeException)
				{
					LogMessage(TraceEventType.Error, Res.GetString("D2E2B610-98BC-44A6-ABE4-84D73EB14E1F", "Failed to allocate document. Error message: {0}", ex.Message));
					SendIncorrectAllocationCodesImportFailureEmail(email, staffMember, ex.Message);
					importSuccessful = false;
				}
				else
				{
					throw;
				}
			}

			return importSuccessful;
		}

		GlbStaff GetStaffMember(MailItem email)
		{
			string emailAddress = email.GetFromEmailAddress();
			ZQuery query = new ZQuery(GlbStaffSchema.GS_EmailAddress, emailAddress);
			return (GlbStaff)email.Factory.LoadTop1(typeof(GlbStaff), query);
		}

		string ValidateEDocsSecurity(ZString docType)
		{
			return GenerateAllocationErrorMessageIfNotAllowed(Env.Security.AllocateDocuments) ??
				(!docType.IsEmpty ? GenerateAllocationErrorMessageIfNotAllowed(Env.Security.GetDocumentTypeUploadCheckPoint(docType)) : null);
		}

		string GenerateAllocationErrorMessageIfNotAllowed(SecurityCheckpoint checkPoint)
		{
			if (checkPoint == null || checkPoint.IsAllowed)
			{
				return null;
			}

			return Res.GetString(
				"03F573CF-56C4-4AD0-A1EE-46B458384E4F",
				"Unable to allocate document. Allocation has been rejected because the staff user does not have appropriate security rights enabled for the security setting at {0}.",
				checkPoint.DisplayTextPathToSecurityRight);
		}

		readonly IMailFilter mailFilter = GetImportMailFilter();

		[SuppressMessage("CargoWiseOne", "CW1012:ProductNamingRule", Justification = "Backward compatability support")]
		[MailFilter(MailFilterCodes.DocumentImportManager)]
		public static IMailFilter GetImportMailFilter()
		{
			return new BatchImportManagerMailFilter(MailFilterCodes.DocumentImportManager, new ZQuery());
		}

		public class BatchImportManagerMailFilter : QueryMailFilter
		{
			public BatchImportManagerMailFilter(string code, ZQuery query) : base(code, query)
			{
			}

			public override bool CanProcess(IMailItem itemDummy)
			{
				var item = (MailItem)itemDummy;
				if (item.MI_Direction != MailDirection.Receive || item.MI_Status != MailStatus.Queued)
				{
					return false;
				}

				if (IsEmailSubjectValidForDirectImport(item))
				{
					return true;
				}

				return IsEmailAddressAllowedForAllocation(item.MI_From);
			}
		}

		static bool IsEmailSubjectValidForDirectImport(MailItem email)
		{
			return email.MI_Subject.StartsWith("[ediDocManager", StringComparison.OrdinalIgnoreCase) || email.MI_Subject.StartsWith("[DocManager", StringComparison.OrdinalIgnoreCase);
		}

		static bool IsEmailAddressAllowedForAllocation(string fromEmail)
		{
			if (SystemDataRegistry.Instance.EmailAddressesAllowedForImport.Value.Count > 0)
			{
				var acceptedEmailAddresses = SystemDataRegistry.Instance.EmailAddressesAllowedForImport.Value.GetAllCodes();
				foreach (var email in acceptedEmailAddresses)
				{
					var emailReplacement = Regex.Escape(email).Replace("\\?", ".").Replace("\\*", ".*");
					if (Regex.IsMatch(fromEmail, FormattableString.Invariant($"(?:.*<)?{emailReplacement}>?"), RegexOptions.IgnoreCase))
					{
						return true;
					}
				}
			}

			return false;
		}

		static bool IsEmailAddressFromOrgContact(MailItem email)
		{
			var emailAddress = email.GetFromEmailAddress();
			if (string.IsNullOrEmpty(emailAddress))
			{
				return false;
			}

			return email.Factory.Exists(typeof(OrgContact), new ZQuery(OrgContactSchema.OC_Email, emailAddress));
		}

		void SendIncorrectFileFormatImportFailureEmail(MailItem email, GlbStaff staffMember, string[] filesNotImported)
		{
			string sentDate = email.MI_ReceivedDateTime.ToLongTimeString();
			string listOfFiles = string.Join("\t" + System.Environment.NewLine, filesNotImported);
			string listOfSupportedExtensions = string.Join(",", FileImporter.SupportedFileFormats);
			string body = Res.GetString("26adda70-6577-4050-8fd8-09fdee9c3b46", @"The DocManager Service Task could not import the attachments sent on {0}
			
{1}

Are you trying to add non-image files?
The DocManager Service Task will recognize these file types: {2}. If the files you are trying to import are not image files, or cannot be converted to image files, you need to specify the 3-letter Reference, 3 or 4 letters Document Type and the unique code in the subject line of your email for the system to automatically allocate your file to the correct place.

	[DocManager (3-letter Reference) (3 or 4 letters Document Type) (Unique Code)]

For example, assuming you have created a SALES PROFILE document type under Config > Reference Files > Document Type with a 3-letter code of PRO, and you want to allocate your documents to the organization ABCSYD, put this in the email subject:

	[DocManager ORG PRO ABCSYD]

You can optionally specify a company code to ensure the documents are allocated to the correct record:

	[DocManager CRT CRS AASDRA C:DEM]

Where DEM is the company code with which this Client Rates record is associated.

The codes to use in the email subject are the same as if you were using the Allocate eDocs form under Operations > DocManager > Allocate eDocs.", sentDate, listOfFiles, listOfSupportedExtensions);

			SendEmail(staffMember, body, sentDate);
		}

		void SendIncorrectAllocationCodesImportFailureEmail(MailItem email, GlbStaff staffMember, string message)
		{
			int numberOfAttachments = email.MailAttachments.Count;
			string[] attachmentNames = new string[numberOfAttachments];
			for (int i = 0; i < numberOfAttachments; i++)
			{
				attachmentNames[i] = EscapeFormattingCharacters(email.MailAttachments[i].MA_FileName);
			}

			string sentDate = email.MI_ReceivedDateTime.ToLongTimeString();
			string listOfFiles = string.Join("\t" + System.Environment.NewLine, attachmentNames);
			string body = Res.GetString("a80f94d4-1bbe-4ead-8c90-19c7aa3cc3f8", @"The DocManager Service Task could not import the attachments sent on {0}:

{1}

{2}

For example, assuming you have created a SALES PROFILE document type under Config > Reference Files > Document Type with a 3-letter code of PRO, and you want to allocate your documents to the organization ABCSYD, put this in the email subject:

	[DocManager ORG PRO ABCSYD]

You can optionally specify a company code to ensure the documents are allocated to the correct record:

	[DocManager CRT CRS AASDRA C:DEM]

Where DEM is the company code with which this Client Rates record is associated.

The codes to use in the email subject are the same as if you were using the Allocate eDocs form under Operations > DocManager > Allocate eDocs.", sentDate, listOfFiles, EscapeFormattingCharacters(message));

			SendEmail(staffMember, body, sentDate);
		}

		string EscapeFormattingCharacters(string filename)
		{
			StringBuilder builder = new StringBuilder(filename);
			builder.Replace("{", "{{");
			builder.Replace("}", "}}");
			return builder.ToString();
		}

		void SendEmail(GlbStaff staffMember, string body, string sentDate)
		{
			if (staffMember != null)
			{
				EmailDef emailToSend = new EmailDef();
				emailToSend.Subject = Res.GetString("c4f37fef-6b6c-4c25-bdc6-4ca6361ec05f", "DocManager Import Service Task Failed ({0})", sentDate);
				emailToSend.AddRecipientForUserCommunication(staffMember.GS_EmailAddress);
				emailToSend.Body = body;
				Env.OutgoingMailManager.CreateAndSave(emailToSend);
			}
		}

		#endregion

		void LogMessage(TraceEventType eventType, string statusMessage)
		{
			LogProgress?.Invoke(new LogEventArgs(eventType, statusMessage));
		}
		public event Action<LogEventArgs> LogProgress;
	}
}
