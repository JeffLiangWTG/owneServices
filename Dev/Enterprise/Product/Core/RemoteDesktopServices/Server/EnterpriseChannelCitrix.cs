using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.RemoteDesktopServices.WtsApi;

namespace Enterprise.RemoteDesktopServices.Server
{
	/// <summary>
	/// Legacy Citrix channel logic for talking to the old plugin that uses the Citrix ICA API.
	/// Needed for telling the old plugin to download and run the upgrade.
	/// Nowadays the Citrix plugin uses the same Microsoft DVC API as the MS Remote App plugin.
	/// </summary>
	public class EnterpriseChannelCitrixLegacy : EnterpriseChannel
	{
		protected override string GetChannelName() => EnterpriseChannelInfo.CitrixChannelLegacyName;
		protected override bool GetDynamicChannel() => false;

		protected override int ChannelPDULength
		{
			get
			{
				return CitrixCHANNEL_PDU_LENGTH;
			}
		}
		const int CitrixCHANNEL_PDU_LENGTH = 4996;

		protected override void HandleRead(byte[] buffer, int length)
		{
			var flag = buffer[0];
			if (length > 0)
			{
				if (flag == (byte)ChannelFlags.First || flag == (byte)ChannelFlags.Only)
				{
					currentMessageType = Encoding.ASCII.GetString(buffer, 1, 4);
					TrackingInfoLogger.Instance.NewLog(() => $"Received message: {currentMessageType}");

					currentMessageStream = new MemoryStream();
					currentMessageStream.Write(buffer, 5, length - 5);
				}
				else if (currentMessageStream != null)
				{
					currentMessageStream.Write(buffer, 1, length - 1);
				}
			}

			if ((flag == (byte)ChannelFlags.Only || flag == (byte)ChannelFlags.Last) && currentMessageStream != null && currentMessageStream.Length > 0)
			{
				if (currentMessageType != null)
				{
					currentMessageStream.Position = 0;
					try
					{
						TrackingInfoLogger.Instance.NewLog(() => $"Handling message: {currentMessageType}");
						MessageHandlers.HandleMessage(this, currentMessageType, currentMessageStream);
						TrackingInfoLogger.Instance.NewLog(() => $"Handled message: {currentMessageType}");
					}
					catch (Exception ex) when (IsDataCorruptedException(ex, currentMessageType))
					{
						TrackingInfoLogger.Instance.NewLog(() => $"Exception in handling message: {ex}");
						ApplicationDispatcher.Current?.Invoke(() => Globals.Message.ShowWarning(dataCorruptedRetryMessage));
					}
				}
				currentMessageType = null;
				currentMessageStream = null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context")]
		bool IsDataCorruptedException(Exception ex, string messageType)
		{
			return
				messageType == EnterpriseChannelMessageTypes.DragDrop && ex is InvalidOperationException && ex.InnerException is XmlException xmlException && xmlException.LineNumber == 7 || // fileData is located at the 7th line of DragDropMessage in XML format
				ex is System.Runtime.Serialization.SerializationException && ex.Message.StartsWith("The input stream is not a valid binary format", StringComparison.Ordinal);
		}

		protected override bool SendCore(Stream stream, byte[] data)
		{
			int length = data.Length;
			IAsyncResult result;
			const int CitrixMaxBufferSize = CitrixCHANNEL_PDU_LENGTH - 1;
			int offset = 0;
			int remain;
			byte[] flag = new byte[1];
			int count;
			try
			{
				while (offset < length)
				{
					remain = length - offset;

					if (remain > CitrixMaxBufferSize)
					{
						count = CitrixMaxBufferSize;
						flag[0] = offset == 0 ? (byte)ChannelFlags.First : (byte)ChannelFlags.Middle;
					}
					else
					{
						count = remain;
						flag[0] = offset == 0 ? (byte)ChannelFlags.Only : (byte)ChannelFlags.Last;
					}
					var dataWithFlag = new byte[count + 1];
					dataWithFlag[0] = flag[0];
					Buffer.BlockCopy(data, offset, dataWithFlag, 1, count);
					offset += CitrixMaxBufferSize;

					lock (writeLock)
					{
						result = stream.BeginWrite(dataWithFlag, 0, count + 1, null, null);
					}
					result.AsyncWaitHandle.WaitOne();
					lock (writeLock)
					{
						stream.EndWrite(result);
					}
				}
			}
			catch (IOException ex) when (ex.Message.StartsWith(CitrixPluginNotInstalledErrorMessage, StringComparison.Ordinal) || ex.HResult == InValidHandle)
			{
				// Ignore
			}

			TrackingInfoLogger.Instance.NewLog(() => $"Message sent");
			return true;
		}
		readonly object writeLock = new object();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string CitrixPluginNotInstalledErrorMessage = "Incorrect function"; // When Citrix Services is not installed
		const int InValidHandle = -2147024890; // When client side is already terminated

		protected override void VirtualChannelClose()
		{
			SendMessage(EnterpriseChannelMessageTypes.CloseCitrixChannel, Array.Empty<byte>());
		}

#if DEBUG
		public int ChannelPDULengthExposed
		{
			get
			{
				return ChannelPDULength;
			}
		}

		public void HandleReadExposed(byte[] buffer, int length)
		{
			HandleRead(buffer, length);
		}

		public bool SendCoreExposed(Stream stream, byte[] data)
		{
			return SendCore(stream, data);
		}
#endif
	}
}
