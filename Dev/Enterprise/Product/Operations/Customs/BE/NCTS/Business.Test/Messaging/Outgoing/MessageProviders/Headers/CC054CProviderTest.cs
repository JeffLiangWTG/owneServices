using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC054CProvider))]
	sealed class CC054CProviderTest : NctsHeaderProviderAbstractTest<CC054CProvider>
	{
		[TestDate(2025, 05, 14, 17, 39, 20, 555)]
		public void TestReleaseRequestDateAndTime() => AssertEquals(new DateTime(2025, 05, 14, 17, 39, 20, 0, DateTimeKind.Unspecified), Provider.ReleaseRequestDateAndTime);

		public void TestReleaseRequested() => CombineAssertions(() =>
		{
			AssertEquals(true, new CC054CProvider(nctsHeader, true).ReleaseRequested);
			AssertEquals(false, new CC054CProvider(nctsHeader, false).ReleaseRequested);
		});

		protected override string MessageType => Constants.MessageTypes.CC054C;

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override void CreateProvider()
		{
			provider = new CC054CProvider(nctsHeader, true);
		}
	}
}
