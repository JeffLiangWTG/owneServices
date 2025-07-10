using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Send.Import;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD
{
	public class DDD2MSendImpMessageBuilder : DDSendImpMessageBaseBuilder
	{
		public DDD2MSendImpMessageBuilder(IDeclarationImportExport declaration
			, ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declaration, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}

		protected override bool ShouldPopulateMainDatasInfos => false;

		protected override TPreval PopulatePreval(ICusProcedure cusProcedure) => null;

		protected override ZString PopulateDeclEmergencyProcDate(ICusProcedure cusProcedure) => null;

		protected override TOperateurDsiComp LoadGenCompOperateurComp(ICusProcedure cusProcedure) => null;

		protected override ZString LoadContainerMode(ICusProcedure cusProcedure) => null;

		protected override TMotivation PopulateMotivation(IHeader motivationHeader) => null;

		protected override IEnumerable<ITariffAdditionalCode> GetCETariffAdditionalCodes(IArticle article) => article.SecondMessageCETariffAdditionalCodes;

		protected override IEnumerable<ITariffAdditionalCode> GetFRTariffAdditionalCodes(IArticle article) => article.SecondMessageFRTariffAdditionalCodes;

		protected override IEnumerable<ISupportingDocumentOnly> GetSupportingDocumentsItem(IArticle article) => article.SecondMessageSupportingDocuments;

		protected override IEnumerable<ITariffAdditionalCode> GetPartDispos(IArticle article) => article.SecondMessagePartDispos;

		protected override void LoadEntryHeaderCharges(TArticleDsiComp articleImport, IArticle article)
		{
			if (article.EntryNumber == 1)
			{
				var myPreCalcEntryLinesCollection = articleImport.LignesPrecalcs ?? new Collection<TTaxationDetail>();
				foreach (var entryHeaderCharge in article.EntryHeaderCharges)
				{
					var myEntryHeaderCharge = new TTaxationDetail()
					{
						Codtax = entryHeaderCharge.TaxCode,
						Typtax = entryHeaderCharge.TaxType,
						Quotax = entryHeaderCharge.TaxRate.Round(3),
						Asstax = entryHeaderCharge.TaxAssessed.Round(0).ToZLong(),
						Montanttax = entryHeaderCharge.TaxAmount.Round(0).ToZLong(),
						Codeport = entryHeaderCharge.ChargePaymentOrDestinationID
					};

					myPreCalcEntryLinesCollection.Add(myEntryHeaderCharge);
				}

				articleImport.LignesPrecalcs = myPreCalcEntryLinesCollection;
			}
		}
	}
}
