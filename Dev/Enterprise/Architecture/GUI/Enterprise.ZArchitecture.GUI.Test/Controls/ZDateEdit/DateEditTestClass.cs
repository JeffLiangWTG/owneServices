using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DateEditTestClass : ZDateEdit
	{
		public DateEditTestClass() { }

		public ZDateEditCore CoreTest
		{
			get { return Core; }
		}

		public ZPopupCalendar CalendarForTest => Calendar;

		public string LayoutErrors
		{
			get
			{
				var result = "";
				var newLine = System.Environment.NewLine;

				if (!CalendarButton.Visible)
				{
					result += "CalendarButton should be visible" + newLine;
				}

				if (DateTextBox.Right != CalendarButton.Left)
				{
					result += "CalendarButton should be on the right" + newLine;
				}

				if (DateTextBox.TabIndex != 0)
				{
					result += "DateTextBox.TabIndex should be 0" + newLine;
				}

				if (CalendarButton.TabIndex != 1)
				{
					result += "CalendarButton.TabIndex should be 1" + newLine;
				}

				if (DateTextBox.TextAlign != HorizontalAlignment.Left)
				{
					result += "Text should be left-aligned" + newLine;
				}

				return result;
			}
		}

		#region Exposing for testing

		#region AutoCompleteYear

		public int GetYearFromGetDateTimeAsObject(string textEntered)
		{
			var result = Core.GetDateTimeAsObject(textEntered, CountryDateTimeFormat.Other);
			return ((DateTime)result).Year;
		}

		#endregion

		#region IncrementTextAsDateTime

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public void IncrementTextAsDateTime_Exposed(int years, int months, int days, int hours, KeyEventArgs e)
		{
			IncrementTextAsDateTime(years, months, days, hours);
		}

		#endregion

		#endregion
	}
}
