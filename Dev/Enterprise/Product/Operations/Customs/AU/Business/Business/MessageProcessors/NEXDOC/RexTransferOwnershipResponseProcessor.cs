using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.AU.Declaration.Business.RexOwnershipSoap;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RexTransferOwnershipResponseProcessor : TransferForwardBaseProcessor<RexTransferOwnershipResponse>
	{
		public RexTransferOwnershipResponseProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override ZString EmailTitle => "Transfer Ownership Response";
		protected override RexOwnershipOutcomeType GetOutcomeTypeFromResponse(RexTransferOwnershipResponse ownershipResponse) => ownershipResponse.outcome;
	}
}
