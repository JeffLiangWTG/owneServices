using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core.Forms;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormDesignerAnalysis]
	public partial class ZCalculator : KForm
	{
#pragma warning disable IDE0001 // Simplify Names
		#region Auto

		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton Number0Button;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton Number9Button;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton Number8Button;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton Number7Button;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton Number6Button;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton Number5Button;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton Number4Button;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton Number3Button;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton Number2Button;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton Number1Button;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton DecimalButton;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton DivideButton;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton MultiplyButton;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton SubtractButton;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton AddButton;
		Enterprise.ZArchitecture.GUI.ZButton AcceptZButton;
		Enterprise.ZArchitecture.GUI.ZButton CancelZButton;
		Enterprise.ZArchitecture.GUI.Internal.ZCalculatorTextBox ValueTextBox;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton EqualsButton;
		CargoWise.Windows.UI.KGroupBox groupBox1;
		Enterprise.ZArchitecture.GUI.Internal.ZTextBoxWithFont OperatorTextBox;
		Enterprise.ZArchitecture.GUI.Internal.ZClearButton ClearButton;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton Number00Button;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton PercentageButton;
		Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton PlusMinusButton;
		Enterprise.ZArchitecture.ZCalcEdit Extra1CalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit Extra2CalcEdit;
		Enterprise.ZArchitecture.ZLabel Extra1Label;
		Enterprise.ZArchitecture.ZLabel Extra2Label;
		Enterprise.ZArchitecture.GUI.ZButton ExtraCalculateButton;
		CargoWise.Windows.UI.KGroupBox ExtraDividerGroupBox;
		readonly System.ComponentModel.Container components;

		#endregion
#pragma warning restore IDE0001 // Simplify Names

		public ZCalculator(ZCalcEditCore core)
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			InitializeComponent();

			this.core = core;
			this.currentOperator = Keys.None;
			UpdateText();
			this.ShowExtraButtons = core.ShowExtraButtons;

			SetCurrentValue(core.TextBox.Text);
			BindExtraFields();

			DivideButton.Text = "" + (char)247;     // /
			MultiplyButton.Text = "" + (char)215;   // x
			PlusMinusButton.Text = "" + (char)177;  // +-
		}

		internal void UpdateText()
		{
			this.Text = " " + core.FriendlyColumnName;
		}

		public void SetCurrentValue(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				Reset();
			}
			else
			{
				ValueTextBox.Text = value;
				ValueTextBox.RemoveTrailingZeros();
				if (ValueTextBox.CanTextBeParsedToDecimal)
				{
					currentValue = ValueTextBox.TextAsDecimal;

					hasCurrentValue = true;
					nextNumberEntryWillReset = true;
				}
				else
				{
					Reset();
				}
			}
		}

		#region Error Message class

		static class ErrorMessages
		{
			public static string CannotDivideByZero
			{
				get { return Res.GetString("c7c98b56-5397-4828-a91e-7ecf6b640d5d", "Cannot divide by zero."); }
			}
			public static string ResultOutOfRange
			{
				get { return Res.GetString("c589745b-9108-4423-87b5-5dbd6a9645f2", "Result out of range."); }
			}
			public static string ValueOutOfRange
			{
				get { return Res.GetString("b4801803-ae0f-4ae5-bb79-cd6829c15904", "Value out of range."); }
			}
		}

		#endregion

		#region Showing the Calculator

		public void PopUp()
		{
			Location = GetLocation();
			Reset();
			SetCurrentValue(core.TextBox.Text);

			Show();

			if (ShowExtraButtons)
			{
				Extra1CalcEdit.Text = Extra1InitialValue.ToString();
				Extra2CalcEdit.Text = Extra2InitialValue.ToString();
			}

			core.TextBox.BackColor = SystemColors.Info; // make it clear which calc edit we're attached to
		}

		[return: DpiState(DpiState.ScaledVariant)]
		protected Point GetLocation()
		{
			Point result;

			if (core.TextBox != null && core.TextBox.Parent != null)
			{
				result = core.TextBox.Parent.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(core.TextBox.Left, core.TextBox.Bottom, false));

				var rightEdge = result.X + Width;
				var bottomEdge = result.Y + Height;
				var leftEdge = result.X;

				var screenBounds = CachedScreenInfo.Instance.FromPoint(result);

				if (leftEdge < screenBounds.Left)
				{
					ControlDpiScalingHelper.SetX(ref result, screenBounds.Left, false);
				}
				else if (rightEdge > screenBounds.Right)
				{
					ControlDpiScalingHelper.SetX(ref result, screenBounds.Right - Width, false);
				}

				if (bottomEdge > screenBounds.Bottom)
				{
					ControlDpiScalingHelper.SetY(ref result, result.Y - (Height + core.TextBox.Height), false); // place on top of the control

					if (result.Y > screenBounds.Bottom - Height) // the control was off-screen!
					{
						ControlDpiScalingHelper.SetY(ref result, screenBounds.Bottom - Height, false);
					}
				}
			}
			else
			{
				result = Location;
			}

			return result;
		}

		readonly ZCalcEditCore core;

		#endregion

		#region Hiding the Calculator

		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);
			Cancel();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = true;
			base.OnClosing(e);

			Cancel();
		}

		void CancelZButton_Click(object sender, EventArgs e)
		{
			Cancel();
		}

		void Cancel()
		{
			Hide();
		}

		#endregion

		#region Extra Buttons

		[DefaultValue(false)]
		public bool ShowExtraButtons
		{
			get { return showAdditionalCalculation; }
			set
			{
				showAdditionalCalculation = value;

				SuspendLayout();
				try
				{
					ControlDpiScalingHelper.SetWidth(this, ExpectedWidth, true);
					Extra1Label.Visible = value;
					Extra2Label.Visible = value;
					Extra1CalcEdit.Visible = value;
					Extra2CalcEdit.Visible = value;
					ExtraCalculateButton.Visible = value;
					ExtraDividerGroupBox.Visible = value;
				}
				finally
				{
					ResumeLayout();
				}
			}
		}

		public ZDecimal Extra1InitialValue
		{
			get { return extra1InitialValue; }
			set { extra1InitialValue = value; }
		}

		public ZDecimal Extra2InitialValue
		{
			get { return extra2InitialValue; }
			set { extra2InitialValue = value; }
		}

		public string Extra1LabelText
		{
			get { return Extra1Label.Text; }
			set
			{
				Extra1Label.Text = (value.Trim() + ":").Replace("::", ":");
				UpdateExtraButtonText();
			}
		}

		public string Extra2LabelText
		{
			get { return Extra2Label.Text; }
			set
			{
				Extra2Label.Text = (value.Trim() + ":").Replace("::", ":");
				UpdateExtraButtonText();
			}
		}

		protected virtual void BindExtraFields()
		{
			var hasBindToExtra1 = !string.IsNullOrEmpty(core.BindToExtra1);
			var hasBindToExtra2 = !string.IsNullOrEmpty(core.BindToExtra2);

			if (hasBindToExtra1 || hasBindToExtra2)
			{
				var dataSource = core.IsInGrid ? ((ZForm)core.TextBox.FindForm()).BusinessEntity : core.TextBox.DataBindings["Text"].DataSource;

				if (hasBindToExtra1)
				{
					this.DataBindings.Add(new KBinding(nameof(Extra1InitialValue), dataSource, core.BindToExtra1));
				}

				if (hasBindToExtra2)
				{
					this.DataBindings.Add(new KBinding(nameof(Extra2InitialValue), dataSource, core.BindToExtra2));
				}
			}
		}

		void ExtraCalculateButton_Click(object sender, EventArgs e)
		{
			Reset();
			SetCurrentValue(CalculateExtraValues());
		}

		string CalculateExtraValues()
		{
			ZDecimal value1;
			ZDecimal value2;
			var parse1Success = ZDecimal.TryParse(Extra1CalcEdit.Text, out value1);
			var parse2Success = ZDecimal.TryParse(Extra2CalcEdit.Text, out value2);

			return parse1Success && parse2Success ? (value1 * value2).ToString() : "0"; // TODO : ensure bounds are not exceeded
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-semantic text")]
		void UpdateExtraButtonText()
		{
			ExtraCalculateButton.Text = Extra1Label.Text.Replace(":", "") + " ×  " + Extra2Label.Text.Replace(":", "");
		}

		[DpiState(DpiState.Unscaled)]
		int ExpectedWidth
		{
			get { return ShowExtraButtons ? 360 : 220; }
		}

		bool showAdditionalCalculation;
		ZDecimal extra1InitialValue = 0M;
		ZDecimal extra2InitialValue = 0M;

		#endregion

		#region Keys: Capturing all key-presses

		protected override bool ProcessDialogKey(Keys keyData)
		{
			var result = true;

			if (Extra1CalcEdit.Focused || Extra2CalcEdit.Focused)
			{
				result = base.ProcessDialogKey(keyData);
			}
			else
			{
				switch (keyData)
				{
					case Keys.NumPad1:
					case Keys.D1:
						HandleNumberKeyPress(Number1Button.Key);
						break;

					case Keys.NumPad2:
					case Keys.D2:
						HandleNumberKeyPress(Number2Button.Key);
						break;

					case Keys.NumPad3:
					case Keys.D3:
						HandleNumberKeyPress(Number3Button.Key);
						break;

					case Keys.NumPad4:
					case Keys.D4:
						HandleNumberKeyPress(Number4Button.Key);
						break;

					case Keys.NumPad5:
					case Keys.D5:
						HandleNumberKeyPress(Number5Button.Key);
						break;

					case Keys.NumPad6:
					case Keys.D6:
						HandleNumberKeyPress(Number6Button.Key);
						break;

					case Keys.NumPad7:
					case Keys.D7:
						HandleNumberKeyPress(Number7Button.Key);
						break;

					case Keys.NumPad8:
					case Keys.D8:
						HandleNumberKeyPress(Number8Button.Key);
						break;

					case Keys.NumPad9:
					case Keys.D9:
						HandleNumberKeyPress(Number9Button.Key);
						break;

					case Keys.NumPad0:
					case Keys.D0:
						HandleNumberKeyPress(Number0Button.Key);
						break;

					case Keys.Oemplus | Keys.Shift:
					case Keys.Add:
						HandleOperatorKeyPress(Keys.Add);
						break;

					case Keys.OemMinus:
					case Keys.Subtract:
						HandleOperatorKeyPress(Keys.Subtract);
						break;

					case Keys.D8 | Keys.Shift:
					case Keys.Multiply:
						HandleOperatorKeyPress(Keys.Multiply);
						break;

					case Keys.OemQuestion:
					case Keys.Divide:
						HandleOperatorKeyPress(Keys.Divide);
						break;

					case Keys.Oemplus:
					case Keys.Enter:
						HandleEqualsKeyPress();
						break;

					case Keys.Back:
					case Keys.Delete:
						HandleBackSpaceOrDeleteKeyPress();
						break;

					case Keys.Decimal:
						HandleDecimalKeyPress();
						break;

					default:
						result = base.ProcessDialogKey(keyData);
						break;
				}
			}

			return result;
		}

		#endregion

		#region Keys: Numbers + Decimal

		void NumberButton_Click(object sender, EventArgs e)
		{
			HandleNumberKeyPress(((ZOperatorButton)sender).Key);
		}

		void DecimalButton_Click(object sender, EventArgs e)
		{
			HandleDecimalKeyPress();
		}

		protected void HandleNumberKeyPress(Keys numberKey)
		{
			if (!IsDecimalValueInvalid)
			{
				if (nextNumberEntryWillReset)
				{
					Reset();
				}

				if (LastKeyWasOperator)
				{
					ValueTextBox.ResetToZero();
				}

				var numberAsString = GetNumberAsString(numberKey);

				if (LastKeyWasDecimal)
				{
					ValueTextBox.Text += Enterprise.ZArchitecture.Core.Culture.CurrentCompanyCountryCulture.NumberFormat.CurrencyDecimalSeparator + numberAsString;
				}
				else if (ValueTextBox.TextIsZero)
				{
					ValueTextBox.Text = numberAsString[0].ToString(); // in case value is "00" we only want "0"
				}
				else
				{
					ValueTextBox.Text += numberAsString;
				}

				previousKey = numberKey;
			}
		}

		protected void HandleDecimalKeyPress()
		{
			if (!IsDecimalValueInvalid && !ValueTextBox.TextContainsDecimal)
			{
				previousKey = Keys.Decimal;
			}
		}

		string GetNumberAsString(Keys numberKey)
		{
			return (numberKey == Keys.None) ? "00" : numberKey.ToString().Replace("NumPad", "");
		}

		#endregion

		#region Keys: Mathematical Operators

		void OperatorButton_Click(object sender, EventArgs e)
		{
			HandleOperatorKeyPress(((ZOperatorButton)sender).Key);
		}

		protected void HandleOperatorKeyPress(Keys operatorKey)
		{
			if (!IsDecimalValueInvalid)
			{
				nextNumberEntryWillReset = false;

				if (!LastKeyWasOperator && !LastKeyWasEquals)
				{
					if (ValueTextBox.CanTextBeParsedToDecimal)
					{
						if (hasCurrentValue)
						{
							operandValue = ValueTextBox.TextAsDecimal;
							Calculate();
						}
						else
						{
							currentValue = ValueTextBox.TextAsDecimal;
							hasCurrentValue = true;
						}
					}
					else
					{
						IsDecimalValueInvalid = true;
						ValueTextBox.Text = ErrorMessages.ValueOutOfRange;
					}
				}

				currentOperator = operatorKey;
				switch (operatorKey)
				{
					case Keys.Add:
						OperatorTextBox.Text = "+";
						break;

					case Keys.Subtract:
						OperatorTextBox.Text = "-";
						break;

					case Keys.Multiply:
						OperatorTextBox.Text = ((char)215).ToString(); // x
						break;

					case Keys.Divide:
						OperatorTextBox.Text = ((char)247).ToString(); // /
						break;
				}

				previousKey = operatorKey;
			}
		}

		void Calculate()
		{
			if (currentOperator != Keys.None)
			{
				IsDecimalValueInvalid = !SafeCalculate();

				if (IsDecimalValueInvalid)
				{
					if (currentOperator == Keys.Divide && operandValue == 0)
					{
						ValueTextBox.Text = ErrorMessages.CannotDivideByZero;
					}
					else
					{
						ValueTextBox.Text = ErrorMessages.ResultOutOfRange;
					}
				}
				else
				{
					ValueTextBox.Text = currentValue.ToString(Enterprise.ZArchitecture.Core.Culture.CurrentCompanyCountryCulture);
					ValueTextBox.RemoveTrailingZeros();
				}
			}
		}

		bool SafeCalculate()
		{
			var success = true;

			try
			{
				switch (currentOperator)
				{
					case Keys.Add:
						currentValue += operandValue;
						break;

					case Keys.Subtract:
						currentValue -= operandValue;
						break;

					case Keys.Multiply:
						currentValue *= operandValue;
						break;

					case Keys.Divide:
						if (operandValue == 0)
						{
							success = false;
						}
						else if (LastKeyWasPercent)
						{
							currentValue /= (operandValue / 100);
						}
						else
						{
							currentValue /= operandValue;
						}
						break;
				}
			}
			catch (OverflowException)
			{
				success = false;
			}

			return success;
		}

		#endregion

		#region Keys: Equals

		void EqualsButton_Click(object sender, EventArgs e)
		{
			HandleEqualsKeyPress();
		}

		protected void HandleEqualsKeyPress()
		{
			if (!IsDecimalValueInvalid)
			{
				nextNumberEntryWillReset = true;

				OperatorTextBox.Clear();

				if (!LastKeyWasEquals)
				{
					if (ValueTextBox.CanTextBeParsedToDecimal)
					{
						operandValue = ValueTextBox.TextAsDecimal;
					}
					else
					{
						IsDecimalValueInvalid = true;
						ValueTextBox.Text = ErrorMessages.ValueOutOfRange;
					}
				}

				Calculate();
				previousKey = Keys.Oemplus;
			}
		}

		#endregion

		#region Keys: Percentage

		void PercentageButton_Click(object sender, EventArgs e)
		{
			HandlePercentage();
		}

		protected void HandlePercentage()
		{
			if (hasCurrentValue)
			{
				nextNumberEntryWillReset = true;

				if (ValueTextBox.CanTextBeParsedToDecimal)
				{
					ValueTextBox.Text = (currentValue * ValueTextBox.TextAsDecimal / 100m).ToString(Culture.CurrentCompanyCountryCulture);
					ValueTextBox.RemoveTrailingZeros();
					previousKey = Keys.Shift | Keys.D5;
				}
				else
				{
					IsDecimalValueInvalid = true;
					ValueTextBox.Text = ErrorMessages.ValueOutOfRange;
				}
			}
		}

		#endregion

		#region Keys: Backspace, Delete

		protected void HandleBackSpaceOrDeleteKeyPress()
		{
			if (!nextNumberEntryWillReset && !IsDecimalValueInvalid)
			{
				var setPreviousKey = true;

				if (!LastKeyWasDecimal & !ValueTextBox.TextIsZero)
				{
					if (ValueTextBox.TextIsOneDigit)
					{
						ValueTextBox.ResetToZero();
					}
					else
					{
						ValueTextBox.RemoveLastCharacter();
						if (ValueTextBox.Text.EndsWith(Enterprise.ZArchitecture.Core.Culture.CurrentCompanyCountryCulture.NumberFormat.CurrencyDecimalSeparator))
						{
							ValueTextBox.RemoveLastCharacter();
							HandleDecimalKeyPress();
							setPreviousKey = false;
						}
					}
				}

				if (setPreviousKey)
				{
					previousKey = Keys.Back;
				}
			}
		}

		#endregion

		#region Kesy: +/- Button

		void PlusMinusButton_Click(object sender, EventArgs e)
		{
			HandlePlusMinus();
		}

		protected void HandlePlusMinus()
		{
			ValueTextBox.ToggleNegative();
		}

		#endregion

		#region Clear Button

		void ClearButton_Click(object sender, EventArgs e)
		{
			Reset();
		}

		public void Reset()
		{
			nextNumberEntryWillReset = false;
			IsDecimalValueInvalid = false;
			hasCurrentValue = false;
			currentOperator = Keys.None;
			OperatorTextBox.Clear();
			ValueTextBox.ResetToZero();

			currentValue = 0M;
			operandValue = 0M;
		}

		#endregion

		#region OK Button

		void AcceptZButton_Click(object sender, EventArgs e)
		{
			if (!LastKeyWasEquals && (!LastKeyWasPercent || currentOperator != Keys.Multiply))
			{
				HandleEqualsKeyPress(); // TODO : if error, don't allow OK button to continue!
			}

			if (!IsDecimalValueInvalid)
			{
				if (CalculationComplete != null)
				{
					CalculationComplete(this, new CalculationCompleteEventArgs(ValueTextBox.TextAsDecimal));
				}

				Hide();

#if !WINZOR
				if (ObjectFactory.Get<TerminalService>().IsRemoteAppSession)
				{
					Owner?.Activate();
				}
#endif
			}
		}

		public class CalculationCompleteEventArgs : EventArgs
		{
			public CalculationCompleteEventArgs(decimal value)
			{
				this.Value = value;
			}

			public readonly decimal Value;
		}

		public delegate void OnCalculationComplete(object sender, CalculationCompleteEventArgs e);
		public event OnCalculationComplete CalculationComplete;

		#endregion

		#region Is the Decimal Value Invalid?

		bool IsDecimalValueInvalid
		{
			get { return decimalValueIsInvalid; }
			set
			{
				decimalValueIsInvalid = value;

				Number1Button.Enabled = !value;
				Number2Button.Enabled = !value;
				Number3Button.Enabled = !value;
				Number4Button.Enabled = !value;
				Number5Button.Enabled = !value;
				Number6Button.Enabled = !value;
				Number7Button.Enabled = !value;
				Number8Button.Enabled = !value;
				Number9Button.Enabled = !value;
				Number0Button.Enabled = !value;
				Number00Button.Enabled = !value;
				DecimalButton.Enabled = !value;
				AddButton.Enabled = !value;
				SubtractButton.Enabled = !value;
				MultiplyButton.Enabled = !value;
				DivideButton.Enabled = !value;
				EqualsButton.Enabled = !value;
				AcceptZButton.Enabled = !value;
				PlusMinusButton.Enabled = !value;
			}
		}

		bool decimalValueIsInvalid;

		#endregion

		#region Implementation

		bool LastKeyWasOperator
		{
			get { return (previousKey == Keys.Add || previousKey == Keys.Subtract || previousKey == Keys.Multiply || previousKey == Keys.Divide); }
		}

		bool LastKeyWasEquals
		{
			get { return previousKey == Keys.Oemplus; }
		}

		bool LastKeyWasPercent
		{
			get { return previousKey == (Keys.Shift | Keys.D5); }
		}

		bool LastKeyWasDecimal
		{
			get { return previousKey == Keys.Decimal; }
		}

		bool nextNumberEntryWillReset;
		bool hasCurrentValue;
		decimal currentValue;
		decimal operandValue;
		Keys currentOperator;
		Keys previousKey;

		#endregion

		#region Dispose()

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
