using System;
using System.Web.UI;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class DateEditTest : TestCaseWithFactory
	{
		class ZDateEditForTest : ZDateEdit
		{
			public void OnPreRenderForTesting() => OnPreRender(EventArgs.Empty);

			protected override void OnInit(EventArgs e)
			{
				base.OnInit(e);
			}
		}

		readonly ZDateEditForTest DateEditControl = new ZDateEditForTest();

		public void TestEnabledDisplayAttribute()
		{
			var dataSource = Factory.New<DummyBusinessObject>();
			DateEditControl.BindTo = DummyBizoSchema.Z0_Description.Name;
			DateEditControl.Bind(dataSource);

			dataSource.Z0_Description_ReadOnly = true;
			DateEditControl.CanBeEnabledByClient = false;
			DateEditControl.OnPreRenderForTesting();
			AssertEquals("inline", DateEditControl.Style[HtmlTextWriterStyle.Display]);
			AssertNull(DateEditControl.Attributes["enabled-display"]);

			dataSource.Z0_Description_ReadOnly = false;
			DateEditControl.CanBeEnabledByClient = true;
			DateEditControl.OnPreRenderForTesting();
			AssertEquals("inline", DateEditControl.Style[HtmlTextWriterStyle.Display]);
			AssertEquals("inline", DateEditControl.Attributes["enabled-display"]);

			dataSource.Z0_Description_ReadOnly = true;
			DateEditControl.OnPreRenderForTesting();
			AssertEquals("none", DateEditControl.Style[HtmlTextWriterStyle.Display]);
			AssertEquals("inline", DateEditControl.Attributes["enabled-display"]);
		}

		public void TestCanBeEnabledByClient()
		{
			DateEditControl.CanBeEnabledByClient = true;
			AssertEquals(true, DateEditControl.CanBeEnabledByClient);

			DateEditControl.CanBeEnabledByClient = false;
			AssertEquals(false, DateEditControl.CanBeEnabledByClient);
		}

		public void TestTimeEditListControlVisibility()
		{
			DateEditControl.DateTimeFormat = ZDateTimePickerFormat.Short;
			Assert("Time edit control shouldn't be visible when date format is Short", !DateEditControl.TimeBox.Visible);
			DateEditControl.DateTimeFormat = ZDateTimePickerFormat.Time;
			Assert("Time edit control should be visible when date format is Time", DateEditControl.TimeBox.Visible);
			DateEditControl.DateTimeFormat = ZDateTimePickerFormat.Long;
			Assert("Time edit control should be visible when date format is Long", DateEditControl.TimeBox.Visible);
		}

		public void TestBinding()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			DateEditControl.BindTo = DummyBizoSchema.Z0_DateTimeOffset.Name;
			dummy.Z0_DateTimeOffset = new ZDateTimeOffset(2008, 11, 21, 13, 40, 00, new TimeSpan(10, 0, 0));
			DateEditControl.Bind(dummy);

			DateEditControl.DateTimeFormat = ZDateTimePickerFormat.Time;
			AssertEquals("Time edit control should be visible when date format is Time", "21-Nov-08 13:40", DateEditControl.Text);
			DateEditControl.DateTimeFormat = ZDateTimePickerFormat.Short;
			AssertEquals("Time edit control shouldn't be visible when date format is Short", "21-Nov-08", DateEditControl.Text);
			DateEditControl.DateTimeFormat = ZDateTimePickerFormat.Long;
			AssertEquals("Time edit control should be visible when date format is Long", "21-Nov-08 13:40", DateEditControl.Text);
		
			DateEditControl.DateTimeFormat = ZDateTimePickerFormat.IncludingTimeZone;
			AssertEquals("Time edit control should be visible when date format is IncludingTimeZone", "21-Nov-08 13:40 GMT +10:00", DateEditControl.Text);

			DateEditControl.DateBox.Text = "22-Nov-08";
			DateEditControl.TimeBox.Text = "13:41";
			DateEditControl.TimezoneBox.Text = "GMT +11:00";
			DateEditControl.HasChanges = true;
			DateEditControl.Bind(dummy);

			AssertEquals(new ZDateTimeOffset(2008, 11, 22, 13, 41, 0, new TimeSpan(11, 0, 0)), dummy.Z0_DateTimeOffset);
		}
	}
}
