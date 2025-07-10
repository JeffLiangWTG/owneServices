using System;
using System.IO;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class XmlMultiTypeImportTask : XmlImportTask
	{
		public XmlMultiTypeImportTask(BatchImportDirector importDirector, INotifications notify)
			: this(SystemDataRegistry.Instance.DefaultMessagesDataImportDirectory, importDirector, notify, SystemDataRegistry.Instance.UnprocessedMessageNotificationGroup)
		{
		}

		public XmlMultiTypeImportTask(StringRegistryItem registryPath, BatchImportDirector importDirector, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.XmlMultiTypeImport)
		{
			this.importDirector = importDirector;
		}

		#region Overrides

		protected override void ProcessFileCore(FileInfo dataFile, INotifications notifications)
		{
			NotificationBuffer buffer = new NotificationBuffer(notifications);
			XmlImportTask task;
			ZString interchangeInfo = "";
			ZString rootCollectionElementName = "";
			ZString collectionXml = "";
			using (XmlTextReader reader = new XmlTextReader(dataFile.FullName))
			{
				try
				{
					reader.WhitespaceHandling = WhitespaceHandling.None;
					reader.MoveToContent();
					if (reader.Name == "XmlInterchange" && reader.IsStartElement())
					{
						reader.Read();
						if (reader.Name == "InterchangeInfo")
						{
							interchangeInfo = reader.ReadOuterXml();
							if (reader.Name == "Payload" && reader.IsStartElement())
							{
								reader.Read();
							}
						}
					}
					while (!reader.EOF && reader.IsStartElement())
					{
						rootCollectionElementName = reader.Name;
						collectionXml = reader.ReadOuterXml();
						task = FindImportTask(rootCollectionElementName, collectionXml);
						if (task == null)
						{
							buffer.Notify(new ErrorNotification(ErrorType.UnknownRecordType, Res.GetString("1caae8cd-ac4a-4b06-bda2-d4ae713f0451", "{0} record type is unknown.", rootCollectionElementName)));
							GenerateBadMessageFile(dataFile.Name, rootCollectionElementName, CompileXmlFileString(interchangeInfo, collectionXml), buffer);
						}
						else
						{
							if (!task.GetSearchingDirectoryForCurrentCompany().IsEmpty)
							{
								GenerateNewMessageFile(task.GetSearchingDirectoryForCurrentCompany(), dataFile.Name, rootCollectionElementName, CompileXmlFileString(interchangeInfo, collectionXml), buffer);
							}
							else
							{
								buffer.Notify(new ErrorNotification(ErrorType.MissingDataDirectory, task.SearchingDirectoryCaption));
								GenerateBadMessageFile(dataFile.Name, rootCollectionElementName, CompileXmlFileString(interchangeInfo, collectionXml), buffer);
							}
						}
					}
				}
				catch (XmlException e)
				{
					buffer.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, e.Message));
				}
				finally
				{
					reader.Close();
				}
			}
			if (buffer.ContainsNotificationType(ErrorType.ImportingDataError))
			{
				//SendEmailToNotificationGroup(dataFile, Buffer);
			}
		}

		#endregion

		#region Implementation

		protected override bool IsEnvironmentDataValid()
		{
			return IsBadMessagesDirectoryEnvironmentValid();
		}

		ZString CompileXmlFileString(ZString interchangeInfoXml, ZString collectionXml)
		{
			if (interchangeInfoXml.IsEmpty)
			{
				return collectionXml;
			}
			else
			{
				return "<XmlInterchange>" + interchangeInfoXml + "<Payload>" + collectionXml + "</Payload></XmlInterchange>";
			}
		}

		void GenerateNewMessageFile(ZString newFileDirectoryName, ZString originalFileName, ZString rootCollectionElementName, ZString collectionXml, NotificationBuffer buffer)
		{
			ZString newFileName = ConstructNewMessageFileName(originalFileName, rootCollectionElementName);
			WriteToFile(newFileName, collectionXml, newFileDirectoryName, buffer);
		}

		static ZString ConstructNewMessageFileName(ZString originalFileName, ZString rootCollectionElementName)
		{
			return GetUniqueFilePrefix() + "_" + originalFileName;
		}

#if DEBUG
		internal static ZString ConstructNewMessageFileNameForTest(ZString originalFileName, ZString rootCollectionElementName)
		{
			return ConstructNewMessageFileName(originalFileName, rootCollectionElementName);
		}
#endif

		XmlImportTask FindImportTask(ZString rootCollectionElementName, ZString collectionXml)
		{
			XmlImportTask result = null;

			foreach (ImportTask task in importDirector.ImportTasks)
			{
				XmlImportTask xmlTask = task as XmlImportTask;
				if (xmlTask != null)
				{
					if (xmlTask.CanImportXmlCollection(rootCollectionElementName, collectionXml))
					{
						result = xmlTask;
						break;
					}
				}
			}

			return result;
		}

		readonly BatchImportDirector importDirector;

		#endregion

		protected override DataImporter NewImporter()
		{
			throw new NotImplementedException(Res.GetString("551cd13b-7445-435a-8133-cf0afd3df9e7", "This XML file cannot be imported via Service Task. Please import this file via the User Interface."));
		}
	}
}
