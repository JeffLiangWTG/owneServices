using CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Send.Export;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC
{
	public class DCRPSSendExpMessageBuilder : DCSendExpMessageBaseBuilder
	{
		public DCRPSSendExpMessageBuilder(IDeclarationImportExport declarationExport
			, ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declarationExport, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}

		protected override TPreval PopulatePreval(ICusProcedure cusProcedure) => null;
	}
}
