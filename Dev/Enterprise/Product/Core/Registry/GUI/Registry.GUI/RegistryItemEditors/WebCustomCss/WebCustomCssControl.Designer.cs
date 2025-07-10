using System.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class WebCustomCssControl
	{
		Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CssTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UrlComboBox = new CargoWise.Windows.UI.KComboBox();
			this.ExportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.WebCustomsCssCollectionWrapper);
			// 
			// CssTextBox
			// 
			this.CssTextBox.AcceptsReturn = true;
			this.CssTextBox.AcceptsTab = true;
			this.CssTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CssTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CssTextBox.Font = new System.Drawing.Font("Courier New", 8.25F);
			this.CssTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 30, true);
			this.CssTextBox.Multiline = true;
			this.CssTextBox.Name = "CssTextBox";
			this.CssTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.CssTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 238, true);
			this.CssTextBox.TabIndex = 0;
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ImportButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b19bd937-9285-4a1e-abb4-b315b0fcd33f", "Import");
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 274, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 23, true);
			this.ImportButton.TabIndex = 1;
			this.ImportButton.ToolTipCaption = null;
			this.ImportButton.UseVisualStyleBackColor = true;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// UrlComboBox
			// 
			this.UrlComboBox.DisplayMember = "DisplayValue";
			this.UrlComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.UrlComboBox.FormattingEnabled = true;
			this.UrlComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.UrlComboBox.Name = "UrlComboBox";
			this.UrlComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 21, true);
			this.UrlComboBox.TabIndex = 2;
			this.UrlComboBox.SelectedIndexChanged += new System.EventHandler(this.UrlComboBox_SelectedIndexChanged);
			// 
			// ExportButton
			// 
			this.ExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ExportButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e61ba851-846c-4f3a-9d2d-4eb8423ac3f3", "Export");
			this.ExportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 274, true);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ExportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 23, true);
			this.ExportButton.TabIndex = 3;
			this.ExportButton.ToolTipCaption = null;
			this.ExportButton.UseVisualStyleBackColor = true;
			this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
			// 
			// WebCustomCssControl
			// 
			this.Controls.Add(this.ExportButton);
			this.Controls.Add(this.UrlComboBox);
			this.Controls.Add(this.ImportButton);
			this.Controls.Add(this.CssTextBox);
			this.Name = "WebCustomCssControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.ZTextBox CssTextBox;
		internal Enterprise.ZArchitecture.GUI.ZButton ImportButton;
		internal Enterprise.ZArchitecture.GUI.ZButton ExportButton;
		internal CargoWise.Windows.UI.KComboBox UrlComboBox;

		#endregion
	}
}