using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.DFD.Export;
using Enterprise.Client.DFD.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"ZD3", 
	"Accounts Receivable Export", 
	"CSP", 
	typeof(ARTransactionsServiceTask), 
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1hour"
	)]

namespace Enterprise.Client.DFD.ServiceTasks
{
	public class ARTransactionsServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			if (DFDDataRegistry.Instance.ARTransactionsExportItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).IsGoodToGo)
			{
				CompanyPK = Guid.Empty;
				RunARTransactionExport(youMustReactToThisToken);
			}

			foreach (GlbBranch branch in ValidBranches)
			{
				using (branch.SetAsTemporaryContext())
				{
					CompanyPK = branch.GB_GC.ToGuid();
					RunARTransactionExport(youMustReactToThisToken);
				}
			}
		}

		void RunARTransactionExport(CancellationToken token)
		{
			ZDateTime localNow = ZDateTime.Now;

			while (ItemValue.LastRunDateTime < localNow)
			{
				token.ThrowIfCancellationRequested();
				ZDateTime batchEndDateTime = ItemValue.LastRunDateTime.AddHours(BatchIntervalInHours);
				ZDateTime endDateTime = (localNow > batchEndDateTime) ? batchEndDateTime : localNow;

				ItemValue.NextRunDateTime = endDateTime;
				DFDDataRegistry.Instance.ARTransactionsExportItem.SetValue(CompanyPK, Guid.Empty, Guid.Empty, ItemValue);

				Export();

				ItemValue.LastRunDateTime = endDateTime;
				DFDDataRegistry.Instance.ARTransactionsExportItem.SetValue(CompanyPK, Guid.Empty, Guid.Empty, ItemValue);
			}
			itemValue = null;
		}

		Registry.AdditionalSettingsRegistryBusinessObject ItemValue
		{
			get
			{
				if (itemValue == null)
				{
					itemValue = DFDDataRegistry.Instance.ARTransactionsExportItem.GetValueWithoutFallback(CompanyPK, Guid.Empty, Guid.Empty);
				}
				return itemValue;
			}
		}
		Registry.AdditionalSettingsRegistryBusinessObject? itemValue;

		Guid CompanyPK { get; set; }

		int BatchIntervalInHours
		{
			get { return 2; }
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is handled locally.")]
		void Export()
		{
			if (Exporter.IsTransactionsExistInBatch)
			{
				Buffer.Notify(new InfoNotification(string.Format(CultureInfo.CurrentCulture, "AR Transactions Export Started at {0}", ZDateTime.Now.ToString())));
				Buffer.Notify(new InfoNotification(string.Format(CultureInfo.CurrentCulture, "Searching for AR Transactions created between '{0}' and '{1}'.", Exporter.LastRun, Exporter.NextRun)));

				string tempPathForExport = Env.TempPath + "DFD";
				if (!Directory.Exists(tempPathForExport))
				{
					Directory.CreateDirectory(tempPathForExport);
				}
				DirectoryInfo directoryPath = new DirectoryInfo(tempPathForExport);

				string tempFile = Env.GetTempFileName(tempPathForExport);
				try
				{
					using (FileStream fileStream = new FileStream(tempFile, FileMode.Create))
					{
						Exporter.Export(fileStream);
					}

					if (Exporter.FileNames.Count > 0)
					{
						List<FileInfo> tempFiles = new List<FileInfo>();
						tempFiles.AddRange(directoryPath.GetFiles("*.tmp"));
						for (int counter = 0; counter < tempFiles.Count; counter++)
						{
							tempFiles[counter].CopyTo(DFDDataRegistry.Instance.ARTransactionsExportDirectory + "\\" + Exporter?.FileNames[tempFiles[counter].FullName]?.ToString()?.Replace("/", "") + ".xml");
							Buffer.Notify(new InfoNotification(Exporter?.FileNames[tempFiles[counter].FullName]?.ToString()?.Replace("/", "") + " created in " + DFDDataRegistry.Instance.ARTransactionsExportDirectory));
						}
					}
					else if (Exporter.HasErrors)
					{
						Buffer.Notify(new ErrorNotification(ErrorType.Error, Exporter.Notifications.ToString()));
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Buffer.Notify(new ErrorNotification(ErrorType.Error, "Error occurred: " + ex.Message));
				}
				finally
				{
					foreach (FileInfo fileInfo in directoryPath.GetFiles())
					{
						if (File.Exists(fileInfo.FullName))
						{
							File.Delete(fileInfo.FullName);
						}
					}
				}
			}

			exporter = null;
		}

		DFDXmlARTransactionsExporterForBatchProcess Exporter
		{
			get
			{
				if (exporter == null)
				{
					exporter = new DFDXmlARTransactionsExporterForBatchProcess(new BusinessObjectFactory(),
						Env.Time.GetUtcFromLocalTime(ItemValue.LastRunDateTime.ToDateTime()),
						Env.Time.GetUtcFromLocalTime(ItemValue.NextRunDateTime.ToDateTime()));
				}
				return exporter;
			}
		}
		DFDXmlARTransactionsExporterForBatchProcess? exporter;

		NotificationBuffer Buffer
		{
			get
			{
				return buffer ?? (buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber()));
			}
		}
		NotificationBuffer? buffer;

		List<GlbBranch> ValidBranches
		{
			get
			{
				if (validBranches == null)
				{
					validBranches = new List<GlbBranch>();
					GlbCompany[] companies = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, true));
					if (companies != null)
					{
						foreach (GlbCompany company in companies)
						{
							GlbBranch branch = company.FirstActiveBranch;
							if (branch != null)
							{
								if (DFDDataRegistry.Instance.ARTransactionsExportItem.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty).IsGoodToGo)
								{
									validBranches.Add(branch);
								}
							}
						}
					}
				}
				return validBranches;
			}
		}
		List<GlbBranch>? validBranches;

		BusinessObjectFactory Factory
		{
			get
			{
				return factory ?? (factory = new BusinessObjectFactory());
			}
		}
		BusinessObjectFactory? factory;
	}
}
