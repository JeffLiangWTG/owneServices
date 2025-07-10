using CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Send.Import;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD
{
	public class DDMDASendImpMessageBuilder : DDSendImpMessageBaseBuilder
	{
		public DDMDASendImpMessageBuilder(IDeclarationImportExport declarationImport
			, ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declarationImport, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}

		protected override TMotivation PopulateMotivation(IHeader motivationHeader) => null;
	}
}
