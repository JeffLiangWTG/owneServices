using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZTimeEditListTest : TestCaseWithFactory
	{
		#region Implementation

		ZTestPage Page
		{
			get
			{
				if (fPage == null)
				{
					fPage = new ZTestPage();
					fPage.EnableEventValidation = false;
				}
				return fPage;
			}
		}
		ZTestPage fPage;

		ZTimeEditListForTest TimeEditList
		{
			get
			{
				if (timeEditList == null)
				{
					timeEditList = new ZTimeEditListForTest(null);
				}
				return timeEditList;
			}
		}

		ZTimeEditListForTest timeEditList;

		ZTimeEditListForTest TimeEditListWithRelatedControl
		{
			get
			{
				if (timeEditListWithRelatedControl == null)
				{
					timeEditListWithRelatedControl = new ZTimeEditListForTest(RelatedControl);
					RelatedControl.DateTimeFormat = ZDateTimePickerFormat.Time;
				}
				return timeEditListWithRelatedControl;
			}
		}

		ZTimeEditListForTest timeEditListWithRelatedControl;

		ZDateEditBox RelatedControl
		{
			get
			{
				if (relatedControl == null)
				{
					relatedControl = new ZDateEditBox();
				}
				return relatedControl;
			}
		}

		ZDateEditBox relatedControl;

		ZTimezoneEditList TimeZoneEditList
		{
			get
			{
				if (timezoneEditList == null)
				{
					timezoneEditList = new ZTimezoneEditList(RelatedControl);
				}
				return timezoneEditList;
			}
		}
		ZTimezoneEditList timezoneEditList;

		protected override void SetUp()
		{
			base.SetUp();
			TimeEditList.Page = Page;
			TimeEditList.OnPreRenderForTest();
		}

		#endregion

		#region TestListContents

		public void TestListContents()
		{
			IBusinessObjectCollection contents = (IBusinessObjectCollection)TimeEditList.ListBoxControlForTest.DataSource;
			AssertEquals("List should contain 48 items", 48, contents.Count);
			for (int i = 0; i < 24; i++)
			{
				string text = i.ToString().PadLeft(2, '0') + ":00";
				AssertEquals("Item number " + (i * 2), text, ((ZTimeEditList.TimeEntry)contents[i * 2]).Time);
				string text2 = text.Replace(":0", ":3");
				AssertEquals("Item number " + (i * 2 + 1), text2, ((ZTimeEditList.TimeEntry)contents[i * 2 + 1]).Time);
			}
		}

		public void TestTimeEntry()
		{
			AssertEquals("22:02", new ZTimeEditList.TimeEntry(22, 2).Time);
		}

		#endregion

		#region TestOnChangeEventHandler

		public void TestOnChangeEventHandler()
		{
			AssertEquals("onchange event handler should be assigned", TimeEditList.NormalizeAndValidateTimeHandlerForTest, TimeEditList.TextBoxControlForTest.Attributes["onchange"]);
			AssertEquals("if (NormalizeAndValidateTime('" + TimeEditList.TextBoxControl.ClientID + "')) { ZDropEditList_SelectItem_AdditionalHandler(''); }", TimeEditList.NormalizeAndValidateTimeHandlerForTest);
			AssertEquals("if (NormalizeAndValidateTime('" + TimeEditListWithRelatedControl.TextBoxControl.ClientID + "')) { ZDropEditList_SelectItem_AdditionalHandler('" + TimeEditListWithRelatedControl.RelatedControlIDForTest + "');ZDateTimeEdit_SetDefaultTimezone('" + TimeZoneEditList.TextBoxClientID + "'); }", TimeEditListWithRelatedControl.NormalizeAndValidateTimeHandlerForTest);
		}

		#endregion

		#region TestSelectedValue

		public void TestSelectedValue()
		{
			AssertEquals(null, TimeEditList.SelectedValue);
			AssertEquals("Control should contain empty value by default", "", TimeEditList.TextBoxControlForTest.Text);
		}

		#endregion

		#region TestSetSelectedValueUpdate

		public void TestSetSelectedValueUpdate()
		{
			AssertEquals(new ZDateTime(), TimeEditListWithRelatedControl.SelectedValue);
			TimeEditListWithRelatedControl.SelectedValue = new ZString("12:30");
			AssertEquals("12:30", TimeEditListWithRelatedControl.TextBoxControlForTest.Text);
			ZDateTime today = ZDateTime.Today;
			AssertEquals(new ZDateTime(today.Year, today.Month, today.Day, 12, 30, 0), TimeEditListWithRelatedControl.SelectedValue);
			TimeEditListWithRelatedControl.SelectedValue = new ZDateTime(1, 1, 1, 13, 13, 0);
			AssertEquals("13:13", TimeEditListWithRelatedControl.TextBoxControlForTest.Text);
			AssertEquals(new ZDateTime(today.Year, today.Month, today.Day, 13, 13, 0), TimeEditListWithRelatedControl.SelectedValue);
		}

		#endregion

		#region TestButtonBackgroundStyle

		public void TestButtonBackgroundStyle()
		{
			AssertEquals("url(" + TimeEditList.ZTimeEditListButtonImageForTest.FileName + ") no-repeat center center", TimeEditList.ButtonBackgroundStyleForTest);
		}

		#endregion

		public void TestResources()
		{
			AssertNotNull("Resources should be not equal to null", TimeEditList.Resources);
			Assert("Should contain at least 2 resources", TimeEditList.Resources.Count >= 2);
			AssertCollectionContains("ZTimeEditScriptBlock should be in Resources", TimeEditList.ZTimeEditListScriptFileForTest, TimeEditList.Resources);
			AssertCollectionContains("ZTimeEditListButton should be in Resources", TimeEditList.ZTimeEditListButtonImageForTest, TimeEditList.Resources);
		}

		public void TestText_WhenSelectedChanges_BoundToDateTimeOffsetProperty()
		{
			var source = new ZDateEditBoxTest.DummyBO();

			RelatedControl.BindTo = "DateOffsetField";
			RelatedControl.Bind(source);

			var date = new ZDateTime(2018, 1, 17, 14, 33, 10);
			TimeEditListWithRelatedControl.SelectedValue = date;
			AssertEquals("14:33", TimeEditListWithRelatedControl.Text);

			TimeEditListWithRelatedControl.SelectedValue = date;
			AssertEquals("14:33", TimeEditListWithRelatedControl.Text);

			TimeEditList.SelectedValue = date;
			AssertEquals("14:33", TimeEditList.Text);

			RelatedControl.SelectedValue = ZDateTime.Empty;
			AssertEquals(string.Empty, TimeEditListWithRelatedControl.Text);

			RelatedControl.SelectedValue = ZDateTime.Invalid;
			AssertEquals("Invalid", TimeEditListWithRelatedControl.Text);
		}

		public void TestSelected_WhenTextChanges_BoundToDateTimeOffsetProperty()
		{
			var source = new ZDateEditBoxTest.DummyBO();

			RelatedControl.BindTo = "DateOffsetField";
			RelatedControl.Bind(source);

			var today = ZDateTime.Today;
			TimeEditListWithRelatedControl.Text = "14:49";
			AssertEquals(new ZDateTime(today.Year, today.Month, today.Day, 14, 49, 0), ((ZDateTimeOffset)TimeEditListWithRelatedControl.SelectedValue).ToZDateTime());

			TimeEditList.Text = "14:49";
			AssertEquals(new ZDateTime(today.Year, today.Month, today.Day, 14, 49, 0), (ZDateTime)TimeEditList.SelectedValue);
		}
	}
}
