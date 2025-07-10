using System;
using Enterprise.Customs.BE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Customs.BE.Business.Testing.PNTSMessageHeaderProviderBaseOnlyTest;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(PNTSMessageHeaderProvider))]
sealed class PNTSMessageHeaderProviderBaseOnlyTest : PNTSMessageHeaderProviderAbstractTest<PNTSMessageHeaderProviderForTest>
{
	public void TestMessageSender()
	{
		var messageVersionRegistryCollection = new MessageVersionRegistryCollection
		{
			new MessageVersionRegistry { DomainCode = SendMessageTypes.Codes.PN, TargetSystemName = "PNTS.NIHAO" }
		};
		using (BECustomsRegistry.Instance.SenderIDs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
		{
			AssertEquals("PNTS.NIHAO", Provider.MessageSender);
		}
	}

	public void TestMessageRecipient()
	{
		var messageVersionRegistryCollection = new MessageVersionRegistryCollection
		{
			new MessageVersionRegistry { DomainCode = SendMessageTypes.Codes.PN, TargetSystemName = "PNTS@1234" }
		};
		using (BECustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
		{
			AssertEquals("PNTS@1234", Provider.MessageRecipient);
		}
	}

	public void TestMessageType()
	{
		AssertNull(Provider.MessageType);
	}

	[TestDate(2064, 10, 18, 12, 0, 0)]
	public void TestPreparationDateTime()
	{
		CombineAssertions(() =>
		{
			AssertEquals(new DateTime(2064, 10, 18, 12, 0, 0), Provider.PreparationDateTime);
			AssertEquals("milliseconds", 0, Provider.PreparationDateTime.Millisecond);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, provider.PreparationDateTime.Kind);
		});
	}

	public void TestMessageIdentification()
	{
		AssertEquals(EDIMessage.SendersReferencePlaceHolder, Provider.MessageIdentification);
	}

	public void TestCorrelationIdentifier()
	{
		AssertEquals(string.Empty, Provider.CorrelationIdentifier);
	}

	protected override PNTSMessageHeaderProviderForTest GetProvider() => provider;

	internal class PNTSMessageHeaderProviderForTest : PNTSMessageHeaderProvider
	{
		public PNTSMessageHeaderProviderForTest(TemporaryStorageMessageSendingObject messageSendingAction) : base(messageSendingAction)
		{
		}
	}
}
