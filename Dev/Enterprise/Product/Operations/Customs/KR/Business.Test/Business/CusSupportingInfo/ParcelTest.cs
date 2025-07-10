using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(Parcel))]
	sealed class ParcelTest : CusSupportingInfoTest<Parcel>
	{
		protected override BusinessObject GetNewBusinessObject() => parcel;

		protected override IEnumerable<Parcel> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var invoice = factory.New<JobDeclaration>().Invoices.AddNew();
			var parcelCollection = new ParcelCollection(invoice);
			yield return parcelCollection.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var businessObj = (Parcel)base.GetBusinessObjectForFetchForLoad();
			businessObj.CSI_Type = CusSupportingInfoTypeList.Codes.Parcel;
			return businessObj;
		}

		public void TestValidation()
		{
			var supportingInfo = Factory.New<Parcel>();
			AssertEquals(typeof(ParcelValidation), supportingInfo.Validation.GetType());
		}

		public void TestParent()
		{
			AssertEquals(invoice, parcel.Parent);
		}

		protected override void SetUp()
		{
			invoice = Factory.New<JobDeclaration>().Invoices.AddNew();
			parcel = new ParcelCollection(invoice).AddNew();
		}
		Parcel parcel;
		JobComInvoiceHeader invoice;
	}
}
