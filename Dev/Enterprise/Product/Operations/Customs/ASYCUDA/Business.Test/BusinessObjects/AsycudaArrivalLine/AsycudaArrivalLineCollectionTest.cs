using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaArrivalLineCollection))]
	sealed class AsycudaArrivalLineCollectionTest : ActiveBusinessObjectCollectionTestCase<AsycudaArrivalLineCollection>
	{
		protected override Type GetExpectedCollectionType() => typeof(AsycudaArrivalLineCollection);

		protected override AsycudaArrivalLineCollection GetCollectionToTest()
		{
			var arrHeader = Factory.NewWithValidTestData<AsycudaArrivalHeader>();
			return new AsycudaArrivalLineCollection(arrHeader);
		}
	}
}
