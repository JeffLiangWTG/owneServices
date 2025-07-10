using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Send.Export;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD
{
	public class DDANNSendExpMessageBuilder : DDSendExpMessageBaseBuilder
	{
		public DDANNSendExpMessageBuilder(IDeclarationImportExport declaration
			, ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declaration, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}

		protected override TGenDse PopulateProcedure() => null;

		protected override Collection<TArticleDse> PopulateArticlesExport() => new Collection<TArticleDse>();

		protected override TPreval PopulatePreval(ICusProcedure cusProcedure) => null;

		protected override ZString PopulateDeclEmergencyProcDate(ICusProcedure cusProcedure) => null;

		protected override TMotivation PopulateMotivation(IHeader motivationHeader) => string.IsNullOrEmpty(motivationHeader?.Motivation?.Motivation) ? null : base.PopulateMotivation(motivationHeader);
	}
}
