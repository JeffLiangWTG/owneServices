using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.Customs.FR.Messaging.Interfaces.DCG;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DCG
{
	public class DCGStatementWrapper : IDCG
	{
		public DCGStatementWrapper(StatementMessageSendingObject objectToSend)
		{
			this.statement = Argument.NotNull(objectToSend?.Statement, nameof(statement));
		}

		public IMessageEnvelope MessageEnvelope => messageEnvelope ?? (messageEnvelope = new StatementMessageEnvelopeWrapper(statement));
		IMessageEnvelope messageEnvelope;

		public ZString MessageType => MessageTypeList.Codes.DCG;

		public ZString DeltaAgreementNumber => statement.B2_EntryFilerCode;

		public ZString DefermentAccountNumber => statement.B2_CheckNo;

		public ZString OperationalRepresentative => statement.B2_ImporterCustomsID;

		public ZString PaymentType => statement.B2_PaymentType;

		public ZDate PeriodStartDate => statement.B2_PeriodStartDate;

		public ZString Frequency => statement.B2_StatementType;

		public ZString Direction => statement.B2_BranchDesignation;
		
		readonly CusStatementHeader statement;
	}
}
