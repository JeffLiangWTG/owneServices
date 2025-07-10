using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZDateTimeColumnTestInterface : TestCaseWithFactory
	{
		public void TestDateTimeFormat()
		{
			AssertEquals("Default value", ZDateTimePickerFormat.Short, new ZDateTimeColumn(string.Empty, string.Empty).DateTimeFormat);
		}

		public virtual void TestGetCustomValue()
		{
			BizObj.Z0_Date = ZDateTime.Empty;

			AssertEquals("GetCustomValue should return Empty", ZDateTime.Empty, TestColumn.GetCustomValue(BizObj));

			BizObj.Z0_Date = new ZDateTime(1983, 7, 10);

			AssertEquals("GetCustomValue should return 10/07/1983", new ZDateTime(1983, 7, 10), TestColumn.GetCustomValue(BizObj));

			BizObj.Z0_Date = SuppressUtil.SuppressedDateTime;

			AssertEquals("GetCustomValue should return *SUPPRESSED*", SuppressUtil.SuppressedText, TestColumn.GetCustomValue(BizObj));
		}

		public void TestGetValueFormat()
		{
			BizObj.Z0_Date = new ZDateTime(1983, 7, 10);

			IZType testvalue = TestColumn.GetCustomValue(BizObj);

			AssertEquals("GetValueFormat should return ShortDateFormat", ZDateTime.ShortDateFormat, TestColumn.GetValueFormat(testvalue));

			TestColumn.DateTimeFormat = ZDateTimePickerFormat.Time;
			AssertEquals("GetValueFormat should return ShortTimeFormat", ZDateTime.ShortTimeFormat, TestColumn.GetValueFormat(testvalue));

			TestColumn.DateTimeFormat = ZDateTimePickerFormat.Long;
			AssertEquals("GetValueFormat should return LongTimeFormat", ZDateTime.LongTimeFormat, TestColumn.GetValueFormat(testvalue));

			TestColumn.DateTimeFormat = ZDateTimePickerFormat.TimeUpTo999HoursAnd45Minutes;
			AssertEquals("GetValueFormat should return empty string", "", TestColumn.GetValueFormat(testvalue));

			TestColumn.DateTimeFormat = ZDateTimePickerFormat.Custom;
			AssertEquals("GetValueFormat should return LongTimeFormat", ZDateTime.LongTimeFormat, TestColumn.GetValueFormat(testvalue));

			AssertEquals("GetValueFormat should return empty string for suppressed DateTime", "", TestColumn.GetValueFormat(SuppressUtil.SuppressedDateTime));
		}

		public void TestGetDescription()
		{
			AssertEquals("GetDescription should return TestColumn", "TestColumn", TestColumn.GetDescription());
		}

		protected ZDateTimeColumn TestColumn
		{
			get
			{
				if (fTestColumn == null)
				{
					fTestColumn = GetTestColumn();
				}
				return fTestColumn;
			}
		}
		ZDateTimeColumn fTestColumn;

		protected virtual ZDateTimeColumn GetTestColumn()
		{
			return new ZDateTimeColumn("TestColumn", DummyBusinessObject.Schema.Z0_Date, ZDateTimePickerFormat.Short);
		}

		protected DummyBusinessObject BizObj
		{
			get
			{
				if (fBizObj == null)
				{
					fBizObj = Factory.New<DummyBusinessObject>();
				}
				return fBizObj;
			}
		}

		DummyBusinessObject fBizObj;
	}
}
