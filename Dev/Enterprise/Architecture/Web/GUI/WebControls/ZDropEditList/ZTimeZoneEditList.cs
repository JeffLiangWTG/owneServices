using System;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls;

internal class ZTimezoneEditList : ZDropEditList
{
	public ZTimezoneEditList(ZDateEditBox relatedDateBox)
	{
		relatedDateEditBox = relatedDateBox;

		if (relatedDateBox != null)
		{
			relatedDateBox.TimeZoneEditListControl = this;
		}

		var collectionToDropDown = new TimezoneList();
		for (int i = 12; i >= -12; i--)
		{
			if (i >= 0)
			{
				collectionToDropDown.Add(new TimezoneEntry(String.Format((NoResString)"GMT +{0:D2}:00", i)));
			}
			else
			{
				collectionToDropDown.Add(new TimezoneEntry(String.Format((NoResString)"GMT {0:D2}:00", i)));
			}
		}

		ListBoxControl.DataSource = collectionToDropDown;
		ListBoxControl.ShowDescription = false;
	}

	[CodeProperty("Timezone"), DescriptionProperty("Timezone")]
	[method: System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanTypeForDuration", Justification = "Baseline")]
	internal class TimezoneEntry(string timezone) : NonPersistentBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanTypeForDuration", Justification = "Baseline")]
		readonly string _timezone = timezone;

		public ZString Timezone
		{
			get { return _timezone; }
		}
	}
#if DEBUG
	public
#endif
		class TimezoneList : NonPersistentBusinessObjectCollection<TimezoneEntry>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TimezoneEntry((NoResString)"none");
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

	public string TextBoxClientID
	{
		get { return TextBoxControl.ClientID; }
	}

	protected override bool AllowBindWithoutListMember()
	{
		return true;
	}

	protected override void BindListControl(object dataSource)
	{
		ListBoxControl.DataBind();
	}

	protected override Unit MinWidth
	{
		get { return Unit.Pixel(100); }
	}

	public override Unit DefaultWidth
	{
		get { return Unit.Pixel(100); }
	}

	protected override Unit ButtonWidth
	{
		get { return Unit.Pixel(18); }
	}

	protected override IZType GetSelectedValue()
	{
		if (relatedDateEditBox != null)
		{
			return relatedDateEditBox.SelectedValue;
		}

		var timezone = Text.Replace(" ", "");
		var r = new Regex(@"^(?<prefix>GMT)(?<sign>[+-])(?<hour>\d{2}):(?<minute>\d{2})");
		Match m = r.Match(timezone);
		if (m.Success && m.Groups["prefix"].Value.Equals("GMT"))
		{
			switch (m.Groups["sign"].Value)
			{
				case "+":
					return new ZDateTimeOffset(new ZDateTime(ZDateTime.Today.Year, ZDateTime.Today.Month, ZDateTime.Today.Day), DateTimeKind.Local, new TimeSpan(Int32.Parse(m.Groups["hour"].Value), int.Parse(m.Groups["minute"].Value), 0));
				case "-":
					return new ZDateTimeOffset(new ZDateTime(ZDateTime.Today.Year, ZDateTime.Today.Month, ZDateTime.Today.Day), DateTimeKind.Local, new TimeSpan(-1 * Int32.Parse(m.Groups["hour"].Value), -1 * int.Parse(m.Groups["minute"].Value), 0));
			}
		}

		return null;
	}

	protected override string GetTextFromValue(IZType newValue)
	{
		string result = null;

		if (newValue is ZDateTimeOffset)
		{
			var offset = ((ZDateTimeOffset)newValue).Offset;
			if (offset.Hours >= 0)
			{
				return string.Format((NoResString)"GMT +{0:D2}:{1:D2}", offset.Hours, offset.Minutes);
			}
			else
			{
				return string.Format((NoResString)"GMT {0:D2}:{1:D2}", offset.Hours, offset.Minutes);
			}
		}

		if (newValue is ZDateTime)
		{
			return "";
		}

		return result;
	}
}
