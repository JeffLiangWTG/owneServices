using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using static Enterprise.Customs.GB.Business.GbDes242MessageFunction;

namespace Enterprise.Customs.GB.CDS
{
	abstract class MasterUcrWithChildUcrMessageBuilder : CDSInventoryLinkingRequestMessageBuilder
	{
		public MasterUcrWithChildUcrMessageBuilder(IUkCinvWrapper messageDataProvider) : base(messageDataProvider)
		{
			Ucr = messageDataProvider.CDSDeclarationUniqueConsignmentReference;
			UcrPartNo = messageDataProvider.CDSDeclarationUniqueConsignmentReferencePartSuffix;
		}

		public MasterUcrWithChildUcrMessageBuilder(IUkCinvWrapper messageDataProvider, MasterUcrWithChildUcrFunction mucrMessageFunction) : this(messageDataProvider)
		{
			if (!mucrMessageFunction.ChildUCRToBeAddedToMasterUCR.IsEmpty)
			{
				Ucr = mucrMessageFunction.ChildUCR;
				UcrPartNo = mucrMessageFunction.ChildUCRPartNo;
			}
		}

		protected readonly ZString Ucr;
		protected readonly ZString UcrPartNo;
	}
}
