using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Intrastat.Business.Testing
{
	sealed class CusIntrastatHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCIH_CountryOfReceipt()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(header.CIH_CountryOfReceiptInfo, "QQ", "DE");
			ValidationTestHelper.AssertErrorIfNotEntered(header.CIH_CountryOfReceiptInfo);
		}

		public void TestCheckCIH_CountryOfSupply()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(header.CIH_CountryOfSupplyInfo, "QQ", "DE");
			ValidationTestHelper.AssertErrorIfNotEntered(header.CIH_CountryOfSupplyInfo);
		}

		public void TestCheckCIH_TradersReference()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(header.CIH_TradersReferenceInfo);
		}

		public void TestCheckCIH_ConsigneeName()
		{
			var org = IntrastatTestDataHelper.New(Factory).GetOrCreateOrgHeader("ORG");
			const string message = "Either Consignee Name or Consignee Code should be entered";
			AssertExactlyOnlyOneOfIsEntered(header.CIH_ConsigneeNameInfo, header.CIH_OH_ConsigneeInfo, org.PK, message);
		}

		public void TestCheckCIH_OH_Consignee()
		{
			const string message = "Either Consignee Code or Consignee Name should be entered";
			AssertExactlyOnlyOneOfIsEntered(header.CIH_OH_ConsigneeInfo, header.CIH_ConsigneeNameInfo, (ZString)"ORG", message);
		}

		public void TestCheckCIH_OH_Supplier()
		{
			const string message = "Either Supplier Code or Supplier Name should be entered";
			AssertExactlyOnlyOneOfIsEntered(header.CIH_OH_SupplierInfo, header.CIH_SupplierNameInfo, (ZString)"ORG", message);
		}

		public void TestCheckCIH_SupplierName()
		{
			var org = IntrastatTestDataHelper.New(Factory).GetOrCreateOrgHeader("ORG");
			const string message = "Either Supplier Name or Supplier Code should be entered";
			AssertExactlyOnlyOneOfIsEntered(header.CIH_SupplierNameInfo, header.CIH_OH_SupplierInfo, org.PK, message);
		}

		void AssertExactlyOnlyOneOfIsEntered(ZPropertyInfo target, ZPropertyInfo other, IZType value, string message)
		{
			other.Value = value;
			ValidationTestHelper.AssertErrorIfEntered(target, message);
			other.ClearValue();
			ValidationTestHelper.AssertErrorIfNotEntered(target, message);
		}

		protected override void SetUp()
		{
			header = Factory.New<CusIntrastatHeader>();
			base.SetUp();
		}

		CusIntrastatHeader header;
	}
}
