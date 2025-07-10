using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	sealed class TimeEditTest : NotificationInGridTestCase
	{
		public override void TestLocationAndSizeWithAndWithoutNotifications()
		{
			CheckSizeAndLocation(50, new Point(37, 21), new Size(49, 13));

			Dummy.Collection[0].Z0_DateInfo.AddError("baad");
			CheckSizeAndLocation(50, new Point(49, 21), new Size(37, 13));
		}

		protected override string GetColumnName()
		{
			return DummyBusinessObject.Schema.Z0_Date;
		}

		protected override ZGridColumnInfo GetInfo()
		{
			return new ZTimeEditExColumnStyleInfo();
		}
	}
}
