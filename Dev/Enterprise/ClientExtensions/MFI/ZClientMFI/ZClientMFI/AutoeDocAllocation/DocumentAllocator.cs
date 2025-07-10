using System;
using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.MFI.AutoeDocAllocation
{
	public abstract class DocumentAllocator
	{
		public void AllocateFiles(INotifications notify)
		{
			AllocateFiles(notify, CancellationToken.None);
		}

		public void AllocateFiles(INotifications notify, CancellationToken token)
		{
			ProcessDocuments(notify, token);
		}

		#region Counters

		protected sealed class Counters
		{
			public Counters()
			{
			}

			public int FilesAttached;
			public int FilesMoved;
			public int FilesRejected;
		}

		protected Counters LogCounters
		{
			get { return logCounters ?? (logCounters = new Counters()); }
		}
		Counters logCounters;

		void ClearLogCounters()
		{
			logCounters = null;
		}

		#endregion

		#region ProcessDocuments

		protected void ProcessDocuments(INotifications notify, CancellationToken token)
		{
			var info = new DirectoryInfo(DirectoryToProcess);
			var files = info.GetFiles();
			CommenceLogging(files, DirectoryToProcess, notify);
			ClearLogCounters();

			foreach (var file in files)
			{
				token.ThrowIfCancellationRequested();
				string fileName = GetReadableFileName(file.Name);
				try
				{
					if (IsAValidDocument(file.Name.ToUpper(CultureInfo.CurrentCulture)))
					{
						notify.Notify(new InfoNotification(string.Format(CultureInfo.InvariantCulture, "Attaching Document: {0}", fileName)));
						var docAttacher = new DocumentAttacher();
						if (docAttacher.AttachFile(file))
						{
							LogCounters.FilesAttached++;
							file.Delete();
						}
						else
						{
							ProcessUnattachedFile(file, fileName, notify);
						}
					}
					else
					{
						notify.Notify(new InfoNotification(string.Format(CultureInfo.InvariantCulture, "File {0} has been rejected as it does not match the required naming convention.", fileName)));
						SendErrorEmail(notify, file);
						file.Delete();
						LogCounters.FilesRejected++;
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					notify.Notify(new ErrorNotification(ErrorType.Error, string.Format(CultureInfo.InvariantCulture, "Error allocating file {0}{1} Exception is: {2}", fileName, System.Environment.NewLine, e.Message)));
				}
			}

			FinishLoggingForThisParse(notify);
		}

		protected virtual void ProcessUnattachedFile(FileInfo documentToAttach, string readableFileName, INotifications notify)
		{
		}

		#endregion

		#region Logging

		protected virtual void CommenceLogging(FileInfo[] filesFound, ZString directoryToProcess, INotifications notify)
		{
			if (filesFound.Length > 0)
			{
				if (filesFound.Length == 1)
				{
					notify.Notify(new InfoNotification("1 file found for automatic eDoc allocation"));
				}
				else
				{
					notify.Notify(new InfoNotification(string.Format(" {0} files found for automatic eDoc allocation", filesFound.Length.ToString())));
				}
			}
			else
			{
				notify.Notify(new InfoNotification("	...no documents found at this time for automatic allocation."));
			}
		}

		protected virtual void FinishLoggingForThisParse(INotifications notify)
		{
			if (LogCounters.FilesRejected > 0)
			{
				if (LogCounters.FilesRejected > 1)
				{
					notify.Notify(new InfoNotification(string.Format("{0} rejected files have been emailed to the notification group", LogCounters.FilesRejected.ToString())));
				}
				else
				{
					notify.Notify(new InfoNotification("1 rejected file has been emailed to the notification group"));
				}
			}
		}

		#endregion

		#region FileValidation

		protected
 bool IsAValidDocument(string fileName)
		{
			bool documentIsValid = false;

			if (fileName.IndexOf(".") != -1 &&
					fileName.LastIndexOf(".") != -1 &&
					fileName.IndexOf(".") != fileName.LastIndexOf("."))
			{
				if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.Various_ORG) ||
					fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.OceanBills_OBL) ||
					fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.OceanBills_O) ||
					fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_BOO) ||
					fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.OceanBills_BO) ||
					fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.HouseBills_BL) ||
					fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_B) ||
					fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_COMOBL) ||
					fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_COM) ||
					fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.AgentAccountNotes_C) ||
					fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_S) ||
					fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.AgentAccountNotes_AGI) ||
					fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.ExpressHouseBill_TLX) ||
					fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.TransportConNote_TSP))
				{
					documentIsValid = true;
				}
				else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_ORD))
				{
					int periodsFound = 0;
					string fileNameAsArray = fileName.TrimStart(new char[] { '0', ' ' });

					foreach (char value in fileNameAsArray)
					{
						if (value == '.')
						{
							periodsFound++;
						}
					}

					if (periodsFound > 2)
					{
						documentIsValid = true;
					}
				}
			}

			return documentIsValid;
		}

		protected virtual string GetReadableFileName(string fileName)
		{
			return fileName;
		}

		#endregion

		#region SendErrorEmail

		protected void SendErrorEmail(INotifications notify, FileInfo file)
		{
			var notificationGroup = MFIDataRegistry.Instance.AutoeDocAllocationNotificationGroup;

			if (notificationGroup != Guid.Empty)
			{
				var attachments = new AttachmentDefCollection();
				attachments.Add(new AttachmentDef(file.FullName));

				var buffer = new NotificationBuffer(notify);
				buffer.Clear();
				buffer.SendEmail(notificationGroup, null, EmailErrorSubject, (EmailBodyHeader + System.Environment.NewLine + System.Environment.NewLine), EmailBodyFooter, notify, Env.OutgoingMailManager, new BusinessObjectFactory(), attachments);
			}
		}

		internal const string EmailErrorSubject = "Error when auto allocating eDocs";
		internal const string EmailBodyHeader = "The attached file has been rejected by the Auto eDoc Allocation batch process, as it does not match the required naming convention. ";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		internal const string EmailBodyFooter = "If the file should validly be attached to a CargoWise One object, either attach it to the relevant CargoWise One reference using the manual eDocs capability within the system, or alternatively, rename the document to meet the agreed naming convention and re-load it back into the document source directory, to be processed on a subsequent run of the batch processor.";

		#endregion

		#region SendExpiredEmail

		protected void SendExpiredEmail(INotifications notify, FileInfo file)
		{
			var notificationGroup = MFIDataRegistry.Instance.AutoeDocAllocationNotificationGroup;

			if (notificationGroup != Guid.Empty)
			{
				var attachments = new AttachmentDefCollection();
				attachments.Add(new AttachmentDef(file.FullName));

				var buffer = new NotificationBuffer(notify);
				buffer.Clear();
				buffer.SendEmail(notificationGroup, null, EmailExpiredSubject, (EmailExpiredBodyHeader + System.Environment.NewLine + System.Environment.NewLine), EmailExpiredFooter, notify, Env.OutgoingMailManager, new BusinessObjectFactory(), attachments);
			}
		}

		internal const string EmailExpiredSubject = "File 'Hold Period' has expired for auto allocating to eDocs";
		internal string EmailExpiredBodyHeader = "The attached file has been rejected by the Auto eDoc Allocation batch process, as the time period for being held, (" + MFIDataRegistry.Instance.AutoeDocAllocationHoldPeriod.ToString() + " days), has elapsed.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		internal const string EmailExpiredFooter = "If the file should validly be attached to an CargoWise One object, either attach it to the relevant CargoWise One reference using the manual eDocs capability within the system, or alternatively, rename the document to the valid CargoWise One reference or create the relevant reference within the system that this document should attach to and re-load it back into the document source directory, to be processed on the next run of the batch processor.";

		#endregion

		protected abstract string DirectoryToProcess { get; }
	}
}
