using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	public class IncidentManagementGroupFilterControlTest : TestCaseWithFactory
	{
		public void TestCustomFieldColumns()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "ING";
			var def1 = template1.GenCustomColumnDefinitions.AddNew();
			def1.XC_Name = "KnowsLawsOfAviation";
			def1.XC_Type = AddOnColumnDataType.Codes.Boolean;
			var def2 = template1.GenCustomColumnDefinitions.AddNew();
			def2.XC_Name = "Likes_Jazz";
			def2.XC_Type = AddOnColumnDataType.Codes.Boolean;
			var def3 = template1.GenCustomColumnDefinitions.AddNew();
			def3.XC_Name = "Hive_Role";
			def3.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();

			using (var filterControl = new IncidentManagementGroupFilterControl(new IncidentManagementGroupCollection(Factory), new IncidentManagementGroupFilterBusinessObject()))
			{
				var columns = filterControl.Grid.ColumnStyles;
				AssertHasColumn(columns, "KnowsLawsOfAviation");
				AssertHasColumn(columns, "Likes_Jazz");
				AssertHasColumn(columns, "Hive_Role");
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.Caption == nameOfColumn);
			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}
	}
}
