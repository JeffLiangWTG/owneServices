using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Application;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Registry.GUI.HotSpotPictureBox;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class DocBuilderThemeRegistryControl : RegistryZUserControl
	{
		DocBuilderThemeRegistry themeRegistry;

		public DocBuilderThemeRegistryControl()
		{
			InitializeComponent();

			SetupFontFamilyComboBox();
			SetupPreviewPictureBox();
			SetupEventHandlers();

#if DEBUG
			SerializeButton.Visible = true;
#endif
		}

#if DEBUG
		void SerializeButton_Click(object sender, EventArgs e)
		{
			var stringBuilder = new StringBuilder();
			var settings = new XmlWriterSettings();
			settings.NewLineHandling = NewLineHandling.Entitize;
			settings.Indent = true;
			var xmlWriter = XmlWriter.Create(stringBuilder, settings);
			ThemeRegistry.SelectedTheme.WriteXml(xmlWriter);
			xmlWriter.Close();
			SafeClipboard.SetText(stringBuilder.ToString());
			Globals.Message.Show(string.Format("Theme [{0}] has been serialized as XML to the clipboard.", ThemeRegistry.SelectedTheme.Name), "Serialize Theme to Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information, DialogResult.OK);
		}
#endif

		void SetupPreviewPictureBox()
		{
			var resourceName = "Enterprise.Registry.GUI.RegistryItemEditors.DocBuilderTheme.DocBuilderThemePreviewHotSpots.png";
			using (var stream = GetType().Assembly.GetManifestResourceStream(resourceName))
			{
				PreviewPictureBox.HotSpots.Clear();

				if (stream != null)
				{
					PreviewPictureBox.HotSpots.Add(new HotSpot(DocBuilderThemeItemList.Codes.DocumentHeading, Color.Red));
					PreviewPictureBox.HotSpots.Add(new HotSpot(DocBuilderThemeItemList.Codes.PageNumberHeading, Color.Blue));
					PreviewPictureBox.HotSpots.Add(new HotSpot(DocBuilderThemeItemList.Codes.PrimaryHeading, Color.Lime));
					PreviewPictureBox.HotSpots.Add(new HotSpot(DocBuilderThemeItemList.Codes.PrimaryBody, Color.Yellow));
					PreviewPictureBox.HotSpots.Add(new HotSpot(DocBuilderThemeItemList.Codes.SecondaryHeading, Color.Magenta));
					PreviewPictureBox.HotSpots.Add(new HotSpot(DocBuilderThemeItemList.Codes.SecondaryBody, Color.Cyan));
					PreviewPictureBox.MapBitmap = new Bitmap(stream);
				}
			}
		}

		void PreviewPictureBox_HotSpotClick(object sender, HotSpotEventArgs e)
		{
			var hotSpot = e.HotSpot;
			if (hotSpot != null)
			{
				foreach (var item in ThemeItemComboBox.Items)
				{
					var themeItem = item as DocBuilderThemeItem;
					if (themeItem != null)
					{
						if (themeItem.Name.Equals(hotSpot.Code))
						{
							ThemeItemComboBox.SelectedItem = themeItem;
							break;
						}
					}
				}
			}
		}

		void SetupThemeSelector()
		{
			ThemeComboBox.Items.Clear();

			if (themeRegistry != null)
			{
				foreach (var theme in themeRegistry.Themes)
				{
					ThemeComboBox.Items.Add(theme);
				}

				if (ThemeRegistry.SelectedTheme != null)
				{
					ThemeComboBox.SelectedItem = ThemeRegistry.SelectedTheme;
				}
				else
				{
					ThemeComboBox.SelectedIndex = 0;
				}
			}
		}

		void SetupFontFamilyComboBox()
		{
			FontFamilyComboBox.Items.Clear();

			foreach (var fontFamily in FontFamily.Families)
			{
				FontFamilyComboBox.Items.Add(fontFamily.Name);
			}
		}

		public DocBuilderThemeRegistry ThemeRegistry
		{
			get { return themeRegistry; }
			set
			{
				if (value != themeRegistry)
				{
					themeRegistry = value;
					SetDataBinding(themeRegistry, string.Empty);
					SetupThemeSelector();
				}
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			ThemeComboBox.Enabled = !readOnly;
			CustomizeGroupBox.Enabled = !readOnly;
			CopyButton.Enabled = !readOnly;
			NewButton.Enabled = !readOnly;
			RemoveButton.Enabled = !readOnly;
		}

		void ThemeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			var theme = ThemeComboBox.SelectedItem as DocBuilderTheme;

			ThemeRegistry.SelectedTheme = theme;
			ThemeItemComboBox.Items.Clear();

			if (theme != null)
			{
				foreach (var themeItem in theme.Items)
				{
					ThemeItemComboBox.Items.Add(themeItem);
				}

				ThemeItemComboBox.SelectedIndex = 0;
			}
		}

		void ThemeItemComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			RefreshThemeItemControls();
		}

		void RefreshThemeItemControls()
		{
			var theme = ThemeComboBox.SelectedItem as DocBuilderTheme;
			var themeItem = ThemeItemComboBox.SelectedItem as DocBuilderThemeItem;
			if (theme != null && themeItem != null)
			{
				SuspendLayout();

				FillButton.BackColor = themeItem.Color1;
				BorderButton.BackColor = themeItem.Color2;

				FontColorButton.BackColor = themeItem.FontColor;
				FontFamilyComboBox.SelectedIndex = FontFamilyComboBox.FindString(themeItem.Font.Name);
				FontSizeNumericUpDown.Value = Convert.ToDecimal(themeItem.FontSize);
				BoldCheckBox.Checked = themeItem.FontBold;
				ItalicCheckBox.Checked = themeItem.FontItalic;

				FillButton.Enabled = theme.IsCustomizable && !themeItem.Color1ArgbInfo.ReadOnly;
				BorderButton.Enabled = theme.IsCustomizable && !themeItem.Color2ArgbInfo.ReadOnly;

				FontFamilyComboBox.Enabled = theme.IsCustomizable && !themeItem.FontNameInfo.ReadOnly;
				FontColorButton.Enabled = theme.IsCustomizable && !themeItem.FontColorArgbInfo.ReadOnly;
				FontSizeNumericUpDown.Enabled = theme.IsCustomizable && !themeItem.FontSizeInfo.ReadOnly;
				BoldCheckBox.Enabled = theme.IsCustomizable && !themeItem.FontBoldInfo.ReadOnly;
				ItalicCheckBox.Enabled = theme.IsCustomizable && !themeItem.FontItalicInfo.ReadOnly;

				FillButton.Visible = !themeItem.Color1ArgbInfo.ReadOnly;
				BorderButton.Visible = !themeItem.Color2ArgbInfo.ReadOnly;
				FontFamilyComboBox.Visible = !themeItem.FontNameInfo.ReadOnly;
				FontColorButton.Visible = !themeItem.FontColorArgbInfo.ReadOnly;
				FontSizeNumericUpDown.Visible = !themeItem.FontSizeInfo.ReadOnly;
				BoldCheckBox.Visible = !themeItem.FontBoldInfo.ReadOnly;
				ItalicCheckBox.Visible = !themeItem.FontItalicInfo.ReadOnly;

				FillLabel.Visible = FillButton.Visible;
				BorderLabel.Visible = BorderButton.Visible;
				FontLabel.Visible = FontFamilyComboBox.Visible;
				FontColorLabel.Visible = FontColorButton.Visible;
				SizeLabel.Visible = FontSizeNumericUpDown.Visible;

				RemoveButton.Visible = theme.IsCustomizable;

				PreviewPictureBox.Image = ObjectFactory.Get<IThemePreviewProvider>().GetPreview(theme);

				ResumeLayout(false);
			}
		}

		void Color1Button_Click(object sender, EventArgs e)
		{
			var themeItem = ThemeItemComboBox.SelectedItem as DocBuilderThemeItem;
			if (themeItem != null)
			{
				var dialog = new ColorDialog();
				dialog.Color = themeItem.Color1;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					themeItem.Color1 = dialog.Color;
					RefreshThemeItemControls();
				}
			}
		}

		void Color2Button_Click(object sender, EventArgs e)
		{
			var themeItem = ThemeItemComboBox.SelectedItem as DocBuilderThemeItem;
			if (themeItem != null)
			{
				var dialog = new ColorDialog();
				dialog.Color = themeItem.Color2;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					themeItem.Color2 = dialog.Color;
					RefreshThemeItemControls();
				}
			}
		}

		void SetupEventHandlers()
		{
			ThemeComboBox.SelectedIndexChanged += new EventHandler(ThemeComboBox_SelectedIndexChanged);
			ThemeItemComboBox.SelectedIndexChanged += new EventHandler(ThemeItemComboBox_SelectedIndexChanged);
			PreviewPictureBox.HotSpotClick += new HotSpotEventHandler(PreviewPictureBox_HotSpotClick);

#if DEBUG
			SerializeButton.Click += new EventHandler(SerializeButton_Click);
#endif
		}

		void FontColorButton_Click(object sender, EventArgs e)
		{
			var themeItem = ThemeItemComboBox.SelectedItem as DocBuilderThemeItem;
			if (themeItem != null)
			{
				var dialog = new ColorDialog();
				dialog.Color = themeItem.FontColor;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					themeItem.FontColor = dialog.Color;
					RefreshThemeItemControls();
				}
			}
		}

		void FontSizeNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			var themeItem = ThemeItemComboBox.SelectedItem as DocBuilderThemeItem;
			if (themeItem != null)
			{
				themeItem.FontSize = Convert.ToInt32(FontSizeNumericUpDown.Value);
				RefreshThemeItemControls();
			}
		}

		void BoldCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			var themeItem = ThemeItemComboBox.SelectedItem as DocBuilderThemeItem;
			if (themeItem != null)
			{
				themeItem.FontBold = BoldCheckBox.Checked;
				RefreshThemeItemControls();
			}
		}

		void ItalicCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			var themeItem = ThemeItemComboBox.SelectedItem as DocBuilderThemeItem;
			if (themeItem != null)
			{
				themeItem.FontItalic = ItalicCheckBox.Checked;
				RefreshThemeItemControls();
			}
		}

		void FontFamilyComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			var themeItem = ThemeItemComboBox.SelectedItem as DocBuilderThemeItem;
			if (themeItem != null)
			{
				themeItem.FontName = FontFamilyComboBox.SelectedItem.ToString();
				RefreshThemeItemControls();
			}
		}

		void CreateNewTheme(UserResponseArgument args, DocBuilderTheme sourceTheme)
		{
			var name = Globals.Message.QueryUserResponse(args);
			if (!String.IsNullOrEmpty(name))
			{
				if (ThemeRegistry.FindTheme(name) == null)
				{
					var theme = new DocBuilderTheme(name, true);
					sourceTheme.CopyThemeItemsTo(theme);
					ThemeRegistry.Themes.Add(theme);
					ThemeRegistry.SelectedTheme = theme;
					SetupThemeSelector();
				}
				else
				{
					Globals.Message.Show(Res.GetString("95f253f9-f937-4cbb-b17f-5dfd12068b64", "Unable to create new theme. The theme [{0}] already exists", name));
				}
			}
		}

		string EnterNewThemeNameMessage
		{
			get { return Res.GetString("6d5ff1e7-4592-42d8-a725-3a4f6262e823", "Please enter a name for the new theme."); }
		}

		void CopyButton_Click(object sender, EventArgs e)
		{
			var args = new UserResponseArgument();
			args.Caption = Res.GetString("f4aa6fef-d5db-4a0f-b8f1-ca0ac4df49af", "Copy Theme");
			args.Message = EnterNewThemeNameMessage;
			args.MinimumResponseLength = 1;
			args.Buttons = ZMessageBoxButtons.OKCancel;
			args.DefaultButton = ZMessageBoxDefaultButton.Button1;

			var sourceTheme = ThemeRegistry.SelectedTheme;
			CreateNewTheme(args, sourceTheme);
		}

		void NewButton_Click(object sender, EventArgs e)
		{
			var args = new UserResponseArgument();
			args.Caption = Res.GetString("b560eae9-4e86-4f38-98be-f641a737f2a6", "New Theme");
			args.Message = EnterNewThemeNameMessage;
			args.MinimumResponseLength = 1;
			args.Buttons = ZMessageBoxButtons.OKCancel;
			args.DefaultButton = ZMessageBoxDefaultButton.Button1;

			var sourceTheme = ThemeRegistry.Themes[0];
			CreateNewTheme(args, sourceTheme);
		}

		void RemoveButton_Click(object sender, EventArgs e)
		{
			var theme = ThemeRegistry.SelectedTheme;
			if (Globals.Message.Show(
				Res.GetString("3ecd383e-3cc8-4459-9931-b59d6111df58", "Are you sure you want to remove theme {0}.", theme.Name),
				Res.GetString("7f8f342b-9328-419f-8504-9e037dbf8abf", "Remove Theme"),
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Warning) == DialogResult.OK)
			{
				if (theme.IsCustomizable)
				{
					ThemeRegistry.Themes.Remove(theme);
					ThemeRegistry.SelectedTheme = ThemeRegistry.Themes[0];
					SetupThemeSelector();
				}
			}
		}
	}
}
