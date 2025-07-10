using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class NotificationInGridTest : NotificationInGridTestCase
	{
		public override void TestLocationAndSizeWithAndWithoutNotifications()
		{
			CheckSizeAndLocation(50, new Point(37, 21), new Size(274, 198));

			Dummy.Collection[0].Z0_DescriptionInfo.AddError("baad");
			CheckSizeAndLocation(50, new Point(49, 21), new Size(262, 198));
		}

		protected override string GetColumnName()
		{
			return DummyBusinessObject.Schema.Z0_Description;
		}

		protected override ZGridColumnInfo GetInfo()
		{
			return new ZMultiLineTextBoxColumnInfo();
		}
	}
}
