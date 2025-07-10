using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public abstract class SadOutgoingCustomsMessageCreationStrategy : IOutgoingCustomsMessageCreationStrategy
{
	readonly BusinessObjectFactory factory;
	readonly ISadOutgoingCustomsMessageGeneratorValuesProvider valuesProvider;
	readonly ICustomsMessageFountainProvider fountainProvider;
	readonly IEnumerable<ISadCustomsMessage> customsMessages;
	readonly BusinessObject parentBizObj;

	protected SadOutgoingCustomsMessageCreationStrategy(BusinessObjectFactory factory, ISadOutgoingCustomsMessageGeneratorValuesProvider valuesProvider)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.valuesProvider = Argument.NotNull(valuesProvider, nameof(valuesProvider));

		fountainProvider = Argument.NotNull(valuesProvider.FountainProvider, nameof(valuesProvider.FountainProvider));
		customsMessages = Argument.NotNull(valuesProvider.GetCustomsMessageObjects(), nameof(valuesProvider.GetCustomsMessageObjects));
		parentBizObj = Argument.NotNull(valuesProvider.Parent, nameof(valuesProvider.Parent));
	}

	public static IOutgoingCustomsMessageCreationStrategy GetStrategy(ZString messageSendingMode, BusinessObjectFactory factory, ISadOutgoingCustomsMessageGeneratorValuesProvider valuesProvider)
	{
		switch (messageSendingMode)
		{
			case CustomsMessageSendingModeList.Codes.AutomaticProcedure:
				return new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(factory, valuesProvider);

			case CustomsMessageSendingModeList.Codes.FallbackProcedure:
				return new FallbackProcedureSadOutgoingCustomsMessageCreationStrategy(factory, valuesProvider);

			case CustomsMessageSendingModeList.Codes.ManualProcedure:
				return new ManualProcedureSadOutgoingCustomsMessageCreationStrategy(factory, valuesProvider);

			default:
				throw new InvalidOperationException(Res.GetString("831D5326-24CC-439D-A3C3-DFFECC4AEC63", "'{0}' is not a valid sending mode", messageSendingMode));
		}
	}

	ITEDIMessage IOutgoingCustomsMessageCreationStrategy.GenerateMessage()
	{
		TryCloneNumberRangesIfNeeded(fountainProvider);

		var message = factory.New<ITEDIMessage>();
		message.EM_MessageText = customsMessages.SerializeWithTabSeparator();
		message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		message.EM_MessageSubType = valuesProvider.GetSubType();
		message.EM_ApplicationReference = valuesProvider.GetApplicationReference();
		message.MessageNumberStrategy = new ITMessageNumberStrategy(fountainProvider);
		message.EM_LinkedObject = parentBizObj;
		GenerateMessageCore(factory, message);
		return message;
	}

	void TryCloneNumberRangesIfNeeded(ICustomsMessageFountainProvider fountainProvider)
	{
		if (fountainProvider.Wrapper == null && fountainProvider.HasClonableNumberRanges)
		{
			fountainProvider.TryCloneLastYearNumberRanges();
		}
	}

	protected abstract void GenerateMessageCore(BusinessObjectFactory factory, ITEDIMessage message);
}

public class AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy : SadOutgoingCustomsMessageCreationStrategy
{
	public AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(BusinessObjectFactory factory, ISadOutgoingCustomsMessageGeneratorValuesProvider valuesProvider) : base(factory, valuesProvider)
	{
	}

	protected override void GenerateMessageCore(BusinessObjectFactory factory, ITEDIMessage message)
	{
		message.EM_Status = EDIMessageStatusList.Codes.Queued;
		message.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
	}
}

public class FallbackProcedureSadOutgoingCustomsMessageCreationStrategy : SadOutgoingCustomsMessageCreationStrategy
{
	protected override void GenerateMessageCore(BusinessObjectFactory factory, ITEDIMessage message)
	{
		message.EM_Status = EDIMessageStatusList.Codes.Manual;
		message.EM_MessageType = FallbackProcedure;
	}

	const string FallbackProcedure = "FBK";

	public FallbackProcedureSadOutgoingCustomsMessageCreationStrategy(BusinessObjectFactory factory, ISadOutgoingCustomsMessageGeneratorValuesProvider valuesProvider) : base(factory, valuesProvider)
	{
	}
}

public class ManualProcedureSadOutgoingCustomsMessageCreationStrategy : SadOutgoingCustomsMessageCreationStrategy
{
	public ManualProcedureSadOutgoingCustomsMessageCreationStrategy(BusinessObjectFactory factory, ISadOutgoingCustomsMessageGeneratorValuesProvider valuesProvider) : base(factory, valuesProvider)
	{
	}

	protected override void GenerateMessageCore(BusinessObjectFactory factory, ITEDIMessage message)
	{
		message.EM_Status = EDIMessageStatusList.Codes.Manual;
		message.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		PackMessageIntoInterchange(factory, message);
	}

	void PackMessageIntoInterchange(BusinessObjectFactory factory, EDIMessage message)
	{
		var messageCollection = new NonDependentEDIMessageCollection(factory);
		messageCollection.Add(message);
		new SadManualProcedureInterchangeProvider(messageCollection).PackCollatedMessagesIntoInterchanges();
	}
}
