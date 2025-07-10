using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class MultiControlTest : NotificationInGridTestCase
	{
		public override void TestLocationAndSizeWithAndWithoutNotifications()
		{
			CheckSizeAndLocation(50, new Point(37, 21), new Size(47, 13));

			Dummy.Collection[0].Z0_DescriptionInfo.AddError("baad");
			CheckSizeAndLocation(50, new Point(49, 21), new Size(35, 13));
		}

		protected override string GetColumnName()
		{
			return AutoDummyBizo.Schema.Z0_Description;
		}

		protected override ZGridColumnInfo GetInfo()
		{
			var info = new ZMultiControlColumnStyleInfo();
			info.Caption = "Field Value";
			info.BindToList = "Collection";
			info.ModuleID = DummyModuleIDs.Dummy;
			info.FieldTypeColumnName = GetColumnName();
			return info;
		}

		protected override void SetUp()
		{
			base.SetUp();
			child.Z0_Description = nameof(FieldType.Text);
		}
	}
}
