using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.Actions
{
	public class ArchiveImageGenerationAction : IArchiveImageGenerationAction
	{
		public void Setup(IArchiveLogger logger, IArchiveSet archiveSet, IArchiveableBusinessObjectProviderCache providerDictionary, IArchiveSystemCache systemCache)
		{
			_ = Argument.NotNull(logger, "logger");
			_ = Argument.NotNull(archiveSet, "archiveSet");
			_ = Argument.NotNull(systemCache, "systemCache");

			documentCommandCache = systemCache.Retrieve<DocumentCommandCache>();
			if (documentCommandCache == null)
			{
				documentCommandCache = new DocumentCommandCache();
				systemCache.Add(documentCommandCache);
			}

			noteCache = systemCache.Retrieve<DocumentNoteCache>();
			if (noteCache == null)
			{
				noteCache = new DocumentNoteCache();
				systemCache.Add(noteCache);
			}

			factoryProvider = systemCache.Retrieve<BusinessObjectFactoryProvider>();
			if (factoryProvider == null)
			{
				factoryProvider = new BusinessObjectFactoryProvider();
				systemCache.Add(factoryProvider);

				factoryProvider.Current.RefreshEnabled = false;
				factoryProvider.Current.Saving += delegate
				{ throw new InvalidOperationException("This is a readonly Factory."); };
			}

			initialMemoryUsed = systemCache.Retrieve<long>();
			if (initialMemoryUsed == 0L)
			{
				initialMemoryUsed = GC.GetTotalMemory(false);
				systemCache.Add(initialMemoryUsed.Value);
			}

			this.logger = logger;
			this.archiveSet = archiveSet;
			this.providerDictionary = providerDictionary;
			initialised = true;
			includeTimeTakenInTheARCLogs = SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.Value;
		}

		[SuppressMessage("CargoWiseOne", "CW1056:DoNotUseGCCollect", Justification = "Only calling GC.Collect after archive manager uses a large amount of memory and clears its caches")]
		public void Execute()
		{
			if (!initialised)
			{
				throw new InvalidOperationException("Setup(..) must be called before you can call Execute() on GenerateArchiveImageAction object");
			}

			foreach (var item in archiveSet.GetArchiveItems())
			{
				if (providerDictionary.HasProvider(item.PKColumn.TableName))
				{
					var provider = providerDictionary.GetProvider(item.PKColumn.TableName);

					if (initialMemoryUsed.HasValue)
					{
						var currentMemoryUsed = GC.GetTotalMemory(false);

						if (currentMemoryUsed - initialMemoryUsed > 209715200L) //200MB allowed memory
						{
							noteCache.Clear();
							documentCommandCache.Clear();
							factoryProvider.CreateNewWithoutSave();

							GC.Collect();
						}
					}

					var factory = factoryProvider.Current;

					var archiveableBizOs = provider.LoadArchiveableBusinessObjects(item, factory);
					foreach (var archiveableBizO in archiveableBizOs)
					{
						if (archiveableBizO == null || archiveableBizO.ArchiveableBusinessObject == null)
						{
							throw new InvalidOperationException($"provider: {provider.GetType()} should not return a null ArchiveableBusinessObject. Archive Bizo type: {archiveableBizO.ArchiveableBusinessObject.GetType()}.");
						}

						if (!(archiveableBizO.ArchiveableBusinessObject is IDocManagerSupport))
						{
							throw new InvalidOperationException($"provider: {provider.GetType()} should return an ArchiveableBusinessObject that implements IDocManagerSupport. Archive Bizo type: {archiveableBizO.ArchiveableBusinessObject.GetType()}.");
						}

						if (!(archiveableBizO is IArchiveableBusinessObjectWithOwnImageGenerationLogic))
						{
							if (!(archiveableBizO.ArchiveableBusinessObject is IStmNoteParent))
							{
								throw new InvalidOperationException($"provider: {provider.GetType()} should return an ArchiveableBusinessObject that implements IStmNoteParent. Archive Bizo type: {archiveableBizO.ArchiveableBusinessObject.GetType()}.");
							}
							else if (!(archiveableBizO.ArchiveableBusinessObject is IDocumentSupportable))
							{
								throw new InvalidOperationException($"provider: {provider.GetType()} should return an ArchiveableBusinessObject that implements IDocumentSupportable. Archive Bizo type: {archiveableBizO.ArchiveableBusinessObject.GetType()}.");
							}
						}

						var context = new TemporaryUserContext()
						{
							StaffLoginName = Env.CurrentUser.LoginName,
							BranchPK = archiveableBizO.BranchPK == Guid.Empty ? Env.CurrentBranch.PK : archiveableBizO.BranchPK,
							DepartmentPK = Env.CurrentDepartment.PK
						};

						using (context.Set())
						{
							ExecuteForBizO(item, archiveableBizO);
						}
					}
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Each task would add its result into concurrent bag. She'll be right.")]
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "This is an optimisation, instead of loading back all references and then delete them.")]
		void ExecuteForBizO(IArchiveItem item, IArchiveableBusinessObject archiveable)
		{
			var docSupportable = archiveable.ArchiveableBusinessObject as IDocumentSupportable;
			var noteParent = archiveable.ArchiveableBusinessObject as IStmNoteParent;
			var docSupport = (IDocManagerSupport)archiveable.ArchiveableBusinessObject;

			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			masterFactory.NameForDebugging = "Document Factory";
			masterFactory.RefreshEnabled = false;
			var storageMain = masterFactory.RetrieveExistingOrCreateStorageMainForPK(archiveable.ArchiveableBusinessObject.PK, archiveable.ArchiveableBusinessObject, docSupport.DocManagerInfo.DocManagerCode);

			var filesToAdd = new ConcurrentBag<List<FileParams>>();

			if (archiveable is IArchiveableBusinessObjectWithOwnImageGenerationLogic archiveableWithOwnImageGenerationLogic)
			{
				foreach (var descriptor in archiveableWithOwnImageGenerationLogic.GenerateArchiveImages())
				{
					_ = storageMain.AddFileOrDocument(descriptor.ArchiveImage, new AddFileOrDocumentDto
					{
						FileName = descriptor.Filename,
						DocumentType = descriptor.DocumentType,
					});
				}
			}
			else
			{
				var commandList = documentCommandCache.GetDocumentCommands(docSupportable.DocumentSupporter.BusinessContext);

				var documentsToGenerate = new List<DocumentCommand>();

				foreach (var documentCommand in commandList)
				{
					try
					{
						documentCommand.Parent = docSupportable;

						if (documentCommand.IsApplicable)
						{
							var alreadyGenerated = true;

							foreach (var pivot in documentCommand.Documents.Cast<StmMenuTemplatePivot>())
							{
								if (pivot.DocType != null && DocTypeNotAlreadyGenerated(storageMain, pivot.DocType.RT_DocType))
								{
									alreadyGenerated = false;
									break;
								}
							}

							if (!alreadyGenerated)
							{
								documentsToGenerate.Add(documentCommand);
							}
						}
					}
					finally
					{
						documentCommand.Parent = null;
					}
				}

				var sw = new Stopwatch();
				if (SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.Value)
				{
					sw.Start();
				}

				foreach (var docToGenerate in documentsToGenerate)
				{
					filesToAdd.Add(GenerateDocumentsToEDocs(item, archiveable, docToGenerate).ToList());
				}

				if (archiveable.ArchiveDocuments != null)
				{
					// A.K it appears there are usually 1-2 DocumentDescriptors per archivable BizO, so no point to parallelise this bit
					// also DocTypeNotAlreadyGenerated is NOT particularly thread safe as it touches factory
					// Example of archiveable with DocumentDescriptors: C:\git\wtg\CargoWise\Dev\Enterprise\Product\Operations\Accounting\Business\ArchiveManager\AccountingArchiveableBusinessObjectProvider.cs : 78

					foreach (var docDescriptor in archiveable.ArchiveDocuments)
					{
						if (string.IsNullOrEmpty(docDescriptor.DocTypeToCheck) || DocTypeNotAlreadyGenerated(storageMain, docDescriptor.DocTypeToCheck))
						{
							filesToAdd.Add(GenerateDocumentsToEDocs(item, archiveable, docDescriptor.MenuName).ToList());
						}
					}
				}

				if (SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.Value)
				{
					sw.Stop();
					if (archiveSet != null)
					{
						archiveSet.TimeTakenToGenerateDocuments += sw.ElapsedMilliseconds;
					}
				}
			}

			var filesToAddFlattened = new List<FileParams>();

			filesToAddFlattened.AddRange(filesToAdd.ToArray().SelectMany(f => f));

			foreach (var fileParam in filesToAddFlattened)
			{
				_ = storageMain.AddFileOrDocument(fileParam.imageBytes, new AddFileOrDocumentDto
				{
					FileName = fileParam.fileName,
					DocumentType = fileParam.docType,
					FileAction = fileParam.fileAction,
					Description = fileParam.menuName,
					Source = fileParam.source,
					IsArchiving = fileParam.isArchiving,
				});
			}

			if (archiveSet != null)
			{
				archiveSet.TotalNumberOfDocumentsGeneratedInSet += filesToAddFlattened.Count;
			}

			storageMain.SM_Archived = ZDateTime.UtcNow;

			var sequence = 0;

			var refSet = new HashSet<StorageReferenceValues>();
			AddReferenceValues(refSet, storageMain.PK, archiveable.NaturalKey, sequence++);

			SetParentReferenceKey(item, archiveable.NaturalKey, refSet, masterFactory, storageMain, sequence++);

			foreach (var key in archiveable.AdditionalKeys)
			{
				if (!string.IsNullOrEmpty(key.Value))
				{
					AddReferenceValues(refSet, storageMain.PK, key, sequence++);
				}
			}

			var mergeQuery = BuildSql();

			var mergeRefAction = new SaveInTransactionDelegateAction(Db.Connection, delegate
			{
				using (var command = Db.Connection.Command(mergeQuery))
				{
					AddSqlParameters(refSet, command);
					_ = command.ExecuteNonQuery();
				}

				return ChangedTableNames.All;
			});

			var transactionPacticipants = new ITransactionParticipant[] { masterFactory, mergeRefAction };

			lock (lockObj)
			{
				try
				{
					BusinessObjectFactory.SaveTogether(transactionPacticipants);
				}
				catch (ZSaveException ex) when (ex.InnerException.Message.Contains(ReadOnly))
				{
					var storageMainDBNumber = storageMain.SM_DB;
					var dbWriteableState = masterFactory.GetDbWriteableState(storageMainDBNumber);
					var message = $"The StorageMain database='{masterFactory.GetDatabaseName(storageMainDBNumber)}', and its writeable state='{dbWriteableState}'";

					ex.Data.Add("ArchiveImageGenerationAction.ExecuteForBizOInfo", message);

					throw;
				}
			}
		}

		static readonly object lockObj = new();

		const string ReadOnly = "ReadOnly";

		public class StorageReferenceValues
		{
			public StorageReferenceValues(ZGuid storageMainPk, ArchiveReferenceKey key, int sequence)
			{
				this.storageMainPk = storageMainPk;
				code = key.KeyType.Code;
				reference = key.Value;
				this.sequence = sequence;
			}

			public ZGuid storageMainPk;
			public int sequence;
			public string code;
			public string reference;

			public override bool Equals(object obj)
			{
				var that = (StorageReferenceValues)obj;
				return storageMainPk == that.storageMainPk
					&& code == that.code
					&& reference == that.reference;
			}

			public override int GetHashCode()
				=> storageMainPk.GetHashCode() ^ code.GetHashCode() ^ reference.GetHashCode();
		}

		[SuppressMessage("CargoWiseOne", "CW1044:DoNotUseFactory.GetDatabaseCount", Justification = "Baseline. I don't want to change it now. This PR should be for prallelisation code only")]
		bool DocTypeNotAlreadyGenerated(StorageMain storageMain, string docType)
		{
			if (storageMain.IsInDatabase)
			{
				var numFactory = storageMain.MasterFactory.GetFactory(storageMain.SM_DB);
				var query = new ZQuery(StorageDocsSchema.SC_SM, storageMain.PK);
				_ = query.AddToFilter(StorageDocsSchema.SC_DocType, docType);
				var count = numFactory.GetDatabaseCount(typeof(StorageDocs), query);
				return count <= 0;
			}
			else
			{
				return true;
			}
		}

		static void AddReferenceValues(HashSet<StorageReferenceValues> refSet, ZGuid storageMainPK, ArchiveReferenceKey key, int sequence)
			=> refSet.Add(new StorageReferenceValues(storageMainPK, key, sequence));

		public static string BuildSql()
		{
			var sqlBuilder = new StringBuilder(400);

			_ = sqlBuilder.AppendLine((NoResString)"MERGE INTO " + AutoStorageReference.Schema.TableName + (NoResString)" USING @referenceSetTVP");
			_ = sqlBuilder.AppendLine((NoResString)@"ON " + AutoStorageReference.Schema.SR_SM + (NoResString)" = StorageMainPk AND "
 + AutoStorageReference.Schema.SR_TYPE + (NoResString)" = ReferenceType AND "
 + AutoStorageReference.Schema.SR_Reference + (NoResString)@" = Reference
WHEN NOT MATCHED THEN
	INSERT (" + AutoStorageReference.Schema.PK + (NoResString)", "
		  + AutoStorageReference.Schema.SR_SM + (NoResString)", "
		  + AutoStorageReference.Schema.SR_Sequence + (NoResString)", "
		  + AutoStorageReference.Schema.SR_TYPE + (NoResString)", "
		  + AutoStorageReference.Schema.SR_Reference + (NoResString)") VALUES (NEWID(), StorageMainPk, ReferenceSequence, ReferenceType, Reference);");

			return sqlBuilder.ToString();
		}

		public static void AddSqlParameters(HashSet<StorageReferenceValues> refSet, DbCommand command)
		{
			var dataTable = new DataTable();
			_ = dataTable.Columns.Add("StorageMainPk", typeof(Guid));
			_ = dataTable.Columns.Add("ReferenceSequence", typeof(int));
			_ = dataTable.Columns.Add("ReferenceType", typeof(string));
			_ = dataTable.Columns.Add((NoResString)"Reference", typeof(string));

			foreach (var elem in refSet)
			{
				var row = dataTable.NewRow();
				row["StorageMainPk"] = elem.storageMainPk.ToGuid();
				row["ReferenceSequence"] = elem.sequence;
				row["ReferenceType"] = elem.code;
				row["Reference"] = elem.reference;

				dataTable.Rows.Add(row);
			}

			command.AddTableValuedParameter("@referenceSetTVP", "dbo.TVP_StorageReference", dataTable);
		}

		IEnumerable<FileParams> GenerateDocumentsToEDocs(IArchiveItem archiveitem, IArchiveableBusinessObject archiveable, string archiveDocumentMenuName)
		{
			var docSupportable = archiveable.ArchiveableBusinessObject as IDocumentSupportable;

			var documentCommand = DocumentCommand.GetDocumentCommand(docSupportable.DocumentSupporter.Factory, docSupportable, archiveDocumentMenuName);

			return documentCommand != null
				? GenerateDocumentsToEDocs(archiveitem, archiveable, documentCommand)
				: new List<FileParams>() { };
		}

		IEnumerable<FileParams> GenerateDocumentsToEDocs(IArchiveItem archiveItem, IArchiveableBusinessObject archiveable, DocumentCommand documentCommand)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var archiveableBizO = archiveable.ArchiveableBusinessObject;
				var docSupportable = archiveableBizO as IDocumentSupportable;
				var stmNoteParent = archiveableBizO as IStmNoteParent;

				documentCommand.Parent = docSupportable;

				using var note = DocumentNote.LoadNote(stmNoteParent);
				var fieldList = note.GetCompleteFieldList(noteCache); // it's unsafe to use noteCache for parallel document generation. ThreadSentry throws excepions

				try
				{
					using var printSet = new DocumentPrintSet(documentCommand, fieldList);
					// A.K: do we really need to save to Disk and then read binaries to add them to the eDocs?
					// Any chance we could do this in memory and avoid using [slow] file system?

					var instruction = new DeliveryInstructions();
					instruction.Destination = DeliveryInstructionDestination.Memory;
					instruction.Recipients.RemoveAndDeleteAll();
					instruction.OutputFormatOverride = (OutputFormatType)Enum.Parse(typeof(OutputFormatType), SystemDataRegistry.Instance.OnlineArchiveDocumentFormat.Value);

					return GenerateDocumentsToEDocsCore(instruction, printSet, documentCommand, archiveItem);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var documentCommandInfo = $"DocumentCommand.HumanReadableShortcutName: {documentCommand?.HumanReadableShortcutName}." +
						$"DocumentCommand.MenuTypeDescription: {documentCommand?.MenuTypeDescription}." +
						$"DocumentCommand.SU_MenuPath: {documentCommand?.SU_MenuPath}.";

					ex.Data.Add("ArchiveItemInfo", $"Affected archive item: {archiveItem?.HumanReadableName}.");
					ex.Data.Add("DocumentCommandInfo", $"Exception occurred whilst generating documents to eDocs: {documentCommandInfo}");
					throw;
				}
			}
		}

		internal IEnumerable<FileParams> GenerateDocumentsToEDocsCore(DeliveryInstructions instruction, DocumentPrintSet printSet, DocumentCommand docCommand, IArchiveItem archiveItem)
		{
			var result = Enumerable.Empty<FileParams>();

			try
			{
				Stopwatch sw = null;
				if (includeTimeTakenInTheARCLogs)
				{
					sw = new Stopwatch();
					sw.Start();
				}

				printSet.Run(instruction);

				if (includeTimeTakenInTheARCLogs)
				{
					sw.Stop();
				}

				result = GenerateFileParamsFromReadFiles(archiveItem, instruction.OutputForMemoryDeliveryMethod, docCommand, sw);
			}

			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ArchiveImageRunPrintEDocsException",
					$"The following exception was thrown when generating eDocs during archiving: {ex.Message}. Archive Item {archiveItem?.HumanReadableName}. " +
					$"DocumentCommand.HumanReadableShortcutName: {docCommand?.HumanReadableShortcutName}. DocumentCommand.MenuTypeDescription: {docCommand?.MenuTypeDescription}." +
					$"DocumentCommand.SU_MenuPath: {docCommand?.SU_MenuPath}.",
					ex);
			}

			return result;
		}

		IEnumerable<FileParams> GenerateFileParamsFromReadFiles(IArchiveItem item, List<(string fileName, string docType, byte[] imageBytes)> files, DocumentCommand docCommand, Stopwatch sw)
		{
			var result = new List<FileParams>();

			if (files.Any())
			{
				logger.LogInfo(archiveSet.SystemDescriptor.Code,
					!string.IsNullOrEmpty(item.HumanReadableName)
						? $"Generating missing {docCommand.SU_MenuName} for {item.HumanReadableName}{Helpers.GetTimeTaken(sw)}"
						: $"Generating missing {docCommand.SU_MenuName}{Helpers.GetTimeTaken(sw)}");

				foreach (var file in files)
				{
					var docType = file.docType;

					if (!ValidDocumentTypeLength(docType.Length))
					{
						var warningMessage = $"Archiving process cannot generate document for '{file.fileName}' as the DocType is invalid. The DocType was '{file.docType}'. " +
							$"Affected Document Menu was '{docCommand.HumanReadableShortcutName}'. The related Archive Item was '{item.HumanReadableName}'.";

						logger.LogWarning(archiveSet.SystemDescriptor.Code, warningMessage);
					}

					result.Add(new FileParams(file.imageBytes, file.fileName, docType, GlbStaff.CurrentUser, FileAction.CreateNew, docCommand.SU_MenuName, "", true));
				}
			}

			return result;
		}

		bool ValidDocumentTypeLength(int docTypeLength)
		{
			const int MinDocTypeLength = 3;
			return docTypeLength == MinDocTypeLength || docTypeLength == RefDocTypeSchema.RT_DocType.MaxLength;
		}

		internal class FileParams
		{
			internal FileParams(byte[] imageBytes, string fileName, string docType, GlbStaff staff, FileAction fileAction, string menuName, string source, bool isArchiving)
			{
				this.imageBytes = imageBytes;
				this.fileName = fileName;
				this.docType = docType;
				this.staff = staff;
				this.fileAction = fileAction;
				this.menuName = menuName;
				this.source = source;
				this.isArchiving = isArchiving;
			}
			internal byte[] imageBytes { get; }
			internal string fileName { get; }
			internal string docType { get; }
			internal GlbStaff staff { get; }
			internal FileAction fileAction { get; }
			internal string menuName { get; }
			internal string source { get; }
			internal bool isArchiving { get; }
		}

		void SetParentReferenceKey(IArchiveItem item, ArchiveReferenceKey itemKey, HashSet<StorageReferenceValues> refSet, DocumentFactory masterFactory, StorageMain storageMain, int sequence)
		{
			var currentItem = item;
			ArchiveReferenceKey parentKey = null;
			IArchiveableBusinessObject parentObject = null;

			while (currentItem.ParentPK != Guid.Empty && parentKey == null)
			{
				currentItem = archiveSet.GetArchiveItem(currentItem.ParentPK);
				if (providerDictionary.HasProvider(currentItem.PKColumn.TableName))
				{
					var provider = providerDictionary.GetProvider(currentItem.PKColumn.TableName);

					// One BizO parent should be enough - NaturalKey for the underlying record should be the same I'd expect
					var parentObjects = provider.LoadArchiveableBusinessObjects(currentItem, factoryProvider.Current);
					parentObject = parentObjects[0];
					parentKey = parentObject.NaturalKey;
				}
			}

			if (parentKey != null)
			{
				if (item.IsReversed && parentObject.ArchiveableBusinessObject is IDocManagerSupport support)
				{
					// add child natural key to parent object in reversed relationship situations.
					var parentStorageMain = masterFactory.RetrieveExistingOrCreateStorageMainForPK(currentItem.PK, parentObject.ArchiveableBusinessObject, support.DocManagerInfo.DocManagerCode);
					AddReferenceValues(refSet, parentStorageMain.PK, itemKey, 1);
				}
				else
				{
					AddReferenceValues(refSet, storageMain.PK, parentKey, sequence);
				}
			}
		}

		IArchiveSet archiveSet;
		IArchiveLogger logger;
		IArchiveableBusinessObjectProviderCache providerDictionary;
		DocumentCommandCache documentCommandCache;
		DocumentNoteCache noteCache;
		bool initialised;
		BusinessObjectFactoryProvider factoryProvider;
		long? initialMemoryUsed;
		bool includeTimeTakenInTheARCLogs;
	}
}
