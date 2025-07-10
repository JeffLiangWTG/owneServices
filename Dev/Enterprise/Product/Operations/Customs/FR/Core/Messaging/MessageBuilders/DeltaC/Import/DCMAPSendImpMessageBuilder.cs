using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC
{
	public class DCMAPSendImpMessageBuilder : DCSendImpMessageBaseBuilder
	{
		public DCMAPSendImpMessageBuilder(IDeclarationImportExport declarationImport
			, ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declarationImport, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}
	}
}
