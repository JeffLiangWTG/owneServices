using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public abstract class XmlImportTask : ImportTask
	{
		public XmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup, BillingInterfaceName interfaceName)
			: base(registryPath, notify, notificationGroup)
		{
			this.BadMessagesDirectoryName = SystemDataRegistry.Instance.UnprocessedMessagesDataDirectory.Value;
			this.InterfaceName = interfaceName;
		}

		readonly BillingInterfaceName InterfaceName;

		bool ContainsErrorTypeThatNeeedsCopyToBadMessageFileDirectory(NotificationBuffer buffer)
		{
			return
				buffer.ContainsNotificationType(ErrorType.XmlSchemaValidation) ||
				buffer.ContainsNotificationType(ErrorType.DataOutOfRangeError);
		}

		protected override sealed void ProcessFile(FileInfo dataFile)
		{
			NotificationBuffer buffer = new NotificationBuffer(Notify);
			ProcessFileCore(dataFile, buffer);
			if (IsBadMessagesDirectoryEnvironmentValid() && ContainsErrorTypeThatNeeedsCopyToBadMessageFileDirectory(buffer))
			{
				CopyFileToBadMessageFileDirectory(dataFile, buffer);
			}
		}

		protected virtual void ProcessFileCore(FileInfo dataFile, INotifications notifications)
		{
			bool successfulImport = false;

			var importer = NewImporter();
			if (importer != null)
			{
				var buffer = new NotificationBuffer(notifications);

				successfulImport = ImportData(importer, dataFile, buffer);
				if (!successfulImport && buffer.ContainsNotificationType(ErrorType.PostToDatabaseError) && File.Exists(dataFile.FullName))
				{
					buffer.AddInformation(Res.GetString("41eb47d1-610f-42d6-a264-6bd4d12ccea5", "Attempting to import same file again."));
					for (int i = 0; i < 3; i++)
					{
						importer = NewImporter();
						var separateNotifications = new NotificationBuffer();
						successfulImport = ImportData(importer, dataFile, separateNotifications);
						if (successfulImport)
						{
							buffer.AddRange(separateNotifications.Events);
							break;
						}
						Thread.Sleep(10);
					}
					if (!successfulImport)
					{
						buffer.AddInformation(Res.GetString("5646999c-92cb-4e6d-8ec9-ed008b4c8f03", "Second import attempt failed."));
					}
				}

				if (!successfulImport || buffer.ContainsNotificationType(ErrorType.ImportingDataError))
				{
					SendEmailToNotificationGroup(NotificationGroup.Value, NotificationGroup, dataFile, buffer.AsString);
				}
				else if (IsEmailToBeSentOnSuccess)
				{
					SendSuccessImportNotificationEmail(ZString.Format("{0} - {1}", NotificationEmailSubjectOnSuccess, dataFile.Name), buffer.AsString);
				}
			}

			AfterProcessFile(successfulImport);
		}

		bool ImportData(IDataImporter importer, FileInfo dataFile, INotifications buffer)
		{
			using (var reader = new StreamReaderWithCharacterReplacement(dataFile.FullName, Encoding.UTF8))
			{
				reader.AddReplacement('“', '"');
				reader.AddReplacement('”', '"');
				reader.AddReplacement('‘', '\'');
				reader.AddReplacement('’', '\'');

				return importer.ImportData(reader, dataFile.Name, buffer, new SourceInfo(BillingDataSource.InterfaceConnector, InterfaceName, ZGuid.Empty, ZGuid.Empty, ZString.Empty, dataFile.Name));
				//importer.ImportData does a Factory.Save, so if the mutex was lost in the mean time, SqlLockLostException will be thrown and we won't accidentally double-process/double-save anything.
			}
		}

		protected bool IsBadMessagesDirectoryEnvironmentValid()
		{
			bool result = true;
			ZString warningMessage = ZString.Empty;
			if (BadMessagesDirectoryName.IsEmpty)
			{
				warningMessage = Res.GetString("c2a48806-5a05-4831-836d-e0e2ff406173", "No {0} registry item set. Associated import not running.", (SystemDataRegistry.Instance.UnprocessedMessagesDataDirectory as IRegistryItemInternals).Location);
			}
			else if (!Directory.Exists(BadMessagesDirectoryName))
			{
				warningMessage = Res.GetString("1ba78901-83ea-4dd4-a236-9fa4afad17e1", "The directory {0} does not exist. Associated import not running", BadMessagesDirectoryName);
			}
			if (!warningMessage.IsEmpty)
			{
				WarningNotification warning = new WarningNotification(warningMessage);
				Notify.Notify(warning);
				result = false;
			}
			return result;
		}

		readonly ZString BadMessagesDirectoryName;

		protected void GenerateBadMessageFile(ZString originalFileName, ZString rootCollectionElementName, ZString collectionXml, NotificationBuffer buffer)
		{
			ZString newFileName = ConstructBadMessageFileName(originalFileName, rootCollectionElementName);
			WriteToFile(newFileName, collectionXml, BadMessagesDirectoryName, buffer);
		}

		void CopyFileToBadMessageFileDirectory(FileInfo fileData, NotificationBuffer buffer)
		{
			ZString newPath = Path.Combine(BadMessagesDirectoryName, ConstructBadMessageFileName(fileData.Name));
			File.Copy(fileData.FullName, newPath);
			buffer.Notify(new InfoNotification(Res.GetString("483e43c6-97e2-414d-82bb-a78b8f24522b", "Original file has been moved from {0} to {1}", fileData.FullName, newPath)));
		}

		protected static ZString ConstructBadMessageFileName(ZString originalFileName, ZString rootCollectionElementName)
		{
			return GetUniqueFilePrefix() + "_" + (rootCollectionElementName.IsEmpty ? "" : rootCollectionElementName + "_from_") + originalFileName;
		}

