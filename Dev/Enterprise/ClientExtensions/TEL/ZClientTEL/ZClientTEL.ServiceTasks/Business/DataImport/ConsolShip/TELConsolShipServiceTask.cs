using CargoWise.Types;
using Enterprise.Client.TEL;
using Enterprise.Client.TEL.Import;
using Enterprise.Client.TEL.ServiceTasks;
using Enterprise.ClientSharedComponents.ServiceTasks;
using Enterprise.DataTransfer.Business;
using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	TELConstants.ServiceTask.Import.ZT1,
	TELConstants.ServiceTask.Import.ConsolShipImportServiceTaskDescription,
	"CSP",
	typeof(TELConsolShipServiceTask),
	MinimumPeriod = MailDataImportServiceTask.MinimumPeriod,
	DefaultScheduleRunEvery = "30seconds",
	ActiveByDefault = true
	)]
[assembly: MailSubscriber(typeof(TELConsolShipServiceTask))]
namespace Enterprise.Client.TEL.ServiceTasks
{
	public class TELConsolShipServiceTask : MailDataImportServiceTask
	{
		protected override DataImporter GetImporter()
		{
			return new TELConsolShipXmlDataImporter();
		}

		protected override IMailFilter GetMailFilter() => CreateMailFilter();

		[MailFilter(MailFilterCodes.TELConsoleShipment)]
		public static IMailFilter CreateMailFilter()
		{
			var result = new QueryMailFilter(MailFilterCodes.TELConsoleShipment, subject: TELDataRegistry.Instance.ConsolShipManifestEmailSubjectIdentifier,
				isEnabled: !string.IsNullOrEmpty(TELDataRegistry.Instance.ConsolShipManifestEmailSubjectIdentifier));
			return result;
		}

		protected override GuidRegistryItem NotificationGroupPKRegistryItem
		{
			get
			{
				return TELDataRegistry.Instance.ConsolShipImportNotificationGroupPKItem;
			}
		}

		protected override ZGuid NotificationGroupPK
		{
			get
			{
				return TELDataRegistry.Instance.ConsolShipImportNotificationGroupPK;
			}
		}

		protected override ZString AttachmentFileExtension
		{
			get
			{
				return ".xml";
			}
		}
	}
}
