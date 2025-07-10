using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class BMSystemSchematicForm : ZChildForm
	{
		public BMSystemSchematicForm(BMSystem system, SchematicDrawMode schematicDrawMode)
			: base(system)
		{
			Argument.NotNull(system, "system");
			this.schematicDrawMode = schematicDrawMode;

			InitializeComponent();

			TitleLabel.Text = Res.GetString("38ef877d-9a61-47ef-914c-04b2fc8634a6", "System Schematic for {0}", system.FS_Name);
			TitleLabel.Font = new Font("Arial", 22, FontStyle.Bold);
			LegendLabel.Font = new Font("Arial", 16, FontStyle.Bold);

			SetupMenuAndScrolling(schematicPictureBox);
			SetupMenuAndScrolling(legendPictureBox);

			DrawSchematic();
			DrawLegend();
		}

		readonly SchematicDrawMode schematicDrawMode;

		public new BMSystem BusinessEntity
		{
			get { return (BMSystem)base.BusinessEntity; }
		}

		#region Implementation

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void SetupMenuAndScrolling(PictureBox pictureBox)
		{
			var menu = new ContextMenu();
			var menuItem = new ZMenuItem();
			menuItem.Caption = ResString.GetMultilingualString("7ba9eed4-839d-41f5-8ed7-059f2e297749", "&Copy to Clipboard");
			menuItem.Click += (sender, args) => SafeClipboard.SetData(DataFormats.Bitmap, pictureBox.Image);
			menu.MenuItems.Add(menuItem);
			pictureBox.Parent.ContextMenu = menu;

			pictureBox.MouseEnter += (sender, args) => pictureBox.Parent.Focus();
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		void RefreshButton_Click(object sender, System.EventArgs e)
		{
			DrawSchematic();
		}

		void IncludeNonPrimaryPathCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			DrawSchematic();
		}

		void LegendButton_Click(object sender, System.EventArgs e)
		{
			MainSplitContainer.Panel2Collapsed = !MainSplitContainer.Panel2Collapsed;
		}

		void DrawSchematic()
		{
			labelInvalidSchematicPictureBoxImage.Visible = false;
			if (schematicDrawMode == SchematicDrawMode.Graphical)
			{
				if (schematicPictureBox.Image != null)
				{
					schematicPictureBox.Image.Dispose();
				}

				schematicPictureBox.Image = new BufferManagementSystemDrawer().GetSchematicImage(BusinessEntity, IncludeNonPrimaryPathCheckBox.Checked, 7000, 7000);
				if (schematicPictureBox.Image != null)
				{
					schematicPictureBox.BringToFront();
				}
				else
				{
					labelInvalidSchematicPictureBoxImage.Visible = true;
					labelInvalidSchematicPictureBoxImage.BringToFront();
				}
			}
			else if (schematicDrawMode == SchematicDrawMode.Text)
			{
				if (schematicPictureBox != null)
				{
					schematicPictureBox.Dispose();
					schematicPictureBox = null;
					legendPictureBox.Dispose();
					legendPictureBox = null;
				}

				if (textSchematicTextBox == null)
				{
					textSchematicTextBox = new ZTextBox
					{
						CharacterCasing = CharacterCasing.Normal,
						Dock = DockStyle.Fill,
						Multiline = true,
						ReadOnly = true,
						ScrollBars = ScrollBars.Both,
					};

					schematicPanel.Controls.Add(textSchematicTextBox);

					var legendTextBox = new ZTextBox
					{
						Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
						CharacterCasing = CharacterCasing.Normal,
						Multiline = true,
						ReadOnly = true,
						ScrollBars = ScrollBars.Both,
						Text = SystemSchematic.TextLegendSchematic,
					};
					ControlDpiScalingHelper.SetHeight(legendTextBox, legendPanel.Height - LegendLabel.Height, false);
					ControlDpiScalingHelper.SetTop(legendTextBox, LegendLabel.Height + LegendLabel.Top, false);
					ControlDpiScalingHelper.SetWidth(legendTextBox, legendPanel.Width, false);
					legendPanel.Controls.Add(legendTextBox);

					textSchematicTextBox.Font = legendTextBox.Font = new Font("Lucida Console", 8.25f);
				}

				textSchematicTextBox.Text = BusinessEntity.TextSchematic;
			}
		}

		ZTextBox textSchematicTextBox;

		void DrawLegend()
		{
			if (schematicDrawMode == SchematicDrawMode.Graphical)
			{
				legendPictureBox.Image = new BufferManagementSystemDrawer().GetLegendImage();
			}
		}

		#endregion
	}
}
