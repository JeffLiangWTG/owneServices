using System;
using Enterprise.RemoteDesktopServices.Client;
using Enterprise.RemoteDesktopServices.Testing;
using Win32.WtsApi32;

namespace Enterprise.RemoteDesktopServices.TestClientHost
{
	class MockListener : IWTSListener
	{
		public void GetConfiguration(out IntPtr ppPropertyBag)
		{
			throw new NotImplementedException();
		}
	}

	class MockChannel : IWTSVirtualChannel
	{
		internal IWTSVirtualChannelCallback Callback { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Testing")]
		public void Write(uint cbSize, byte[] data, object pReserved)
		{
			Console.Out.WriteLine(Convert.ToBase64String(data, 0, (int)cbSize));
			Console.Out.Flush();
		}

		public void Close()
		{
		}
	}

	class MockChannelManager : IWTSVirtualChannelManager
	{
		internal static MockChannelManager Instance
		{
			get { return instance ?? (instance = new MockChannelManager()); }
		}
		static MockChannelManager instance;

		static IWTSListenerCallback callback;
		static MockChannel channel;
		static WtsPlugin plugin;

		void IWTSVirtualChannelManager.CreateListener(string pszChannelName, int ulFlags, IWTSListenerCallback pListenerCallback, out IWTSListener ppListener)
		{
			callback = pListenerCallback;
			ppListener = new MockListener();
		}

		public static void InitializeChannel()
		{
			plugin = new WtsPluginForTest();
			((IWTSPlugin)plugin).Initialize(Instance);
		}

		public static void ConnectChannel()
		{
			channel = new MockChannel();
			bool accept = false;
			IWTSVirtualChannelCallback channelCallback;
			MockChannelManager.callback.OnNewChannelConnection(channel, null, out accept, out channelCallback);
			EnterpriseChannel enterpriseChannel = (EnterpriseChannel)channelCallback;
			channel.Callback = channelCallback;
		}

		public static void ReceiveMessage(byte[] data)
		{
			channel.Callback.OnDataReceived((uint)data.Length, data);
		}

		public static void Send(byte[] data)
		{
			channel.Write((uint)data.Length, data, null);
		}
	}
}
