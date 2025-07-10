using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(ICS2InboundEDIMessage))]
	sealed class ICS2InboundEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			AssertEquals("EM_ReceiveTransmit", Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("EM_ApplicationCode", ApplicationCodes.IC2, message.EM_ApplicationCode);
		}

		public void TestRegisteredLinkedObjectTypes() => AssertEquals(typeof(AsycudaManifestHeader), Factory.New<ICS2InboundEDIMessageForTest>().RegisteredLinkedObjectTypesExposed[AsycudaManifestHeader.Schema.TableName].Invoke());

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<ICS2InboundEDIMessage>();
		}

		ICS2InboundEDIMessage message;

		sealed class ICS2InboundEDIMessageForTest : ICS2InboundEDIMessage
		{
			public ICS2InboundEDIMessageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public Dictionary<string, Func<Type>> RegisteredLinkedObjectTypesExposed => RegisteredLinkedObjectTypes;
		}
	}
}
