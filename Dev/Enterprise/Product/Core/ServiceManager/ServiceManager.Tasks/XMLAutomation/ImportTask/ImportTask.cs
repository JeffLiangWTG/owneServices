using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public abstract class ImportTask : XMLTask
	{
		public ImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(notify)
		{
			if (notificationGroup == null)
			{
				throw new ArgumentNullException(nameof(notificationGroup), "Notification group should not be null");
			}

			this.searchingDirectories = GetSearchingDirectories(registryPath);
			this.SearchingDirectoryCaption = registryPath.Caption;
			this.NotificationGroup = notificationGroup;
		}

		static Dictionary<ZString, string> GetSearchingDirectories(StringRegistryItem registryPath)
		{
			Dictionary<ZString, string> searchingDirectories = new Dictionary<ZString, string>();
			string searchingDirectory = registryPath.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (searchingDirectory != registryPath.DefaultValue)
			{
				searchingDirectories.Add(ZString.Empty, searchingDirectory);
			}

			if ((registryPath.Storage & RegistryStorageFlags.Company) != 0)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				foreach (string companyCode in DisposableEnvironment.GetActiveCompanies())
				{
					GlbCompany company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);
					searchingDirectory = registryPath.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
					if (searchingDirectory != registryPath.DefaultValue && !searchingDirectories.ContainsValue(searchingDirectory))
					{
						searchingDirectories.Add(companyCode, searchingDirectory);
					}
				}
			}

			return searchingDirectories;
		}

		protected override void RunTask()
		{
			bool process;

			List<string> filesProcessed = new List<string>();

			using (var mutexes = new DisposableList(16))
			{
				foreach (FileInfo dataFile in CheckForNewData())
				{
					if (!File.Exists(dataFile.FullName))
					{
						continue;
					}

					SqlApplicationLock mutex;
					process = false;
					try
					{
						if (Db.Connection.TryGetLock(string.Format(CultureInfo.InvariantCulture, "ImportTask. Caption: {0}. Path: {1}.", SearchingDirectoryCaption, dataFile.FullName), out mutex))
						{
							mutexes.Add(mutex);
							//handle race condition
							if (!File.Exists(dataFile.FullName))
							{
								continue;
							}

							SetInfoNotification(Res.GetString("10ee19a7-e674-42bb-8c0e-f46ef8d44eeb", "Importing {0}...", dataFile.Name), true);
							ProcessFile(dataFile);
							SetInfoNotification(Res.GetString("91e2c227-03e4-43d5-b1a9-6a2f8da14cb2", "Finished importing {0}.", dataFile.Name), false);
							process = true;
						}
					}
					catch (IOException ex)
					{
						NotifyIOException(dataFile, ex);
					}
					catch (UnauthorizedAccessException ex)
					{
						NotifyIOException(dataFile, ex);
					}
					catch (System.Xml.XmlException ex)
					{
						process = NotifyImportException(dataFile, ex);
					}
					catch (InvalidOperationException ex)
					{
						if (ex.Source == "System.Xml")
						{
							process = NotifyImportException(dataFile, ex);
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce("XmlAndFlatFileDirectory_ImportTask_RunTask_" + ex.GetType().Name, "Error occurred during batch import of xml", ex);
						process = NotifyImportException(dataFile, ex);
					}
					finally
					{
						if (process)
						{
							string mes;
							if (!TempFile.TryDeleteHandleAllExceptions(dataFile.FullName, out mes))
							{
								SendEmailToNotificationGroupWithoutFile(dataFile.Name, Res.GetString("33e8aa3b-665c-4c11-81e6-371f9314319a", "The file {0} cannot be deleted. The error message is:\r\n{1}", dataFile.Name, mes));
							}
							else
							{
								filesProcessed.Add(dataFile.FullName);
							}
						}
					}
				}
				AfterImport();

				//Apparently deleting a file returns before the actual delete occurs - so we need to wait until File.Exists returns false for all our files before releasing our locks.
				foreach (string file in filesProcessed)
				{
					int retries = 0;
					while (File.Exists(file) && retries < 3)
					{
						string mes;
						if (!TempFile.TryDeleteHandleAllExceptions(file, out mes))
						{
							break; //no reporting exception here - most likely cause anyway is that the delete finally came through, so of course deleting fails.
						}
						Thread.Sleep(100);
						++retries;
					}
				}
			}
		}

		void NotifyIOException(FileInfo dataFile, Exception ex)
		{
			SetInfoNotification(Res.GetString("438b87a7-f85f-4771-a932-da47cd2b7c80", "Could not import file due to an IO error:{0}. {1}", dataFile.Name, ex.Message), true);
		}

		bool NotifyImportException(FileInfo dataFile, Exception ex)
		{
			string errorMessage = Res.GetString("7438f707-b6b7-4166-b18f-b16ba8c9ead8", "The file {0} could not be imported as an error has occurred: {2}\r\n\r\nException details follow:\r\n{1}",
						dataFile.FullName, ex, ex.Message);
			Notify.Notify(new ErrorNotification(ErrorType.Error, errorMessage));
			return SendEmailToNotificationGroup(NotificationGroup.Value, NotificationGroup, dataFile, errorMessage);
		}

		protected virtual void AfterImport()
		{
		}

		protected void SendEmailToNotificationGroupWithoutFile(ZString filename, ZString message)
		{
			EmailDef email = new EmailDef();
			email.Subject = Res.GetString("0a3ae66d-97a3-4006-a488-d433fba829b3", "{0} - {1}", NotificationEmailSubject, filename);
			email.Body = message;
			try
			{
				Env.OutgoingMailManager.CreateAndSave(email, NotificationGroup.Value, GroupSourceLocator.GetFromRegistryItem(NotificationGroup));
			}
			catch (EmailSendFailedException) { }
		}

		void SetInfoNotification(ZString message, bool newLine1)
		{
			if (newLine1)
			{
				NewlineNotification newLine = new NewlineNotification();
				Notify.Notify(newLine);
			}
			InfoNotification info = new InfoNotification(message);
			Notify.Notify(info);
		}

		IEnumerable<FileInfo> CheckForNewData()
		{
			foreach (var kv in searchingDirectories)
			{
				string searchingDirectory = kv.Value;
				if (!String.IsNullOrEmpty(searchingDirectory))
				{
					if (Directory.Exists(searchingDirectory))
					{
						DirectoryInfo directoryPath = new DirectoryInfo(searchingDirectory);
						FileInfo[] files = null;

						try
						{
							files = directoryPath.GetFiles(FileExtension);
						}
						catch (IOException ex)
						{
							Notify.Notify(new WarningNotification(Res.GetString("38181f60-29ee-453f-a171-656f5e7f32cc", "Data cannot be imported from directory {0}", searchingDirectory) + "\r\n" + ex.Message));
						}

						if (files != null && files.Length > 0)
						{
							using (!kv.Key.IsEmpty && Env.CurrentCompany.Code != kv.Key ? DisposableEnvironment.ForCompany(kv.Key) : null)
							{
								foreach (FileInfo file in files)
								{
									yield return file;
								}
							}
						}
					}
					else
					{
						Notify.Notify(new WarningNotification(Res.GetString("86359b2f-5ee4-4d18-92e8-bc6ec89874ce", "The directory {0} does not exist. Data cannot be imported from this location", searchingDirectory)));
					}
				}
			}
		}

		public ZString GetSearchingDirectoryForCurrentCompany()
		{
			string result;
			if (searchingDirectories.TryGetValue(GlbCompany.CurrentCompany.GC_Code, out result))
			{
				return result;
			}
			else if (searchingDirectories.TryGetValue(ZString.Empty, out result))
			{
				return result;
			}
			else
			{
				return ZString.Empty;
			}
		}
		readonly Dictionary<ZString, string> searchingDirectories;

		public override ZString UniqueIdentifier
		{
			get
			{
				return ZString.Format("ImportTask. Caption: {0}.", SearchingDirectoryCaption);
			}
		}

		protected abstract void ProcessFile(FileInfo dataFile);
		protected abstract ZString FileExtension { get; }

		public readonly ZString SearchingDirectoryCaption;

#if DEBUG
		public
#else
		protected
#endif
 readonly GuidRegistryItem NotificationGroup;

		protected override string NotificationEmailSubject
		{
			get { return Res.GetString("473087a5-d77b-490b-8560-9d1b05b33bd9", "XML Import Notification Error"); }
		}
	}
}
