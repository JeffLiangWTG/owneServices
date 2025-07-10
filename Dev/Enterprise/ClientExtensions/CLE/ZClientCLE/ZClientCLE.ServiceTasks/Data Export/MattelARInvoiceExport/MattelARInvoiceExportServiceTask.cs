using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.CLE.MattelARInvoiceExport.ServiceTasks;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	MattelARInvoiceExportServiceTask.Code,
	"Export of AR Invoices in EDIFACT format for Mattel",
	"CSP",
	typeof(MattelARInvoiceExportServiceTask),
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true,
	CanRunInAnyBranch = true)
]

namespace Enterprise.Client.CLE.MattelARInvoiceExport.ServiceTasks
{
	class MattelARInvoiceExportServiceTask : ServiceProviderImpl
	{
		#region Serice Task override

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			Buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber());

			if (IsEnvironmentDataValid())
			{
				try
				{
					Buffer.Notify(new InfoNotification("Mattel AR invoices data export begins"));
					Exporter.ExportARInvoice();
					Buffer.Notify(new InfoNotification("Mattel AR invoices data export completed"));
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					Buffer.Notify(new InfoNotification(e.Message));
				}
			}
		}

		#endregion

		#region Implementation

		#region Exporter

		MattelARInvoiceExporter Exporter
		{
			get { return exporter ?? (exporter = new MattelARInvoiceExporter(Buffer, Factory)); }
		}
		MattelARInvoiceExporter exporter = null!;

		#endregion

		#region IsEnvironmentDataValid

		bool IsEnvironmentDataValid()
		{
			ZStringBuilder errorMessage = new ZStringBuilder();

			if (CLEDataRegistry.Instance.MattelEmailAddress.Trim().IsEmpty || !EmailAddressValidation.IsEmailAddressValid(CLEDataRegistry.Instance.MattelEmailAddress.Trim()))
			{
				errorMessage.Append("The Mattel Data Export Email Address is not set up or not valid");
			}
			if (CLEDataRegistry.Instance.ClemengerEmailAddress.IsEmpty || !EmailAddressValidation.IsEmailAddressValid(CLEDataRegistry.Instance.ClemengerEmailAddress.Trim()))
			{
				errorMessage.Append("The Clemenger Email Address is not set up or not valid");
			}
			if (CLEDataRegistry.Instance.MattelEmailSubject.Trim().IsEmpty)
			{
				errorMessage.Append("The Mattel Data Export Email Subject is not set up");
			}
			if (CLEDataRegistry.Instance.MattelDebtor.IsEmpty)
			{
				errorMessage.Append("The Mattel Debtor is not set up");
			}
			if (CLEDataRegistry.Instance.MattelMailboxNumber.Trim().IsEmpty)
			{
				errorMessage.Append("The Mattel Mailbox Number is not set up");
			}
			if (CLEDataRegistry.Instance.ClemengerMailboxNumber.Trim().IsEmpty)
			{
				errorMessage.Append("The Clemenger Mailbox Number is not set up");
			}
			if (!errorMessage.IsEmpty)
			{
				Buffer.Notify(new ErrorNotification(ErrorType.Error, errorMessage.ToStringWithNewLineBetweenAppends()));
			}

			return errorMessage.IsEmpty;
		}

		#endregion

		#region Facorty

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory = null!;

		#endregion

		#region Buffer

		NotificationBuffer Buffer = null!;
		public NotificationBuffer GetBuffer() => Buffer;
		#endregion

		#endregion

		public const string Code = "ZC2";
	}
}

#region Implementation
#endregion
#region Overridea
#endregion
