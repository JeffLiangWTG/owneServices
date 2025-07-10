using CargoWise.Types;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Client.AWH.Testing
{
	class AWHFlatFileDataRowCollectionTest : FlatFileDataRowCollectionTest
	{
		public void TestAdd()
		{
			AWHFlatFileDataRowCollection collection = new AWHFlatFileDataRowCollection();
			AWHFlatFileDataRow dataRow1 = new AWHFlatFileDataRow();
			dataRow1.ClientAccount = "123456";
			dataRow1.GLAccount = "111111";
			dataRow1.Date = ZDateTime.Now;
			dataRow1.Desc = "Description";
			dataRow1.Amount = 100;
			dataRow1.Year = ZDateTime.Now;
			dataRow1.TaxCode = "tax code";
			dataRow1.Invoice = "INVOICE";
			collection.Add(dataRow1);
			AssertEquals(1, collection.Count);
			AWHFlatFileDataRow dataRow2 = new AWHFlatFileDataRow();
			dataRow2.ClientAccount = "aaaaa";
			dataRow2.GLAccount = "111111";
			dataRow2.Date = ZDateTime.Now;
			dataRow2.Desc = "Description";
			dataRow2.Amount = 200;
			dataRow2.Year = ZDateTime.Now;
			dataRow2.TaxCode = "tax code";
			dataRow2.Invoice = "INVOICE";
			collection.Add(dataRow2);
			AssertEquals(1, collection.Count);
			AssertEquals("300", collection[0][AWHFlatFileDataRow.Schema.Amount.Name]);
			AWHFlatFileDataRow dataRow3 = new AWHFlatFileDataRow();
			dataRow3.ClientAccount = "aaaaa";
			dataRow3.GLAccount = "122211";
			dataRow3.Date = ZDateTime.Now;
			dataRow3.Desc = "Description";
			dataRow3.Amount = 200;
			dataRow3.Year = ZDateTime.Now;
			dataRow3.TaxCode = "tax code";
			dataRow3.Invoice = "INVOICE";
			collection.Add(dataRow3);
			AssertEquals(2, collection.Count);
		}
	}
}
