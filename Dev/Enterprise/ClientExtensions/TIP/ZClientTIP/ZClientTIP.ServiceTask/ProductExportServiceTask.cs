using System.IO;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.TIP;
using Enterprise.Client.TIP.ServiceTask;
using Enterprise.ClientSharedComponents;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	TIPConstants.ProductExportToCSVCode,
	"Product Export To CSV",
	"CSP",
	typeof(ProductExportServiceTask),
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1hours"
	)]
namespace Enterprise.Client.TIP.ServiceTask
{
	class ProductExportServiceTask : ServiceProviderImpl
	{
		#region Service Task

		public override void RunTask(CancellationToken token)
		{
			Buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber());
			if (IsEnvironmentDataValid())
			{
				Buffer.Notify(new InfoNotification($"====== Product Export to CSV for High Water Mark: '{TIPDataRegistry.Instance.HighWaterMark}' Started ======"));
				Exporter.Export(token);
				Buffer.Notify(new InfoNotification("====== Product Export to CSV Finished ======"));
				TIPDataRegistry.Instance.HighWaterMark = ZDateTime.UtcNow;
			}
		}

		#endregion

		#region Implementation

		#region Is Environment Data Valid

		bool IsEnvironmentDataValid()
		{
			ZStringBuilder errorMessage = new ZStringBuilder();
			if (TIPDataRegistry.Instance.ProductExportDirectory.IsEmpty || !Directory.Exists(TIPDataRegistry.Instance.ProductExportDirectory))
			{
				errorMessage.Append("Export Directory not specified or invalid.");
			}
			if (!NotificationGroupValidator.IsValid(TIPDataRegistry.Instance.ProductExportNotifyGroupPK, Factory))
			{
				errorMessage.Append("Notification Group not specified or invalid.");
			}
			if (TIPDataRegistry.Instance.OrganisationProductRegistryItem.Value.Count == 0)
			{
				errorMessage.Append("Organisations not specified.");
			}
			if (!errorMessage.IsEmpty)
			{
				Buffer.Notify(new ErrorNotification(ErrorType.Error, errorMessage.ToStringWithNewLineBetweenAppends()));
			}

			return errorMessage.IsEmpty;
		}

		#endregion

		#region Exporter

		ProductExporter Exporter
		{
			get { return exporter ?? (exporter = new ProductExporter(Buffer)); }
		}
		ProductExporter exporter;

		#endregion

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#endregion

		#region Buffer

		NotificationBuffer Buffer;

		internal NotificationBuffer GetBuffer()
		{
			return Buffer;
		}

		#endregion

		#endregion
	}
}
