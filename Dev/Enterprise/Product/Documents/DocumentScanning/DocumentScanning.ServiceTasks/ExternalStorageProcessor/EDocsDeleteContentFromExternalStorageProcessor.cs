using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.DocumentScanning.ServiceTasks
{
	class EDocsDeleteContentFromExternalStorageProcessor : BaseEDocsContentToExternalStorageProcessor
	{
		protected override string ServiceFailedKey => "DedExecutionFailed";

		protected override string FailToExecuteNotificationSubject => (NoResString)"DED service failed to execute";

		public override void Run(ILogger serviceLogger, CancellationToken token)
		{
			var shouldLoop = true;
			var excludedPKs = new List<ZGuid>();
			var totalDocsDeleted = 0;
			var externalStorageProcessor = ExternalPersister;

			if (externalStorageProcessor != null)
			{
				serviceLogger.Information((NoResString)"Run Start.");
				while (shouldLoop)
				{
					var factory = new BusinessObjectFactory() { RefreshEnabled = false };
					var query = new ZDBOnlyQuery(typeof(StorageDocsToDelete)) { MaximumRows = BatchSize };

					if (excludedPKs.Any())
					{
						query.AddToFilter(StorageDocsToDeleteSchema.PK, SQLComparisonOperator.NotEqual, excludedPKs);
					}

					var batch = factory.Load(typeof(StorageDocsToDelete), query);

					if (batch.Any())
					{
						foreach (StorageDocsToDelete doc in batch)
						{
							var storageDocsGuid = doc.SCD_StorageDocIdentifier;

							try
							{
								if (externalStorageProcessor.Delete(storageDocsGuid))
								{
									doc.Delete();
									totalDocsDeleted++;
								}
								else
								{
									excludedPKs.Add(doc.PK);
								}
							}
							catch (Exception ex)
							{
								if (ex is ExternalStorageException)
								{
									throw new HostedServiceException(ex.Message, ex);
								}

								var errorMessage = FormattableString.Invariant($"Failed to delete eDocs from external storage with last StorageDocs PK: {storageDocsGuid}");
								serviceLogger.Error(errorMessage, ex);
								ErrorReporter.ReportOnce("StorageDocs_DeleteFromExternalStorage_Error", errorMessage, ex);

								excludedPKs.Add(doc.PK);
							}
						}

						factory.Save();

						if (token.IsCancellationRequested)
						{
							serviceLogger.Information((NoResString)"Stopped processing because of cancellation");
							shouldLoop = false;
						}
					}
					else
					{
						shouldLoop = false;
					}
				}

				serviceLogger.Information(FormattableString.Invariant($"Run Completed, successfully deleted {totalDocsDeleted} eDocs from external storage"));
			}
			else
			{
				serviceLogger.Information($"Not required to run.");
			}
		}
	}
}
