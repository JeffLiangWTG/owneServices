using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.DP2.Testing
{
	public class DP2FlatFileDataRowTest : TestCaseWithFactory
	{
		[TestDate(2007, 1, 1)]
		public void TestDP2DataRowProperties()
		{
			DP2FlatFileDataRow dataRow = new DP2FlatFileDataRow();
			//default values
			AssertEquals("Code", DP2Constants.TransType, dataRow[DP2FlatFileDataRow.Schema.Code.Name]);
			AssertEquals("Zero1", DP2Constants.FixedZero, dataRow[DP2FlatFileDataRow.Schema.Zero1.Name]);
			AssertEquals("Zero2", DP2Constants.FixedZero, dataRow[DP2FlatFileDataRow.Schema.Zero2.Name]);
			AssertEquals("One", DP2Constants.FixedOne, dataRow[DP2FlatFileDataRow.Schema.One.Name]);
			AssertEquals("A", DP2Constants.FixedA, dataRow[DP2FlatFileDataRow.Schema.A.Name]);
			dataRow.ClientAccount = "123456";
			dataRow.GLAccount = "111111";
			dataRow.Date = ZDateTime.Now;
			dataRow.Desc = "Description";
			dataRow.Amount = 100;
			dataRow.Year = ZDateTime.Now;
			dataRow.Invoice = "INVOICE";
			AssertEquals("ClientAccount", "123456", dataRow[DP2FlatFileDataRow.Schema.ClientAccount.Name]);
			AssertEquals("GLAccount", "111111", dataRow[DP2FlatFileDataRow.Schema.GLAccount.Name]);
			AssertEquals("Date", "20070101", dataRow[DP2FlatFileDataRow.Schema.Date.Name]);
			AssertEquals("Desc", "Description", dataRow[DP2FlatFileDataRow.Schema.Desc.Name]);
			AssertEquals("Amount", "100", dataRow[DP2FlatFileDataRow.Schema.Amount.Name]);
			AssertEquals("Year", "2007", dataRow[DP2FlatFileDataRow.Schema.Year.Name]);
			AssertEquals("Invoice", "INVOICE", dataRow[DP2FlatFileDataRow.Schema.Invoice.Name]);
			dataRow.TaxCode = DP2Constants.GSTExc;
			AssertEquals("GST Code", "G03E", dataRow[DP2FlatFileDataRow.Schema.TaxCode.Name]);
			dataRow.TaxCode = DP2Constants.GSTInc;
			AssertEquals("GST Code", "G01I", dataRow[DP2FlatFileDataRow.Schema.TaxCode.Name]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CodeDescriptionPairList brchList = new CodeDescriptionPairList();
			brchList.Add(new CodeDescriptionPair(Env.CurrentBranch.Code, "ABC"));
			DP2DataRegistry.Instance.BranchList = brchList;
			CodeDescriptionPairList deptList = new CodeDescriptionPairList();
			deptList.Add(new CodeDescriptionPair(Env.CurrentDepartment.Code, "1"));
			DP2DataRegistry.Instance.DepartmentList = deptList;
		}
	}
}
