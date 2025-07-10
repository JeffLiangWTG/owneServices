using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.CDS
{
	class CloseRequestMessageBuilder : CDSInventoryLinkingRequestMessageBuilder
	{
		public CloseRequestMessageBuilder(IUkCinvWrapper messageDataProvider) : base(messageDataProvider)
		{
		}

		protected override ZString Build() => new inventoryLinkingConsolidationRequest
		{
			messageCode = messageCodeConsolidation.CST,
			masterUCR = messageDataProvider.MasterUniqueConsignmentReference
		}.Serialize();
	}
}
