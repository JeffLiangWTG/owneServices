using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class ProductXmlImportTask : XmlImportTask
	{
		public ProductXmlImportTask(INotifications notify)
			: this(SystemDataRegistry.Instance.ProductsXMLDataImportDirectory, notify, NotificationDataRegistry.Instance.ProductImportNotificationGroup)
		{
		}

		public ProductXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.ProductXmlImport)
		{
		}

		protected override void ProcessFileCore(FileInfo dataFile, INotifications notifications)
		{
			bool successfulImport = false;
			NotificationBuffer buffer = new NotificationBuffer(notifications);
			ProductXmlDataImporter productDataImporter = NewImporter() as ProductXmlDataImporter;

			if (productDataImporter != null)
			{
				using (StreamReader reader = dataFile.OpenText())
				{
					successfulImport = productDataImporter.ImportData(reader, dataFile.Name, buffer, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.ProductXmlImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, dataFile.Name));
				}
				if (!successfulImport || buffer.ContainsNotificationType(ErrorType.ImportingDataError))
				{
					SendEmailToNotificationGroup(NotificationGroup.Value, NotificationGroup, dataFile, Res.GetString("ed3f8266-2ff8-4a05-b5ae-97fda3cdd29d", "Batch Processor Log:{0}{1}", System.Environment.NewLine, buffer.AsString));
				}
				else
				{
					if (IsEmailToBeSentOnSuccess)
					{
						SendSuccessImportNotificationEmail(String.Format("{0} - {1}", NotificationEmailSubjectOnSuccess, dataFile.Name), Res.GetString("404c2fb1-282a-4d64-ba53-bff254ae1ab6", "Batch Processor Log:{0}{1}", System.Environment.NewLine, buffer.AsString));
					}
				}
			}
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("a41f3f48-a635-4d9b-8aea-54593582c3d5", "Product XML Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new ProductXmlDataImporter(ProductValueObjectDataAdapter.New());
		}
	}
}
