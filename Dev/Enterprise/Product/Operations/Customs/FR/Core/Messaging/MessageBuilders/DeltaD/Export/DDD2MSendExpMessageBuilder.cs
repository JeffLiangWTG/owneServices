using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Send.Export;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD
{
	public class DDD2MSendExpMessageBuilder : DDSendExpMessageBaseBuilder
	{
		public DDD2MSendExpMessageBuilder(IDeclarationImportExport declaration
			, ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declaration, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}

		protected override bool ShouldPopulateMainDatasInfos => false;

		protected override TPreval PopulatePreval(ICusProcedure cusProcedure) => null;

		protected override ZString PopulateDeclEmergencyProcDate(ICusProcedure cusProcedure) => null;

		protected override TOperateurDseComp LoadGenCompOperateurComp(ICusProcedure cusProcedure) => null;

		protected override ZString LoadContainerMode(ICusProcedure cusProcedure) => null;

		protected override TMotivation PopulateMotivation(IHeader motivationHeader) => null;

		protected override IEnumerable<ITariffAdditionalCode> GetCETariffAdditionalCodes(IArticle article) => article.SecondMessageCETariffAdditionalCodes;

		protected override IEnumerable<ITariffAdditionalCode> GetFRTariffAdditionalCodes(IArticle article) => article.SecondMessageFRTariffAdditionalCodes;

		protected override IEnumerable<ISupportingDocumentOnly> GetSupportingDocumentsItem(IArticle article) => article.SecondMessageSupportingDocuments;

		protected override IEnumerable<ITariffAdditionalCode> GetPartDispos(IArticle article) => article.SecondMessagePartDispos;
	}
}
