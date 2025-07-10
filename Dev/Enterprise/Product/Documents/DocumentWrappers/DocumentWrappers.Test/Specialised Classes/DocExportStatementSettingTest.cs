using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocExportStatementSetting))]
	sealed class DocExportStatementSettingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			DocExportStatementSetting statement = new DocExportStatementSetting(null);
			AssertEquals("CountryCode", "", statement.CountryCode);
			AssertEquals("Name", "", statement.Name);
			AssertEquals("Statement", "", statement.Statement);
			AssertEquals("UseOnHawb", false, statement.UseOnHawb);
			AssertEquals("UseOnHawb", false, statement.UseOnDirectIATAMawb);
			AssertEquals("UseOnHawb", false, statement.UseOnConsolidationMawb);
			AssertEquals("UseOnHawb", false, statement.UseOnHouseBillOfLading);
			AssertEquals("UseOnHawb", false, statement.UseOnDirectMasterBillOfLading);
			AssertEquals("UseOnHawb", false, statement.UseOnConsolidationMasterBillOfLading);
		}

		public void TestToString()
		{
			DocExportStatementSetting statement = new DocExportStatementSetting(null);
			AssertEquals("ToString()", "", statement.ToString());
			statement = new DocExportStatementSetting(new ExportStatementSetting());
			AssertEquals("ToString()", "", statement.ToString());
			ExportStatementSetting.Statement = "Hello World Statement";
			statement = new DocExportStatementSetting(ExportStatementSetting);
			AssertEquals("ToString()", "Hello World Statement", statement.ToString());
		}

		public void TestCountryCode()
		{
			DocExportStatementSetting statement = new DocExportStatementSetting(new ExportStatementSetting());
			AssertEquals("", statement.CountryCode);
			statement = new DocExportStatementSetting(ExportStatementSetting);
			AssertEquals(CountryExportStatementSetting.CountryCode, statement.CountryCode);
		}

		public void TestName()
		{
			ExportStatementSetting.Code = "NDR";
			AssertEquals("NDR", DocExportStatementSetting.Name);
			ExportStatementSetting.Code = "";
			AssertEquals("", DocExportStatementSetting.Name);
		}

		public void TestStatement()
		{
			ExportStatementSetting.Statement = "TESTING STATEMENT";
			AssertEquals("TESTING STATEMENT", DocExportStatementSetting.Statement);
			ExportStatementSetting.Statement = "";
			AssertEquals("", DocExportStatementSetting.Statement);
		}

		public void TestUseOnHawb()
		{
			ExportStatementSetting.UseOnHawb = true;
			AssertEquals(true, DocExportStatementSetting.UseOnHawb);
			ExportStatementSetting.UseOnHawb = false;
			AssertEquals(false, DocExportStatementSetting.UseOnHawb);
		}

		public void TestUseOnDirectIATAMawb()
		{
			ExportStatementSetting.UseOnDirectIATAMawb = true;
			AssertEquals(true, DocExportStatementSetting.UseOnDirectIATAMawb);
			ExportStatementSetting.UseOnDirectIATAMawb = false;
			AssertEquals(false, DocExportStatementSetting.UseOnDirectIATAMawb);
		}

		public void TestUseOnConsolidationMawb()
		{
			ExportStatementSetting.UseOnConsolidationMawb = true;
			AssertEquals(true, DocExportStatementSetting.UseOnConsolidationMawb);
			ExportStatementSetting.UseOnConsolidationMawb = false;
			AssertEquals(false, DocExportStatementSetting.UseOnConsolidationMawb);
		}

		public void TestUseOnHouseBillOfLading()
		{
			ExportStatementSetting.UseOnHouseBillOfLading = true;
			AssertEquals(true, DocExportStatementSetting.UseOnHouseBillOfLading);
			ExportStatementSetting.UseOnHouseBillOfLading = false;
			AssertEquals(false, DocExportStatementSetting.UseOnHouseBillOfLading);
		}

		public void TestUseOnDirectMasterBillOfLading()
		{
			ExportStatementSetting.UseOnDirectMasterBillOfLading = true;
			AssertEquals(true, DocExportStatementSetting.UseOnDirectMasterBillOfLading);
			ExportStatementSetting.UseOnDirectMasterBillOfLading = false;
			AssertEquals(false, DocExportStatementSetting.UseOnDirectMasterBillOfLading);
		}

		public void TestUseOnConsolidationMasterBillOfLading()
		{
			ExportStatementSetting.UseOnConsolidationMasterBillOfLading = true;
			AssertEquals(true, DocExportStatementSetting.UseOnConsolidationMasterBillOfLading);
			ExportStatementSetting.UseOnConsolidationMasterBillOfLading = false;
			AssertEquals(false, DocExportStatementSetting.UseOnConsolidationMasterBillOfLading);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CountryExportStatementSetting = new CountryExportStatementSetting();
			CountryExportStatementSetting.CountryCode = "AU";
			ExportStatementSetting = CountryExportStatementSetting.Statements.AddNew();
			DocExportStatementSetting = new DocExportStatementSetting(ExportStatementSetting);
		}

		CountryExportStatementSetting CountryExportStatementSetting;
		ExportStatementSetting ExportStatementSetting;
		DocExportStatementSetting DocExportStatementSetting;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocExportStatementSetting(ExportStatementSetting);
		}
	}
}
