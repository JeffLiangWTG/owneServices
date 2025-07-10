using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Interop.DataObjects;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Graph;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class RemoteFile : Disposable, IRemoteFile
	{
		public static bool IsSupported
		{
			get
			{
				return Array.IndexOf(InitializationMessageHandler.RegisteredRemoteMessageTypes, EnterpriseChannelMessageTypes.OpenFile) > -1;
			}
		}

		public RemoteFile()
		{
		}

		public RemoteFile(string fileName, byte[] fileData, bool readOnly) : this(fileName, fileData, readOnly, false)
		{
		}

		public RemoteFile(string fileName, byte[] fileData, bool readOnly, bool sendingExecutionResult)
		{
			openFileMessage = new OpenFileMessage(fileName, fileData, readOnly, sendingExecutionResult);
			if (!readOnly)
			{
				lock (remoteFilesLocker)
				{
					OpenFileChangedHandler.RemoteFiles.Add(openFileMessage.id, this);
				}
			}
		}
		static readonly object remoteFilesLocker = new object();

		readonly OpenFileMessage openFileMessage;
		public OpenFileMessage OpenFileMessage
		{
			get { return openFileMessage; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public bool Open()
		{
			bool openFileWithReturnHandlerExists = Array.IndexOf(InitializationMessageHandler.RegisteredRemoteMessageTypes, EnterpriseChannelMessageTypes.OpenFileWithReturn) > -1;
			bool isSuccess = true;
			var microsoftOffce365ExtensionList = EnvProxy.Instance.Registry.OpenInMicrosoftOffice365FileTypeList
				.Select(e => e.StartsWith(".") ? e : "." + e);
			if (microsoftOffce365ExtensionList.Any(e =>
				e == ".*" ||
				string.Compare(e, Path.GetExtension(openFileMessage.filename), StringComparison.InvariantCultureIgnoreCase) == 0))
			{
				TrackingInfoLogger.Instance.NewLog(() => "Open file in Microsoft Office 365");
				var uploadTask = new Task(() =>
				{
					try
					{
						TrackingInfoLogger.Instance.NewLog(() => "Background upload task started");
						UploadToMicrosoftOffice365AndSendUriToClient();
						TrackingInfoLogger.Instance.NewLog(() => "Background upload task completed");
					}
					catch (Exception ex)
					{
						if (ex is TaskCanceledException canceledException)
						{
							Globals.Message.ShowWarning($"Upload timed out (3 mins): {canceledException.Message}");
						}
						if (ex is ServiceException serviceException)
						{
							Globals.Message.ShowWarning($"Upload to Microsoft Office 365 failed: {serviceException.Message}");
						}
						TrackingInfoLogger.Instance.NewLog(() => $"Upload document to Microsoft Office 365 failed: {ex}");
					}
				});
				uploadTask.Start();
			}
			else if (openFileWithReturnHandlerExists)
			{
				isSuccess = EnterpriseChannel.Instance.SendMessage<OpenFileMessage, bool>(EnterpriseChannelMessageTypes.OpenFileWithReturn, openFileMessage);
			}
			else
			{
				EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.OpenFile, openFileMessage);
			}
			if (!isSuccess)
			{
				OpenFileChangedHandler.RemoteFiles.Remove(openFileMessage.id);
			}
			return isSuccess;
		}

		protected virtual void UploadToMicrosoftOffice365AndSendUriToClient()
		{
			IEnumerable<string> fileUris;
			using (var content = new MemoryStream(openFileMessage.filedata))
			using (var tokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(3)))
			{
				TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Uploading file to Microsoft Office 365");
				fileUris = helper.Upload(openFileMessage.filename, content, tokenSource.Token).GetAwaiter().GetResult();
			}
			TrackingInfoLogger.Instance.NewLog(() => (NoResString)"Sending share links to client side to open");
			fileUris.ForEach(uri => EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.WebUrl, Encoding.UTF8.GetBytes(uri)));
		}

		static string RetriveAccessTokenForEdocs()
		{
			return EnterpriseChannel.Instance.SendMessage<string, string>(EnterpriseChannelMessageTypes.GetMicroSoftOffice365TokenMessage, string.Empty);
		}

		public OpenFileStatusMessage GetStatus()
		{
			return EnterpriseChannel.Instance.SendMessage<OpenFileStatusMessage>(EnterpriseChannelMessageTypes.OpenFileStatus, openFileMessage.id.ToByteArray());
		}

		public
#if DEBUG
 virtual
#endif
 byte[] FetchFileData()
		{
			return EnterpriseChannel.Instance.SendMessage<byte[]>(EnterpriseChannelMessageTypes.OpenFileFetch, openFileMessage.id.ToByteArray());
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (openFileMessage != null)
				{
					EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.OpenFileDispose, openFileMessage.id.ToByteArray());
					if (!openFileMessage.readOnly)
					{
						lock (remoteFilesLocker)
						{
							OpenFileChangedHandler.RemoteFiles.Remove(openFileMessage.id);
						}
					}
				}
			}
			catch (OperationCanceledException)
			{ }
		}

		public event EventHandler FileChanged;

		internal void OnFileChanged()
		{
			if (FileChanged != null)
			{
				FileChanged(this, EventArgs.Empty);
			}
		}

		string IRemoteFile.FileName => OpenFileMessage.filename;

		ReadOnlyMemory<byte> IRemoteFile.OriginalFileData => OpenFileMessage.filedata;

		public bool RemoteFilesSupported => ObjectFactory.Get<TerminalService>().IsWTSSession && IsSupported;

		bool IRemoteFile.GetIsOpenStatus() => GetStatus().isOpen;

		bool IRemoteFile.GetDoesExistStatus() => GetStatus().isExist;

		static GraphClientHelper helper => new GraphClientHelper(RetriveAccessTokenForEdocs());
	}
}
