using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class WebThemeControl : RegistryZUserControl
	{
		internal ZDropEdit ThemeDropEdit;
		internal CargoWise.Windows.UI.KComboBox UrlComboBox;

		void InitializeComponent()
		{
			this.UrlComboBox = new CargoWise.Windows.UI.KComboBox();
			this.ThemeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// UrlComboBox
			// 
			this.UrlComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.UrlComboBox.FormattingEnabled = true;
			this.UrlComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.UrlComboBox.Name = "UrlComboBox";
			this.UrlComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 21, true);
			this.UrlComboBox.TabIndex = 2;
			this.UrlComboBox.SelectedIndexChanged += new System.EventHandler(this.UrlComboBox_SelectedIndexChanged);
			// 
			// ThemeDropEdit
			// 
			this.ThemeDropEdit.AllowDrop = true;
			this.ThemeDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b985108a-3cd1-46d0-87a4-2318ee1b8d6b", "Theme");
			this.ThemeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 31, true);
			this.ThemeDropEdit.Name = "ThemeDropEdit";
			this.ThemeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ThemeDropEdit.TabIndex = 3;
			this.ThemeDropEdit.SelectedIndexChanged += new System.EventHandler(this.ThemeDropEdit_SelectedIndexChanged);
			// 
			// WebThemeControl
			// 
			this.Controls.Add(this.ThemeDropEdit);
			this.Controls.Add(this.UrlComboBox);
			this.CaptionRenderingEnabled = true;
			this.Name = "WebThemeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

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
	}
}
