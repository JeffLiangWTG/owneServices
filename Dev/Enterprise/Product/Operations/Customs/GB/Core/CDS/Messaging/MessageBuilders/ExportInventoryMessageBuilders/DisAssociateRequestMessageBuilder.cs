using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using static Enterprise.Customs.GB.Business.GbDes242MessageFunction;

namespace Enterprise.Customs.GB.CDS
{
	class DisAssociateRequestMessageBuilder : MasterUcrWithChildUcrMessageBuilder
	{
		public DisAssociateRequestMessageBuilder(IUkCinvWrapper messageDataProvider) : base(messageDataProvider)
		{
		}

		public DisAssociateRequestMessageBuilder(IUkCinvWrapper messageDataProvider, MucrDisAssociate mucrDisassociate) : base(messageDataProvider, mucrDisassociate)
		{
		}

		protected override ZString Build()
		{
			return new inventoryLinkingConsolidationRequest
			{
				messageCode = messageCodeConsolidation.EAC,
				ucrBlock = UCRHelper.ComposeUcrBlock(Ucr, UcrPartNo, ucrType.D),
			}.Serialize();
		}
	}
}
