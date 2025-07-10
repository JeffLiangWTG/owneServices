using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE044TransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<IE044TransitOperationProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new IE044TransitOperationProvider(null));
			});
		}

		protected override IE044TransitOperationProvider GetProvider() => new IE044TransitOperationProvider(nctsHeader);

		public void TestOtherThingsToReport()
		{
			nctsHeader.ArrivalMovementHeader.OtherThingsToReport = "Test data";
			var provider = GetProvider();
			AssertEquals("Other things to report", "Test data", provider.OtherThingsToReport);
		}

		public void TestMRN()
		{
			nctsHeader.ArrivalMrnFromUser = "1234567";
			var provider = GetProvider();
			AssertEquals("MRN", "1234567", provider.MRN);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.CusAuthorizationUsages.RemoveAndDeleteAll();
		}

		NctsHeader nctsHeader;
	}
}
