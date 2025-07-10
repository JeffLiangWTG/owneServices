using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	internal class ZTimeEditList : ZDropEditList
	{
		public ZTimeEditList(ZDateEditBox relatedDateBox)
		{
			relatedDateEditBox = relatedDateBox;

			if (relatedDateBox != null)
			{
				relatedDateBox.TimeEditListControl = this;
			}

			TimeList collectionToDropDown = new TimeList();
			for (int i = 0; i < 24; i++)
			{
				collectionToDropDown.Add(new TimeEntry(i, 0));
				collectionToDropDown.Add(new TimeEntry(i, 30));
			}

			ListBoxControl.DataSource = collectionToDropDown;
			ListBoxControl.ShowDescription = false;
		}

		[CodeProperty("Time"), DescriptionProperty("Time")]
		internal class TimeEntry : NonPersistentBusinessObject
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanTypeForDuration", Justification = "Baseline")]
			public TimeEntry(int hours, int minutes)
			{
				Hours = hours;
				Minutes = minutes;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanTypeForDuration", Justification = "Baseline")]
			readonly int Hours;
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanTypeForDuration", Justification = "Baseline")]
			readonly int Minutes;

			public ZString Time
			{
				get { return string.Format("{0}:{1}", Hours.ToString().PadLeft(2, '0'), Minutes.ToString().PadLeft(2, '0')); }
			}
		}
#if DEBUG
		public
#endif
		class TimeList : NonPersistentBusinessObjectCollection<TimeEntry>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new TimeEntry(0, 0);
			}
		}

		readonly ZDateEditBox relatedDateEditBox;

		protected override ZString RelatedControlID
		{
			get
			{
				return relatedDateEditBox != null ? relatedDateEditBox.TextBoxControl.ClientID : string.Empty;
			}
		}

		#region Properties

		public string TextBoxClientID
		{
			get { return TextBoxControl.ClientID; }
		}

		protected virtual string NormalizeAndValidateTimeHandler
		{
			get
			{
				EnsureChildControls();
				if (relatedDateEditBox != null)
				{
					return string.Format("if (NormalizeAndValidateTime('{0}')) {{ ZDropEditList_SelectItem_AdditionalHandler('{1}');ZDateTimeEdit_SetDefaultTimezone('{2}'); }}", TextBoxControl.ClientID, RelatedControlID, relatedDateEditBox.TimeZoneEditListControl.TextBoxClientID);
				}
				else
				{
					return string.Format("if (NormalizeAndValidateTime('{0}')) {{ ZDropEditList_SelectItem_AdditionalHandler('{1}'); }}", TextBoxControl.ClientID, RelatedControlID);
				}
			}
		}

		protected string TimeDropDownImageResourceUrl
		{
			get { return ZTimeEditListButtonImage.FileName; }
		}

		#endregion

		#region Overrides

		protected override bool AllowBindWithoutListMember()
		{
			return true;
		}

		protected override void BindListControl(object dataSource)
		{
			ListBoxControl.DataBind();
		}

		protected override IZType GetSelectedValue()
		{
			if (relatedDateEditBox != null)
			{
				return relatedDateEditBox.SelectedValue;
			}

			ZDateTime value;
			if (!string.IsNullOrEmpty(TextBoxControl.Text) && ZDateTime.TryParseExact(TextBoxControl.Text.Trim(Whitespace), out value, ZDateTime.ShortTimeFormat))
			{
				value = new ZDateTime(ZDateTime.Today.Year, ZDateTime.Today.Month, ZDateTime.Today.Day, value.Hour, value.Minute, 0);
				if (Info?.PropertyType == typeof(ZDateTimeOffset))
				{
					return new ZDateTimeOffset(value);
				}

				return value;
			}

			return null;
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
					result = newDate.ToString(ZDateTime.ShortTimeFormat);
				}
				else if (!newDate.IsEmpty)
				{
					result = Res.GetString("7df6508e-599b-4b3e-bcd5-5606a0412d10", "Invalid");
				}
			}
			return result;
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			Style.Add(HtmlTextWriterStyle.WhiteSpace, "nowrap");
			RegisterZTimeEditListScriptBlock();

			TextBoxControl.Attributes.Add("onchange", NormalizeAndValidateTimeHandler);
		}

		void RegisterZTimeEditListScriptBlock()
		{
			if (Page != null && !Page.ZClientScript.IsClientScriptIncludeRegistered("ZTimeEditList_ClientScriptBlock"))
			{
				Page.ZClientScript.RegisterClientScriptInclude("ZTimeEditList_ClientScriptBlock", ZTimeEditListScriptFile.FileName);
			}
		}

		protected override Unit MinWidth
		{
			get { return Unit.Pixel(60); }
		}

		public override Unit DefaultWidth
		{
			get { return Unit.Pixel(60); }
		}

		protected override Unit ButtonWidth
		{
			get { return Unit.Pixel(18); }
		}

		protected override string ButtonBackgroundStyle
		{
			get
			{
				return String.Format("url({0}) {1} {2} {2}", TimeDropDownImageResourceUrl, "no-repeat", "center");
			}
		}

		#endregion

		#region Resources

		public override ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = base.Resources;
				result.Add(ZTimeEditListScriptFile);
				result.Add(ZTimeEditListButtonImage);
				return result;
			}
		}

		protected ZWebResource ZTimeEditListScriptFile
		{
			get
			{
				if (fTimeEditListScriptFile == null)
				{
					fTimeEditListScriptFile = new ZWebResource(typeof(ZDropEditList), "ZTimeEditScriptBlock.js", Page);
				}
				return fTimeEditListScriptFile;
			}
		}

		ZWebResource fTimeEditListScriptFile;

		protected ZWebResource ZTimeEditListButtonImage
		{
			get
			{
				if (fZTimeEditListButtonImage == null)
				{
					fZTimeEditListButtonImage = new ZWebResource(typeof(ZDropEditList), "ZTimeEditListButton.gif", Page);
				}
				return fZTimeEditListButtonImage;
			}
		}

		ZWebResource fZTimeEditListButtonImage;

		#endregion
	}

	#endregion
}
