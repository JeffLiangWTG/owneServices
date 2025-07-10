namespace Enterprise.Customs.DE.GUI
{
	partial class CHGTSTREGDeclarationUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
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
			this.REGLineGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NewLocationOfGoodsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.REGLineGroupBox.SuspendLayout();
			this.NewLocationOfGoodsDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// REGLineGroupBox
			// 
			this.REGLineGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("23651125-8207-4183-a106-296ad9d8c301", "Line Details");
			this.REGLineGroupBox.Controls.Add(this.NewLocationOfGoodsDropEdit);
			this.REGLineGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.REGLineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.REGLineGroupBox.Name = "REGLineGroupBox";
			this.REGLineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 56, true);
			this.REGLineGroupBox.TabIndex = 1;
			this.REGLineGroupBox.TabStop = false;
			// 
			// NewLocationOfGoodsDropEdit
			// 
			this.NewLocationOfGoodsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewLocationOfGoodsDropEdit, "CHGTSTCusTempStorageDecs.CusTempStorageLines.TSL_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_LocationOfGoods)));
			this.NewLocationOfGoodsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 17, true);
			this.NewLocationOfGoodsDropEdit.Name = "NewLocationOfGoodsDropEdit";
			this.NewLocationOfGoodsDropEdit.ShouldResizeByMaxLength = true;
			this.NewLocationOfGoodsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.NewLocationOfGoodsDropEdit.TabIndex = 3;
			// 
			// CHGTSTREGDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.REGLineGroupBox);
			this.Name = "CHGTSTREGDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.REGLineGroupBox.ResumeLayout(false);
			this.REGLineGroupBox.PerformLayout();
			this.NewLocationOfGoodsDropEdit.ResumeLayout(true);
			this.NewLocationOfGoodsDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox REGLineGroupBox;
		private ZArchitecture.GUI.ZDropEdit NewLocationOfGoodsDropEdit;
	}
}
