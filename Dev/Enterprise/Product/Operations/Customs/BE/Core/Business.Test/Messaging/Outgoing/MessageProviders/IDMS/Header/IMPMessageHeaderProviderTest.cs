using System;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.BE.Business.Testing.IMPMessageHeaderProviderTest;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(IMPMessageHeaderProvider))]
sealed class IMPMessageHeaderProviderTest : MessageHeaderProviderAbstractTest<IMPMessageHeaderProviderForTest>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("BEJobDeclarationMessageSendingObject mandatory", () => new IMPMessageHeaderProviderForTest(null));
	}

	protected override string MessageType => Constants.BECMessageTypes.Outgoing.IE415B;

	public void TestMessageSender()
	{
		var messageVersionRegistryCollection = new MessageVersionRegistryCollection
		{
			new MessageVersionRegistry { DomainCode = SendMessageTypes.Codes.IMP, TargetSystemName = "IMP.OVERRIDE" }
		};
		using (BECustomsRegistry.Instance.SenderIDs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
		{
			AssertEquals("IMP.OVERRIDE", Provider.MessageSender);
		}
	}

	public void TestMessageRecipient()
	{
		var messageVersionRegistryCollection = new MessageVersionRegistryCollection
		{
			new MessageVersionRegistry { DomainCode = SendMessageTypes.Codes.IMP, TargetSystemName = "IMP.OVERRIDE" }
		};
		using (BECustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
		{
			AssertEquals("IMP.OVERRIDE", Provider.MessageRecipient);
		}
	}

	public void TestPreparationDateTime()
	{
		CombineAssertions(() =>
		{
			AssertEquals("milliseconds", 0, Provider.PreparationDateTime.Millisecond);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, provider.PreparationDateTime.Kind);
		});
	}

	public void TestMessageIdentification()
	{
		AssertEquals(AESMessage.MessageNumberPlaceHolder, Provider.MessageIdentification);
	}

	public void TestCorrelationIdentifier()
	{
		var cidEntryNumber = CusEntryNumber.LoadOrCreate(cusEntryHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, cusEntryHeader.CountryCode);
		cidEntryNumber.CE_EntryLineReference = "CorrelationID";
		AssertEquals("CorrelationID", Provider.CorrelationIdentifier);
	}

	protected override IMPMessageHeaderProviderForTest GetProvider() => provider;

	internal class IMPMessageHeaderProviderForTest : IMPMessageHeaderProvider
	{
		public IMPMessageHeaderProviderForTest(BEJobDeclarationMessageSendingObject messageSendingAction) : base(messageSendingAction)
		{
		}

		public override string MessageType => "IE415B";
	}
}
