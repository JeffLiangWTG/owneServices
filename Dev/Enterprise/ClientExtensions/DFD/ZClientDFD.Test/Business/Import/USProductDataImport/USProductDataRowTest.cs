using CargoWise.Common;
using Enterprise.ClientSharedComponents.DataTransfer.Testing;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.DFD.Business.Import.Testing
{
	class USProductDataRowTest : SharedFlatFileDataRowTest
	{
		public void TestProperties()
		{
			string dataLine = "EMIN25,CARISOPROPODOL,CARISOPROPODOL,BLAHH!!,2924198000,EMIN25,CARISOPROPODOL,65PCS57,A,IT,DLS,49287-001-1,ITLABCHI10MIL,ITLABCHI10MIL,FDA,CARISOPROPODOL,PAVLO,5162567227,DDFG";
			var dataRow = new USProductDataRow(new FlatFileDataRow(new OCsvLine(dataLine).FieldValues));
			AssertEquals("EMIN25", dataRow.Customer);
			AssertEquals("CARISOPROPODOL", dataRow.ProductCode);
			AssertEquals("CARISOPROPODOL", dataRow.ProductDesc1);
			AssertEquals("BLAHH!!", dataRow.ProductDesc2);
			AssertEquals("2924198000", dataRow.HTCode);
			AssertEquals("EMIN25", dataRow.CustomerCode_Ignore);
			AssertEquals("CARISOPROPODOL", dataRow.ProductCode_Ignore);
			AssertEquals("65PCS57", dataRow.FDAProductCode);
			AssertEquals("A", dataRow.CargoStorageStatus);
			AssertEquals("IT", dataRow.CountryOfProduction);
			AssertEquals("DLS", dataRow.AffirmatnOfCompliance);
			AssertEquals("49287-001-1", dataRow.AffrmtnOfCmplnceQlfr);
			AssertEquals("ITLABCHI10MIL", dataRow.ActualManufacturerID);
			AssertEquals("ITLABCHI10MIL", dataRow.ActualSupplierShipper);
			AssertEquals("FDA", dataRow.FDAConsigneeCode);
			AssertEquals("CARISOPROPODOL", dataRow.BrandName);
			AssertEquals("PAVLO", dataRow.ContactName_Ignore);
			AssertEquals("5162567227", dataRow.TelephoneNumber_Ignore);
			AssertEquals("DDFG", dataRow.AffirmatnOfComplian2);
		}
	}
}
