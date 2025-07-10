using System;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class HouseConsignmentType06ProviderTest : Customs.Business.Testing.DataProviderTestCase<HouseConsignmentType06Provider>
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
