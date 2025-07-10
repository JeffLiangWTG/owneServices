using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZPopupCalendar : ContainerControl
	{
		protected Control ParentControl;
		public event OnDateTimeSelected DateTimeSelected;
		public delegate void OnDateTimeSelected(object sender, DateTimeSelectedEventArgs e);

		public bool HasTimeZoneFindBox { get; set; }

		public bool ReturnsDateTimeOffset { get; set; }

		TimeSpan? CurrentDateTimeOffset { get; set; }

		public ZPopupCalendar()
		{
		}

		public void OnDateChanged(DateTime changedDate)
		{
			if (ReturnsDateTimeOffset)
			{
				//Html date time picker not support timezone I set it to zero right now
				//we need address this later. https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/datetime-local
				DateTimeSelected?.Invoke(this, new DateTimeOffsetSelectedEventArgs(changedDate, CurrentDateTimeOffset ?? TimeSpan.Zero));
			}
			else
			{
				DateTimeSelected?.Invoke(this, new DateTimeSelectedEventArgs(changedDate));
			}
		}

		public void SetDateTimeOffset(TimeSpan? dateTimeOffset)
		{
			CurrentDateTimeOffset = dateTimeOffset;
		}

		public void Popup(Point location, DateTime initialDateTime, Control parentControl, ZDateTimePickerFormat dateTimeFormat)
		{
		}

		public event FormClosingEventHandler FormClosing;

		public void Close() { }

		public class DateTimeSelectedEventArgs : EventArgs
		{
			public DateTimeSelectedEventArgs(DateTime selectedDateTime)
			{
				Value = selectedDateTime;
			}

			public DateTime Value { get; set; }
		}

		public class DateTimeOffsetSelectedEventArgs : DateTimeSelectedEventArgs
		{
			public DateTimeOffsetSelectedEventArgs(DateTime selectedDateTime, TimeSpan offset) : base(selectedDateTime)
			{
				OffsetValue = offset;
			}

			public TimeSpan OffsetValue { get; set; }
		}
	}
}
