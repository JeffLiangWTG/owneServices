using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZDateTimeStatusColumnTestInterface : ZDateTimeColumnTestInterface
	{
		public override void TestGetCustomValue()
		{
			BizObj.Z0_Date = ZDateTime.Empty;
			BizObj.Z0_Description = ZString.Empty;
			AssertEquals("GetCustomValue should return Empty", ZDateTime.Empty, TestColumn.GetCustomValue(BizObj));

			BizObj.Z0_Date = new ZDateTime(1983, 7, 10);
			AssertEquals("GetCustomValue should return 10/07/1983", new ZDateTime(1983, 7, 10), TestColumn.GetCustomValue(BizObj));

			BizObj.Z0_Date = new ZDateTime(1983, 7, 10);
			BizObj.Z0_Description = Constants.DateTimeStatus.Overdue;
			AssertEquals("GetCustomValue should return 10/07/1983", new ZDateTime(1983, 7, 10), TestColumn.GetCustomValue(BizObj));

			BizObj.Z0_Date = SuppressUtil.SuppressedDateTime;
			AssertEquals("GetCustomValue should return *SUPPRESSED*", SuppressUtil.SuppressedText, TestColumn.GetCustomValue(BizObj));
		}

		public void TestGetCustomColor()
		{
			BizObj.Z0_Date = ZDateTime.Empty;
			BizObj.Z0_Description = ZString.Empty;
			AssertEquals("GetCustomColor should return White Color", Color.White, TestColumn.GetCustomColor(BizObj));

			BizObj.Z0_Date = new ZDateTime(1983, 7, 10);
			AssertEquals("GetCustomColor should return White Color", Color.White, TestColumn.GetCustomColor(BizObj));

			BizObj.Z0_Description = Constants.DateTimeStatus.Overdue;
			AssertEquals("GetCustomColor should return Color", Color.FromArgb(255, 182, 193), TestColumn.GetCustomColor(BizObj));

			BizObj.Z0_Description = Constants.DateTimeStatus.Late;
			AssertEquals("GetCustomColor should return Color", Color.FromArgb(255, 204, 153), TestColumn.GetCustomColor(BizObj));

			BizObj.Z0_Description = Constants.DateTimeStatus.OnTime;
			AssertEquals("GetCustomValue should return Color", Color.FromArgb(152, 252, 142), TestColumn.GetCustomColor(BizObj));
		}

		new ZDateTimeStatusColumn TestColumn
		{
			get { return (ZDateTimeStatusColumn)base.TestColumn; }
		}

		protected override ZDateTimeColumn GetTestColumn()
		{
			return new ZDateTimeStatusColumn("TestColumn", DummyBusinessObject.Schema.Z0_Date, DummyBusinessObject.Schema.Z0_Description, ZDateTimePickerFormat.Short);
		}
	}
}
