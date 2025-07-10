using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public partial class ColorThemeSelectorControl : RegistryZUserControl
	{
		public ColorThemeSelectorControl()
		{
			InitializeComponent();
			ThemeDropEdit.SelectedIndexChanged += new EventHandler(ThemeDropEdit_SelectedIndexChanged);
			ThemeDropEdit.DropDownClosed += new EventHandler(ThemeDropEdit_DropDownClosed);
		}

		#region Color Changing

#if DEBUG
		internal
#endif
		void ThemeDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			RefreshColors();
		}

		void ThemeDropEdit_DropDownClosed(object sender, EventArgs e)
		{
			if (this.IsHandleCreated)
			{
				BeginInvoke((() => { RefreshColors(); }));
			}
		}

		void RefreshColors()
		{
			int rowNum = 0;
			for (int i = RowLayoutPanel.Controls.Count - 1; i >= 0; i--)
			{
				RowLayoutPanel.Controls[i].Dispose();
			}

			if (FieldValue != null && FieldValue.ChosenTheme != null)
			{
				foreach (DescribedColor item in FieldValue.ChosenTheme.ChosenColors)
				{
					var colorPickerButton = new Button();
					colorPickerButton.BackColor = item.Color;
					colorPickerButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 0);
					colorPickerButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(colorPickerButton.Width), 18);
					colorPickerButton.Click += new EventHandler(ColorPickerButton_Click);

					var renderer = new LabelCaptionRenderer(colorPickerButton);
					renderer.Caption = item.Usage;
					colorPickerButton.Tag = item.ColorPropertyName;

					RowLayoutPanel.Controls.Add(colorPickerButton);
					RowLayoutPanel.SetRow(colorPickerButton, rowNum);

					rowNum++;
				}
			}
		}

		#endregion

		#region Copy Colors

		void CopyColorsButton_Click(object sender, EventArgs e)
		{
			if (FieldValue.ChosenTheme != null)
			{
				if (FieldValue.ChosenTheme.CanBeModified)
				{
					if (FieldValue.ColorThemeList[FieldValue.ColorThemeToCopy] != null)
					{
						FieldValue.SetDefaultsFromTheme();
						RefreshColors();
					}
					else
					{
						Globals.Message.Show(Res.GetString("c50f6163-1dab-488f-b785-7abe882817f9", "Please select a valid theme to copy from."));
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("103aa4eb-2cd9-44fd-9214-f19c5b255939", "You can only copy to a Custom Theme."));
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("c76e5316-49cb-45ea-bab5-872d63162765", "Please select a theme to copy to."));
			}
		}

		#endregion

		#region Color Picker

		void ColorPickerButton_Click(object sender, EventArgs e)
		{
			CustomColorTheme customTheme = Data.ChosenTheme as CustomColorTheme;
			if (Data.ChosenTheme != null)
			{
				if (Data.ChosenTheme.CanBeModified && customTheme != null)
				{
					Button button = (Button)sender;
					Color result = GetColorDialogResponse(button.BackColor);
					if (result.ToArgb() != 0)
					{
						button.BackColor = result;
						customTheme.UpdateColor(button.Tag.ToString(), result);
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("3074b9df-560c-46cd-bddf-1d6bf0ff09f4", "The {0} theme is system defined and cannot be changed. Please pick a Custom theme if you wish to set colors yourself.", Data.ChosenTheme.Name));
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("be06e6ea-c98b-4c3a-9b2b-ce0f0eff9e2d", "Please choose a valid theme."));
			}
		}

#if DEBUG
		protected virtual
#endif
		Color GetColorDialogResponse(Color initialColor)
		{
			ColorDialog dialog = new ColorDialog();
			dialog.Color = initialColor;
			dialog.FullOpen = true;
			DialogResult result = dialog.ShowDialog();
			return result == DialogResult.OK && dialog.Color.ToArgb() != 0 ? Color.FromArgb(255, dialog.Color) : Color.Empty; //ensure opacity
		}

		#endregion

		#region Binding

		internal ColorThemeSelector FieldValue
		{
			get { return Data; }
			set
			{
				ColorThemeSelector oldValue = Data;
				Data = value ?? new ColorThemeSelector();
				SetDataBinding(Data, "");
				if (oldValue == null)
				{
					RefreshColors();
				}
			}
		}

		ColorThemeSelector Data;

		#endregion

		#region ReadOnly

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ThemeDropEdit.Enabled = !readOnly;
			MainGroupBox.Enabled = !readOnly;
		}

		#endregion
	}
}
