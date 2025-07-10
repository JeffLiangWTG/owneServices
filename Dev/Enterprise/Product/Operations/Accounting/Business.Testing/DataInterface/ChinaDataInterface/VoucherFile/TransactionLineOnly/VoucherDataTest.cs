using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(VoucherDataSource))]
	class VoucherDataTest : BusinessObjectBaseTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new VoucherDataSource(Factory, GetDummyDataRow(Guid.NewGuid(), "", 0));
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return new VoucherDataSource(Factory, GetDummyDataRow(Guid.NewGuid(), "", 0));
		}

		protected DataRow GetDummyDataRow(Guid gLHeader, string type, decimal amount)
		{
			DataTable testTable = new DataTable();
			testTable.Columns.Add("GLHeader", typeof(Guid));
			testTable.Columns.Add("TransactionType", typeof(string));
			testTable.Columns.Add("Amount", typeof(decimal));
			testTable.Columns.Add("VoucherNumber", typeof(string));
			return testTable.Rows.Add(new object[] { gLHeader, type, amount, "ABC" });
		}
	}
}
