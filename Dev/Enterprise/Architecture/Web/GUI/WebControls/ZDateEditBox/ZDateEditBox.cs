using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.ServerServices;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// ZDateEditBox control to use on web forms
	/// </summary> 
	[DefaultProperty("Text"), ToolboxData("<{0}:ZDateEditBox runat=server></{0}:ZDateEditBox>"),
	Designer(typeof(Design.ZTextBoxButtonDesigner))]
	internal class ZDateEditBox : ZTextIFramePopup, IDateInputControl, IWebServiceMethodsCaller
	{
		public ZDateEditBox()
		{
			fLoadIFrameOnDemand = false;
		}

		#region Properties

		public ZTimezoneEditList TimeZoneEditListControl
		{
			get { return timeZoneEditListControl; }
			set
			{
				timeZoneEditListControl = value;
				timeZoneEditListControl.Visible = DateTimeFormat == ZDateTimePickerFormat.IncludingTimeZone;
			}
		}
		ZTimezoneEditList timeZoneEditListControl;

		public ZTimeEditList TimeEditListControl
		{
			get { return timeEditListControl; }
			set
			{
				timeEditListControl = value;
				timeEditListControl.Visible = DateTimeFormat != ZDateTimePickerFormat.Short;
			}
		}
		ZTimeEditList timeEditListControl;

		protected ZDateTimeLabel DateTimeLabel
		{
			get { return dateTimelabel ?? (dateTimelabel = new ZDateTimeLabel() { ID = "ReadOnly" }); }
		}
		ZDateTimeLabel dateTimelabel;

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(ZDateTimePickerFormat.Short)]
		public ZDateTimePickerFormat DateTimeFormat
		{
			get { return fDateTimeFormat; }
			set
			{
				fDateTimeFormat = value;
				if (TimeEditListControl != null)
				{
					TimeEditListControl.Visible = value != ZDateTimePickerFormat.Short;
				}

				if (TimeZoneEditListControl != null)
				{
					TimeZoneEditListControl.Visible = value == ZDateTimePickerFormat.IncludingTimeZone;
				}

				DateTimeLabel.DateTimeFormat = value;
			}
		}
		ZDateTimePickerFormat fDateTimeFormat = ZDateTimePickerFormat.Short;

		protected override string IFrameSourcePageName
		{
			get { return "calendar.htm"; }
		}

		protected override Unit PopupHeight
		{
			get { return Unit.Pixel(175); }
		}

		protected override Unit PopupWidth
		{
			get { return Unit.Pixel(190); }
		}

		protected override Unit ButtonWidth
		{
			get { return Unit.Pixel(18); }
		}

		public override Unit DefaultWidth
		{
			get { return Unit.Pixel(90); }
		}

		public override Unit Width
		{
			get { return Unit.Pixel(90); }
		}

		protected override Unit MinWidth
		{
			get { return Unit.Pixel(90); }
		}

		protected override int MaxLength
		{
			get { return 9; }
		}

		protected virtual string DateValidateHandler
		{
			get
			{
				if (Page != null && Page.ExternalServicesSupported)
				{
					return string.Format("FormatDate('{0}', '{1}', '{2}'); ZDateEdit_SetDefaultTimezone('{3}', '{0}');", TextBoxControl.ClientID, (TimeEditListControl != null ? TimeEditListControl.TextBoxClientID : string.Empty), nameof(ZDateTimePickerFormat.Short), TimeZoneEditListControl.TextBoxClientID);
				}
				return TimeEditListControl != null
										? string.Format("if (NormalizeAndValidateDate('{0}','{1}')) ZDateEdit_SetTime('{2}','00:00'); else ZDateEdit_SetTime('{2}','');", TextBoxControl.ClientID, UserLanguage, TimeEditListControl.TextBoxClientID)
										: string.Format("NormalizeAndValidateDate('{0}','{1}');", TextBoxControl.ClientID, UserLanguage);
			}
		}

		protected virtual string TimezoneValidateHandler
		{
			get
			{
				if (DateTimeFormat == ZDateTimePickerFormat.IncludingTimeZone)
				{
					return string.Format("ZDateTimeEditValidateHandler('{0}')", TimeZoneEditListControl.TextBoxClientID);
				}

				return "";
			}
		}

		string UserLanguage
		{
			get
			{
				string language = (
							HttpContext.Current != null &&
							HttpContext.Current.Request != null &&
							HttpContext.Current.Request.UserLanguages != null &&
							HttpContext.Current.Request.UserLanguages.Length > 0
								? HttpContext.Current.Request.UserLanguages[0]
								: string.Empty
							).ToLower();

				if (language == "ja-jp")
				{
					return "ja";
				}

				return language;
			}
		}

		protected override string AdditionalButtonClickHandler
		{
			get { return "ZDateEdit_ShowCalendar();"; }
		}

		protected override string ButtonBackgroundStyle
		{
			get { return String.Format("url({0}) {1} {2} {2}", ZDateEditButtonImage.FileName, "no-repeat", "center"); }
		}

		#endregion

		#region Overrides

		protected override void BindCore(object dataSource)
		{
			base.BindCore(dataSource);
			DateTimeLabel.BindTo = BindTo;
			DateTimeLabel.Bind(dataSource);
		}

		protected override void RenderScriptBlocks()
		{
			base.RenderScriptBlocks();
			if (!ReadOnly)
			{
				if (Page != null && !Page.ZClientScript.IsClientScriptIncludeRegistered("ZDateEdit_ClientScript"))
				{
					Page.ZClientScript.RegisterClientScriptInclude("ZDateEdit_ClientScript", CalendarOpener.FileName);
				}
			}
		}

		public override void RenderControl(HtmlTextWriter writer)
		{
			if (ReadOnly)
			{
				DateTimeLabel.Width = Width;
				DateTimeLabel.RenderControl(writer);
			}
			else
			{
				base.RenderControl(writer);
			}
		}

		protected override string PopupCssClass
		{
			get
			{
				if (fPopupCssClass == null)
				{
					fPopupCssClass = ZCssHelper.Join(base.PopupCssClass, CssConstants.PopupCalendar);
				}
				return fPopupCssClass;
			}
		}

		string fPopupCssClass;

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			this.Attributes.Add("align", "left");
			TextBoxControl.Attributes.Add("onchange", DateValidateHandler);

			TimeZoneEditListControl.Attributes.Add("onchange", TimezoneValidateHandler);
		}

		protected override IZType GetSelectedValue()
		{
			var dateValue = GetSelectedDate();
			var propertyType = Info?.PropertyType;

			if (propertyType == typeof(ZDateTimeOffset))
			{
				if (TimeZoneEditListControl != null)
				{
					var timezone = TimeZoneEditListControl.Text.Replace(" ", "");
					var r = new Regex(@"^(?<prefix>GMT)(?<sign>[+-])(?<hour>\d{2}):(?<minute>\d{2})");
					Match m = r.Match(timezone);
					if (m.Success && m.Groups["prefix"].Value.Equals("GMT"))
					{
						switch (m.Groups["sign"].Value)
						{
							case "+":
								return new ZDateTimeOffset(dateValue, DateTimeKind.Local, new TimeSpan(int.Parse(m.Groups["hour"].Value), int.Parse(m.Groups["minute"].Value), 0));
							case "-":
								return new ZDateTimeOffset(dateValue, DateTimeKind.Local, new TimeSpan(-1 * int.Parse(m.Groups["hour"].Value), -1 * int.Parse(m.Groups["minute"].Value), 0));
						}
					}
				}
				return new ZDateTimeOffset(dateValue);
			}
			else if (propertyType == typeof(ZDate))
			{
				return new ZDate(dateValue);
			}

			return dateValue;
		}

		ZDateTime GetSelectedDate()
		{
			var selectedDate = WebDateTimeFormatter.GetParsedDate(TextBoxControl.Text, DateTimeFormat);

			if (TimeEditListControl != null && TimeEditListControl.TextBoxControl.Text.Length > 0 && DateTimeFormat != ZDateTimePickerFormat.Short)
			{
				if (string.IsNullOrEmpty(TextBoxControl.Text))
				{
					selectedDate = ZDateTime.Today.Date;
				}

				if (selectedDate.IsValid)
				{
					var parsedTime = WebDateTimeFormatter.GetParsedDate(TimeEditListControl.TextBoxControl.Text, DateTimeFormat);
					selectedDate = selectedDate
						.AddHours(parsedTime.Hour)
						.AddMinutes(parsedTime.Minute);
				}
			}

			if (TimeZoneEditListControl != null && TimeZoneEditListControl.TextBoxControl.Text.Length > 0 && DateTimeFormat == ZDateTimePickerFormat.IncludingTimeZone)
			{
				if (string.IsNullOrEmpty(TextBoxControl.Text))
				{
					selectedDate = ZDateTime.Today.Date;
				}
			}

			return selectedDate;
		}

		protected override void UpdateControlWithNewSelectedValue(IZType newValue)
		{
			if (TimeEditListControl != null && !TimeEditListControl.HasChanges)
			{
				TimeEditListControl.SelectedValue = newValue;
			}
			base.UpdateControlWithNewSelectedValue(newValue);
		}

		protected override string GetTextFromValue(IZType newValue)
		{
			string result = null;
			if (newValue is ZDateTimeOffset)
			{
				newValue = ((ZDateTimeOffset)newValue).ToZDateTime();
			}

			if (newValue is ZDateTime)
			{
				var newDate = ((ZDateTime)newValue);
				if (newDate.IsValid)
				{
					result = WebDateTimeFormatter.GetFormattedDate(newDate, ZDateTimePickerFormat.Short);
				}
				else if (!newDate.IsEmpty)
				{
					result = "Invalid";
				}
			}

			return result;
		}

		#endregion

		#region Resources

		protected ZWebResource CalendarOpener
		{
			get { return fCalendarOpener ?? (fCalendarOpener = new ZWebResource(typeof(ZDateEditBox), "calopener.js", Page)); }
		}
		ZWebResource fCalendarOpener;

		protected internal ZWebResource ZDateEditButtonImage
		{
			get
			{
				if (fZDateEditButtonImage == null)
				{
					fZDateEditButtonImage = new ZWebResource(typeof(ZDateEditBox), "ZDateEditButton.gif", Page, "Enterprise.ZArchitecture.Web.GUI.WebControls.ZDateEditBox");
				}
				return fZDateEditButtonImage;
			}
		}
		ZWebResource fZDateEditButtonImage;

		public override ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = base.Resources;
				result.Add(new ZWebResource(typeof(ZDateEditBox), "calendar.js", Page));
				result.Add(new ZWebResource(typeof(ZDateEditBox), "calendar-en.js", Page));
				result.Add(new ZWebResource(typeof(ZDateEditBox), "calendar.css", Page));
				result.Add(CalendarOpener);
				result.Add(ZDateEditButtonImage);
				return result;
			}
		}

		#endregion

		#region IDateInputControl Members

		public int AutoCompleteMonthThreshold
		{
			get { return 0; }
		}

		public bool AutoCompleteYear
		{
			get { return true; }
		}

		public string FormatString
		{
			get
			{
				switch (DateTimeFormat)
				{
					case ZDateTimePickerFormat.Long:
						return ZDateTime.LongTimeFormat;
					case ZDateTimePickerFormat.Time:
						return ZDateTime.ShortTimeFormat;
					default:
						return ZDateTime.ShortDateFormat;
				}
			}
		}

		public bool AllowNull
		{
			get { return true; }
		}
		#endregion

		#region IWebServiceMethodsCaller

		public List<IWebServiceMethod> WebServiceMethods
		{
			get
			{
				if (webServiceMethods == null)
				{
					webServiceMethods = GetServiceMethods();
				}
				return webServiceMethods;
			}
		}

		List<IWebServiceMethod> webServiceMethods;

		protected virtual List<IWebServiceMethod> GetServiceMethods()
		{
			List<IWebServiceMethod> result = new List<IWebServiceMethod>();
			result.Add(new DateFormatterWebServiceMethod());
			return result;
		}

		#endregion

		#region Internal Properties

		internal Unit PopupWidthInternal => PopupWidth;
		internal Unit PopupHeightInternal => PopupHeight;
		internal string DateValidateHandlerInternal => DateValidateHandler;
		internal int MaxLengthInternal => MaxLength;

		#endregion
	}

	#endregion
}
