using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class DatesPlacesProviderTest : DataProviderTestCase<DatesPlacesProvider>
	{
		public void TestDate()
		{
			AssertEquals("Date", preparationDateAndTime, Provider.Date);
		}

		public void TestOfficeOfDebt()
		{
			sendingObject.OfficeOfDebt = "Test Office Of Debt";
			AssertEquals("OfficeOfDebt", "Test Office Of Debt", Provider.OfficeOfDebt);
		}
		public void TestOfficeOfResponsibility()
		{
			sendingObject.OfficeOfResponsibility = "Test Office Of Responsibility";
			AssertEquals("OfficeOfResponsibility", "Test Office Of Responsibility", Provider.OfficeOfResponsibility);
		}

		public void TestLocationOfGoods()
		{
			goodsLocation.CGL_Qualifier = "T";
			goodsLocation.CGL_AdditionalIdentifier = "B";
			goodsLocation.CGL_Type = "A";
			AssertType<RF415GoodsLocationProvider>(Provider.LocationOfGoods);
			AssertEquals("QualifierIdentification", "T", Provider.LocationOfGoods.QualifierIdentification);
			AssertEquals("AdditionalIdentifier", "B", Provider.LocationOfGoods.AdditionalIdentifier);
			AssertEquals("LocationTypeCode", "A", Provider.LocationOfGoods.LocationTypeCode);
		}

		protected sealed override DatesPlacesProvider GetProvider()
		{
			return new DatesPlacesProvider(sendingObject, preparationDateAndTime);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			goodsLocation = bill.CusGoodsLocation;
			sendingObject = new RF415MessageSendingObject(bill);

			preparationDateAndTime = new DateTime(2019, 05, 09, 9, 15, 0);
		}

		DateTime preparationDateAndTime;
		RF415MessageSendingObject sendingObject;
		AsycudaBill bill;
		AsycudaManifestHeader header;
		CusGoodsLocation goodsLocation;
	}
}

