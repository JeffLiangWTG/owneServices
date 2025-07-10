using System;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(MessageBuilderFactory))]
sealed class MessageBuilderFactoryTest : TestCaseWithFactory
{
	public void TestNewMessageBuilder_Null() => AssertNull(MessageBuilderFactory.NewMessageBuilder(null));

	public void TestNewMessageBuilder_NT013MessageBuilder_v4() => AssertDepartureMessageBuilderType<NT013_v4MessageBuilder>(PassarMessageTypeList.Codes.NT013, FunctionalityTypes.CHNT015V4, true);
	public void TestNewMessageBuilder_NT013MessageBuilder_v5() => AssertDepartureMessageBuilderType<NT013_v5MessageBuilder>(PassarMessageTypeList.Codes.NT013, FunctionalityTypes.CHNT015V4, false);

	public void TestNewMessageBuilder_NT015MessageBuilder_v4() => AssertDepartureMessageBuilderType<NT015_v4MessageBuilder>(PassarMessageTypeList.Codes.NT015, FunctionalityTypes.CHNT015V4, true);
	public void TestNewMessageBuilder_NT015MessageBuilder_v5() => AssertDepartureMessageBuilderType<NT015_v5MessageBuilder>(PassarMessageTypeList.Codes.NT015, FunctionalityTypes.CHNT015V4, false);

	public void TestNewMessageBuilder_NT014MessageBuilder() => AssertType<NT014_v3MessageBuilder>(MessageBuilderFactory.NewMessageBuilder(SetUpMessageSendingObject<NctsHeaderDepartureMessageSendingObject>(NctsMovementType.Codes.Departure, PassarMessageTypeList.Codes.NT014)));

	public void TestNewMessageBuilder_NT007MessageBuilder() => AssertType<NT007_v3MessageBuilder>(MessageBuilderFactory.NewMessageBuilder(SetUpMessageSendingObject<NctsHeaderArrivalMessageSendingObject>(NctsMovementType.Codes.Arrival, PassarMessageTypeList.Codes.NT007)));

	public void TestNewMessageBuilder_NT044MessageBuilder_v4() => AssertArrivalMessageBuilderType<NT044_v4MessageBuilder>(PassarMessageTypeList.Codes.NT044, FunctionalityTypes.CHNT044V4, true);
	public void TestNewMessageBuilder_NT044MessageBuilder_v5() => AssertArrivalMessageBuilderType<NT044_v5MessageBuilder>(PassarMessageTypeList.Codes.NT044, FunctionalityTypes.CHNT044V4, false);

	public void TestNewMessageBuilder_NT141MessageBuilder() => AssertType<NT141_v2MessageBuilder>(MessageBuilderFactory.NewMessageBuilder(SetUpMessageSendingObject<NctsHeaderDepartureMessageSendingObject>(NctsMovementType.Codes.Departure, PassarMessageTypeList.Codes.NT141)));

	public void TestNewMessageBuilder_NT513MessageBuilder_v4() => AssertDepartureMessageBuilderType<NT513_v4MessageBuilder>(PassarMessageTypeList.Codes.NT513, FunctionalityTypes.CHNT515V4, true);
	public void TestNewMessageBuilder_NT513MessageBuilder_v5() => AssertDepartureMessageBuilderType<NT513_v5MessageBuilder>(PassarMessageTypeList.Codes.NT513, FunctionalityTypes.CHNT515V4, false);

	public void TestNewMessageBuilder_NT515MessageBuilder_v4() => AssertDepartureMessageBuilderType<NT515_v4MessageBuilder>(PassarMessageTypeList.Codes.NT515, FunctionalityTypes.CHNT515V4, true);
	public void TestNewMessageBuilder_NT515MessageBuilder_v5() => AssertDepartureMessageBuilderType<NT515_v5MessageBuilder>(PassarMessageTypeList.Codes.NT515, FunctionalityTypes.CHNT515V4, false);

	public void TestNewMessageBuilder_NC016MessageBuilder() => AssertType<NC016_v1MessageBuilder>(MessageBuilderFactory.NewMessageBuilder(SetUpMessageSendingObject<NctsHeaderDepartureMessageSendingObject>(NctsMovementType.Codes.Departure, PassarMessageTypeList.Codes.NC016)));

	void AssertDepartureMessageBuilderType<TMessageBuilder>(string passarMessageType, string funcsCode, bool funcsCodeActive)
	{
		AssertMessageBuilderType<TMessageBuilder, NctsHeaderDepartureMessageSendingObject>(NctsMovementType.Codes.Departure, passarMessageType, funcsCode, funcsCodeActive);
	}

	void AssertArrivalMessageBuilderType<TMessageBuilder>(string passarMessageType, string funcsCode, bool funcsCodeActive)
	{
		AssertMessageBuilderType<TMessageBuilder, NctsHeaderArrivalMessageSendingObject>(NctsMovementType.Codes.Arrival, passarMessageType, funcsCode, funcsCodeActive);
	}

	void AssertMessageBuilderType<TMessageBuilder, TMessageSendingObject>(string movementHeaderType, string passarMessageType, string funcsCode, bool funcsCodeActive)
		where TMessageSendingObject : NctsHeaderCommonMessageSendingObject
	{
		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(funcsCode, Core.Constants.CountryCodes.Switzerland, funcsCodeActive ? ZDate.Today : ZDate.Today.AddDays(1), true))
			{
				AssertType<TMessageBuilder>($"{passarMessageType} FUNCS {funcsCode} active={funcsCodeActive}", MessageBuilderFactory.NewMessageBuilder(SetUpMessageSendingObject<TMessageSendingObject>(movementHeaderType, passarMessageType)));
			}
			if (!funcsCodeActive)
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(funcsCode, Core.Constants.CountryCodes.Switzerland, ZDate.Empty, false))
				{
					AssertType<TMessageBuilder>($"{passarMessageType} FUNCS {funcsCode} not set", MessageBuilderFactory.NewMessageBuilder(SetUpMessageSendingObject<TMessageSendingObject>(movementHeaderType, passarMessageType)));
				}
			}
		});
	}

	IMessageSendingObject SetUpMessageSendingObject<TMessageSendingObject>(string movementType, string messageType)
		where TMessageSendingObject : NctsHeaderCommonMessageSendingObject
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(movementType);
		var sendingObject = (TMessageSendingObject)Activator.CreateInstance(typeof(TMessageSendingObject), nctsHeader);
		sendingObject.MessageType = messageType;
		return sendingObject;
	}
}
