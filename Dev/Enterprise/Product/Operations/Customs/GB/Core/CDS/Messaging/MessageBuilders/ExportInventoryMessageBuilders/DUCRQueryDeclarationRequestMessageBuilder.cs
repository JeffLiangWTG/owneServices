using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS
{
	class DUCRQueryDeclarationRequestMessageBuilder : CDSInventoryLinkingRequestMessageBuilder
	{
		public DUCRQueryDeclarationRequestMessageBuilder(IUkCinvWrapper messageDataProvider) : base(messageDataProvider)
		{
		}

		protected override ZString Build()
		{
			return new inventoryLinkingQueryRequest
			{
				queryUCR = UCRHelper.ComposeUcrBlock(messageDataProvider.CDSDeclarationUniqueConsignmentReference,
					messageDataProvider.CDSDeclarationUniqueConsignmentReferencePartSuffix, ucrType.D),
			}.Serialize();
		}
	}
}
