using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.AWH.Testing
{
	public class AWHFlatFileDataRowTest : TestCaseWithFactory
	{
		[TestDate(2007, 1, 1)]
		public void TestAWHDataRowProperties()
		{
			AWHFlatFileDataRow dataRow = new AWHFlatFileDataRow();
			//default values
			AssertEquals("Code", AWHConstants.TransType, dataRow[AWHFlatFileDataRow.Schema.Code.Name]);
			AssertEquals("Zero1", AWHConstants.FixedZero, dataRow[AWHFlatFileDataRow.Schema.Zero1.Name]);
			AssertEquals("Zero2", AWHConstants.FixedZero, dataRow[AWHFlatFileDataRow.Schema.Zero2.Name]);
			AssertEquals("One", AWHConstants.FixedOne, dataRow[AWHFlatFileDataRow.Schema.One.Name]);
			AssertEquals("A", AWHConstants.FixedA, dataRow[AWHFlatFileDataRow.Schema.A.Name]);
			dataRow.ClientAccount = "123456";
			dataRow.GLAccount = "111111";
			dataRow.Date = ZDateTime.Now;
			dataRow.Desc = "Description";
			dataRow.Amount = 100;
			dataRow.Year = ZDateTime.Now;
			dataRow.Invoice = "INVOICE";
			AssertEquals("ClientAccount", "123456", dataRow[AWHFlatFileDataRow.Schema.ClientAccount.Name]);
			AssertEquals("GLAccount", "111111", dataRow[AWHFlatFileDataRow.Schema.GLAccount.Name]);
			AssertEquals("Date", "20070101", dataRow[AWHFlatFileDataRow.Schema.Date.Name]);
			AssertEquals("Desc", "Description", dataRow[AWHFlatFileDataRow.Schema.Desc.Name]);
			AssertEquals("Amount", "100", dataRow[AWHFlatFileDataRow.Schema.Amount.Name]);
			AssertEquals("Year", "2007", dataRow[AWHFlatFileDataRow.Schema.Year.Name]);
			AssertEquals("Invoice", "INVOICE", dataRow[AWHFlatFileDataRow.Schema.Invoice.Name]);
			dataRow.TaxCode = AWHConstants.GSTExc;
			AssertEquals("GST Code", "G03E", dataRow[AWHFlatFileDataRow.Schema.TaxCode.Name]);
			dataRow.TaxCode = AWHConstants.GSTInc;
			AssertEquals("GST Code", "G01I", dataRow[AWHFlatFileDataRow.Schema.TaxCode.Name]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CodeDescriptionPairList brchList = new CodeDescriptionPairList();
			brchList.Add(new CodeDescriptionPair(Env.CurrentBranch.Code, "ABC"));
			AWHDataRegistry.Instance.BranchList = brchList;
			CodeDescriptionPairList deptList = new CodeDescriptionPairList();
			deptList.Add(new CodeDescriptionPair(Env.CurrentDepartment.Code, "1"));
			AWHDataRegistry.Instance.DepartmentList = deptList;
		}
	}
}
