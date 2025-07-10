using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class RF415DatesPlacesTypeProviderTest : DataProviderTestCase<RF415DatesPlacesTypeProvider>
	{
		[TestDate(2023, 12, 01)]
		public void TestDate()
		{
			AssertEquals("1/12/2023 12:00:00 AM", Provider.Date);
		}

		[TestDate(2023, 12, 01)]
		public void TestOfficeOfDept()
		{
			SendingObject.OfficeOfDebt = "OOD12345";
			AssertEquals("OOD12345", Provider.OfficeOfDept);
		}

		[TestDate(2023, 12, 01)]
		public void TestOfficeOfResponsibility()
		{
			SendingObject.OfficeOfResponsibility = "OOD12345";
			AssertEquals("OOD12345", Provider.OfficeOfResponsibility);
		}

		public void TestLocationOfGoods()
		{
			var location = SendingObject.Bill.CusGoodsLocation;
			location.CGL_Qualifier = "T";
			AssertType<GoodsLocationProvider>(Provider.LocationOfGoods);
			AssertEquals("QualifierIdentification", "T", Provider.LocationOfGoods.QualifierOfIdentification);
		}

		protected override RF415DatesPlacesTypeProvider GetProvider() => new RF415DatesPlacesTypeProvider(SendingObject);

		RF415MessageSendingObject SendingObject => sendingObject ?? (sendingObject = new RF415MessageSendingObject(Factory.New<AsycudaManifestHeader>().Bills.AddNew()));
		RF415MessageSendingObject sendingObject;
	}
}
