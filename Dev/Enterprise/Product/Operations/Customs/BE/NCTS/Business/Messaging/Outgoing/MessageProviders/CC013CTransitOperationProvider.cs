using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC013CTransitOperationProvider : TransitOperationProvider
	{
		public CC013CTransitOperationProvider(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		public override string LRN => MRN.IsEmpty() ? base.LRN : null;

		public override string MRN => header.MovementReferenceNumber;
	}
}
