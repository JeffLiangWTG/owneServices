using System;
using Moq;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop.Tests
{
	public class DummyBrowserInteropWindowFactory : IBrowserInteropWindowFactory
	{
		readonly Mock<IBrowserAndWebViewInteropWindow> browserInteropWindowMock = new Mock<IBrowserAndWebViewInteropWindow>();
		public Mock<IMessageTransportLayer> MessageTransportLayerMock { get; }
		public IMessageTransportLayer MessageTransportLayer => MessageTransportLayerMock.Object;
		public Mock<ISendMessageToBrowser> SendMessageToBrowserMock { get; }
		public int windowsCreated;

		public DummyBrowserInteropWindowFactory()
		{
			SendMessageToBrowserMock = new Mock<ISendMessageToBrowser>();
			var messageTransportLayerProxy = new Mock<MessageTransportLayer>(() => new MessageTransportLayer(SendMessageToBrowserMock.Object)) { CallBase = true };
			MessageTransportLayerMock = messageTransportLayerProxy.As<IMessageTransportLayer>();
			browserInteropWindowMock.SetupGet(t => t.MessageTransportLayer).Returns(MessageTransportLayerMock.Object);
		}

		IBrowserInteropWindow IBrowserInteropWindowFactory.CreateBrowserInteropWindow(string windowName, Uri address)
		{
			windowsCreated += 1;
			return browserInteropWindowMock.Object;
		}
	}

	public interface IBrowserAndWebViewInteropWindow : IBrowserInteropWindow { }
}
