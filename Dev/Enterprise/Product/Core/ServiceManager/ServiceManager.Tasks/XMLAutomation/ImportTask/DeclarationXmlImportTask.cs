using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class DeclarationXmlImportTask : XmlImportTask
	{
		public DeclarationXmlImportTask(INotifications notify)
			: this(SystemDataRegistry.Instance.CustomsDeclarationsDataImportDirectory, notify, NotificationDataRegistry.Instance.CustomsDeclarationImportNotificationGroup)
		{
		}

		public DeclarationXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.DeclarationXmlImport)
		{
		}

		protected override void ProcessFileCore(FileInfo dataFile, INotifications notifications)
		{
			bool successfulImport = false;
			ZString declarationImportStatuses = ZString.Empty;
			NotificationBuffer buffer = new NotificationBuffer(notifications);
			DeclarationXmlDataImporter jobDecDataImporter = NewImporter() as DeclarationXmlDataImporter;

			if (jobDecDataImporter != null)
			{
				using (StreamReader reader = dataFile.OpenText())
				{
					successfulImport = jobDecDataImporter.ImportData(reader, dataFile.Name, buffer, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.DeclarationXmlImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, dataFile.Name));
					//importer.ImportData does a Factory.Save, so if the mutex was lost in the mean time, SqlLockLostException will be thrown and we won't accidentally double-process/double-save anything.
					declarationImportStatuses += jobDecDataImporter.GetDeclarationCreatedUpdatedRejectedMessage() + System.Environment.NewLine;
				}
				if (!successfulImport || buffer.ContainsNotificationType(ErrorType.ImportingDataError) || jobDecDataImporter.IsImportedDeclarationRejected)
				{
					SendEmailToNotificationGroup(NotificationGroup.Value, NotificationGroup, dataFile, Res.GetString("e3e8ad52-344e-4b96-8101-5827cbb57ec1", "{0}\r\nBatch Processor Log:\r\n{1}", declarationImportStatuses, buffer.AsString));
				}
				else
				{
					if (IsEmailToBeSentOnSuccess)
					{
						SendSuccessImportNotificationEmail(String.Format("{0} - {1}", NotificationEmailSubjectOnSuccess, dataFile.Name), Res.GetString("e3e8ad52-344e-4b96-8101-5827cbb57ec1", "{0}\r\nBatch Processor Log:\r\n{1}", declarationImportStatuses, buffer.AsString));
					}
				}
			}
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("14cd81ac-c9ac-45f6-a35a-24bd49649a9a", "Declaration XML Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new DeclarationXmlDataImporter(DeclarationValueObjectDataAdapter.New());
		}
	}
}
