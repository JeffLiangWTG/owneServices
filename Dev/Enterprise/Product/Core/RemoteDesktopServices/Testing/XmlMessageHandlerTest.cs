using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	public class XmlMessageHandlerTest : TestCase
	{
		public void TestHandleMessageWithSpecialCharacters()
		{
			byte[] xmlBytes = Encoding.UTF8.GetBytes("<?xml version=\"1.0\"?><hello>\x0000world\t&#x1F;<\x0000/hello>");
			IMessageHandler messageHandler = new MockXmlMessageHandler();
			using (Stream xmlStream = new MemoryStream(xmlBytes))
			{
				var channel = new myChannel();
				AssertNoExceptionThrown(() => messageHandler.Handle(channel, xmlStream));
			}
		}
		[XmlRoot("hello")]
		public class MockXmlElement
		{
			[XmlText]
			public string World { get; set; }
		}
		public class MockXmlMessageHandler : XmlMessageHandler<MockXmlElement>
		{
			protected override void Handle(IEnterpriseChannel channel, MockXmlElement message)
			{
			}
		}
		public class myChannel : IEnterpriseChannel
		{
			public bool Send(byte[] data)
			{
				throw new NotImplementedException();
			}

			public bool IsConnected
			{
				get { throw new NotImplementedException(); }
			}

			public Version RDPVersion
			{
				get { throw new NotImplementedException(); }

				set { throw new NotImplementedException(); }
			}
		}
	}
}