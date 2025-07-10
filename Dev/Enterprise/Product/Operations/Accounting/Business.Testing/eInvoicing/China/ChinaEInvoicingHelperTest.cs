using System;
using System.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.eInvoicing.China;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.EInvoicing.China.Testing
{
	public class ChinaEInvoicingHelperTest : TestCaseWithFactory
	{
		public void TestGetRemarkStringWithoutBranchId()
		{
			var dataRow = CreateDataRowForTest();
			var remark = ChinaEInvoicingHelper.CreateRemarkString(Factory, dataRow, GlbCompany.CurrentCompany.PK.ToGuid());
			AssertEquals("MBL: demo1 HBL: demo2 Vessel: demo3 Voyage/Flight: demo4 ETD: demo17 ETA: demo18 LoadPort: demo5 DischargePort: demo6 JobInvoiceNumber: demo7", remark);
		}

		public void TestGetRemarkStringWithBranchId()
		{
			var currentBranchId = GlbBranch.CurrentBranch.PK.ToGuid();
			using (AccountingConfigurationRegistry.Instance.AccountingWebServiceRemarks.SetTemporaryValue(Guid.Empty, currentBranchId, Guid.Empty, "This is Custom Branch Remark"))
			{
				var dataRow = CreateDataRowForTest();
				var remarkWithBranchLevel = ChinaEInvoicingHelper.CreateRemarkString(Factory, dataRow, GlbCompany.CurrentCompany.PK.ToGuid(), currentBranchId);
				AssertEquals("This is Custom Branch Remark", remarkWithBranchLevel);

				var remarkWithCompanyLevel = ChinaEInvoicingHelper.CreateRemarkString(Factory, dataRow, GlbCompany.CurrentCompany.PK.ToGuid());
				AssertEquals("MBL: demo1 HBL: demo2 Vessel: demo3 Voyage/Flight: demo4 ETD: demo17 ETA: demo18 LoadPort: demo5 DischargePort: demo6 JobInvoiceNumber: demo7", remarkWithCompanyLevel);
			}
		}

		DataRow CreateDataRowForTest()
		{
			var table = new DataTable();
			table.Columns.Add("MasterBill", typeof(string));
			table.Columns.Add("HouseBill", typeof(string));
			table.Columns.Add("Vessel", typeof(string));
			table.Columns.Add("Voyage", typeof(string));
			table.Columns.Add("LoadPort", typeof(string));
			table.Columns.Add("DischargePort", typeof(string));
			table.Columns.Add("JobInvoiceNumber", typeof(string));
			table.Columns.Add("AH_OSTotal", typeof(string));
			table.Columns.Add("AH_RX_NKTransactionCurrency", typeof(string));
			table.Columns.Add("AH_TransactionNum", typeof(string));
			table.Columns.Add("AH_ExchangeRate", typeof(string));
			table.Columns.Add("OperatorFullName", typeof(string));
			table.Columns.Add("OperatorFriendlyName", typeof(string));
			table.Columns.Add("RepSalesFullName", typeof(string));
			table.Columns.Add("RepSalesFriendlyName", typeof(string));
			table.Columns.Add("AH_Desc", typeof(string));
			table.Columns.Add("ETD", typeof(string));
			table.Columns.Add("ETA", typeof(string));
			table.Rows.Add("demo1", "demo2", "demo3", "demo4", "demo5", "demo6", "demo7", "demo8", "demo9", "demo10", "demo11", "demo12", "demo13", "demo14", "demo15", "demo16", "demo17", "demo18");
			return table.Rows[0];
		}
	}
}