#if DEBUG
		internal static ZString ConstructBadMessageFileNameForTest(ZString originalFileName, ZString rootCollectionElementName)
		{
			return ConstructBadMessageFileName(originalFileName, rootCollectionElementName);
		}
#endif

		protected static ZString ConstructBadMessageFileName(ZString originalFileName)
		{
			return ConstructBadMessageFileName(originalFileName, "");
		}

#if DEBUG
		internal static ZString ConstructBadMessageFileNameForTest(ZString originalFileName)
		{
			return ConstructBadMessageFileName(originalFileName);
		}
#endif

		protected static ZString GetUniqueFilePrefix()
		{
			return ZDateTime.Now.ToString("yyMMddHHmmssffff");
		}

		protected void WriteToFile(ZString fileFullName, ZString xmlZString, ZString toDir, NotificationBuffer buffer)
		{
			ZString filePath = Path.Combine(toDir, fileFullName);
			using (var writer = new XmlTextWriter(filePath, Encoding.ASCII)
			{
				Formatting = Formatting.Indented
			})
			{
				writer.WriteStartDocument();
				writer.WriteRaw(xmlZString);
				writer.Flush();
				writer.Close();
			}
			buffer.Notify(new InfoNotification(Res.GetString("bc616f67-ed74-4a4d-af9a-173162b99127", "New file has been created: {0}", filePath)));
		}

		protected virtual bool IsEmailToBeSentOnSuccess
		{
			get { return !SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.Value; }
		}

		protected override string NotificationEmailSubject
		{
			get { return Res.GetString("dc3b3e95-f6b6-4dfe-8d2a-d6c1ecb04e2b", "{0} Failed", TaskDescription); }
		}

		protected virtual string NotificationEmailSubjectOnSuccess
		{
			get { return Res.GetString("502740ea-a4cb-4218-b91b-51076dfe4d58", "{0} Succeeded", TaskDescription); }
		}

		protected virtual void AfterProcessFile(bool successfulImport)
		{
		}

		protected virtual void SendSuccessImportNotificationEmail(ZString subject, ZString body)
		{
			EmailDef email = new EmailDef()
			{
				Subject = subject,
				Body = body
			};
			try
			{
				Env.OutgoingMailManager.CreateAndSave(email, NotificationGroup.Value, GroupSourceLocator.GetFromRegistryItem(NotificationGroup));
			}
			catch (EmailSendFailedException) { }
		}

		protected override ZString FileExtension
		{
			get { return "*.xml"; }
		}

		public virtual ZString TaskDescription
		{
			get { return Res.GetString("3573a945-089c-4174-85b4-597145f6a4e8", "XML Import"); }
		}

		public override ZString UniqueIdentifier
		{
			get
			{
				return ZString.Format("XmlImportTask. Caption: {0}. InterfaceName: {1}.", SearchingDirectoryCaption, InterfaceName.ToString());
			}
		}

		protected abstract DataImporter NewImporter();

		public bool CanImportXmlCollection(string rootCollectionElementName, string collectionXml)
		{
			DataImporter importer = NewImporter();

			if (importer != null)
			{
				if (importer is XmlDataImporter xmlImporter)
				{
					return xmlImporter.CanImportXmlCollection(rootCollectionElementName, collectionXml);
				}
			}

			return false;
		}
	}
}
