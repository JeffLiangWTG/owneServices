using System;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.Actions
{
	public class ArchiveImageDeletionAction : IArchiveImageDeletionAction
	{
		IArchiveSet archiveSet;
		IArchiveLogger logger;
		IArchiveConfiguration configuration;
		bool initialised;
		BusinessObjectFactoryProvider factoryProvider;

		public void Setup(IArchiveLogger logger, IArchiveSet archiveSet, IArchiveSystemCache systemCache, IArchiveConfiguration config)
		{
			_ = Argument.NotNull(logger, "logger");
			_ = Argument.NotNull(archiveSet, "archiveSet");
			_ = Argument.NotNull(systemCache, "systemCache");
			_ = Argument.NotNull(config, "config");

			factoryProvider = systemCache.Retrieve<BusinessObjectFactoryProvider>();
			if (factoryProvider == null)
			{
				factoryProvider = new BusinessObjectFactoryProvider();
				systemCache.Add(factoryProvider);

				factoryProvider.Current.RefreshEnabled = false;
				factoryProvider.Current.Saving += delegate
				{ throw new InvalidOperationException("This is a readonly Factory."); };
			}

			this.logger = logger;
			this.archiveSet = archiveSet;
			this.configuration = config;
			initialised = true;
		}

		public void Execute()
		{
			var sw = new Stopwatch();
			if (SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.Value)
			{
				sw.Start();
			}

			if (!initialised)
			{
				throw new InvalidOperationException("Setup(..) must be called before you can call Execute() on DeleteArchiveImageAction object");
			}

			foreach (var item in archiveSet.GetArchiveItems())
			{
				try
				{
					var factory = factoryProvider.Current;

					var context = new TemporaryUserContext()
					{
						StaffLoginName = Env.CurrentUser.LoginName,
						BranchPK = Env.CurrentBranch.PK,
						DepartmentPK = Env.CurrentDepartment.PK
					};

					using (context.Set())
					{
						ExecuteForBizO(item);
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					e.Data.Add("ArchiveItemInfo", $"Exception occurred whilst processing {item}.");
					throw;
				}
			}

			if (SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.Value)
			{
				sw.Stop();
				archiveSet.TimeTakenToDeleteAllDocuments = sw.ElapsedMilliseconds;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "it shows information which i donot need.")]
		void ExecuteForBizO(IArchiveItem item)
		{
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			masterFactory.NameForDebugging = "Document Factory";
			masterFactory.RefreshEnabled = false;
			var storageMain = masterFactory.GetStorageMainForPK(item.PK);
			var eDocsFileNames = Array.Empty<string>();

			if (storageMain != null)
			{
				var storageDocsCount = storageMain.eDocs.Count;

				if (archiveSet?.SystemDescriptor?.Code == ArchiveManagerConstants.Codes.PDO && storageMain?.eDocs != null)
				{
					foreach (var eDoc in storageMain.eDocs.Cast<StorageDocsBase>())
					{
						var reference = GetReferenceForStmALog(eDoc);
						if (storageMain.DocumentOwner is EnterpriseBusinessObject ownerBizO && ownerBizO.Logs != null)
						{
							_ = ownerBizO.Logs.AddNew(AutoEvents.DocumentDeletedPermanently, reference + " by PDO Archive Schedule Task");
						}
					}
				}

				if (configuration.IsVerboseLog)
				{
					eDocsFileNames = storageMain.eDocs.Select(eDoc => eDoc.SC_Desc.ToString() + " - " + eDoc.SC_FileNameWithExtension.ToString()).ToArray();
				}

				storageMain.Delete();

				if (configuration.IsVerboseLog)
				{
					eDocsFileNames.ForEach(doc => logger.LogInfo(archiveSet.SystemDescriptor.Code, $"Deleted document {doc}."));
				}

				masterFactory.Save();
				item.TotalDocumentsDeleted = storageDocsCount;

				logger.LogInfo(archiveSet.SystemDescriptor.Code, $"Deleted all documents related to {GetHumanReadableNameWithFallback(item)}");
			}
		}

		string GetHumanReadableNameWithFallback(IArchiveItem item)
		{
			var bizoHumanReadableName = item.HumanReadableName;
			var schemaResolver = new EnterpriseSchemaResolver();

			return bizoHumanReadableName.IsNullOrEmpty()
				? schemaResolver.GetTableSchemaFromColumnNamePrefix(item.TableCode).TableName
				: bizoHumanReadableName;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:Customizable Data Translation Rule", Justification = "Log references should be in English only.")]
		string GetReferenceForStmALog(StorageDocsBase eDoc) => $"eDoc '{eDoc.SC_Desc} - {eDoc.SC_FileNameWithExtension}' {AutoEvents.DocumentDeletedPermanently.Description}";
	}
}
