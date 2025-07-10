using System;
using System.Data;
using System.IO;
using System.Text;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(OutboundMessageInterpreter))]
	sealed class OutboundMessageInterpreterTest : TestCaseWithFactory
	{
		const string content = "Test Content";

		public void TestConstructor() => CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("Message missing", () => new OutboundMessageInterpreter(null, Provider, Builder));
			AssertExceptionThrown<ArgumentException>("Provider missing", () => new OutboundMessageInterpreter(Message, null, Builder));
			AssertExceptionThrown<ArgumentException>("Builder missing", () => new OutboundMessageInterpreter(Message, Provider, null));
		});

		public void TestGetInterpretation() => CombineAssertions(() =>
		{
			AssertEquals("Message interpretation", $"<html><body><xmp>{content}</xmp></body></html>", new OutboundMessageInterpreter(Message, Provider, Builder).GetInterpretation());
		});

		OutboundMessageForTest Message => message ?? (message = Factory.New<OutboundMessageForTest>());
		OutboundMessageForTest message;

		IE013MessageProvider Provider => provider ?? (provider = GetProvider());
		IE013MessageProvider provider;

		IXmlMessageBuilder Builder => builder ?? (builder = new MessageBuilderForTest());
		IXmlMessageBuilder builder;

		IE013MessageProvider GetProvider()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return new IE013MessageProvider(header);
		}

		sealed class MessageBuilderForTest : IXmlMessageBuilder, IXmlSerializationStrategy
		{
			public IXmlMessage GenerateXmlMessage() => new XmlMessage<string>(content, this);

			public Stream GetSerializedStream<T>(T @object) => new MemoryStream(Encoding.UTF8.GetBytes(content));

			public string GetSerializedString<T>(T @object) => content;
		}

		sealed class OutboundMessageForTest : OutboundEDIMessage
		{
			public OutboundMessageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}
	}
}
