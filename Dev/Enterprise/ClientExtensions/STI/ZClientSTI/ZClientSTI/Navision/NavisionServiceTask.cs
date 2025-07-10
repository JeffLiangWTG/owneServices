using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.BatchProcessor;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"ZS1", 
	"Navision Data Export", 
	"CSP", 
	typeof(Enterprise.Client.STI.Navision.NavisionServiceTask), 
	MinimumPeriod = "10minutes",
	DefaultScheduleRunEvery = "1hour"
	)]
namespace Enterprise.Client.STI.Navision
{
	public class NavisionServiceTask : ServiceProviderImpl
	{
		#region Overrides

		public override void RunTask(CancellationToken token)
		{
			if (IsEnvironmentDataValid())
			{
				ExportAllOrganisationsIfNecessary();
				ExportOrganisationsAndJobs(token);

				if (!STIDataRegistry.Instance.DisableInvoiceExport)
				{
					ExportAccountingDebtorTransactions();
				}
			}
		}

		#endregion

		#region ExportAllOrganisationsIfNecessary

		void ExportAllOrganisationsIfNecessary()
		{
			if (STIDataRegistry.Instance.ExportingOrganisationsForTheFirstTime && !STIDataRegistry.Instance.DisableOrganisationExport)
			{
				string message = "This is the First time an export of Organisations has occured, therefore All Organisations will be exported.";
				message += " This may Take some time.";
				Buffer.Notify(new InfoNotification(message));

				ExportAllOrganisations();

				STIDataRegistry.Instance.ExportingOrganisationsForTheFirstTime = false;

				Buffer.Notify(new InfoNotification("Exporting All Organisations Complete"));
				CreateListenersWithoutOrgBatchListener();
			}
		}

		void ExportAllOrganisations()
		{
			CollectionWrapperBusinessObjectReader reader = new CollectionWrapperBusinessObjectReader(OrganisationCollectionForInitialExport);
			string orgFileName = Constants.FileNamePrefixes.OrganisationFiles + FileName;

			ExportInstructions instructions = new ExportInstructions();
			instructions.BasePath = STIDataRegistry.Instance.OrganisationExportDirectory;
			instructions.MethodOfExport = ExportType.File;
			instructions.FileExtension = FileExtensionType.Csv;
			instructions.SpecifiedFilename = orgFileName;

			string fullPath = Path.Combine(STIDataRegistry.Instance.OrganisationExportDirectory, orgFileName);
			OrgFlatFileExporter exporter = new OrgFlatFileExporter(FactoryProvider.Current, instructions, fullPath);
			exporter.Export(reader, Buffer);
			OrganisationCollectionForInitialExport.RemoveAll();
		}

		protected internal virtual OrgHeaderCollection OrganisationCollectionForInitialExport
		{
			get
			{
				if (fOrganisationCollectionForInitialExport == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					query.AddSubQuery(subQuery, JoinCondition.And);

					fOrganisationCollectionForInitialExport = new OrgHeaderCollection(new BusinessObjectFactory(), query);
					fOrganisationCollectionForInitialExport.Load();
					fOrganisationCollectionForInitialExport.Sort(OrgHeaderSchema.OH_Code.Name, ListSortDirection.Ascending);
				}
				return fOrganisationCollectionForInitialExport;
			}
		}
		OrgHeaderCollection fOrganisationCollectionForInitialExport;

		#endregion

		#region ExportOrganisationsAndJobs

		void ExportOrganisationsAndJobs(CancellationToken token)
		{
			while (HighWaterMark < UTCOfInitialExport)
			{
				token.ThrowIfCancellationRequested();
				ZDateTime batchEndDateTime = HighWaterMark.AddHours(BatchIntervalInHours);
				if (batchEndDateTime > UTCOfInitialExport)
				{
					batchEndDateTime = UTCOfInitialExport;
				}

				ExportByBatch(batchEndDateTime);

				HighWaterMark = batchEndDateTime;
			}
		}

		protected virtual void ExportByBatch(ZDateTime batchEndDateTime)
		{
			var reader = new FilteredBusinessObjectReader(FactoryProvider, CreateFilter(batchEndDateTime), typeof(StmALog));
			foreach (var log in reader.Cast<StmALog>())
			{
				foreach (var listener in Listeners.Cast<LogBatchListener>())
				{
					listener.Match(log, Buffer);
				}
			}
		}

		ZQuery CreateFilter(ZDateTime endDateTime)
		{
			ZQuery result = new ZQuery(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThan, HighWaterMark); //select logs between last hwm
			result.AddToFilter(JoinCondition.And, StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, endDateTime); //and Now.
			result.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name;
			result.IsNoLock = false;
			return result;
		}

		#endregion

		#region ExportAccountingDebtorTransactions

