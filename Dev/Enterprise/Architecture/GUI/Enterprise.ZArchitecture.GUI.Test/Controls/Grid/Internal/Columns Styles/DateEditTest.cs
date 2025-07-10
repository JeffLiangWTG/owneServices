using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.ZArchitecture
{
	sealed class DateEditTest : NotificationInGridTestCase
	{
		public override void TestLocationAndSizeWithAndWithoutNotifications()
		{
			CheckSizeAndLocation(50, new Point(35, 19), new Size(70, 16));

			Dummy.Collection[0].Z0_DateInfo.AddError("baad");
			CheckSizeAndLocation(50, new Point(47, 19), new Size(58, 16));
		}

		public void TestNullIsReturnedAsBlankString()
		{
			var info = new ZDateEditColumnStyleInfo();
			using (var columnStyle = new ZDateEditColumnStyle(info))
			{
				AssertEquals("", columnStyle.ZDateTimeToString(null));
			}
		}

		protected override string GetColumnName()
		{
			return AutoDummyBizo.Schema.Z0_Date;
		}

		protected override ZGridColumnInfo GetInfo()
		{
			return new ZDateEditColumnStyleInfo();
		}
	}
}
