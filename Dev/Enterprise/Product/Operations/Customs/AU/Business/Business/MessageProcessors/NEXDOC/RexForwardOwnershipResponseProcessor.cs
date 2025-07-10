using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.AU.Declaration.Business.RexOwnershipSoap;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RexForwardOwnershipResponseProcessor : TransferForwardBaseProcessor<RexForwardOwnershipResponse>
	{
		public RexForwardOwnershipResponseProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override ZString EmailTitle => "Forward Ownership Response";
		protected override RexOwnershipOutcomeType GetOutcomeTypeFromResponse(RexForwardOwnershipResponse ownershipResponse) => ownershipResponse.outcome;
	}
}
