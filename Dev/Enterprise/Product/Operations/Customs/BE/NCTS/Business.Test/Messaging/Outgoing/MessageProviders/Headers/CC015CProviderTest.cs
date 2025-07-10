using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC015CHeaderProvider))]
	sealed class CC015CProviderTest : NctsHeaderProviderAbstractTest<CC015CHeaderProvider>
	{
		protected override string MessageType => Constants.MessageTypes.CC015C;

		protected override string MovementType => NctsMovementType.Codes.Departure;
	}
}
