using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaArrivalLineCollection<AsycudaArrivalLine>))]
	sealed class AsycudaArrivalLineCollectionGenericTest : ActiveBusinessObjectCollectionTestCase<AsycudaArrivalLineCollection<AsycudaArrivalLine>>
	{
		protected override Type GetExpectedCollectionType() => typeof(AsycudaArrivalLineCollection<AsycudaArrivalLine>);
		protected override AsycudaArrivalLineCollection<AsycudaArrivalLine> GetCollectionToTest()
		{
			var arrHeader = Factory.NewWithValidTestData<AsycudaArrivalHeader>();
			return new AsycudaArrivalLineCollection<AsycudaArrivalLine>(arrHeader);
		}
	}
}
