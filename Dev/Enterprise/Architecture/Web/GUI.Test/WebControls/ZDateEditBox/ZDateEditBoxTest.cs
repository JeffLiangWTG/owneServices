using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZDateEditBoxTest : ZTextIFramePopupTest
	{
		public override void TestPopupDimension()
		{
			AssertEquals("Width", Unit.Pixel(190), DateEditBox.PopupWidthInternal);
			AssertEquals("Height", Unit.Pixel(175), DateEditBox.PopupHeightInternal);
		}

		public override void TestControlDimension()
		{
			AssertEquals(DateEditBox.ControlHeightInternal.Value, DateEditBox.Height.Value);
			AssertEquals((double)90, DateEditBox.MinWidthInternal.Value);
			AssertEquals((double)90, DateEditBox.Width.Value);
		}

		public override void TestAdditionalButtonClickHandler()
		{
			AssertEquals("ZDateEdit_ShowCalendar();", DateEditBox.AdditionalButtonClickHandlerInternal);
		}

		public void TestDateValidateHandler()
		{
			AssertEquals(string.Format("if (NormalizeAndValidateDate('{0}','{1}')) ZDateEdit_SetTime('{2}','00:00'); else ZDateEdit_SetTime('{2}','');", DateEditBox.TextBoxControl.ClientID, string.Empty, DateEditBox.TimeEditListControl.TextBoxClientID), DateEditBox.DateValidateHandlerInternal);
			using (TestPage page = new TestPage())
			{
				page.Controls.Add(DateEditBox);

				page.ExternalServicesSupported = false;
				AssertEquals(string.Format("if (NormalizeAndValidateDate('{0}','{1}')) ZDateEdit_SetTime('{2}','00:00'); else ZDateEdit_SetTime('{2}','');", DateEditBox.TextBoxControl.ClientID, string.Empty, DateEditBox.TimeEditListControl.TextBoxClientID), DateEditBox.DateValidateHandlerInternal);

				page.ExternalServicesSupported = true;
				AssertEquals(string.Format("FormatDate('{0}', '{1}', '{2}'); ZDateEdit_SetDefaultTimezone('{3}', '{0}');", DateEditBox.TextBoxControl.ClientID, DateEditBox.TimeEditListControl.TextBoxClientID, DateEditBox.DateTimeFormat.ToString(), DateEditBox.TimeZoneEditListControl.TextBoxClientID), DateEditBox.DateValidateHandlerInternal);
			}
		}

		public void TestDateValidatesIntoCorrectSmallDateTime()
		{
			using (TestPage page = new TestPage())
			{
				var year = ZDateTime.Now.Year;

				page.Controls.Add(DateEditBox);
				page.ExternalServicesSupported = false;
				AssertEquals("Prerequisite:", string.Format("if (NormalizeAndValidateDate('{0}','{1}')) ZDateEdit_SetTime('{2}','00:00'); else ZDateEdit_SetTime('{2}','');", DateEditBox.TextBoxControl.ClientID, string.Empty, DateEditBox.TimeEditListControl.TextBoxClientID), DateEditBox.DateValidateHandlerInternal);
				DateEditBox.TextBoxControl.Text = "4/30";
				AssertEquals(new ZDateTime(year, 4, 30, 0, 0, 0), DateEditBox.SelectedValue);
				DateEditBox.TextBoxControl.Text = "103";
				AssertEquals(new ZDateTime(year, 3, 1, 0, 0, 0), DateEditBox.SelectedValue);
			}
		}

		class TestPage : ZPage
		{
			protected override bool GetExternalServicesSupported()
			{
				return externalServicesSuported;
			}

			public new bool ExternalServicesSupported
			{
				get { return base.ExternalServicesSupported; }
				set { externalServicesSuported = value; }
			}

			bool externalServicesSuported;
		}

		protected override void AssertChildControlsNameAndType()
		{
			AssertEquals(2, TextBoxButton.Controls.Count);
			AssertEquals(typeof(ZTextBox), TextBoxButton.Controls[0].GetType());
			AssertEquals(typeof(HtmlInputButton), TextBoxButton.Controls[1].GetType());
			AssertEquals("TextBox", TextBoxButton.Controls[0].ID);
		}

		public void TestAlign()
		{
			DateEditBox.OnPreRenderInternal(EventArgs.Empty);
			AssertEquals("Align should be left", DateEditBox.Attributes["align"], "left");
		}

		public void TestOnChangeEventHandler()
		{
			DateEditBox.OnPreRenderInternal(EventArgs.Empty);
			AssertEquals("onchange event handler should be assigned", DateEditBox.DateValidateHandlerInternal, DateEditBox.TextBoxControl.Attributes["onchange"]);
		}

		public void TestMaxLength()
		{
			AssertEquals(ZDateTime.ShortDateFormat.Length, DateEditBox.MaxLengthInternal);
		}

		public void TestBindToZDateTime()
		{
			TextBoxButton.BindTo = "Z0_Date";
			Page.Controls.Add(TextBoxButton);
			TextBoxButton.Bind(TestBizO);
			AssertEquals(TestBizO.Z0_Date, DateEditBox.SelectedValue);
		}

		public void TestBindToZDateTimeOffset_WhileHasChanges()
		{
			var source = new DummyBO();
			var date = new ZDateTime(2024, 1, 5, 13, 42, 20);
			DateEditBox.SelectedValue = date;
			DateEditBox.HasChanges = true;
			AssertEquals(true, DateEditBox.HasChanges);
			DateEditBox.BindTo = "DateOffsetField";
			DateEditBox.Bind(source);
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2024, 1, 5, 0, 0, 0)), source.DateOffsetField);
			AssertEquals(false, DateEditBox.HasChanges);
		}

		public override void TestAssignSelectedValueToInvalidZType()
		{
			DateEditBox.SelectedValue = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, DateEditBox.SelectedValue);
			AssertEquals(ZDateTime.Today.ToString(ZDateTime.ShortDateFormat, ObjectCache.CultureProvider.Culture), DateEditBox.TextBoxControl.Text);

			TextBoxButton.SelectedValue = new ZString("test");
			AssertEquals(ZDateTime.Invalid, DateEditBox.SelectedValue);
			AssertEquals("test", DateEditBox.TextBoxControl.Text);
		}

		public void TestAssignSelectedTextToInvalidDate()
		{
			DateEditBox.TextBoxControl.Text = "30/2/2004";
			AssertEquals(ZDateTime.Invalid, DateEditBox.SelectedValue);

			DateEditBox.TextBoxControl.Text = "31/9/2004";
			AssertEquals(ZDateTime.Invalid, DateEditBox.SelectedValue);
		}

		public void TestAssignSelectedTextToChineseDateForamt()
		{
			DateEditBox.DateTimeFormat = ZDateTimePickerFormat.Long;
			DateEditBox.TextBoxControl.Text = "25-SEP-14";
			DateEditBox.TimeEditListControl.TextBoxControl.Text = "01:23";
			AssertEquals(new ZDateTime(2014, 9, 25, 1, 23, 0), DateEditBox.SelectedValue);

			ObjectFactory.Get<IResourceStrings>().CurrentLanguage = Enterprise.Core.SharedConstants.Languages.ChineseSimplified;
			AssertEquals(new ZDateTime(2014, 9, 25, 1, 23, 0), DateEditBox.SelectedValue);
		}

		public void TestSelectedValue_BoundToDateProperty()
		{
			var source = new DummyBO();

			DateEditBox.BindTo = "DateField";
			DateEditBox.Bind(source);
			DateEditBox.SelectedValue = new ZDateTime(2018, 1, 16, 15, 28, 10);

			AssertEquals("The Type of the selected value should match the type of the bound property", typeof(ZDate), DateEditBox.SelectedValue.GetType());
		}

		public void TestTextAndSelectedValue_BoundToDateTimeOffsetProperty()
		{
			var source = new DummyBO();

			DateEditBox.BindTo = "DateOffsetField";
			DateEditBox.Bind(source);

			var date = new ZDateTime(2018, 1, 16, 15, 28, 10);
			DateEditBox.SelectedValue = date;
			AssertEquals(new ZDateTimeOffset(new ZDateTime(2018, 1, 16, 0, 0, 0)), DateEditBox.SelectedValue);
			AssertEquals("16-Jan-18", DateEditBox.Text);
			AssertEquals("15:28", DateEditBox.TimeEditListControl.Text);

			DateEditBox.Text = "17-01-18";
			DateEditBox.TimeEditListControl.TextBoxControl.Text = "16:30";
			DateEditBox.TimeZoneEditListControl.TextBoxControl.Text = "GMT +11:00";
			AssertEquals(new ZDateTimeOffset(new DateTime(2018, 1, 17), new TimeSpan(11, 0, 0)), DateEditBox.SelectedValue);
			AssertEquals("17-01-18", DateEditBox.Text);
			AssertEquals("16:30", DateEditBox.TimeEditListControl.Text);
			AssertEquals("GMT +11:00", DateEditBox.TimeZoneEditListControl.Text);

			DateEditBox.SelectedValue = ZDateTime.Empty;
			AssertEquals(ZDateTimeOffset.Empty, DateEditBox.SelectedValue);
			AssertEquals(string.Empty, DateEditBox.Text);
			AssertEquals("", DateEditBox.TimeEditListControl.Text);

			DateEditBox.SelectedValue = ZDateTime.Invalid;
			AssertEquals(ZDateTimeOffset.Invalid, DateEditBox.SelectedValue);
			AssertEquals("Invalid", DateEditBox.Text);
			AssertEquals("Invalid", DateEditBox.TimeEditListControl.Text);
		}

		internal class DummyBO : NonPersistentBusinessObject
		{
			public ZDate DateField { get; set; }

			public ZPropertyInfo DateFieldInfo => GetZPropertyInfo(nameof(DateField));

			public ZDateTimeOffset DateOffsetField { get; set; }

			public ZPropertyInfo DateOffsetFieldInfo
			{
				get { return GetZPropertyInfo(nameof(DateOffsetField)); }
			}
		}

		public virtual void TestGetSelectedValue()
		{
			ZDateTime today = ZDateTime.Today;
			ZDateTime now = ZDateTime.Now;
			now = now.AddSeconds(-now.Second);

			AssertEquals("Default Value", ZDateTime.Empty, DateEditBox.SelectedValue);

			AssertEquals("Default DateTimeFormat", ZDateTimePickerFormat.Short, DateEditBox.DateTimeFormat);
			DateEditBox.SelectedValue = today;
			AssertEquals("Value", today, DateEditBox.SelectedValue);
			AssertEquals("Text", today.ToString(ZDateTime.ShortDateFormat, ObjectCache.CultureProvider.Culture), DateEditBox.TextBoxControl.Text);

			DateEditBox.SelectedValue = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, DateEditBox.SelectedValue);
			AssertEquals("", DateEditBox.TextBoxControl.Text);

			DateEditBox.DateTimeFormat = ZDateTimePickerFormat.Long;
			AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Long, DateEditBox.DateTimeFormat);
			DateEditBox.SelectedValue = now;
			AssertEquals("LongDateTimeFormat Text", now.ToString(ZDateTime.LongTimeFormat, ObjectCache.CultureProvider.Culture), DateEditBox.TextBoxControl.Text + " " + DateEditBox.TimeEditListControl.TextBoxControl.Text);

			DateEditBox.SelectedValue = ZDateTime.Invalid;
			AssertEquals(ZDateTime.Invalid, DateEditBox.SelectedValue);
			AssertEquals("Invalid", DateEditBox.TextBoxControl.Text);
		}

		public void TestForeignLongDateParsing()
		{
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.Swedish))
			{
				DateEditBox.DateTimeFormat = ZDateTimePickerFormat.Long;
				DateEditBox.TextBoxControl.Text = "24-Aug-18";
				DateEditBox.TimeEditListControl.TextBoxControl.Text = "1:00";
				var expectedDate = new ZDateTime(2018, 8, 24, 1, 0, 0);
				AssertEquals(expectedDate, DateEditBox.SelectedValue);
			}
		}

		public void TestDateParsingWhenCompanyCountryDateFormatIsDifferent()
		{
			var unitedStatesCompany = Factory.New<IGlbCompany>();
			unitedStatesCompany.SetCountry("US");
			((BusinessObject)unitedStatesCompany)[GlbCompanySchema.GC_Code] = "USA";

			var branch = Factory.New<IGlbBranch>();
			branch.GB_GC = unitedStatesCompany.PK;
			((BusinessObject)branch)[GlbBranchSchema.GB_Code] = "USA";
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			{
				DateEditBox.DateTimeFormat = ZDateTimePickerFormat.Long;
				DateEditBox.TextBoxControl.Text = "24/10/18";
				DateEditBox.TimeEditListControl.TextBoxControl.Text = "1:00";
				var expectedDate = new ZDateTime(2018, 10, 24, 1, 0, 0);
				AssertEquals(expectedDate, DateEditBox.SelectedValue);
			}
		}

		public virtual void TestSetSelectedValue()
		{
			ZDateTime today = ZDateTime.Today;
			ZDateTime now = ZDateTime.Now;
			now = now.AddSeconds(-now.Second);

			AssertEquals("Default Value", ZDateTime.Empty, DateEditBox.SelectedValue);
			AssertEquals("Default Text", "", DateEditBox.TextBoxControl.Text);

			AssertEquals("Default DateTimeFormat", ZDateTimePickerFormat.Short, DateEditBox.DateTimeFormat);
			DateEditBox.TextBoxControl.Text = today.ToShortDateString();
			AssertEquals("Value", today, DateEditBox.SelectedValue);
			AssertEquals("Text", today.ToShortDateString(), DateEditBox.TextBoxControl.Text);

			DateEditBox.TextBoxControl.Text = "";
			AssertEquals(ZDateTime.Empty, DateEditBox.SelectedValue);
			AssertEquals("", DateEditBox.TextBoxControl.Text);

			DateEditBox.DateTimeFormat = ZDateTimePickerFormat.Long;
			AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Long, DateEditBox.DateTimeFormat);

			DateEditBox.SelectedValue = ZDateTime.Invalid;
			AssertEquals(ZDateTime.Invalid, DateEditBox.SelectedValue);
			AssertEquals("Invalid", DateEditBox.TextBoxControl.Text);

			DateEditBox.DateTimeFormat = ZDateTimePickerFormat.IncludingTimeZone;
			AssertEquals("DateTimeFormat", ZDateTimePickerFormat.IncludingTimeZone, DateEditBox.DateTimeFormat);
		}

		#region Implementation

		protected override Control GetNewControl()
		{
			return new ZDateEditBox();
		}

		ZDateEditBox DateEditBox
		{
			get
			{
				ZDateEditBox result = (ZDateEditBox)Control;
				if (result.TimeEditListControl == null)
				{
					result.TimeEditListControl = new ZTimeEditList(result);
				}
				if (result.TimeZoneEditListControl == null)
				{
					result.TimeZoneEditListControl = new ZTimezoneEditList(result);
				}
				return result;
			}
		}

		protected override Type ResourceContainerType
		{
			get { return typeof(ZDateEditBox); }
		}

		protected override string ExpectedButtonBackgroundStyle
		{
			get
			{
				return String.Format("url({0}) {1} {2} {2}", DateEditBox.ZDateEditButtonImage.FileName, "no-repeat", "center");
			}
		}

		protected override string[] ExpectedResourceNames
		{
			get { return new string[] { "calendar.js", "calendar-en.js", "calendar.css", "calendar.htm", "calopener.js", "ZDateEditButton.gif" }; }
		}

		protected override string ExpectedIFrameSourcePage
		{
			get { return "calendar.htm"; }
		}

		#endregion Implementation
	}
}
