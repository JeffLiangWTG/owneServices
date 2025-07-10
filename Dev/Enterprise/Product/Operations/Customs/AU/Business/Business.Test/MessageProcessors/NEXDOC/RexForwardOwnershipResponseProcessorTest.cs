using CargoWise.Types;
using Enterprise.BatchProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(RexForwardOwnershipResponseProcessor))]
	sealed class RexForwardOwnershipResponseProcessorTest : TransferForwardBaseProcessorAbstractTest
	{
		protected override ZString EmailTitle => "Forward Ownership Response";

		protected override NEXDOCMessageProcessor OwnershipResponseProcessor => new RexForwardOwnershipResponseProcessor(new LoggingInformation());

		protected override string OwnershipResponseType => "RexForwardOwnershipResponse";
	}
}
