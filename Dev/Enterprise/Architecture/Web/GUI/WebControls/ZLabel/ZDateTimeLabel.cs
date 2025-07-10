using System.ComponentModel;
using System.Web.UI;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Label to display Date/Time values
	/// </summary>
	[DefaultProperty("Text"), ToolboxData("<{0}:ZDateTimeLabel runat=server></{0}:ZDateTimeLabel>")]
	public class ZDateTimeLabel : ZLabelBase
	{
		public ZDateTimeLabel()
		{
			EnableHtmlEncoding = false;
		}

		protected internal override string GetText(IZType value)
		{
			if (value is ZDateTime dateTime)
			{
				return SuppressUtil.GetFormattedDate(dateTime, DateTimeFormat);
			}
			if (value is ZDateTimeOffset dateTimeOffset)
			{
				return SuppressUtil.GetFormattedDate(dateTimeOffset, DateTimeFormat);
			}
			if (value is ZDate date)
			{
				return SuppressUtil.GetFormattedDate(date);
			}

			return string.Empty;
		}

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(ZDateTimePickerFormat.Short)]
		public ZDateTimePickerFormat DateTimeFormat
		{
			get { return fDateTimeFormat; }
			set { fDateTimeFormat = value; }
		}

		ZDateTimePickerFormat fDateTimeFormat = ZDateTimePickerFormat.Short;
	}
}
