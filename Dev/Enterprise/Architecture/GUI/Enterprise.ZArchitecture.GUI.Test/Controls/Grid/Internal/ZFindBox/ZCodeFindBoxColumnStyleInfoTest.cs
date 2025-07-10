using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCodeFindBoxColumnStyleInfoTest : NotificationInGridTestCase
	{
		public override void TestLocationAndSizeWithAndWithoutNotifications()
		{
			CheckSizeAndLocation(50, new Point(35, 19), new Size(68, 16));

			Dummy.Collection[0].Z0_DescriptionInfo.AddError("baad");
			CheckSizeAndLocation(50, new Point(47, 19), new Size(56, 16));
		}

		protected override string GetColumnName()
		{
			return DummyBusinessObject.Schema.Z0_Description;
		}

		protected override ZGridColumnInfo GetInfo()
		{
			var info = new ZCodeFindBoxColumnStyleInfo();
			info.BindToList = "Lookups+DummyList";
			return info;
		}
	}
}
