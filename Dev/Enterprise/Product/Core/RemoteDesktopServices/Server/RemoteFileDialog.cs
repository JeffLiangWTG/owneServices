using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;

namespace Enterprise.RemoteDesktopServices.Server
{
	public static class RemoteFileDialog
	{
		public static bool IsSupported
		{
			get
			{
				return Array.IndexOf(InitializationMessageHandler.RegisteredRemoteMessageTypes, EnterpriseChannelMessageTypes.SaveFileDialog) > -1;
			}
		}

		public static FileDialogResultMessage ShowSaveFileDialog(SaveFileDialog dialog)
		{
			try
			{
				return EnterpriseChannel.Instance.SendMessage<SaveFileDialogMessage, FileDialogResultMessage>(EnterpriseChannelMessageTypes.SaveFileDialog, new SaveFileDialogMessage(dialog));
			}
			catch (OperationCanceledException)
			{
				return CreateCancelledMessage(dialog);
			}
		}

		public static bool SaveFile(string fileName, byte[] data)
		{
			return EnterpriseChannel.Instance.SendMessage<SaveFileMessage, bool>(EnterpriseChannelMessageTypes.SaveFile, new SaveFileMessage(fileName, data));
		}

		public const int LargeFileSizeDefinition = 1 * 1024 * 1024; // 1MB
		public const int FileChunkSize = 64 * 1024; // 64KB < 85,000 (Large Object)
		public static bool SaveStream(string fileName, Stream stream)
		{
			try
			{
				if (stream.Length > LargeFileSizeDefinition)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Saving large file [{fileName}]");
					return SaveLargeFile();
				}

				TrackingInfoLogger.Instance.NewLog(() => $"Saving file [{fileName}]");
				return SaveFile(fileName, GetArrayFromStream());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				TrackingInfoLogger.Instance.NewLog(() => $"Save file [{fileName}] exception: {ex}");
				return false;
			}

			byte[] GetArrayFromStream()
			{
				using (var binaryReader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true)) // do not dispose stream passed in
				{
					return binaryReader.ReadBytes((int)stream.Length);
				}
			}

