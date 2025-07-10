using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC
{
	public class DCANTSendExpMessageBuilder : DCSendExpMessageBaseBuilder
	{
		public DCANTSendExpMessageBuilder(IDeclarationImportExport declarationExport
			, ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declarationExport, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}
	}
}
