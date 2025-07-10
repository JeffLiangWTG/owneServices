using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.HRMS
{
	public class RemoveWorkTimeAndStaffHolidayWithInvalidParentTableCodes : DataTransformation
	{
		public override string UserDescription => "Remove WorkTime and StaffHoliday with invalid parent table codes";

		protected override void OfflinePostUpgradeTransform()
			=> Db.Connection.ExecuteNonQuery(@"
			DELETE GlbStaffHoliday WHERE GA_ParentTableCode NOT IN ('GS', 'FC', 'GD', '');
			DELETE GlbWorkTime WHERE GW_ParentTableCode NOT IN ('GE', 'GS', 'GWP', 'GWT', 'WW', 'GD');");
	}
}
