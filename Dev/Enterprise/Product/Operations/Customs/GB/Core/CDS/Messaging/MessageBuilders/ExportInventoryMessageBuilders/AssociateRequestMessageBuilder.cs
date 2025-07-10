using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using static Enterprise.Customs.GB.Business.GbDes242MessageFunction;

namespace Enterprise.Customs.GB.CDS
{
	class AssociateRequestMessageBuilder : MasterUcrWithChildUcrMessageBuilder
	{
		public AssociateRequestMessageBuilder(IUkCinvWrapper messageDataProvider) : base(messageDataProvider)
		{
		}

		public AssociateRequestMessageBuilder(IUkCinvWrapper messageDataProvider, MucrAssociate mucrAssociate) : base(messageDataProvider, mucrAssociate)
		{
		}

		protected override ZString Build()
		{
			return new inventoryLinkingConsolidationRequest
			{
				messageCode = messageCodeConsolidation.EAC,
				masterUCR = messageDataProvider.MasterUniqueConsignmentReference,
				ucrBlock = UCRHelper.ComposeUcrBlock(Ucr, UcrPartNo, ucrType.D)
			}.Serialize();
		}
	}
}
