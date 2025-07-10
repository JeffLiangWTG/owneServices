using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ParcelLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeDescriptionPairList()
		{
			var list = parcel.Lookups.DeliveryTypeCodeList;

			AssertEquals(2, list.Count);
			AssertEquals(Messaging.DeliveryTypeCodeList.Codes.A, list[0].Code);
			AssertEquals(Messaging.DeliveryTypeCodeList.Descriptions.B, list[1].Description);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			parcel = invoice.Parcels.AddNew();
		}
		Parcel parcel;
	}
}
