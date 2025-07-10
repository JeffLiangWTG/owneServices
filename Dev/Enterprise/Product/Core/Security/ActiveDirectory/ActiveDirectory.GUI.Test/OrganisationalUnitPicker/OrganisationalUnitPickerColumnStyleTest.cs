using CargoWise.EntityFramework.Testing;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	class OrganisationalUnitPickerColumnStyleTest : TestCaseWithDummy
	{
		public void TestGridFindBoxType()
		{
			using (var columnStyle = new OrganisationalUnitPickerColumnStyle(new OrganisationalUnitPickerColumnStyleInfo()))
			using (var gridFindBox = columnStyle.EditControl as OrganisationalUnitPickerFindBox)
			{
				AssertNotNull("The GridFindBox should be of type OrganisationalUnitPickerFindBox", gridFindBox);
			}
		}

		public void TestColumnStyleType()
		{
			var columnStyleInfo = new OrganisationalUnitPickerColumnStyleInfo();
			AssertEquals(typeof(OrganisationalUnitPickerColumnStyle), columnStyleInfo.ColumnStyleType);
		}
	}
}
