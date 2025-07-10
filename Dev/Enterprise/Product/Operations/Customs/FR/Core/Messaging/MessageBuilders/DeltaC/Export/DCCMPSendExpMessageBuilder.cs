using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Send.Export;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC
{
	public class DCCMPSendExpMessageBuilder : DCSendExpMessageBaseBuilder
	{
		public DCCMPSendExpMessageBuilder(IDeclarationImportExport declarationExport
			, ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declarationExport, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}

		protected override TGenExport PopulateProcedure() => null;

		protected override Collection<TArticleExport> PopulateArticlesExport(TGenExport gen) => new Collection<TArticleExport>();

		protected override TPreval PopulatePreval(ICusProcedure cusProcedure) => null;

		protected override ZString PopulateDeclEmergencyProcDate(ICusProcedure cusProcedure) => null;
	}
}
