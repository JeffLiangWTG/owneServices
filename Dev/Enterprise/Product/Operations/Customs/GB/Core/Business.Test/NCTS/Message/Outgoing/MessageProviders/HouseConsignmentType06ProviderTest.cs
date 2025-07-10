using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class HouseConsignmentType06ProviderTest : DataProviderTestCase<HouseConsignmentType06Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new HouseConsignmentType06Provider(null, 0));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(20, Provider.SequenceNumber);
		}

		public void TestDepartureTransportMeans()
		{
			bill.DepartureTransportInfos.AddNew();
			bill.DepartureTransportInfos.AddNew();
			AssertEquals(2, Provider.DepartureTransportMeans.Count);
		}

		protected override HouseConsignmentType06Provider GetProvider() => new HouseConsignmentType06Provider(bill, 20);

		protected override void SetUp()
		{
			base.SetUp();
			bill = Factory.New<NctsBill>();
		}
		NctsBill bill;
	}
}