			bool SaveLargeFile()
			{
				SaveLargeFileMessageCommand currentCommand;
				var fileId = default(int?);

				using (BeginMessageScope(SaveLargeFileMessageCommand.Begin))
				{
					var beginResponse = SendChunk(new SaveLargeFileMessage(currentCommand, fileName), null);
					if (beginResponse.Error != SaveFileErrorCode.NoError)
					{
						TrackingInfoLogger.Instance.NewLog(() => $"Error: [{beginResponse.Error}] | {beginResponse.ErrorMessage}");
						return false;
					}
					fileId = beginResponse.FileId;
				}

				var chunkBuf = new byte[FileChunkSize];

				using (BeginMessageScope(SaveLargeFileMessageCommand.Data))
				{
					int commandIndexBeingSent = 0;
					foreach (var length in new StreamChunker(stream, chunkBuf))
					{
						var dataResponse = SendChunk(
							new SaveLargeFileMessage(currentCommand, fileId.Value, GetAvailableBuffer(length)),
							() => TrackingInfoLogger.Instance.NewLog(() => $"Sending message {++commandIndexBeingSent}"));
						if (dataResponse.Error != SaveFileErrorCode.NoError)
						{
							TrackingInfoLogger.Instance.NewLog(() => $"Error: [{dataResponse.Error}] | {dataResponse.ErrorMessage}");
							return false;
						}
					}
				}

				using (BeginMessageScope(SaveLargeFileMessageCommand.End))
				{
					var endResponse = SendChunk(new SaveLargeFileMessage(currentCommand, fileId.Value), null);
					if (endResponse.Error != SaveFileErrorCode.NoError)
					{
						TrackingInfoLogger.Instance.NewLog(() => $"Error: [{endResponse.Error}] | {endResponse.ErrorMessage}");
						return false;
					}
				}

				return true;

				SaveLargeFileResponseMessage SendChunk(SaveLargeFileMessage message, Action preSendAction)
				{
					preSendAction?.Invoke();
					return EnterpriseChannel.Instance
						// need to finish sending data of FileChunkSize in 120s which period looks long enough
						.SendMessageWithTimeOut<SaveLargeFileMessage, SaveLargeFileResponseMessage>(EnterpriseChannelMessageTypes.SaveLargeFile, TimeSpan.FromSeconds(120d), message)
						.Value;
				}

				byte[] GetAvailableBuffer(int availableLength)
				{
					if (availableLength == FileChunkSize)
					{
						return chunkBuf;
					}

					// It's impossible that availableLength is greater than chunkSize.
					// Then it's safe to copy data to a smaller-size array.
					var lastChunk = new byte[availableLength];
					Array.Copy(chunkBuf, lastChunk, availableLength);
					return lastChunk;
				}

				IDisposable BeginMessageScope(SaveLargeFileMessageCommand command)
				{
					currentCommand = command;
					TrackingInfoLogger.Instance.NewLog(() => $@"
##################################
Sending command: [{command}]");
					return new DisposableAction(() => TrackingInfoLogger.Instance.NewLog(() => $@"
Done
##################################"));
				}
			}
		}

		public static FileDialogResultMessage ShowOpenFileDialog(OpenFileDialog dialog)
		{
			try
			{
				return EnterpriseChannel.Instance.SendMessage<OpenFileDialogMessage, FileDialogResultMessage>(
					EnterpriseChannelMessageTypes.OpenFileDialog,
					new OpenFileDialogMessage(dialog));
			}
			catch (OperationCanceledException)
			{
				return CreateCancelledMessage(dialog);
			}
		}

		static FileDialogResultMessage CreateCancelledMessage(FileDialog dlg)
		{
			var result = new FileDialogResultMessage();
			result.dialogResult = DialogResult.Cancel;
			result.filterIndex = dlg.FilterIndex;
			result.fileName = dlg.FileName;
			result.fileNames = dlg.FileNames;
			return result;
		}

		public static byte[] OpenFile(string fileName)
		{
			return EnterpriseChannel.Instance.SendMessage<string, byte[]>(EnterpriseChannelMessageTypes.ReadFile, fileName);
		}

		public static FolderBrowserDialogResultMessage ShowFolderBrowserDialog(FolderBrowserDialog dialog, bool createDirectory)
		{
			try
			{
				return EnterpriseChannel.Instance.SendMessage<FolderBrowserDialogMessage, FolderBrowserDialogResultMessage>(
					EnterpriseChannelMessageTypes.FolderBrowserDialog,
					new FolderBrowserDialogMessage(dialog, createDirectory));
			}
			catch (OperationCanceledException)
			{
				var msg = new FolderBrowserDialogResultMessage();
				msg.selectedPath = dialog.SelectedPath;
				msg.dialogResult = DialogResult.Cancel;
				return msg;
			}
		}

		public static string[] ListDirectoryFiles(string path, string searchPattern)
		{
			var requests = new ListDirectoryRequest[]
			{
				new ListDirectoryRequest(path, SearchMode.Files, TimeSpan.FromSeconds(15)),
			};
			var results = ListDirectory(requests);
			if (results?.Length == 1 && results[0].error == ListDirectoryError.None)
			{
				return results[0].result.Where(x => x.EndsWith(searchPattern, StringComparison.InvariantCultureIgnoreCase)).ToArray();
			}

			return null;
		}

		public static ListDirectoryResult[] ListDirectory(ListDirectoryRequest[] requests)
		{
			return EnterpriseChannel.Instance.SendMessage<ListDirectoryRequest[], ListDirectoryResult[]>(
				EnterpriseChannelMessageTypes.ListDirectory,
				requests);
		}

		struct StreamChunker
		{
			readonly Stream stream;
			readonly byte[] buffer;

			public StreamChunker(Stream stream, byte[] buffer)
			{
				this.stream = stream;
				this.buffer = buffer;
			}

			public IEnumerator<int> GetEnumerator()
			{
				while (true)
				{
					var numRead = ReadChunk();
					if (numRead == 0)
					{
						break;
					}

					AssertNumReadIsLessThanChunkSize(numRead, buffer.Length);
					yield return numRead;
				}
			}

			[Conditional("DEBUG")]
			static void AssertNumReadIsLessThanChunkSize(int numRead, int chunkSize)
			{
				if (numRead > chunkSize)
				{
					throw new InvalidOperationException($"numRead [{numRead}] should be less than chunkSize [{chunkSize}].");
				}
			}

			int ReadChunk()
			{
				var offset = 0;
				var countLeftToRead = buffer.Length;

				do
				{
					var numReadThisTime = stream.Read(buffer, offset, countLeftToRead);
					if (numReadThisTime == 0)
					{
						break;
					}

					offset += numReadThisTime;
					countLeftToRead -= numReadThisTime;
				}
				while (countLeftToRead > 0);

				return offset;
			}
		}
	}
}
