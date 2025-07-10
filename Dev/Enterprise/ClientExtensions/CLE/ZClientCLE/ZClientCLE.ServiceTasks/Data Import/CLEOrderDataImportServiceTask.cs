using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Client.CLE.OrdersDataImport.ServiceTasks;
using Enterprise.ClientSharedComponents;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CLEOrderDataImportServiceTask.Code,
	"Import of Orders in Clemenger-Specific csv-File Format",
	"CSP",
	typeof(CLEOrderDataImportServiceTask),
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "5minutes",
	ActiveByDefault = true,
	CanRunInAnyBranch = true
	)]
namespace Enterprise.Client.CLE.OrdersDataImport.ServiceTasks
{
	internal class CLEOrderDataImportServiceTask : ServiceProviderImpl
	{
		#region Service Task overrides

		public override void RunTask(CancellationToken token)
		{
			Buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber());
			if (CLEDataRegistry.Instance.EnableOrdersDataImportInterface && IsEnvironmentDataValid())
			{
				DirectoryInfo directory = new DirectoryInfo(CLEDataRegistry.Instance.OrderImportDirectory);
				foreach (FileInfo file in directory.GetFiles())
				{
					token.ThrowIfCancellationRequested();
					ImportFile(file);
				}
			}
		}

		#endregion

		#region Implementation

		#region Import File

		void ImportFile(FileInfo file)
		{
			CLEOrderDataImporter importer = new CLEOrderDataImporter();
			Buffer.Notify(new InfoNotification("Running Order Import from file " + file.Name));
			file.Attributes = FileAttributes.Normal;

			bool imported;
			using (var reader = File.OpenText(file.FullName))
			{
				imported = importer.ImportData(reader, file.FullName, Buffer, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.ClientSpecifiedImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, file.FullName));
			}

			if (!imported && importer.ErrorOrderNumbers.IsEmpty)
			{
				Buffer.Notify(new ErrorNotification(ErrorType.Error, "Error: Invalid file format " + file.Name));
				SendEmail(file.FullName, "Order Import Error", "An error occurred while trying to import file " + file.Name + " - Invalid file format (file attached)");
			}
			else if (!importer.ErrorOrderNumbers.IsEmpty)
			{
				Buffer.Notify(new ErrorNotification(ErrorType.Error, "Error: Order " + importer.ErrorOrderNumbers + " cannot be created"));
				SendEmail(file.FullName, "Order Import Error", "An error occurred while trying to import Order(s): " + importer.ErrorOrderNumbers + ", from file - " + file.Name);
			}

			try
			{
				file.Delete();
			}
			catch (IOException ex)
			{
				Buffer.Notify(new ErrorNotification(ErrorType.Error, "Cannot delete file " + file.Name + System.Environment.NewLine + ex.Message));
			}

			Buffer.Notify(new InfoNotification("Finish Import file " + file.Name));
		}

		#endregion

		#region Is Environment Data Valid

		ZBool IsEnvironmentDataValid()
		{
			ZStringBuilder errorMessage = new ZStringBuilder();
			if (CLEDataRegistry.Instance.OrderImportDirectory.IsEmpty || !Directory.Exists(CLEDataRegistry.Instance.OrderImportDirectory))
			{
				errorMessage.Append("Import Directory not specified or does not exists.");
			}
			if (!NotificationGroupValidator.IsValid(CLEDataRegistry.Instance.OrderImportNotifyGroupPK, Factory))
			{
				errorMessage.Append("Notification group not specified or has no email recipients.");
			}
			if (!errorMessage.IsEmpty)
			{
				Buffer.Notify(new ErrorNotification(ErrorType.Error, errorMessage.ToStringWithNewLineBetweenAppends()));
			}
			return errorMessage.IsEmpty;
		}

		#endregion

		#region Send Email

		void SendEmail(ZString fileName, ZString subject, ZString body)
		{
			EmailDef email = new EmailDef();
			email.Subject = subject;
			email.Body = body;

			if (!fileName.IsEmpty)
			{
				try
				{
					AttachmentDef attachDef = new AttachmentDef(fileName);
					email.Attachments.Add(attachDef);
				}
				catch (Exception e) when (!e.IsCriticalException()) { }
			}
			Env.OutgoingMailManager.CreateAndSave(email, CLEDataRegistry.Instance.OrderImportNotifyGroupPK.ToGuid(), GroupSourceLocator.GetFromRegistryItem(CLEDataRegistry.Instance.SwitchOrderImportItem));
		}

		#endregion

		#region Factory

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

		public const string Code = "ZC1";
	}
}

#region Implementation
#endregion
#region Set Up
#endregion
