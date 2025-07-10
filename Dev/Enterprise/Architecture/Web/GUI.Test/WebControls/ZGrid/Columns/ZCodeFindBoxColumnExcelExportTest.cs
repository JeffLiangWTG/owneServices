using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Excel;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZCodeFindBoxColumnExcelExportTest : TestCaseWithFactory
	{
		public void TestGetValue()
		{
			DummyEnterpriseBusinessObject testBizOWithLookup = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			testBizOWithLookup.Z0_Description = "YYY";

			DummyBusinessObject child1 = testBizOWithLookup.Lookups.DummyList.AddNew();
			child1.Z0_Code = "XXX";
			child1.Z0_Description = "Description1";

			DummyBusinessObject child2 = testBizOWithLookup.Lookups.DummyList.AddNew();
			child2.Z0_Code = "YYY";
			child2.Z0_Description = "Description2";

			AssertEquals("Lookup list should have two objects", 2, testBizOWithLookup.Lookups.DummyList.Count); //2DummyLookup + 2 new lookups(XXX and YYY)

			ZCodeFindBoxColumn testColumn = new ZCodeFindBoxColumn("Test Column", DummyBusinessObject.Schema.Z0_Description, "Lookups.DummyList");
			AssertEquals("BindToList should be Lookups.DummyList", "Lookups.DummyList", testColumn.BindToList);

			testColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AssertEquals("GetCustomValue when DescriptionOnly", "Description2", ((IExcelExportCustomValue)testColumn).GetCustomValue(testBizOWithLookup));

			testColumn.DisplayStyle = OComboBoxDropDownStyle.CodeOnly;
			AssertEquals("GetCustomValue when CodeOnly", "YYY", ((IExcelExportCustomValue)testColumn).GetCustomValue(testBizOWithLookup));

			testColumn.DisplayStyle = OComboBoxDropDownStyle.CodeAndDescription;
			AssertEquals("GetCustomValue when CodeAndDescription", "Description2 (YYY)", ((IExcelExportCustomValue)testColumn).GetCustomValue(testBizOWithLookup));

			testBizOWithLookup.Z0_Description = "XXX";
			AssertEquals("GetCustomValue should be Description1 (XXX)", "Description1 (XXX)", ((IExcelExportCustomValue)testColumn).GetCustomValue(testBizOWithLookup));

			testBizOWithLookup.Z0_Description = "ZZZ";

			testColumn.DisplayStyle = OComboBoxDropDownStyle.CodeOnly;
			AssertEquals("GetCustomValue when BizO is not found in a list by Code", ZString.Empty, ((IExcelExportCustomValue)testColumn).GetCustomValue(testBizOWithLookup));

			testColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AssertEquals("GetCustomValue when BizO is not found in a list by Code", ZString.Empty, ((IExcelExportCustomValue)testColumn).GetCustomValue(testBizOWithLookup));

			testColumn.DisplayStyle = OComboBoxDropDownStyle.CodeAndDescription;
			AssertEquals("GetCustomValue when BizO is not found in a list by Code", ZString.Empty, ((IExcelExportCustomValue)testColumn).GetCustomValue(testBizOWithLookup));

			ZCodeFindBoxColumn testColumnWithoutBindToList = new ZCodeFindBoxColumn("Test Column2", DummyBusinessObject.Schema.Z0_Code);
			DummyBusinessObject testBizOWithoutLookup = Factory.NewWithValidTestData<DummyBusinessObject>();
			testBizOWithoutLookup.Z0_Code = "BBB";
			AssertEquals("GetCustomValue should be BBB", "BBB", ((IExcelExportCustomValue)testColumnWithoutBindToList).GetCustomValue(testBizOWithoutLookup));
		}

		public void TestGetDescription()
		{
			ZCodeFindBoxColumn testColumn = new ZCodeFindBoxColumn("Test Column", "Test");
			AssertEquals("GetDescription should be Test Column", "Test Column", ((IExcelExportCustomValue)testColumn).GetDescription());
			testColumn.HeaderText = "Test";
			AssertEquals("GetDescription should be Test", "Test", ((IExcelExportCustomValue)testColumn).GetDescription());
		}

		public void TestGetValueFormat()
		{
			ZCodeFindBoxColumn testColumn = new ZCodeFindBoxColumn("Test Column", "Test");
			AssertEquals("GetValueFormat should be Test Column", "", ((IExcelExportCustomValue)testColumn).GetValueFormat(new ZString("test")));
		}
	}
}
