using System;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public enum JXCExportValidationType
	{
		None,
		Air,
		Ocean,
		ProfitShare,
		Invoicing
	}

	public abstract class JXCMessageExporter
	{
		public JXCMessageExporter(IJXCExportHeader headerData, INotifications notificationSubscriber)
		{
			this.HeaderData = headerData;
			this.NotificationSubscriber = notificationSubscriber;
			CheckForNullArguments(headerData, notificationSubscriber);
		}

		public bool WriteToFile()
		{
			return WriteToFile("");
		}

		public bool WriteToFile(ZString exportPath)
		{
			bool result = false;

			try
			{
				if (exportPath.IsEmpty)
				{
					exportPath = JASDataRegistry.Instance.JXCOutgoingDirectoryName;
				}

				result = WriteToFileCore(exportPath);
			}
			catch (IOException ex)
			{
				NotifyIOExceptionError(ex);
			}
			catch (UnauthorizedAccessException ex)
			{
				NotifyIOExceptionError(ex);
			}
			catch (NotSupportedException ex) // exporting to a drive that is not formatted to NTFS will throw this exception; please check relevant work item for details
			{
				NotifyIOExceptionError(ex);
			}

			return result;
		}

		#region Implementation

		void NotifyIOExceptionError(Exception ex)
		{
			ErrorNotification errorNotification = new ErrorNotification(ErrorType.IOError, "Cannot create and/or write file. " + ex.Message);
			NotificationSubscriber.Notify(errorNotification);
		}

		void NotifyNothingToExport()
		{
			WarningNotification warningNotification = new WarningNotification("No data to be exported");
			NotificationSubscriber.Notify(warningNotification);
		}

		void NotifyExportStarting()
		{
			string message = string.Format("Start Exporting JXC message for '{0}' by {1} ({2})", HeaderData.HumanReadableName, GlbStaff.CurrentUser.GS_LoginName, GlbStaff.CurrentUser.GS_FullName);
			InfoNotification infoNotification = new InfoNotification(message);
			NotificationSubscriber.Notify(infoNotification);
		}

		void NotifyExportSuccessful(ZString exportPath)
		{
			InfoNotification infoNotification = new InfoNotification("JXC Message for '" + HeaderData.HumanReadableName + "' has been successfully exported to \"" + exportPath + "\"");
			NotificationSubscriber.Notify(infoNotification);
		}

		readonly IFileMapper fileMapper = ObjectFactory.Get<IFileMapper>();

		protected virtual
		bool WriteToFileCore(ZString exportPath)
		{
			var result = false;

			var fileNamesAndContents = GetMessageFileNamesAndContents();
			if (fileNamesAndContents != null && fileNamesAndContents.Length > 0)
			{
				var notifiedStart = false;
				foreach (var fileNameAndContents in GetMessageFileNamesAndContents())
				{
					var fileFullPath = Path.Combine(exportPath, PathValidation.GetSafeFilename(fileNameAndContents.FileName));
					using (var fileStream = GetFileStreamForWriting(fileFullPath))
					{
						if (fileStream == null || fileStream == Stream.Null)
						{
							return false;
						}
						if (!notifiedStart)
						{
							NotifyExportStarting();
							notifiedStart = true;
						}

						using (var writer = new StreamWriter(fileStream))
						{
							var messageAsString = GetMessageAsString(fileNameAndContents.MessageLines);
							writer.Write(messageAsString);
						}
					}
				}
				NotifyExportSuccessful(exportPath);
				result = true;
			}
			else
			{
				NotifyNothingToExport();
				result = false;
			}

			return result;
		}

		Stream GetFileStreamForWriting(string filePath)
		{
			Stream fileStream = Stream.Null;

			if (!string.IsNullOrEmpty(filePath))
			{
				try
				{
					fileStream = fileMapper.OpenWrite(filePath);
				}
				catch (IOException)
				{
					fileStream = null;
				}
				catch (UnauthorizedAccessException)
				{
					fileStream = null;
				}
				catch (NotSupportedException) // exporting to a drive that is not formatted to NTFS will throw this exception; please check relevant work item for details
				{
					fileStream = null;
				}
				catch (ArgumentException)
				{
					fileStream = null;
				}
			}

			if (fileStream == null || fileStream == Stream.Null)
			{
				string errorMessage = string.Format(CultureInfo.CurrentCulture, "Cannot create Export Path \"{0}\". Please check if path is valid", Path.GetDirectoryName(filePath));
				ErrorNotification errorNotification = new ErrorNotification(ErrorType.Error, errorMessage);
				NotificationSubscriber.Notify(errorNotification);
			}

			return fileStream;
		}

		ZString GetMessageAsString(MessageLine[] contentLines)
		{
			StringBuilder builder = new StringBuilder();

			HEADLine header = new HEADLine(HeaderData);
			TRLRLine trailer = new TRLRLine();
			builder.Append(header.LineAsString);
			builder.Append("\r\n");
			builder.Append(GetMessageContent(contentLines));
			builder.Append(trailer.LineAsString);

			return builder.ToString();
		}

		ZString GetMessageContent(MessageLine[] contentLines)
		{
			StringBuilder result = new StringBuilder();
			foreach (MessageLine line in contentLines)
			{
				result.Append(line.LineAsString);
				result.Append("\r\n");
			}
			return result.ToString();
		}

		void CheckForNullArguments(IJXCExportHeader headerData, INotifications notificationSubscriber)
		{
			if (headerData == null)
			{
				throw new ArgumentNullException(nameof(headerData));
			}

			if (notificationSubscriber == null)
			{
				throw new ArgumentNullException(nameof(notificationSubscriber));
			}
		}

		#endregion

		#region MessageFileNameAndContents

		protected abstract MessageFileNameAndContents[] GetMessageFileNamesAndContents();

		public struct MessageFileNameAndContents
		{
			public MessageFileNameAndContents(ZString fileName, MessageLine[] messageLines)
			{
				this.FileName = fileName;
				this.MessageLines = messageLines;
			}

			public ZString FileName;
			public MessageLine[] MessageLines;
		}

		#endregion

		public abstract JXCExportValidationType ExportValidationTypeToUse { get; }

		public readonly IJXCExportHeader HeaderData;
		public readonly INotifications NotificationSubscriber;
	}
}
