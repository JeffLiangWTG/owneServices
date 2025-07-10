using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public class SingleWindowInterchangeProvider : ITInterchangeProvider
{
	public SingleWindowInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
	{
	}

	protected override IInterchangeHeaderTextStrategy GetInterchangeHeaderTextStrategy(IOutgoingInterchangeHeaderTextFieldsProvider fieldsProvider)
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
				.AppendMessageType(fieldsProvider.MessageType)
				.AppendAccountNumber(fieldsProvider.AccountNumber)
				.Build();
		}
	}

	#endregion
}
