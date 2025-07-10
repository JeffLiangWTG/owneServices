using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	internal partial class ZDataSetListBoxWithGrid 
	{
		DataTableGridView gridView;
		ZListBox listBox;

		void InitializeComponent()
		{
			this.gridView = new DataTableGridView();
			this.listBox = new ZListBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
			this.SuspendLayout();
			// 
			// GridView
			// 
			this.gridView.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.gridView.DataMember = "";
			this.gridView.Name = "gridView";
			this.gridView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 0, true);
			this.gridView.TabIndex = 0;
			this.gridView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 316, true);
			this.gridView.AllowUserToDeleteRows = false;
			this.gridView.MouseClick += TableGridViewOnMouseClick;
			// 
			// listBox
			// 
			this.listBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left);
			this.listBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.listBox.Name = "listBox";
			this.listBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 316, true);
			this.listBox.Sorted = true;
			this.listBox.TabIndex = 2;
			// 
			// ZDataSetListBoxWithGrid
			// 
			this.Controls.Add(this.listBox);
			this.Controls.Add(this.gridView);
			this.Name = "ZDataSetListBoxWithGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 324, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
