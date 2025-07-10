namespace Enterprise.Registry.GUI
{
	partial class StringArrayControl
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
		void InitializeComponent()
		{
			zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.StringGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StringGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Internal.StringLine);
			// 
			// StringGrid
			// 
			this.StringGrid.AllowNavigation = false;
			this.StringGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.StringGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Internal.StringLine)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.Internal.StringLine)(null)).Value)));
			this.StringGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("StringArrayControl|c8143857-ba83-46a1-98e0-d6156d6b6a95", "Value");
			zTextBoxColumnStyleInfo1.ColumnName = "Value";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.StringGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.StringGrid.CopySelectedRowsAllowed = true;
			this.StringGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StringGrid.GridId = "60b832c3-a168-415f-a9c4-34a5dc2ca275";
			this.StringGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StringGrid.LayoutKey = "StringGrid";
			this.StringGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StringGrid.Name = "StringGrid";
			this.StringGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 145, true);
			this.StringGrid.TabIndex = 0;
			// 
			// StringArrayControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StringGrid);
			this.Name = "StringArrayControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 145, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StringGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid StringGrid;
		internal Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1;
	}
}
