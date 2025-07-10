using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZCalculator
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.ValueTextBox = new Enterprise.ZArchitecture.GUI.Internal.ZCalculatorTextBox();
			this.Number0Button = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.Number9Button = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.Number8Button = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.Number7Button = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.Number6Button = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.Number5Button = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.Number4Button = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.Number3Button = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.Number2Button = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.Number1Button = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.DecimalButton = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.DivideButton = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.MultiplyButton = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.SubtractButton = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.AddButton = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.AcceptZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EqualsButton = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.groupBox1 = new CargoWise.Windows.UI.KGroupBox();
			this.OperatorTextBox = new Enterprise.ZArchitecture.GUI.Internal.ZTextBoxWithFont();
			this.ClearButton = new Enterprise.ZArchitecture.GUI.Internal.ZClearButton();
			this.PercentageButton = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.Number00Button = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.PlusMinusButton = new Enterprise.ZArchitecture.GUI.Internal.ZOperatorButton();
			this.Extra1CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.Extra2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.Extra1Label = new Enterprise.ZArchitecture.ZLabel();
			this.Extra2Label = new Enterprise.ZArchitecture.ZLabel();
			this.ExtraCalculateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExtraDividerGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.SuspendLayout();
			// 
			// ValueTextBox
			// 
			this.ValueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right);
			this.ValueTextBox.BackColor = System.Drawing.Color.White;
			this.ValueTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.ValueTextBox.Name = "ValueTextBox";
			this.ValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 21, true);
			this.ValueTextBox.TabIndex = 17;
			this.ValueTextBox.TabStop = false;
			this.ValueTextBox.Text = "0";
			this.ValueTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// Number0Button
			// 
			this.Number0Button.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Number0Button.Font = new System.Drawing.Font(OFont.NormalFontName, 9F);
			this.Number0Button.Key = System.Windows.Forms.Keys.NumPad0;
			this.Number0Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 128, true);
			this.Number0Button.Name = "Number0Button";
			this.Number0Button.TabIndex = 16;
			this.Number0Button.Text = "0";
			this.Number0Button.Click += new System.EventHandler(this.NumberButton_Click);
			// 
			// Number9Button
			// 
			this.Number9Button.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Number9Button.Font = new System.Drawing.Font(OFont.NormalFontName, 9F);
			this.Number9Button.Key = System.Windows.Forms.Keys.NumPad9;
			this.Number9Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 44, true);
			this.Number9Button.Name = "Number9Button";
			this.Number9Button.TabIndex = 15;
			this.Number9Button.Text = "9";
			this.Number9Button.Click += new System.EventHandler(this.NumberButton_Click);
			// 
			// Number8Button
			// 
			this.Number8Button.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Number8Button.Font = new System.Drawing.Font(OFont.NormalFontName, 9F);
			this.Number8Button.Key = System.Windows.Forms.Keys.NumPad8;
			this.Number8Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 44, true);
			this.Number8Button.Name = "Number8Button";
			this.Number8Button.TabIndex = 14;
			this.Number8Button.Text = "8";
			this.Number8Button.Click += new System.EventHandler(this.NumberButton_Click);
			// 
			// Number7Button
			// 
			this.Number7Button.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Number7Button.Font = new System.Drawing.Font(OFont.NormalFontName, 9F);
			this.Number7Button.Key = System.Windows.Forms.Keys.NumPad7;
			this.Number7Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 44, true);
			this.Number7Button.Name = "Number7Button";
			this.Number7Button.TabIndex = 13;
			this.Number7Button.Text = "7";
			this.Number7Button.Click += new System.EventHandler(this.NumberButton_Click);
			// 
			// Number6Button
			// 
			this.Number6Button.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Number6Button.Font = new System.Drawing.Font(OFont.NormalFontName, 9F);
			this.Number6Button.Key = System.Windows.Forms.Keys.NumPad6;
			this.Number6Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 72, true);
			this.Number6Button.Name = "Number6Button";
			this.Number6Button.TabIndex = 12;
			this.Number6Button.Text = "6";
			this.Number6Button.Click += new System.EventHandler(this.NumberButton_Click);
			// 
			// Number5Button
			// 
			this.Number5Button.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Number5Button.Font = new System.Drawing.Font(OFont.NormalFontName, 9F);
			this.Number5Button.Key = System.Windows.Forms.Keys.NumPad5;
			this.Number5Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 72, true);
			this.Number5Button.Name = "Number5Button";
			this.Number5Button.TabIndex = 11;
			this.Number5Button.Text = "5";
			this.Number5Button.Click += new System.EventHandler(this.NumberButton_Click);
			// 
			// Number4Button
			// 
			this.Number4Button.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Number4Button.Font = new System.Drawing.Font(OFont.NormalFontName, 9F);
			this.Number4Button.Key = System.Windows.Forms.Keys.NumPad4;
			this.Number4Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 72, true);
			this.Number4Button.Name = "Number4Button";
			this.Number4Button.TabIndex = 10;
			this.Number4Button.Text = "4";
			this.Number4Button.Click += new System.EventHandler(this.NumberButton_Click);
			// 
			// Number3Button
			// 
			this.Number3Button.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Number3Button.Font = new System.Drawing.Font(OFont.NormalFontName, 9F);
			this.Number3Button.Key = System.Windows.Forms.Keys.NumPad3;
			this.Number3Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 100, true);
			this.Number3Button.Name = "Number3Button";
			this.Number3Button.TabIndex = 9;
			this.Number3Button.Text = "3";
			this.Number3Button.Click += new System.EventHandler(this.NumberButton_Click);
			// 
			// Number2Button
			// 
			this.Number2Button.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Number2Button.Font = new System.Drawing.Font(OFont.NormalFontName, 9F);
			this.Number2Button.Key = System.Windows.Forms.Keys.NumPad2;
			this.Number2Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 100, true);
			this.Number2Button.Name = "Number2Button";
			this.Number2Button.TabIndex = 8;
			this.Number2Button.Text = "2";
			this.Number2Button.Click += new System.EventHandler(this.NumberButton_Click);
			// 
			// Number1Button
			// 
			this.Number1Button.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Number1Button.Font = new System.Drawing.Font(OFont.NormalFontName, 9F);
			this.Number1Button.Key = System.Windows.Forms.Keys.NumPad1;
			this.Number1Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 100, true);
			this.Number1Button.Name = "Number1Button";
			this.Number1Button.TabIndex = 7;
			this.Number1Button.Text = "1";
			this.Number1Button.Click += new System.EventHandler(this.NumberButton_Click);
			// 
			// DecimalButton
			// 
			this.DecimalButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DecimalButton.Font = new System.Drawing.Font(OFont.NormalFontName, 9F);
			this.DecimalButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 128, true);
			this.DecimalButton.Name = "DecimalButton";
			this.DecimalButton.TabIndex = 6;
			this.DecimalButton.Text = Enterprise.ZArchitecture.Core.Culture.CurrentCompanyCountryCulture.NumberFormat.CurrencyDecimalSeparator;
			this.DecimalButton.Click += new System.EventHandler(this.DecimalButton_Click);
			// 
			// DivideButton
			// 
			this.DivideButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DivideButton.Font = new System.Drawing.Font("Arial", 10.5F, System.Drawing.FontStyle.Bold);
			this.DivideButton.Key = System.Windows.Forms.Keys.Divide;
			this.DivideButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 44, true);
			this.DivideButton.Name = "DivideButton";
			this.DivideButton.TabIndex = 5;
			this.DivideButton.Text = "/";
			this.DivideButton.Click += new System.EventHandler(this.OperatorButton_Click);
			// 
			// MultiplyButton
			// 
			this.MultiplyButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MultiplyButton.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
			this.MultiplyButton.Key = System.Windows.Forms.Keys.Multiply;
			this.MultiplyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 72, true);
			this.MultiplyButton.Name = "MultiplyButton";
			this.MultiplyButton.TabIndex = 4;
			this.MultiplyButton.Text = "*";
			this.MultiplyButton.Click += new System.EventHandler(this.OperatorButton_Click);
			// 
			// SubtractButton
			// 
			this.SubtractButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SubtractButton.Font = new System.Drawing.Font("Arial", 16F);
			this.SubtractButton.Key = System.Windows.Forms.Keys.Subtract;
			this.SubtractButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 100, true);
			this.SubtractButton.Name = "SubtractButton";
			this.SubtractButton.TabIndex = 3;
			this.SubtractButton.Text = "-";
			this.SubtractButton.Click += new System.EventHandler(this.OperatorButton_Click);
			// 
			// AddButton
			// 
			this.AddButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AddButton.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
			this.AddButton.Key = System.Windows.Forms.Keys.Add;
			this.AddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 128, true);
			this.AddButton.Name = "AddButton";
			this.AddButton.TabIndex = 2;
			this.AddButton.Text = "+";
			this.AddButton.Click += new System.EventHandler(this.OperatorButton_Click);
			// 
			// AcceptZButton
			// 
			this.AcceptZButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.AcceptZButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AcceptZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 177, true);
			this.AcceptZButton.Name = "AcceptZButton";
			this.AcceptZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.AcceptZButton.TabIndex = 1;
			this.AcceptZButton.Text = "OK";
			this.AcceptZButton.Click += new System.EventHandler(this.AcceptZButton_Click);
			// 
			// CancelZButton
			// 
			this.CancelZButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.CancelZButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelZButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CancelZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 177, true);
			this.CancelZButton.Name = "CancelZButton";
			this.CancelZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.CancelZButton.TabIndex = 1;
			this.CancelZButton.Text = "&Cancel";
			this.CancelZButton.Click += new System.EventHandler(this.CancelZButton_Click);
			// 
			// EqualsButton
			// 
			this.EqualsButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EqualsButton.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold);
			this.EqualsButton.Key = System.Windows.Forms.Keys.Oemplus;
			this.EqualsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 128, true);
			this.EqualsButton.Name = "EqualsButton";
			this.EqualsButton.TabIndex = 18;
			this.EqualsButton.Text = "=";
			this.EqualsButton.Click += new System.EventHandler(this.EqualsButton_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right);
			this.groupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(-4, 165, true);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 4, true);
			this.groupBox1.TabIndex = 19;
			this.groupBox1.TabStop = false;
			// 
			// OperatorTextBox
			// 
			this.OperatorTextBox.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.OperatorTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.OperatorTextBox.Font = new System.Drawing.Font(OFont.NormalFontName, 8.5F);
			this.OperatorTextBox.ForeColor = System.Drawing.Color.Red;
			this.OperatorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 12, true);
			this.OperatorTextBox.Name = "OperatorTextBox";
			this.OperatorTextBox.ReadOnly = true;
			this.OperatorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 21, true);
			this.OperatorTextBox.TabIndex = 21;
			this.OperatorTextBox.TabStop = false;
			this.OperatorTextBox.Text = "";
			this.OperatorTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// ClearButton
			// 
			this.ClearButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ClearButton.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold);
			this.ClearButton.ForeColor = System.Drawing.Color.Red;
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 45, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.TabIndex = 23;
			this.ClearButton.Text = "C";
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// PercentageButton
			// 
			this.PercentageButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PercentageButton.Font = new System.Drawing.Font(OFont.NormalFontName, 9F);
			this.PercentageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 100, true);
			this.PercentageButton.Name = "PercentageButton";
			this.PercentageButton.TabIndex = 25;
			this.PercentageButton.Text = "%";
			this.PercentageButton.Click += new System.EventHandler(this.PercentageButton_Click);
			// 
			// Number00Button
			// 
			this.Number00Button.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Number00Button.Font = new System.Drawing.Font(OFont.NormalFontName, 9F);
			this.Number00Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 128, true);
			this.Number00Button.Name = "Number00Button";
			this.Number00Button.TabIndex = 26;
			this.Number00Button.Text = "00";
			this.Number00Button.Click += new System.EventHandler(this.NumberButton_Click);
			// 
			// PlusMinusButton
			// 
			this.PlusMinusButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PlusMinusButton.Font = new System.Drawing.Font("Arial", 13F);
			this.PlusMinusButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 72, true);
			this.PlusMinusButton.Name = "PlusMinusButton";
			this.PlusMinusButton.TabIndex = 27;
			this.PlusMinusButton.Text = "±";
			this.PlusMinusButton.Click += new System.EventHandler(this.PlusMinusButton_Click);
			// 
			// Extra1CalcEdit
			// 
			this.Extra1CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 56, true);
			this.Extra1CalcEdit.Name = "Extra1CalcEdit";
			this.Extra1CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.Extra1CalcEdit.TabIndex = 28;
			this.Extra1CalcEdit.Text = "0.00";
			this.Extra1CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// Extra2CalcEdit
			// 
			this.Extra2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 100, true);
			this.Extra2CalcEdit.Name = "Extra2CalcEdit";
			this.Extra2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.Extra2CalcEdit.TabIndex = 29;
			this.Extra2CalcEdit.Text = "0.00";
			this.Extra2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// Extra1Label
			// 
			this.Extra1Label.AutoSize = true;
			this.Extra1Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 40, true);
			this.Extra1Label.Name = "Extra1Label";
			this.Extra1Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 16, true);
			this.Extra1Label.TabIndex = 30;
			this.Extra1Label.Text = "Extra1:";
			// 
			// Extra2Label
			// 
			this.Extra2Label.AutoSize = true;
			this.Extra2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 84, true);
			this.Extra2Label.Name = "Extra2Label";
			this.Extra2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 16, true);
			this.Extra2Label.TabIndex = 31;
			this.Extra2Label.Text = "Extra2:";
			// 
			// ExtraCalculateButton
			// 
			this.ExtraCalculateButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExtraCalculateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 133, true);
			this.ExtraCalculateButton.Name = "ExtraCalculateButton";
			this.ExtraCalculateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.ExtraCalculateButton.TabIndex = 32;
			this.ExtraCalculateButton.Text = "Extra1 × Extra2";
			this.ExtraCalculateButton.Click += new System.EventHandler(this.ExtraCalculateButton_Click);
			// 
			// ExtraDividerGroupBox
			// 
			this.ExtraDividerGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right);
			this.ExtraDividerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 45, true);
			this.ExtraDividerGroupBox.Name = "ExtraDividerGroupBox";
			this.ExtraDividerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(6, 107, true);
			this.ExtraDividerGroupBox.TabIndex = 33;
			this.ExtraDividerGroupBox.TabStop = false;
			// 
			// ZCalculator
			// 

			this.CancelButton = this.CancelZButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 207, true);
			this.Controls.Add(this.ExtraDividerGroupBox);
			this.Controls.Add(this.ExtraCalculateButton);
			this.Controls.Add(this.Extra2Label);
			this.Controls.Add(this.Extra1Label);
			this.Controls.Add(this.Extra2CalcEdit);
			this.Controls.Add(this.Extra1CalcEdit);
			this.Controls.Add(this.OperatorTextBox);
			this.Controls.Add(this.ValueTextBox);
			this.Controls.Add(this.PlusMinusButton);
			this.Controls.Add(this.Number00Button);
			this.Controls.Add(this.PercentageButton);
			this.Controls.Add(this.ClearButton);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.AcceptZButton);
			this.Controls.Add(this.CancelZButton);
			this.Controls.Add(this.SubtractButton);
			this.Controls.Add(this.MultiplyButton);
			this.Controls.Add(this.DivideButton);
			this.Controls.Add(this.Number0Button);
			this.Controls.Add(this.Number9Button);
			this.Controls.Add(this.Number8Button);
			this.Controls.Add(this.Number7Button);
			this.Controls.Add(this.Number6Button);
			this.Controls.Add(this.Number5Button);
			this.Controls.Add(this.Number4Button);
			this.Controls.Add(this.Number3Button);
			this.Controls.Add(this.Number1Button);
			this.Controls.Add(this.AddButton);
			this.Controls.Add(this.Number2Button);
			this.Controls.Add(this.DecimalButton);
			this.Controls.Add(this.EqualsButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.Name = "ZCalculator";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.TopMost = true;
			this.ResumeLayout(false);
		}
		#endregion
	}
}
