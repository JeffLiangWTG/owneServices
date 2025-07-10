using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageBillValidationUCC5Test : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V1;
			var bill = header.Bills.AddNew();
			if (bill.Validation is TemporaryStorageBillValidationUCC5 validation)
			{
				AssertType<TemporaryStorageBill>(validation.Parent);
			}
			else
			{
				Assert("Incorrect validation type", false);
			}
		}

		public void TestCheckABL_GrossWeight()
		{
			AssertNoErrors(bill.ABL_GrossWeightInfo);
		}

		public void TestCheckABL_GrossWeightUQ()
		{
			AssertNoErrors(bill.ABL_GrossWeightUQInfo);
		}

		public void TestCheckConsignorOrgPK()
		{
			AssertNoErrors(bill.ConsignorOrgPKInfo);
		}

		public void TestCheckABL_ShipperName()
		{
			AssertNoErrors(bill.ABL_ShipperNameInfo);
		}

		public void TestCheckABL_RN_NKShipperCountry()
		{
			AssertNoErrors(bill.ABL_RN_NKShipperCountryInfo);
		}

		public void TestCheckABL_ShipperPostcode()
		{
			AssertNoErrors(bill.ABL_ShipperPostcodeInfo);
		}

		public void TestCheckConsigneeOrgPK()
		{
			AssertNoErrors(bill.ConsigneeOrgPKInfo);
		}

		public void TestCheckABL_ConsigneeName()
		{
			AssertNoErrors(bill.ABL_ConsigneeNameInfo);
		}

		public void TestCheckABL_RN_NKConsigneeCountry()
		{
			AssertNoErrors(bill.ABL_RN_NKConsigneeCountryInfo);
		}

		public void TestCheckABL_ConsigneePostcode()
		{
			AssertNoErrors(bill.ABL_ConsigneePostcodeInfo);
		}

		public void TestCheckTypeOfBillDocument()
		{
			AssertNoErrors(bill.TypeOfBillDocumentInfo);
		}

		public void TestCheckABL_BillNumber()
		{
			AssertNoErrors(bill.ABL_BillNumberInfo);
		}

		public void TestCheckABL_BolType()
		{
			AssertNoErrors(bill.ABL_BolTypeInfo);
		}

		public void TestCheckABL_ShipperRegNoType()
		{
			AssertNoErrors(bill.ABL_ShipperRegNoInfo);
		}

		public void TestCheckABL_ShipperState()
		{
			AssertNoErrors(bill.ABL_ShipperStateInfo);
		}

		public void TestCheckABL_ConsigneeRegNoType()
		{
			AssertNoErrors(bill.ABL_ConsigneeRegNoTypeInfo);
		}

		public void TestCheckABL_ConsigneeState()
		{
			AssertNoErrors(bill.ABL_ConsigneeStateInfo);
		}

		public void TestCheckABL_NotifyPartyRegNoType()
		{
			AssertNoErrors(bill.ABL_NotifyPartyRegNoInfo);
		}

		public void TestCheckABL_NotifyPartyState()
		{
			AssertNoErrors(bill.ABL_NotifyPartyStateInfo);
		}

		protected override void SetUp()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			bill = header.Bills.AddNew();
		}
		TemporaryStorageBill bill;
	}
}