		internal void ExportAccountingDebtorTransactions()
		{
			AccHeaderFlatFileExporter accHeaderExporter = new AccHeaderFlatFileExporter(FactoryProvider.Current);
			if (accHeaderExporter.IsTransactionsExistInBatch)
			{
				ExportAccountingDebtorFile(Constants.FileNamePrefixes.InvoiceHeaderFiles, STIDataRegistry.Instance.InvoiceHeaderExportDirectory, accHeaderExporter);
				AccLinesFlatFileExporter accLinesExporter = new AccLinesFlatFileExporter(accHeaderExporter.FilterProvider.CurrentBatchNo, FactoryProvider.Current);
				ExportAccountingDebtorFile(Constants.FileNamePrefixes.InvoiceLineFiles, STIDataRegistry.Instance.InvoiceLinesExportDirectory, accLinesExporter);
				Buffer.Notify(new InfoNotification(string.Format(CultureInfo.CurrentCulture, "Batch {0} of the Accounting Transactions was exported", accLinesExporter.FilterProvider.CurrentBatchNo)));
			}
		}

		void ExportAccountingDebtorFile(string fileNamePreFix, string exportDirectory, FlatFileAccountingTransactionExporter exporter)
		{
			string tempFile = Env.GetTempFileName(Env.TempPath);
			try
			{
				using (FileStream stream = new FileStream(tempFile, FileMode.Create))
				{
					exporter.Export(stream);
				}
				string finalFileName = Path.Combine(exportDirectory, fileNamePreFix + FileName);
				File.Copy(tempFile, finalFileName, true);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		#endregion

		#region Implementation
		bool IsEnvironmentDataValid()
		{
			bool result = true;

			if (!STIDataRegistry.Instance.DisableOrganisationExport)
			{
				result = DirectoryExists(result, STIDataRegistry.Instance.OrganisationExportDirectory, "OrganisationExportDirectory");
			}

			if (!STIDataRegistry.Instance.DisableShipmentExport)
			{
				result = DirectoryExists(result, STIDataRegistry.Instance.ShipmentExportDirectory, "ShipmentExportDirectory");
			}

			if (!STIDataRegistry.Instance.DisableInvoiceExport)
			{
				result = DirectoryExists(result, STIDataRegistry.Instance.InvoiceHeaderExportDirectory, "InvoiceHeaderExportDirectory");
				result = DirectoryExists(result, STIDataRegistry.Instance.InvoiceLinesExportDirectory, "InvoiceLinesExportDirectory");
			}

			return result;
		}

		bool DirectoryExists(bool result, ZString registryDirectory, ZString directoryName)
		{
			if (!Directory.Exists(registryDirectory))
			{
				Buffer.Notify(new ErrorNotification(ErrorType.Error, string.Format(CultureInfo.CurrentCulture, "Invalid Export Directories: {0} have not been specified in System -> Registry -> Strang International Client Extensions -> Navision", directoryName)));
				return false;
			}

			return result;
		}

		string FileName
		{
			get { return DateAndTimeOfInitialExport.ToString(Constants.DateTimeFileNameFormat, CultureInfo.CurrentCulture) + ".csv"; }
		}

		ZDateTime HighWaterMark
		{
			get { return STIDataRegistry.Instance.DataExportHighWaterMark; }
			set { STIDataRegistry.Instance.DataExportHighWaterMark = value; }
		}

		internal int BatchIntervalInHours
		{
			get { return 2; }
		}

		ZDateTime UTCOfInitialExport
		{
			get
			{
				if (utcOfInitialExport.IsEmpty)
				{
					utcOfInitialExport = ZDateTime.UtcNow;
				}
				return utcOfInitialExport;
			}
		}
		ZDateTime utcOfInitialExport;

		ZDateTime DateAndTimeOfInitialExport
		{
			get
			{
				if (fDateAndTimeOfInitialExport.IsEmpty)
				{
					fDateAndTimeOfInitialExport = UTCOfInitialExport.ToLocalBranchTime();
				}
				return fDateAndTimeOfInitialExport;
			}
		}
		ZDateTime fDateAndTimeOfInitialExport;

		ILogBatchListenerProxy[] Listeners
		{
			get
			{
				if (fListeners == null)
				{
					fListeners = new ArrayList();

					if (!STIDataRegistry.Instance.DisableOrganisationExport)
					{
						fListeners.Add(new OrgBatchListener(DateAndTimeOfInitialExport));
					}

					if (!STIDataRegistry.Instance.DisableShipmentExport)
					{
						fListeners.Add(new ShipmentBatchListener(DateAndTimeOfInitialExport));
						fListeners.Add(new ConsolBatchListener(DateAndTimeOfInitialExport));
						fListeners.Add(new DeclarationBatchListener(DateAndTimeOfInitialExport));
					}
				}

				return (NavisionBatchListener[])fListeners.ToArray(typeof(NavisionBatchListener));
			}
		}
		ArrayList fListeners;

		void CreateListenersWithoutOrgBatchListener()
		{
			fListeners = new ArrayList();

			if (!STIDataRegistry.Instance.DisableShipmentExport)
			{
				fListeners.Add(new ShipmentBatchListener(DateAndTimeOfInitialExport));
				fListeners.Add(new ConsolBatchListener(DateAndTimeOfInitialExport));
				fListeners.Add(new DeclarationBatchListener(DateAndTimeOfInitialExport));
			}
		}

		internal NotificationBuffer Buffer
		{
			get
			{
				return buffer ?? (buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber()));
			}
		}
		NotificationBuffer buffer;

		internal BusinessObjectFactoryProvider FactoryProvider
		{
			get { return factoryProvider ?? (factoryProvider = new BusinessObjectFactoryProvider()); }
		}
		BusinessObjectFactoryProvider factoryProvider;

		#endregion
	}
}
