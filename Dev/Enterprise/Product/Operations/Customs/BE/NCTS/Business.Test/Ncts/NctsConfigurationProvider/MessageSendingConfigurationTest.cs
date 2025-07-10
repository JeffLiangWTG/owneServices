using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class MessageSendingConfigurationTest : MessageSendingConfigurationAbstractTest<MessageSendingConfiguration>
{
	public void TestShouldFillAdditionalWarningsOnSendScreen()
	{
		AssertEquals(expected: false, configuration.ShouldFillAdditionalWarningsOnSendScreen);
	}

	public void TestReleaseRequestCode() => AssertEquals(NCTS5DeparturePhaseList.Codes.ReleaseRequest, configuration.ReleaseRequestCode);

	public void TestShouldHideSendWithAdditionalWarningCheckBox()
	{
		AssertEquals(expected: false, configuration.ShouldHideSendWithAdditionalWarningCheckBox);
	}

	public override void TestMessageTypeList()
	{
		Assert("There is no changes to be tested here", true);
	}

	public override void TestSetDefaultMessageType()
	{
		Assert("There is no changes to be tested here", true);
	}

	public override void TestGetNewNctsMessageSendingObjectParent()
	{
		AssertType<NctsHeaderMessageSendingObjectParent>(configuration.GetNewNctsHeaderMessageSendingObjectParent(header));
	}

	public override void TestGetShouldSendDefault()
	{
		Assert("There is no changes to be tested here", true);
	}

	public override void TestShowJustification()
	{
		var sendingObj = new NctsHeaderMessageSendingObject(header);
		AssertEquals(expected: true, configuration.ShowJustification(header));
	}

	protected override Type ExpectedNctsHeaderMessageSendingObjectValidationDeciderType => typeof(NctsHeaderMessageSendingObjectValidationDecider);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
	}
	NctsHeader header;
}
