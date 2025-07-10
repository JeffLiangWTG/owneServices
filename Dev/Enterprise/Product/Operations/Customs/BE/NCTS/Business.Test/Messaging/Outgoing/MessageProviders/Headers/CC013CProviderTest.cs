using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC013CHeaderProvider))]
	sealed class CC013CProviderTest : NctsHeaderProviderAbstractTest<CC013CHeaderProvider>
	{
		public void TestTransitOperation()
		{
			AssertType<CC013CTransitOperationProvider>(Provider.TransitOperation);
		}

		protected override string MessageType => Constants.MessageTypes.CC013C;

		protected override string MovementType => NctsMovementType.Codes.Departure;
	}
}
