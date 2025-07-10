using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.DevTools
{
	[SuppressFormDesignerAnalysis]
	public class DeveloperDiagnosticsForm : KForm
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		public DeveloperDiagnosticsForm(KForm form, IEnumerable<IDevTool> tools)
		{
			var toolButtons = new List<Button>();

			foreach (var tool in tools)
			{
				if (tool.AddAsButton)
				{
					var toolButton = NewButton(tool.Name, toolButton_Click);
					toolButton.Tag = tool;

					toolButtons.Add(toolButton);
				}
				else
				{
					RadioButton radioButton = new ZRadioButton();
					radioButton.Text = tool.Name;
					radioButton.Tag = tool;

					radioButtons.Add(radioButton);
				}
			}

			var showButton = NewButton("Show Form", showButton_Click);
			var closeButton = NewButton("Close", closeButton_Click);

			toolButtons.Add(showButton);
			toolButtons.Add(closeButton);

			var bottomPanel = NewPanelWithButtons(toolButtons.ToArray());
			bottomPanel.Dock = DockStyle.Bottom;
			bottomPanel.TabIndex = 2;

			var box = NewGroupBoxWithRadioButtons(bottomPanel.Width, radioButtons);
			box.Dock = DockStyle.Fill;
			box.TabIndex = 1;

			this.Text = "Developer Diagnostics";
			this.FormBorderStyle = FormBorderStyle.FixedDialog;

			this.Size = this.SizeFromClientSize(ControlDpiScalingHelper.NewScaledSize(
				Math.Max(ControlDpiScalingHelper.ScaleToCurrentDpiX(250), box.Width) + ControlDpiScalingHelper.ScaleToCurrentDpiX(10),
				box.Height + bottomPanel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(10), false));
			this.Padding = ControlDpiScalingHelper.NewScaledPadding(5);
			this.MinimumSize = this.Size;
			this.CancelButton = closeButton;
			this.AcceptButton = showButton;
			this.Controls.Add(box);
			this.Controls.Add(bottomPanel);

			this.form = form;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (radioButtons.Count > 0)
			{
				radioButtons[0].Checked = true;
				ActiveControl = radioButtons[0];
			}
		}

		IDevTool FindTool()
		{
			foreach (var button in radioButtons)
			{
				if (button.Checked)
				{
					return (IDevTool)button.Tag;
				}
			}

			return null;
		}

		static Button NewButton(string text, EventHandler handler)
		{
			var result = new Button();
			result.Text = text;

			int width;

			using (var g = result.CreateGraphics())
			{
				width = (int)Math.Truncate(0.9 + g.MeasureString(result.Text, result.Font).Width);
			}

			result.Size = ControlDpiScalingHelper.NewScaledSize(
				Math.Max(width + ControlDpiScalingHelper.ScaleToCurrentDpiX(10),
				ControlDpiScalingHelper.ScaleToCurrentDpiX(75)),
				ControlDpiScalingHelper.ScaleToCurrentDpiY(21), false);
			result.Click += handler;
			return result;
		}

		static Panel NewPanelWithButtons(Button[] toolButtons)
		{
			var right = ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
			var maxHeight = 0;

			foreach (var button in toolButtons)
			{
				button.Location = ControlDpiScalingHelper.NewScaledPoint(right, ControlDpiScalingHelper.ScaleToCurrentDpiY(5), false);
				button.Anchor = AnchorStyles.Right | AnchorStyles.Top;

				right = button.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
				maxHeight = Math.Max(maxHeight, button.Height);
			}

			var result = new Panel();
			result.Size = ControlDpiScalingHelper.NewScaledSize(right, maxHeight + ControlDpiScalingHelper.ScaleToCurrentDpiY(10), false);
			result.Controls.AddRange(toolButtons);

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
		static GroupBox NewGroupBoxWithRadioButtons(int groupBoxWidth, IEnumerable<RadioButton> radioButtons)
		{
			var bottom = ControlDpiScalingHelper.ScaleToCurrentDpiY(15);
			var width = groupBoxWidth - ControlDpiScalingHelper.ScaleToCurrentDpiX(20);
			var tabIndex = 1;

			GroupBox result = new ZGroupBox { Text = "Forms" };
			ControlDpiScalingHelper.SetWidth(result, groupBoxWidth, false);

			foreach (var radioButton in radioButtons)
			{
				radioButton.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
				radioButton.Size = ControlDpiScalingHelper.NewScaledSize(width, ControlDpiScalingHelper.ScaleToCurrentDpiY(24), false);
				radioButton.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.ScaleToCurrentDpiX(15), bottom, false);
				radioButton.TabIndex = tabIndex++;

				result.Controls.Add(radioButton);
				bottom = radioButton.Bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(2);
			}

			ControlDpiScalingHelper.SetHeight(ref result, bottom + ControlDpiScalingHelper.ScaleToCurrentDpiY(5), false);
			return result;
		}

		void toolButton_Click(object sender, EventArgs e)
		{
			var control = sender as Control;
			IDevTool tool;

			if (control != null && (tool = control.Tag as IDevTool) != null)
			{
				tool.Show(form);
			}
		}

		void closeButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void showButton_Click(object sender, EventArgs e)
		{
			var tool = FindTool();
			if (tool != null)
			{
				tool.Show(form);
			}
		}

		readonly KForm form;
		readonly List<RadioButton> radioButtons = new List<RadioButton>();
	}
}
