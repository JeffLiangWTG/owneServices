using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Send.Import;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC
{
	public class DCINVSendImpMessageBuilder : DCSendImpMessageBaseBuilder
	{
		public DCINVSendImpMessageBuilder(IDeclarationImportExport declarationImport
			, ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declarationImport, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}

		protected override TGenImport PopulateProcedure() => null;

		protected override Collection<TArticleImport> PopulateArticlesImport() => null;

		protected override TMotivation PopulateMotivation() => LoadMotivation();

		protected override TPreval PopulatePreval(ICusProcedure cusProcedure) => null;

		protected override ZString PopulateDeclEmergencyProcDate(ICusProcedure cusProcedure) => null;
	}
}
