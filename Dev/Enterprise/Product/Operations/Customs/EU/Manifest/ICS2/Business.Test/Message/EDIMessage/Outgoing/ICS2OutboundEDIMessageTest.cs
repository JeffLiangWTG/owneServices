using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ICS2OutboundEDIMessage))]
	sealed class ICS2OutboundEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<ICS2OutboundEDIMessage>();
			AssertEquals("EM_ApplicationCode", ApplicationCodes.IC2, message.EM_ApplicationCode);
		}

		public void TestGetMessageReferenceNumber()
		{
			var message1 = Factory.New<ICS2OutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(001) from IEMessageControlNumber sequense", "ICS200000000000001", message1.EM_MessageNum);

			var message2 = Factory.New<ICS2OutboundEDIMessage>();
			Factory.Save();
			AssertEquals("Should have created MessageNumber(002) from IEMessageControlNumber sequense", "ICS200000000000002", message2.EM_MessageNum);
		}

		public void TestMessageInterpretation()
		{
			var message = Factory.New<ICS2OutboundEDIMessage>();
			message.EM_MessageText = @"
<IE3N99>
<key>001</key>
</IE3N99>";
			Factory.Save();

			var expectedInterpretation = "<div class=\"expander-open\">&lt;<span class=\"start-tag\">IE3N99</span>&gt;<div class=\"expander-content\"><div>&lt;<span class=\"start-tag\">key</span>&gt;<span class=\"text\">001</span>&lt;/<span class=\"end-tag\">key</span>&gt;</div></div>&lt;/<span class=\"end-tag\">IE3N99</span>&gt;</div>";
			AssertContains(expectedInterpretation, message.EM_MessageInterpretation);
		}

		public void TestRegisteredLinkedObjectTypes() => AssertEquals(typeof(AsycudaManifestHeader), Factory.New<ICS2OutboundEDIMessageForTest>().RegisteredLinkedObjectTypesExposed[AsycudaManifestHeader.Schema.TableName].Invoke());

		sealed class ICS2OutboundEDIMessageForTest : ICS2OutboundEDIMessage
		{
			public ICS2OutboundEDIMessageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public Dictionary<string, Func<Type>> RegisteredLinkedObjectTypesExposed => RegisteredLinkedObjectTypes;
		}
	}
}
