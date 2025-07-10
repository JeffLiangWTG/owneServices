using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	sealed class FieldFindBoxGridColumnTest : NotificationInGridTestCase
	{
		public override void TestLocationAndSizeWithAndWithoutNotifications()
		{
			CheckSizeAndLocation(50, new Point(35, 19), new Size(65, 16));

			Dummy.Collection[0].Z0_DescriptionInfo.AddError("baad");
			CheckSizeAndLocation(50, new Point(47, 19), new Size(53, 16));
		}

		#region Implementation

		protected override string GetColumnName()
		{
			return AutoDummyBizo.Schema.Z0_Description;
		}

		protected override ZGridColumnInfo GetInfo()
		{
			return new FieldFindBoxColumnStyleInfo();
		}

		#endregion
	}
}
