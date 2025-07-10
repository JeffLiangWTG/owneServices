using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class Z2FindBoxPopupTreeViewForm
	{
		private void InitializeComponent()
		{
			this.TreeView = new ZTreeView();
			this.oButtonOK = new Enterprise.ZArchitecture.GUI.ZButton();
			this.oButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SuspendLayout();
			// 
			// TreeView
			// 
			this.TreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.TreeView.ImageIndex = -1;
			this.TreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.TreeView.Name = "TreeView";
			this.TreeView.SelectedImageIndex = -1;
			this.TreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 360, true);
			this.TreeView.TabIndex = 0;
			this.TreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeView_BeforeExpand);
			this.TreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView_AfterSelect);
			// 
			// oButtonOK
			// 
			this.oButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.oButtonOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 376, true);
			this.oButtonOK.Name = "oButtonOK";
			this.oButtonOK.TabIndex = 1;
			this.oButtonOK.Text = "OK";
			this.oButtonOK.Click += new System.EventHandler(this.oButtonOK_Click);
			// 
			// oButtonCancel
			// 
			this.oButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.oButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.oButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 376, true);
			this.oButtonCancel.Name = "oButtonCancel";
			this.oButtonCancel.TabIndex = 2;
			this.oButtonCancel.Text = "Cancel";
			this.oButtonCancel.Click += new System.EventHandler(this.oButtonCancel_Click);
			// 
			// Z2FindBoxPopupTreeViewForm
			// 
			this.AcceptButton = this.oButtonOK;

			this.CancelButton = this.oButtonCancel;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 414, true);
			this.Controls.Add(this.oButtonCancel);
			this.Controls.Add(this.oButtonOK);
			this.Controls.Add(this.TreeView);
			this.Name = "Z2FindBoxPopupTreeViewForm";
			this.ResumeLayout(false);
		}

		protected internal ZTreeView TreeView;
		private ZButton oButtonOK;
		private ZButton oButtonCancel;
	}
}
