using CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Send.Import;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC
{
	public class DCRECSendImpMessageBuilder : DCSendImpMessageBaseBuilder
	{
		public DCRECSendImpMessageBuilder(IDeclarationImportExport declarationImport
			, ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declarationImport, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}

		protected override TMotivation PopulateMotivation() => LoadMotivation();

		protected override TPreval PopulatePreval(ICusProcedure cusProcedure) => null;
	}
}
