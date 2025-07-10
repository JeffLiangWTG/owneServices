using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.IO;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"ZK1",
	"XML File Import From Inhouse System CEIL",
	"CSP",
	typeof(Enterprise.Client.KNA.ServiceTasks.ImportXmlServiceTask),
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1minute"
	)]
namespace Enterprise.Client.KNA.ServiceTasks
{
	public class ImportXmlServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			if (ValidateDirectories)
			{
				DirectoryInformation directoryInfo = new DirectoryInformation(DirectoryPath);
				FileInformation[] files = directoryInfo.GetFiles("*.xml");

				ForwardingConsolValueObjectDataAdapter adapter = new ForwardingConsolValueObjectDataAdapter();
				MainFormConsolCollection collection = new MainFormConsolCollection(Factory);
				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(adapter.ValueObjectType);

				foreach (FileInformation xmlFile in files)
				{
					token.ThrowIfCancellationRequested();
					try
					{
						ImportXmlFile(xmlFile.FileInfo, adapter, collection, serializer);
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						Buffer.Notify(new ErrorNotification(ErrorType.Error, string.Format(CultureInfo.InvariantCulture, "Occured in file: {0} \n {1} \n {2}", xmlFile.Name, e.Message, e.StackTrace)));
					}
					finally
					{
						xmlFile.FileInfo.CopyTo(Path.Combine(ProcessedPath, xmlFile.Name), true);
						xmlFile.Delete();
						//XmlFile.FileInfo.MoveTo(Path.Combine(ProcessedPath, XmlFile.Name));
					}
				}
				PopulateAndSendAirCargo(collection, Buffer);
			}
			else
			{
				Buffer.Notify(new ErrorNotification(ErrorType.MissingDataDirectory, "Please specify an existing Import Directory and Processed Directory in Config -> System -> Registry -> Kuehne & Nagel Australia Client Extensions"));
			}
		}

		protected virtual
 void ImportXmlFile(FileInfo xmlFile, ForwardingConsolValueObjectDataAdapter adapter, MainFormConsolCollection collection, XmlValueObjectSerializer serializer)
		{
			using (FileStream stream = xmlFile.OpenRead())
			{
				serializer.ImportXmlData(stream, adapter, collection, null, Buffer);
			}
			Factory.Save();
			Buffer.Notify(new InfoNotification(string.Format("Data from {0} has been imported.", xmlFile.Name)));
		}

		#region Implementation

		void PopulateAndSendAirCargo(MainFormConsolCollection collection, NotificationBuffer notify)
		{
			foreach (CommonConsol consol in collection)
			{
				ForwardingConsol forwardingConsol = Factory.Load<ForwardingConsol>(consol.PK);
				if (forwardingConsol.IsAir && forwardingConsol.IsImport())
				{
					var processor = new HouseBillsCargoMessageProcessor(notify);
					using (var job = new AirCargoProcessorJobForConsol(forwardingConsol))
					{
						processor.Process(job);
					}
				}
			}
		}

		bool ValidateDirectories
		{
			get { return (!DirectoryPath.IsEmpty && Directory.Exists(DirectoryPath) && !ProcessedPath.IsEmpty && Directory.Exists(ProcessedPath)); }
		}

		ZString DirectoryPath
		{
			get { return KNADataRegistry.Instance.DirectoryToImportXml; }
		}

		ZString ProcessedPath
		{
			get { return KNADataRegistry.Instance.ProcessedDirectoryForXmlFiles; }
		}

		public NotificationBuffer Buffer
		{
			get
			{
				return buffer ?? (buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber()));
			}
		}
		NotificationBuffer buffer = null!;

		#region Factory

		protected virtual
 BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}
		BusinessObjectFactory fFactory = null!;

		#endregion

		#endregion
	}
}
