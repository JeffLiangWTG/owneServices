using CargoWise.Types;
using Enterprise.BatchProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(RexTransferOwnershipResponseProcessor))]
	sealed class RexTransferOwnershipResponseProcessorTest : TransferForwardBaseProcessorAbstractTest
	{
		protected override ZString EmailTitle => "Transfer Ownership Response";

		protected override NEXDOCMessageProcessor OwnershipResponseProcessor => new RexTransferOwnershipResponseProcessor(new LoggingInformation());

		protected override string OwnershipResponseType => "RexTransferOwnershipResponse";
	}
}
