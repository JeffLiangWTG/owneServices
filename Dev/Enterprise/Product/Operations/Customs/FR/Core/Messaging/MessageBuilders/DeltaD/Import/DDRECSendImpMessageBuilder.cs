using CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Send.Import;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD
{
	public class DDRECSendImpMessageBuilder : DDSendImpMessageBaseBuilder
	{
		public DDRECSendImpMessageBuilder(IDeclarationImportExport declarationImport
			, ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declarationImport, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}

		protected override TPreval PopulatePreval(ICusProcedure cusProcedure) => null;
	}
}
