using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Windows.UI
{
	internal class TextBoxDecimalPlacesHandler
	{
		public TextBoxDecimalPlacesHandler(TextBoxBase textBox)
		{ this.TextBox = textBox; }

		public TextBoxBase TextBox
		{
			get { return textBox; }
			set
			{
				if (textBox != null)
				{
					textBox.TextChanged -= TextBox_TextChanged;
					textBox.Validating -= TextBox_Validating;
				}
				this.textBox = value;
				if (textBox != null)
				{
					textBox.TextChanged += TextBox_TextChanged;
					textBox.Validating += TextBox_Validating;
				}
			}
		}
		TextBoxBase textBox;

		public IDisposable NotifyInTextSetter()
		{
			inTextSetter = true;
			return new DisposableAction(delegate
			{
				inTextSetter = false;
			});
		}
		bool inTextSetter;

		#region DecimalPlaces

		public int DecimalPlaces
		{
			get { return decimalPlaces; }
			set
			{
				if (decimalPlaces != value)
				{
					decimalPlaces = value;
					if (IsActive && TextBox.Text.Trim().Length > 0)
					{
						FormatTextDecimalPlaces(true);
					}
				}
			}
		}
		int decimalPlaces = -1;

		#endregion

		#region Event Handlers

		void TextBox_Validating(object sender, CancelEventArgs e)
		{
			if (IsActive)
			{
				TextBox.Text = FormatTextDecimalPlaces(TextBox.Text, false);
			}
		}

		void TextBox_TextChanged(object sender, EventArgs e)
		{
			if (!inTextBox_TextChanged && IsActive)
			{
				inTextBox_TextChanged = true;
				try
				{
					if (inTextSetter)
					{
						OnTextChangedFromTextSetter();
					}
					else
					{
						OnTextChangedFromEnteredData();
					}
				}
				finally
				{
					inTextBox_TextChanged = false;
				}
			}
		}
		bool inTextBox_TextChanged;

		#endregion

		#region Implementation

		bool IsActive
		{ get { return DecimalPlaces >= 0; } }

#if DEBUG
		protected internal virtual
#endif
			int SelectionStart
		{
			get { return TextBox.SelectionStart; }
			set { TextBox.SelectionStart = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		int SelectionLength
		{
			get { return TextBox.SelectionLength; }
			set { TextBox.SelectionLength = value; }
		}

		void OnTextChangedFromTextSetter()
		{
			if (TextBox.Text.Trim().Length > 0)
			{
				FormatTextDecimalPlaces(true);
			}
		}

		void OnTextChangedFromEnteredData()
		{
			var originalText = TextBox.Text;
			var text = FormatTextDecimalPlaces(originalText, false);

			if (text != originalText)
			{
				var savedSelectionStart = SelectionStart;
				TextBox.Text = text;
				SelectionStart = savedSelectionStart > TextBox.Text.Length ? TextBox.Text.Length : savedSelectionStart;
			}
		}

		void FormatTextDecimalPlaces(bool appendDecimalPlaces)
		{
			string originalText = TextBox.Text;
			string text = FormatTextDecimalPlaces(originalText, appendDecimalPlaces);
			if (text != originalText)
			{
				int savedSelectionStart = SelectionStart;
				TextBox.Text = text;
				SelectionStart = savedSelectionStart;
			}
		}

		string FormatTextDecimalPlaces(string text, bool appendDecimalPlaces)
		{
			string result = RemoveRedundantDecimalPoints(text);
			result = result.Trim();
			int visibleDecimalPlaces = GetVisibleDecimalPlaces(result, DecimalPoint);

			if (visibleDecimalPlaces > DecimalPlaces)
			{
				int decimalCharCount = visibleDecimalPlaces - DecimalPlaces;
				if (DecimalPlaces == 0 && visibleDecimalPlaces > 0)
				{
					decimalCharCount += DecimalPoint.Length;
				}
				result = result.Substring(0, result.Length - decimalCharCount);
			}
			else if (appendDecimalPlaces && visibleDecimalPlaces < DecimalPlaces)
			{
				if (visibleDecimalPlaces == 0 && DecimalPlaces != 0)
				{
					result += ".";
				}
				result += new string('0', DecimalPlaces - visibleDecimalPlaces);
			}
			return result;
		}

		string RemoveRedundantDecimalPoints(string text)
		{
			string result = text;
			int i = text.LastIndexOf(DecimalPoint, StringComparison.Ordinal);
			if (i != -1)
			{
				if (DecimalPlaces == 0 || (i != 0 && text.LastIndexOf(DecimalPoint, i - 1, StringComparison.Ordinal) != -1))
				{
					if (SelectionLength == 0 &&
						SelectionStart >= 1 &&
						text.Length >= SelectionStart + DecimalPoint.Length &&
						text.Substring(SelectionStart - 1, DecimalPoint.Length) == DecimalPoint)
					{
						result = result.Remove(SelectionStart - 1, DecimalPoint.Length);
						SelectionStart--;
					}
					else
					{
						result = result.Substring(0, i);
					}
				}
			}
			return result;
		}

		static int GetVisibleDecimalPlaces(string text, string numberDecimalSeparator)
		{
			int lastDecimalPoint = text.LastIndexOf(numberDecimalSeparator, StringComparison.Ordinal);
			return lastDecimalPoint == -1 ? 0 : (text.Length - lastDecimalPoint - 1);
		}

		string DecimalPoint
		{ get { return decimalPoint ?? (decimalPoint = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator); } }
		string decimalPoint;

		#endregion
	}
}
