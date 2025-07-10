using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Interop.DataObjects;
using Enterprise.RemoteDesktopServices.Client;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.RemoteDesktopServices.WtsApi;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class CitrixLegacyTransmissionTest : TestCase
	{
		public void TestGetEnterpriseChannel()
		{
			var citrixPlugin = new CitrixLegacyPlugin();
			AssertEquals(0, CitrixLegacyPlugin.AllChannels.Length);
			var citrixChannel1 = citrixPlugin.GetEnterpriseChannel(1, mockWritePointer);
			AssertEquals(1, CitrixLegacyPlugin.AllChannels.Length);
			var citrixChannel2 = citrixPlugin.GetEnterpriseChannel(1, mockWritePointer);
			AssertEquals(1, CitrixLegacyPlugin.AllChannels.Length);
			Assert(citrixChannel1 == citrixChannel2);
			var citrixChannel3 = citrixPlugin.GetEnterpriseChannel(2, mockWritePointer);
			AssertEquals(2, CitrixLegacyPlugin.AllChannels.Length);
		}

		public void TestSendMessageLoop()
		{
			//initialize//
			var handler = new TestMessageHandler();
			MessageHandlers.Register("TEST", handler);
			//server
			var serverChannel = Server.EnterpriseChannel.Instance as EnterpriseChannelCitrixLegacy;
			AssertNotNull(serverChannel);
			AssertEquals(4996, serverChannel.ChannelPDULengthExposed);
			//client
			var clientPlugin = new CitrixLegacyPlugin();

			//transmission//
			//server send
			var rawData = new byte[6000];
			Encoding.ASCII.GetBytes("TEST", 0, 4, rawData, 0);
			var channelStream = new MemoryStream();
			serverChannel.SendCoreExposed(channelStream, rawData);
			channelStream.Position = 0;

			//client receive
			var streamLength = (uint)channelStream.Length;
			AssertEquals(6002, streamLength);
			var serverData = new byte[streamLength];
			channelStream.Read(serverData, 0, (int)streamLength);
			int remain = 0;
			int offset = 0;
			byte[] currentBuffer = new byte[4996];
			Assert(handler.IsDataCorrupted);
			while (offset < streamLength)
			{
				remain = (int)streamLength - offset;
				Array.Copy(serverData, offset, currentBuffer, 0, remain > 4996 ? 4996 : remain);
				clientPlugin.OnDataReceived(mockWritePointer, 1, currentBuffer, (uint)currentBuffer.Length);
				offset = offset + currentBuffer.Length;
			}
			handler.TestHandled.WaitOne(TimeSpan.FromSeconds(3));
			Assert(!handler.IsDataCorrupted);

			//client send

			//server receive
			remain = 0;
			offset = 0;
			handler.Reset();
			Assert(handler.IsDataCorrupted);
			while (offset < streamLength)
			{
				remain = (int)streamLength - offset;
				Array.Copy(serverData, offset, currentBuffer, 0, remain > 4996 ? 4996 : remain);
				serverChannel.HandleReadExposed(currentBuffer, currentBuffer.Length);
				offset = offset + currentBuffer.Length;
			}
			handler.TestHandled.WaitOne(TimeSpan.FromSeconds(3));
			Assert(!handler.IsDataCorrupted);
		}

		public void TestUnexpectedMiddleMessageIgnored()
		{
			var handler = new TestMessageHandler();
			MessageHandlers.Register("TEST", handler);
			var serverChannel = Server.EnterpriseChannel.Instance as EnterpriseChannelCitrixLegacy;
			var buffer = new byte[] { 0, 1, 2, 3 }; // Middle
			serverChannel.HandleReadExposed(buffer, buffer.Length);
			buffer = new byte[] { 3, 0x54, 0x45, 0x53, 0x54, 0x0 };
			serverChannel.HandleReadExposed(buffer, buffer.Length);
			handler.TestHandled.WaitOne(TimeSpan.FromSeconds(3));
			Assert(!handler.IsDataCorrupted);
		}

		public void TestHandleCorruptedDragDropMessage()
		{
			var corruptedMessageBytes = CreateCorruptedDragDropMessageBytes();

			MessageHandlers.Register(EnterpriseChannelMessageTypes.DragDrop, new Server.DragDropHandler());
			var serverChannel = Server.EnterpriseChannel.Instance as EnterpriseChannelCitrixLegacy;
			var totalMessageLength = corruptedMessageBytes.Length + 1;
			var messageBuffer = new byte[totalMessageLength];
			messageBuffer[0] = (byte)ChannelFlags.Only;
			corruptedMessageBytes.CopyTo(messageBuffer, 1);
			AssertNoExceptionThrown(() => serverChannel.HandleReadExposed(messageBuffer, totalMessageLength));
		}

		[SuppressMessage("Enterprise", "EDI011:TempPathRule", Justification = "Test Only")]
		byte[] CreateCorruptedDragDropMessageBytes()
		{
			var testFileName = Path.Combine(Path.GetTempPath(), "test.txt");

			using (var dragDropDataObject = new ZAutoDeleteFileDropDataObject(testFileName, false))
			{
				var notificationService = new Mock<INotificationService>();
				notificationService.Setup(x => x.ShowError(It.IsAny<IWin32Window>(), It.IsAny<string>())).Verifiable();

				File.WriteAllText(testFileName, "test file");
				var dragDropMessage = DragDropMessage.CreateDragDropMessage(dragDropDataObject, notificationService.Object);
				var messageBytes = MessageChannel.Serialize(EnterpriseChannelMessageTypes.DragDrop, dragDropMessage);
				var messageString = Encoding.ASCII.GetString(messageBytes);
				messageBytes = Encoding.ASCII.GetBytes(messageString.Replace("</fileData>", "Data>"));

				notificationService.Verify(x => x.ShowError(It.IsAny<IWin32Window>(), It.IsAny<string>()), Times.Never);
				return messageBytes;
			}
		}

		public void TestGetMessageType()
		{
			var plugin = new CitrixPluginExposeForTest();
			var data = Encoding.ASCII.GetBytes("test");
			var result = plugin.GetMessageTypeExposed(data);
			Assert(string.IsNullOrEmpty(result));

			var dataWithFlag = new byte[data.Length + 1];
			dataWithFlag[0] = (byte)ChannelFlags.First;
			Buffer.BlockCopy(data, 0, dataWithFlag, 1, data.Length);
			result = plugin.GetMessageTypeExposed(dataWithFlag);
			Assert(result == "test");
		}

		class CitrixPluginExposeForTest : CitrixLegacyPlugin
		{
			public string GetMessageTypeExposed(byte[] data)
			{
				return GetMessageType(data);
			}
		}

		readonly IntPtr mockWritePointer = Marshal.GetFunctionPointerForDelegate(mockWrite);
		static readonly MockWrite mockWrite = WriteForTest;
		public delegate int MockWrite(uint length, byte[] pBuf);

		static int WriteForTest(uint length, byte[] pBuf)
		{
			return 0;
		}

		class TestMessageHandler : IMessageHandler
		{
			public void Handle(IEnterpriseChannel channel, Stream messageData)
			{
				var data = new byte[messageData.Length];
				messageData.Read(data, 0, (int)messageData.Length);
				IsDataCorrupted = data.OfType<byte>().Any(singledata => singledata != 0);
				TestHandled.Set();
			}
			public bool IsDataCorrupted { get; private set; } = true;
			public AutoResetEvent TestHandled = new AutoResetEvent(false);

			public void Reset()
			{
				IsDataCorrupted = true;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest(isCitrixICA: true));
			Server.EnterpriseChannel.Initialize(null, useLegacyCitrix: true);
		}

		protected override void TearDown()
		{
			try
			{
				Server.EnterpriseChannel.Instance.Close();
				Server.EnterpriseChannel.Reset();
				CitrixLegacyPlugin.CloseAllChannels_ForTest();
				WtsPlugin.UnregisterUnhandledExceptionHandlers();
			}
			finally
			{
				base.TearDown();
				ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
			}
		}
	}
}
