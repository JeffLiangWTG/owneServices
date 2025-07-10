using System.Drawing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZTimeLineColumnTestInterface : ZDateTimeColumnTestInterface
	{
		public override void TestGetCustomValue()
		{
			BizObj.Z0_Date = ZDateTime.Empty;
			BizObj.Z0_AnotherDate = ZDateTime.Empty;

			AssertEquals("GetCustomValue should return Empty", ZDateTime.Empty, TestColumn.GetCustomValue(BizObj));

			BizObj.Z0_Date = new ZDateTime(1983, 7, 10);

			AssertEquals("GetCustomValue should return 10/07/1983", new ZDateTime(1983, 7, 10), TestColumn.GetCustomValue(BizObj));

			BizObj.Z0_Date = new ZDateTime(1983, 7, 10);
			BizObj.Z0_AnotherDate = new ZDateTime(1963, 8, 1);

			AssertEquals("GetCustomValue should return 10/07/1983", new ZDateTime(1983, 7, 10), TestColumn.GetCustomValue(BizObj));

			BizObj.Z0_Date = ZDateTime.Empty;

			AssertEquals("GetCustomValue should return 1/8/1963", new ZDateTime(1963, 8, 1), TestColumn.GetCustomValue(BizObj));

			BizObj.Z0_Date = SuppressUtil.SuppressedDateTime;

			AssertEquals("GetCustomValue should return *SUPPRESSED*", SuppressUtil.SuppressedText, TestColumn.GetCustomValue(BizObj));
		}

		public void TestGetCustomColor()
		{
			BizObj.Z0_Date = ZDateTime.Empty;
			BizObj.Z0_AnotherDate = ZDateTime.Empty;

			AssertEquals("GetCustomColor should return White Color", Color.White, TestColumn.GetCustomColor(BizObj));

			BizObj.Z0_AnotherDate = new ZDateTime(1983, 7, 10);

			AssertEquals("GetCustomValue should return Grey Color (Overdue)", Color.LightGray, TestColumn.GetCustomColor(BizObj));

			BizObj.Z0_Date = ZDateTime.Today;
			BizObj.Z0_AnotherDate = new ZDateTime(1963, 8, 1);

			AssertEquals("GetCustomValue should return Color (Completed Late)", Color.Pink, TestColumn.GetCustomColor(BizObj));

			BizObj.Z0_AnotherDate = ZDateTime.Today;

			AssertEquals("GetCustomValue should return Color (Completed)", Color.FromArgb(255, 204, 204, 255), TestColumn.GetCustomColor(BizObj));

			BizObj.Z0_AnotherDate = ZDateTime.Empty;

			AssertEquals("GetCustomValue should return Color (Completed)", Color.FromArgb(255, 204, 204, 255), TestColumn.GetCustomColor(BizObj));

			BizObj.Z0_Date = ZDateTime.Empty;
			BizObj.Z0_AnotherDate = ZDateTime.Now.AddDays(1);

			AssertEquals("GetCustomValue should return Color (Pending)", Color.FromArgb(255, 204, 255, 204), TestColumn.GetCustomColor(BizObj));
		}

		public void TestGetComment()
		{
			BizObj.Z0_Date = ZDateTime.Empty;
			BizObj.Z0_AnotherDate = ZDateTime.Empty;

			AssertEquals("GetCustomComment", "Estimated time not set", TestColumn.GetComment(BizObj).ToString());

			BizObj.Z0_AnotherDate = new ZDateTime(1983, 7, 10);

			AssertEquals("GetCustomComment",
@"Estimated: 10-Jul-83
Overdue", TestColumn.GetComment(BizObj).ToString());

			BizObj.Z0_Date = new ZDateTime(1983, 7, 10);
			BizObj.Z0_AnotherDate = new ZDateTime(1963, 8, 1);

			AssertEquals("GetCustomComment",
@"Estimated: 01-Aug-63
Actual: 10-Jul-83
Completed Late", TestColumn.GetComment(BizObj).ToString());

			BizObj.Z0_AnotherDate = new ZDateTime(1983, 7, 10);

			AssertEquals("GetCustomComment",
@"Estimated: 10-Jul-83
Actual: 10-Jul-83
Completed", TestColumn.GetComment(BizObj).ToString());

			BizObj.Z0_AnotherDate = ZDateTime.Empty;

			AssertEquals("GetCustomComment",
@"Estimated time not set
Actual: 10-Jul-83
Completed", TestColumn.GetComment(BizObj).ToString());

			BizObj.Z0_Date = new ZDateTime(1963, 8, 1);
			BizObj.Z0_AnotherDate = new ZDateTime(1983, 7, 10);

			AssertEquals("GetCustomComment",
@"Estimated: 10-Jul-83
Actual: 01-Aug-63
Completed", TestColumn.GetComment(BizObj).ToString());
		}

		new ZTimelineColumn TestColumn
		{
			get { return (ZTimelineColumn)base.TestColumn; }
		}

		protected override ZDateTimeColumn GetTestColumn()
		{
			return new ZTimelineColumn("TestColumn", DummyBusinessObject.Schema.Z0_Date, DummyBusinessObject.Schema.Z0_AnotherDate, ZDateTimePickerFormat.Short);
		}
	}
}
