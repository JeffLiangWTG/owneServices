using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public class SadInterchangeProvider : ITInterchangeProvider
{
	public SadInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
	{
	}

	protected sealed override IInterchangeFileNameStrategy GetInterchangeFileNameStrategy(EDIMessage message, CustomsInterchangeAndAccountInfo customsInterchangeAndAccountInfo)
	{
		return new InterchangeFileNameStrategy(customsInterchangeAndAccountInfo.Account, message.EM_MessageType, message.Factory);
	}

	protected sealed override IInterchangeHeaderTextStrategy GetInterchangeHeaderTextStrategy(IOutgoingInterchangeHeaderTextFieldsProvider fieldsProvider)
	{
		return new InterchangeHeaderTextStrategy(fieldsProvider);
	}

	#region InterchangeHeaderTextStrategy

	class InterchangeHeaderTextStrategy : IInterchangeHeaderTextStrategy
	{
		public InterchangeHeaderTextStrategy(IOutgoingInterchangeHeaderTextFieldsProvider fieldsProvider)
		{
			this.fieldsProvider = Argument.NotNull(fieldsProvider, nameof(fieldsProvider));
		}

		readonly IOutgoingInterchangeHeaderTextFieldsProvider fieldsProvider;

		public ZString GetText()
		{
			return new InterchangeHeaderTextBuilder()
				.AppendStaff(fieldsProvider.Staff)
				.AppendNode(fieldsProvider.Node)
				.AppendMessageType(fieldsProvider.MessageType)
				.AppendAccountNumber(fieldsProvider.AccountNumber)
				.AppendHeader(fieldsProvider.CustomsInterchangeHeader)
				.Build();
		}
	}

	#endregion
}
