using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	[CompositeFieldControl]
	public partial class OScheduleControl : ZUserControl
	{
		public OScheduleControl()
		{
			InitializeComponent();
			AllowableChars = "FDL";
			SetFixedFonts();
		}

		/// <summary>
		/// Gets or sets a string of allowable characters. Case-insensitive. Default is "*".
		/// </summary>
		[DefaultValue("FDL")]
		public string AllowableChars
		{
			get { return fAllowableChars; }
			set
			{
				if (value.ToUpper().IndexOf(fDaySeparator.ToUpper()) >= 0)
				{
					throw new ArgumentException("The separator character cannot be used as a time marker");
				}
				fAllowableChars = value.ToUpper();
			}
		}

		/// <summary>
		/// Gets or sets a char to be the day separator in a full week schedule text.
		/// </summary>
		[Browsable(true)]
		public string DayScheduleSeparator
		{
			get { return fDaySeparator; }
			set
			{
				if (value.Length != 1)
				{
					throw new ArgumentException("The separator must be a 1-character string");
				}
				if (fAllowableChars.ToUpper().IndexOf(value.ToUpper()) >= 0)
				{
					throw new ArgumentException("The separator character cannot be one of the allowable time markers");
				}
				fDaySeparator = value;
			}
		}

		/// <summary>
		/// Returns the Text of the text box for the specified day
		/// </summary>
		/// <param name="day">Day of Week</param>
		/// <returns></returns>
		public string GetDayText(DayOfWeek day)
		{
			return DayTextBox(day).Text;
		}

		/// <summary>
		/// Sets the Text of the text box for the specified day
		/// </summary>
		/// <param name="day">Day of Week</param>
		/// <param name="text">Text to set</param>
		public void SetDayText(DayOfWeek day, string text)
		{
			DayTextBox(day).Text = Truncate(text);
		}

		/// <summary>
		/// Gets the character in the position corresponding to the specified time
		/// </summary>
		/// <param name="day">Day of Week</param>
		/// <param name="time">Time of Day</param>
		/// <returns></returns>
		public Char GetTimeChar(DayOfWeek day, DateTime time)
		{
			return GetChar(GetDayText(day), time);
		}

		/// <summary>
		/// Indicates if the Day and Time contains a non-blank character
		/// </summary>
		/// <param name="day">Day of Week</param>
		/// <param name="time">Time of Day</param>
		/// <returns></returns>
		public bool IsMarked(DayOfWeek day, DateTime time)
		{
			return GetTimeChar(day, time) != ' ';
		}

		/// <summary>
		/// Get all days concatenated schedule separeted by the default day separator
		/// </summary>
		public string FullWeekText
		{
			get
			{
				return SundayText +
					fDaySeparator + MondayText + fDaySeparator + TuesdayText +
					fDaySeparator + WednesdayText + fDaySeparator + ThursdayText +
					fDaySeparator + FridayText + fDaySeparator + SaturdayText;
			}
			set
			{
				string[] dayValues = value.Split(fDaySeparator[0]);
				for (int i = 0; i < 7; i++)
				{
					string dayValue = (i < dayValues.Length) ? dayValues[i] : "";
					SetDayText((DayOfWeek)i, dayValue);
				}
			}
		}

		[Browsable(false)]
		public string MondayText
		{
			get { return MondayTextBox.Text; }
			set { MondayTextBox.Text = Truncate(value); }
		}

		[Browsable(false)]
		public string TuesdayText
		{
			get { return TuesdayTextBox.Text; }
			set { TuesdayTextBox.Text = Truncate(value); }
		}

		[Browsable(false)]
		public string WednesdayText
		{
			get { return WednesdayTextBox.Text; }
			set { WednesdayTextBox.Text = Truncate(value); }
		}

		[Browsable(false)]
		public string ThursdayText
		{
			get { return ThursdayTextBox.Text; }
			set { ThursdayTextBox.Text = Truncate(value); }
		}

		[Browsable(false)]
		public string FridayText
		{
			get { return FridayTextBox.Text; }
			set { FridayTextBox.Text = Truncate(value); }
		}

		[Browsable(false)]
		public string SaturdayText
		{
			get { return SaturdayTextBox.Text; }
			set { SaturdayTextBox.Text = Truncate(value); }
		}

		[Browsable(false)]
		public string SundayText
		{
			get { return SundayTextBox.Text; }
			set { SundayTextBox.Text = Truncate(value); }
		}

		#region implementation

		string fAllowableChars = "*";
		string fDaySeparator = ",";
		const int MaxLength = 48;

		ZTextBox DayTextBox(DayOfWeek day)
		{
			foreach (Control ctrl in this.Controls)
			{
				if (ctrl is ZTextBox)
				{
					if ((ctrl as ZTextBox).Name == day.ToString() + "TextBox")
					{
						return (ctrl as ZTextBox);
					}
				}
			}
			return null;
		}

		Char GetChar(string dayString, DateTime time)
		{
			string str = dayString.PadRight(48);
			int index = TimeToIndex(time);
			return str[index];
		}

		int TimeToIndex(DateTime time)
		{
			int halfHour = 0;
			if (time.Minute >= 30)
			{
				halfHour = 1;
			}

			return time.Hour * 2 + halfHour;
		}

		string Truncate(string value)
		{
			if (value.Length > MaxLength)
			{
				return value.Substring(0, MaxLength);
			}
			else
			{
				return value;
			}
		}

		void SetFixedFonts()
		{
			Font fixedWidthFont = new Font("Courier New", 10F);
			this.MondayTextBox.Font = fixedWidthFont;
			this.TuesdayTextBox.Font = fixedWidthFont;
			this.WednesdayTextBox.Font = fixedWidthFont;
			this.ThursdayTextBox.Font = fixedWidthFont;
			this.FridayTextBox.Font = fixedWidthFont;
			this.SaturdayTextBox.Font = fixedWidthFont;
			this.SundayTextBox.Font = fixedWidthFont;
		}

		void HoursTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			Char key = e.KeyChar;
			if (Char.IsLower(key))
			{
				key = Char.ToUpper(key);
			}

			if (key == ' ' ||
				key == (Char)8 || //Backspace
				fAllowableChars.IndexOf(key) >= 0)
			{
				//Valid key, do nothing;
			}
			else
			{
				e.Handled = true;
			}
		}
		#endregion
	}
}
