using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.CDS
{
	class MUCRQueryDeclarationRequestMessageBuilder : CDSInventoryLinkingRequestMessageBuilder
	{
		public MUCRQueryDeclarationRequestMessageBuilder(IUkCinvWrapper messageDataProvider) : base(messageDataProvider)
		{
		}

		protected override ZString Build() => new inventoryLinkingQueryRequest
		{
			queryUCR = new ucrBlock
			{
				ucr = messageDataProvider.MasterUniqueConsignmentReference,
				ucrType = ucrType.M
			},
		}.Serialize();
	}
}
