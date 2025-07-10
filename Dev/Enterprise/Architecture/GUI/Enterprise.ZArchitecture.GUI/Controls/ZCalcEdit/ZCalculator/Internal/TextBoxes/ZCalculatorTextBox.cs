using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	internal class ZCalculatorTextBox : ZTextBox
	{
		public ZCalculatorTextBox()
		{
			Text = "0";
			Font = new Font("Courier New", 9);
			CharacterCasing = CharacterCasing.Normal;
			TextAlign = HorizontalAlignment.Right;
			this.ReadOnly = true;
		}

		public override Color BackColor
		{
			get { return Color.White; }
		}

		public void ResetToZero()
		{
			Text = "0";
		}

		public void RemoveLastCharacter()
		{
			if (Text.Length > 0)
			{
				Text = Text.Remove(Text.Length - 1, 1);
			}
		}

		public void RemoveTrailingZeros()
		{
			if (TextContainsDecimal)
			{
				Text = Text.TrimEnd('0');

				if (!TextContainsDecimalsGreaterThanZero)
				{
					Text = Text.TrimEnd(Enterprise.ZArchitecture.Core.Culture.CurrentCompanyCountryCulture.NumberFormat.CurrencyDecimalSeparator.ToCharArray());
				}
			}
		}

		public void ToggleNegative()
		{
			if (Text.StartsWith("-"))
			{
				Text = Text.Remove(0, 1);
			}
			else if (!TextIsZero)
			{
				Text = "-" + Text;
			}
		}

		public bool TextContainsDecimalsGreaterThanZero
		{
			get
			{
				var decimalValue = TextAsDecimal;
				return decimalValue - Utilities.Round(decimalValue, 0) != 0;
			}
		}

		public bool CanTextBeParsedToDecimal
		{
			get
			{
				decimal tempValue;
				return decimal.TryParse(Text, out tempValue);
			}
		}

		public decimal TextAsDecimal
		{
			get
			{
				var result = 0m;
				decimal.TryParse(Text, NumberStyles.Any, Enterprise.ZArchitecture.Core.Culture.CurrentCompanyCountryCulture, out result);
				return result;
			}
		}

		public bool TextContainsDecimal
		{
			get { return Text.IndexOf(Enterprise.ZArchitecture.Core.Culture.CurrentCompanyCountryCulture.NumberFormat.CurrencyDecimalSeparator) != -1; }
		}

		public bool TextIsZero
		{
			get { return Text.Replace("0", "").Trim().Length == 0; }
		}

		public bool TextIsOneDigit
		{
			get { return Text.Length == 1 && char.IsDigit(Text, 0); }
		}
	}
}
