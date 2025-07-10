using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(AsycudaPreBoardingNotificationFilterStripBusinessObject))]
	sealed class AsycudaPreBoardingNotificationFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new AsycudaPreBoardingNotificationFilterStripBusinessObject();
	}
}
