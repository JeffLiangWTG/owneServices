using CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Send.Export;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD
{
	public class DDMDVSendExpMessageBuilder : DDSendExpMessageBaseBuilder
	{
		public DDMDVSendExpMessageBuilder(IDeclarationImportExport declaration
			, ErrorCollector errorCollectorObject
			, TransactionTypes transactionType
			, int newSequenceNumeric)
			: base(declaration, errorCollectorObject, transactionType, newSequenceNumeric)
		{
		}

		protected override ZString PopulateDeclEmergencyProcDate(ICusProcedure cusProcedure) => null;

		protected override TMotivation PopulateMotivation(IHeader motivationHeader) => null;
	}
}
