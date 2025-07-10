using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.DocumentImaging
{
	public class DocumentImageImporter
	{
		public DocumentImageImporter(INotifications notifications)
		{
			this.notifications = notifications;
			this.emailedNotifications = new UPEDocumentImagingNotifications(notifications);
		}

		ZDateTime lastTimeDocumentsImported = ZDateTime.UtcNow;

		internal int ExecuteBatchForTest()
		{
			return ExecuteBatch(CancellationToken.None);
		}

		public int ExecuteBatch(CancellationToken token)
		{
			ZInt result = 0;
			List<DocumentIndexFileObject> documentsImported = ImportAllAvailableDocuments(token);
			if (documentsImported.Count != 0)
			{
				result = documentsImported.Count;
				lastTimeDocumentsImported = ZDateTime.UtcNow;
			}
			else
			{
				NotifyUserIfNoDocumentsImportedInLast4Hours();
			}

			emailedNotifications.SendEmailIfRequired();
			return result;
		}

		List<DocumentIndexFileObject> ImportAllAvailableDocuments(CancellationToken token)
		{
			List<DocumentIndexFileObject> result = new List<DocumentIndexFileObject>();
			if (IOExecuter.ExecuteIOTask(DocumentIndexFilesDelegate, DocumentImagingFolder.FullName))
			{
				foreach (FileInfo documentIndexFile in documentIndexFiles)
				{
					token.ThrowIfCancellationRequested();
					if (documentIndexFile != null && documentIndexFile.Exists)
					{
						DocumentIndexFileObject documentIndexFileObject = Import(new DocumentIndexFileObject(documentIndexFile, notifications, emailedNotifications));
						if (documentIndexFileObject != null)
						{
							result.Add(documentIndexFileObject);
						}
					}
				}
			}
			return result;
		}

		void DocumentIndexFilesCore()
		{
			documentIndexFiles = DocumentImagingFolder.GetFiles(DocumentIndexFileObject.IndexFilePattern);
		}
		FileInfo[] documentIndexFiles;

		DocumentIndexFileObject Import(DocumentIndexFileObject documentIndexFileObject)
		{
			DocumentIndexFileObject result;
			notifications.Notify(new InfoNotification(string.Format(importingDocumentMessageFormat, documentIndexFileObject.IndexFile.FullName)));
			DocumentImageSet documentSet = new DocumentImageSet(documentIndexFileObject, notifications, emailedNotifications);
			result = documentSet.Import();
			emailedNotifications.Flush();
			return result;
		}
		const string importingDocumentMessageFormat = "Importing document with index file '{0}'";

		void NotifyUserIfNoDocumentsImportedInLast4Hours()
		{
			if ((ZDateTime.UtcNow - lastTimeDocumentsImported) > new TimeSpan(4, 0, 0))
			{
				emailedNotifications.Notify(new WarningNotification(noDocumentsImportedForSomeTimeMessage));
			}
		}
		const string noDocumentsImportedForSomeTimeMessage = "*** No documents have been successfully imported for the past 4 hours ***";

		DirectoryInfo DocumentImagingFolder
		{
			get { return documentImagingFolder ?? (documentImagingFolder = UPEDataRegistry.Instance.DocumentImagingRepository); }
		}
		DirectoryInfo documentImagingFolder;

		IOTaskDelegate DocumentIndexFilesDelegate
		{
			get { return readIndexFileDelegate ?? (readIndexFileDelegate = new IOTaskDelegate(DocumentIndexFilesCore)); }
		}
		IOTaskDelegate readIndexFileDelegate;

		NonPersistentIOExecuter IOExecuter
		{
			get { return ioExecuter ?? (ioExecuter = new NonPersistentIOExecuter(notifications, emailedNotifications)); }
		}
		NonPersistentIOExecuter ioExecuter;

		readonly INotifications notifications;
		readonly UPEDocumentImagingNotifications emailedNotifications;
	}
}
