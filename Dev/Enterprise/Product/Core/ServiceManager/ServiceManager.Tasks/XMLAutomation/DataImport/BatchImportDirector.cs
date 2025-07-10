using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class BatchImportDirector : XMLDirector
	{
		public BatchImportDirector(INotifications notifications)
			: base(notifications)
		{
			ImportTasks = new List<ImportTask>();
			ImportTasks.Add(new ConsolXmlImportTask(SystemDataRegistry.Instance.ConsolsDataImportDirectory, Notify, NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup));
			ImportTasks.Add(new DeclarationXmlImportTask(SystemDataRegistry.Instance.CustomsDeclarationsDataImportDirectory, Notify, NotificationDataRegistry.Instance.CustomsDeclarationImportNotificationGroup));
			ImportTasks.Add(new OrderXmlImportTask(SystemDataRegistry.Instance.OrdersXMLDataImportDirectory, Notify, NotificationDataRegistry.Instance.OrderImportNotificationGroup));
			ImportTasks.Add(new BookingXmlImportTask(SystemDataRegistry.Instance.BookingsDataImportDirectory, Notify, NotificationDataRegistry.Instance.BookingImportNotificationGroup));
			ImportTasks.Add(new LocalCartageXmlImportTask(SystemDataRegistry.Instance.LocalCartageDataImportDirectory, Notify, NotificationDataRegistry.Instance.LocalCartageNotificationGroup));
			ImportTasks.Add(new ScheduleXmlImportTask(SystemDataRegistry.Instance.ScheduleDataImportDirectory, Notify, NotificationDataRegistry.Instance.SchedulesImportNotificationGroup));
			ImportTasks.Add(new FlatFileImportTask(SystemDataRegistry.Instance.CommercialInvoicesDataImportDirectory, new InvoiceFlatFileImporter(NotificationDataRegistry.Instance.CommercialInvoiceImportNotificationGroup.Value), Notify, NotificationDataRegistry.Instance.CommercialInvoiceImportNotificationGroup));
			ImportTasks.Add(new CommercialInvoiceXmlImportTask(SystemDataRegistry.Instance.CommercialInvoicesDataImportDirectoryXml, Notify, NotificationDataRegistry.Instance.CommercialInvoiceImportNotificationGroup));
			ImportTasks.Add(new OrderCsvImportTask(Notify));
			ImportTasks.Add(new ShipmentXmlImportTask(SystemDataRegistry.Instance.ShipmentDataImportDirectory, Notify, NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup));
			ImportTasks.Add(new ContainerEventsXmlImportTask(SystemDataRegistry.Instance.ContainerEventsDataImportDirectory, Notify, NotificationDataRegistry.Instance.ContainerEventsImportNotificationGroup));
			ImportTasks.Add(new XmlMultiTypeImportTask(SystemDataRegistry.Instance.DefaultMessagesDataImportDirectory, this, Notify, SystemDataRegistry.Instance.UnprocessedMessageNotificationGroup));
			ImportTasks.Add(new WarehouseXmlImportTask(SystemDataRegistry.Instance.WarehouseDataImportDirectory, Notify, NotificationDataRegistry.Instance.WarehouseImportNotificationGroup));
			ImportTasks.Add(new WarehouseCartageXmlImportTask(Notify));
			ImportTasks.Add(new WarehouseIFSXmlImportTask(Notify));
			ImportTasks.Add(new OrganisationXmlImportTask(SystemDataRegistry.Instance.OrganisationDataImportDirectory, Notify, NotificationDataRegistry.Instance.OrganisationImportNotificationGroup));
			ImportTasks.Add(new FlatFileImportTask(SystemDataRegistry.Instance.PODDataImportDirectory, new PODFlatFileDataImporter(), Notify, NotificationDataRegistry.Instance.PODImportNotificationGroup));
			ImportTasks.Add(new FlatFileImportTask(SystemDataRegistry.Instance.WarehouseOrderFlatFileImportDirectory, new WarehouseOrderFlatFileImporter(NotificationDataRegistry.Instance.WarehouseImportNotificationGroup), Notify, NotificationDataRegistry.Instance.WarehouseImportNotificationGroup));
			ImportTasks.Add(new AccountBalancesXmlImportTask(Notify));
			ImportTasks.Add(new EventXmlImportTask(SystemDataRegistry.Instance.EventDataImportDirectory, Notify, NotificationDataRegistry.Instance.EventImportNotificationGroup));
			ImportTasks.Add(new ProductXmlImportTask(SystemDataRegistry.Instance.ProductsXMLDataImportDirectory, Notify, NotificationDataRegistry.Instance.ProductImportNotificationGroup));

			if (EligibleForCSVProductImport)
			{
				ImportTasks.Add(new FlatFileImportTask(SystemDataRegistry.Instance.ProductsDataImportDirectory, new ProductFlatFileImporter(), Notify, NotificationDataRegistry.Instance.ProductImportNotificationGroup));
				ImportTasks.Add(new FlatFileImportTask(SystemDataRegistry.Instance.ProductLastCostImportDirectory, new ProductLastCostFlatFileDataImporter(), Notify, NotificationDataRegistry.Instance.ProductImportNotificationGroup));
			}
		}

		protected override void RunCore(CancellationToken token)
		{
			foreach (var task in ImportTasks)
			{
				token.ThrowIfCancellationRequested();
				var xmlTask = task as XmlImportTask;
				var taskDescription = xmlTask != null ? (string)xmlTask.TaskDescription : task.GetType().Name;

				var initialBranch = Env.CurrentBranch != null ? Env.CurrentBranch.Code : string.Empty;
				var initialCompany = Env.CurrentCompany != null ? Env.CurrentCompany.Code : string.Empty;
				var initialDepartment = Env.CurrentDepartment != null ? Env.CurrentDepartment.Code : string.Empty;

				Notify.AddInformation(string.Format(CultureInfo.InvariantCulture, "Running task {0}, current Branch - {1}", taskDescription, initialBranch));

				Env.Instance.UserContextChanging += EnvInstance_UserContextChanging;
				try
				{
					task.Run(); //Parallelization is done inside ImportTask.RunTask().
				}
				finally
				{
					Env.Instance.UserContextChanging -= EnvInstance_UserContextChanging;
				}

				var finalBranch = Env.CurrentBranch != null ? Env.CurrentBranch.Code : string.Empty;
				var finalCompany = Env.CurrentCompany != null ? Env.CurrentCompany.Code : string.Empty;
				var finalDepartment = Env.CurrentDepartment != null ? Env.CurrentDepartment.Code : string.Empty;

				if (!string.Equals(initialBranch, finalBranch, StringComparison.Ordinal))
				{
					var message = new StringBuilder();
					message.Append(string.Format(CultureInfo.InvariantCulture, "Current Branch has changed while running task {0}.", taskDescription));
					message.Append(string.Format(CultureInfo.InvariantCulture, "\nInitial Branch was - {0}, nem Branch is - {1}.", initialBranch, finalBranch));
					message.Append(string.Format(CultureInfo.InvariantCulture, "\nInitial Company was - {0}, final Company is - {1}.", initialCompany, finalCompany));
					message.Append(string.Format(CultureInfo.InvariantCulture, "\nInitial Department was - {0}, final Department is - {1}.", initialDepartment, finalDepartment));

					Notify.AddWarning(message.ToString());

					userContextChangedLogs.ForEach(changedLog => message.Append("\n\n").Append(changedLog));
					userContextChangedLogs.Clear();

					ErrorReporter.ReportOnce("BatchImportDirector_RunCore_BranchChanged", "CS00412721: " + message.ToString());
				}
			}
		}

#if DEBUG
		internal List<string> UserContextChangedLogsForTest => userContextChangedLogs;
#endif

		readonly List<string> userContextChangedLogs = new List<string>();

		void EnvInstance_UserContextChanging(object sender, IUserContextChangingEventArgs e)
		{
			lock (userContextChangedLogs)
			{
				userContextChangedLogs.Add(string.Format(CultureInfo.InvariantCulture, "User Context is changing, old branch is '{0}', new branch is '{1}', setCurrentThreadContext is '{2}'.", e.OldUserContext?.Branch?.Code, e.NewUserContext?.Branch?.Code, e.SetCurrentThreadContext));
				userContextChangedLogs.Add(System.Environment.StackTrace);
			}
		}

		#region EligibleForProductImport

#if DEBUG
		protected internal
#endif
		bool EligibleForCSVProductImport
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia ||
						GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand ||
						GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates;
			}
		}

		#endregion

		public readonly List<ImportTask> ImportTasks;
	}
}
