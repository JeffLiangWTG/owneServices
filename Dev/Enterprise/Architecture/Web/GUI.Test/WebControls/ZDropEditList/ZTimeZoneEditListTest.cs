using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZTimeZoneEditListTest : TestCaseWithFactory
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

		ZTimezoneEditList TimezoneEditList
		{
			get
			{
				if (timezoneEditList == null)
				{
					timezoneEditList = new ZTimezoneEditList(null);
				}
				return timezoneEditList;
			}
		}

		ZTimezoneEditList timezoneEditList;

		ZTimezoneEditList TimezoneEditListWithRelatedControl
		{
			get
			{
				if (timezoneEditListWithRelatedControl == null)
				{
					timezoneEditListWithRelatedControl = new ZTimezoneEditList(RelatedControl);
					RelatedControl.DateTimeFormat = ZDateTimePickerFormat.IncludingTimeZone;
				}
				return timezoneEditListWithRelatedControl;
			}
		}

		ZTimezoneEditList timezoneEditListWithRelatedControl;

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

		protected override void SetUp()
		{
			base.SetUp();
			TimezoneEditList.Page = Page;
			TimezoneEditList.OnPreRenderInternal(EventArgs.Empty);
		}

		#endregion

		public void TestListContents()
		{
			IBusinessObjectCollection contents = (IBusinessObjectCollection)TimezoneEditList.ListBoxControl.DataSource;
			AssertEquals("List should contain 48 items", 25, contents.Count);
			for (int i = 12; i >= -12; i--)
			{
				if (i >= 0)
				{
					AssertEquals(String.Format((NoResString)"GMT +{0:D2}:00", i), ((ZTimezoneEditList.TimezoneEntry)contents[12 - i]).Timezone);
				}
				else
				{
					AssertEquals(String.Format((NoResString)"GMT {0:D2}:00", i), ((ZTimezoneEditList.TimezoneEntry)contents[12 - i]).Timezone);
				}
			}
		}

		#region TestSelectedValue

		public void TestSelectedValue()
		{
			AssertEquals(null, TimezoneEditList.SelectedValue);
			AssertEquals("Control should contain empty value by default", "", TimezoneEditList.TextBoxControl.Text);
		}

		#endregion

		#region TestSetSelectedValueUpdate

		public void TestSetSelectedValueUpdate()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			RelatedControl.BindTo = DummyBizoSchema.Z0_DateTimeOffset.Name;
			dummy.Z0_DateTimeOffset = ZDateTimeOffset.Empty;
			RelatedControl.Bind(dummy);

			AssertEquals(new ZDateTimeOffset(), TimezoneEditListWithRelatedControl.SelectedValue);
			TimezoneEditListWithRelatedControl.SelectedValue = new ZString("GMT +11:00");
			AssertEquals("GMT +11:00", TimezoneEditListWithRelatedControl.TextBoxControl.Text);
			ZDateTime today = ZDateTime.Today;
			AssertEquals(new ZDateTimeOffset(today.Year, today.Month, today.Day, 0, 0, 0, new TimeSpan(11, 0, 0)), TimezoneEditListWithRelatedControl.SelectedValue);
			TimezoneEditListWithRelatedControl.SelectedValue = new ZDateTimeOffset(1, 1, 1, 13, 13, 0, new TimeSpan(9, 0, 0));
			AssertEquals("GMT +09:00", TimezoneEditListWithRelatedControl.TextBoxControl.Text);
			AssertEquals(new ZDateTimeOffset(today.Year, today.Month, today.Day, 0, 0, 0, new TimeSpan(9, 0, 0)), TimezoneEditListWithRelatedControl.SelectedValue);
		}

		#endregion

	}
}
