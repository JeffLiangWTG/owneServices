using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

[assembly: HostedService(
	"ZW1",
	"Shipment and Consol Import",
	"CSP",
	typeof(Enterprise.Client.WFN.ServiceTasks.WFNShipmentAndConsolImportServiceTask),
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1minute"
	)]
namespace Enterprise.Client.WFN.ServiceTasks
{
	public class WFNShipmentAndConsolImportServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			if (IsEnvironmentDataValid())
			{
				Buffer.Notify(new InfoNotification("Starting Dimerco XML Import..."));
				ProcessImportingXml(youMustReactToThisToken);
				Buffer.Notify(new InfoNotification("XML Import Completed..."));
			}
		}

		#region Implementation

		#region ProcessImportingXml

		void ProcessImportingXml(CancellationToken token)
		{
			foreach (FileInfo xmlData in CheckForNewData())
			{
				token.ThrowIfCancellationRequested();
				string fileName = xmlData.Name;

				try
				{
					IValueObject[] externalXmlValue = ImportDataFromXmlFile(xmlData);

					if (externalXmlValue != null)
					{
						ConvertAndSaveToDatabase(externalXmlValue);
						SendNotificationEmail(xmlData);
						BackupFile(xmlData);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (Marshal.GetHRForException(ex) == TempFile.FileIsInUseByAnotherProcess)
					{
						Buffer.Notify(new ErrorNotification(ErrorType.Error, (Res.GetString("a50010df-99de-4d53-9699-a499700a93b7", "Cannot process file {0} as it is locked by another process. The file will be processed in the next run", fileName))));
					}
					else
					{
						Buffer.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
						SendEmail(xmlData.FullName, "Dimerco XML Import - Failure", ex.Message);

						if (xmlData.Exists)
						{
							try
							{
								xmlData.Delete();
							}
							catch (IOException ioEx)
							{
								Buffer.Notify(new ErrorNotification(ErrorType.Error, (Res.GetString("90fcd749-6bf2-45a9-8a89-6db83d6975f5", "Error deleting file {0} - {1}", xmlData.Name, ioEx.Message))));
							}
						}
					}
				}
			}
		}

		void SendNotificationEmail(FileInfo xmlData)
		{
			bool hasError = Buffer.ContainsNotificationType(ErrorType.ImportingDataError);
			string fileToSent = hasError ? xmlData.FullName : "";
			string importStatus = hasError ? "Failure" : "Success";
			string emailBody = "Processed file " + xmlData.Name + System.Environment.NewLine + Buffer.AsString;

			SendEmail(fileToSent, "Dimerco XML Import - " + importStatus, emailBody);
		}

		IValueObject[] ImportDataFromXmlFile(FileInfo xmlData)
		{
			IValueObject[] externalXmlValue = null;

			using (StreamReader reader = xmlData.OpenText())
			{
				WFNXmlDocument document = new WFNXmlDocument(reader, Buffer);
				externalXmlValue = document.ConvertToValueObjects();
			}
			return externalXmlValue;
		}

		void BackupFile(FileInfo xmlData)
		{
			DirectoryInfo backupPath = new DirectoryInfo(xmlData.Directory + @"\Backup");
			if (!backupPath.Exists)
			{
				backupPath.Create();
			}

			string backupFileName = ZDateTime.Now.ToString("dd-MMM-yy_hhmm", CultureInfo.InvariantCulture) + xmlData.Name;

			int count = 0;

			while (xmlData.Exists)
			{
				try
				{
					xmlData.MoveTo(Path.Combine(backupPath.FullName, backupFileName));
					return;
				}
				catch (IOException ex)
				{
					if (count == MaximumRetry)
					{
						throw new IOException(Res.GetString("7ef9b19d-46a2-4395-bacf-2716acd9f328", "Error(s) occurred while moving processed file {0} to backup directory: {1}", xmlData.Name, ex.Message));
					}

					xmlData.Refresh();
					count++;
				}
			}
		}

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
				catch (Exception e) when (!e.IsCriticalException())
				{ }
			}
			Env.OutgoingMailManager.CreateAndSave(email, WFNDataRegistry.Instance.DimercoXMLImportNotificationGroup.Value, GroupSourceLocator.GetFromGroup(NotificationGroup));
		}

#if DEBUG
		protected virtual
#endif
		FileInfo[] CheckForNewData()
		{
			List<FileInfo> result = new List<FileInfo>();
			DirectoryInfo directoryPathInfo = new DirectoryInfo(DirectoryPath);

			Array.ForEach(directoryPathInfo.GetFiles("*.xml"),
				delegate(FileInfo f) { if ((ZDateTime.UtcNow - f.CreationTimeUtc) > new TimeSpan(0, 2, 0))
					{
						result.Add(f);
					}
				}
				);

			return result.ToArray();
		}

		#endregion

		#region ConvertAndSaveToDatabase

#if DEBUG
		protected virtual
#endif
		void ConvertAndSaveToDatabase(IValueObject[] valueObjects)
		{
			WFNXmlConverter converter = new WFNXmlConverter(Factory);
			Xsd.Consol[] consolValues = converter.Convert(valueObjects);

			foreach (Xsd.Consol consolValue in consolValues)
			{
				if (consolValue != null)
				{
					Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
					interchange.InterchangeInfo.EDIOrganisation.OwnerCode = WFNConstants.AgentCode;

					ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, interchange, Buffer);
					WFNForwardingConsolValueObjectDataAdapter adapter = new WFNForwardingConsolValueObjectDataAdapter();
					adapter.CreateOrUpdateFromValueObject(consolValue, importContext);

					Factory.Save();
				}
			}
		}

		#endregion

		#region IsEnvironmentDataValid

		bool IsEnvironmentDataValid()
		{
			bool result = true;

			if (DirectoryPath.IsEmpty || !Directory.Exists(DirectoryPath))
			{
				Buffer.Notify(new ErrorNotification(ErrorType.Error, "The Import Directory is empty or invalid. Please check in System->Registry->WFN Client Extensions->Import->Dimerco"));
				result = false;
			}

			if (WFNDataRegistry.Instance.DimercoXMLImportNotificationGroup.Value == Guid.Empty || NotificationGroup == null)
			{
				Buffer.Notify(new ErrorNotification(ErrorType.Error, "The Notification Group is empty or invalid. Please check in System->Registry->WFN Client Extensions->Import->Dimerco"));
				result = false;
			}

			return result;
		}

		#endregion

		ZString DirectoryPath
		{
			get { return WFNDataRegistry.Instance.DimercoXMLImportDirectory; }
		}

		GlbGroup NotificationGroup
		{
			get
			{
				return notificationGroup ?? (notificationGroup = Factory.Load<GlbGroup>(WFNDataRegistry.Instance.DimercoXMLImportNotificationGroup.Value));
			}
		}
		GlbGroup notificationGroup;

#if DEBUG
		internal
#endif
		NotificationBuffer Buffer
		{
			get
			{
				return buffer ?? (buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber()));
			}
		}
		NotificationBuffer buffer;

#if DEBUG
		internal
#endif
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
		BusinessObjectFactory fFactory;

		const int MaximumRetry = 2;

		#endregion
	}
}
