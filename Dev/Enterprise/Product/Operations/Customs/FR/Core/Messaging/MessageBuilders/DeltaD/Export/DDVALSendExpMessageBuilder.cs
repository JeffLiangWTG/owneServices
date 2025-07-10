using CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Send.Export;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD
{
	public class DDVALSendExpMessageBuilder : DDSendExpMessageBaseBuilder
	{
		public DDVALSendExpMessageBuilder(IDeclarationImportExport declaration
			, ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declaration, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}

		protected override string PopulateRefdecItem(IHeader motivationHeader) => null;

		protected override TPreval PopulatePreval(ICusProcedure cusProcedure) => null;

		protected override TMotivation PopulateMotivation(IHeader motivationHeader) => null;
	}
}
