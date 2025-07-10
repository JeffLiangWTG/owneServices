using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	internal class ZDayAndTimeEditCore : ZTimeEditCore
	{
		internal ZDayAndTimeEditCore(string timeUnit, ZDayAndTimeEdit editor) : base(editor)
		{
			TimeUnit = timeUnit;
		}

		public string TimeUnit
		{
			get
			{
				return timeUnit;
			}
			set
			{
				if (timeUnit != value)
				{
					timeUnit = value;
				}
			}
		}
		string timeUnit;

		public override bool AllowNegative
		{
			get => TimeUnit != ContainerPenaltyTimeUnit.Codes.Days && base.AllowNegative;
			set
			{
				if (TimeUnit == ContainerPenaltyTimeUnit.Codes.Days)
				{
					if (!value)
					{
						base.AllowNegative = value;
					}
				}
				else
				{
					base.AllowNegative = value;
				}
			}
		}

		#region Format

		protected internal override string GetTextFromTime(ZDateTime time, bool allowNegativeOverride)
		{
			if (TimeUnit == ContainerPenaltyTimeUnit.Codes.Days)
			{
				return GetDaysFromTime(time);
			}
			else
			{
				return base.GetTextFromTime(time, allowNegativeOverride);
			}
		}

		internal static string GetDaysFromTime(ZDateTime time)
		{
			byte result = 0;

			if (time.IsValid)
			{
				var days = Math.Max(Byte.MinValue, Math.Min(Byte.MaxValue, (time - new ZDateTime(time.Year, 1, 1)).TotalDays));
				result = (byte)days;
			}
			else if (!time.IsEmpty)
			{
				return time.ToString();
			}

			return result.ToString(CultureInfo.InvariantCulture);
		}

		#endregion

		#region Parse

		protected internal override ZDateTime GetTimeFromText(string value)
		{
			if (TimeUnit == ContainerPenaltyTimeUnit.Codes.Days)
			{
				ZDateTime result;

				if (string.IsNullOrEmpty(value))
				{
					result = ZDateTime.Empty;
				}
				else
				{
					int days;
					if (int.TryParse(value, out days) && days >= Byte.MinValue)
					{
						if (days > Byte.MaxValue)
						{
							days = Byte.MaxValue;
						}
						result = new ZDateTime(ZDateTime.Today.Year, 1, 1).AddDays(days);
					}
					else
					{
						result = ZDateTime.Invalid;
					}
				}

				return result;
			}
			else
			{
				return base.GetTimeFromText(value);
			}
		}

		#endregion

		#region Key Down / Press

		protected override void Editor_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (TimeUnit == ContainerPenaltyTimeUnit.Codes.Days)
			{
				if (!Char.IsControl(e.KeyChar))
				{
					if (!char.IsDigit(e.KeyChar))
					{
						e.Handled = true;
					}
					else if (Editor.NonSelectedText.Length == 3)
					{
						e.Handled = true; // cannot type 4 numbers
					}
				}
			}
			else
			{
				base.Editor_KeyPress(sender, e);
			}
		}

		#endregion

		protected override void Editor_KeyDown(object sender, KeyEventArgs e)
		{
			if (TimeUnit == ContainerPenaltyTimeUnit.Codes.Days)
			{
				if (!Editor.ReadOnly)
				{
					switch (e.KeyData)
					{
						case Keys.Alt | Keys.Up:
							ModifyDays(1);
							e.Handled = true;
							break;
						case Keys.Alt | Keys.Down:
							ModifyDays(-1);
							e.Handled = true;
							break;
						case Keys.Control | Keys.Up:
							ModifyDays(1);
							e.Handled = true;
							break;
						case Keys.Control | Keys.Down:
							ModifyDays(-1);
							e.Handled = true;
							break;

						case Keys.F2:

							if (Editor.IsOnGrid)
							{
								Editor.SelectionLength = 0;
								Editor.SelectionStart = Editor.Text.Length;
							}
							break;
					}
				}
			}
			else
			{
				base.Editor_KeyDown(sender, e);
			}
		}

		void ModifyDays(int delta)
		{
			if (Int32.TryParse(Editor.Text, out int days))
			{
				unchecked
				{
					days += delta;
					if (days > Byte.MaxValue)
					{
						days = Byte.MaxValue;
					}
					if (days < 0)
					{
						days = 0;
					}
					Editor.Text = days.ToString(CultureInfo.InvariantCulture);
				}
			}
		}
	}
}
