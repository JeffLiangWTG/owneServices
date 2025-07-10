using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[SuppressFormDesignerAnalysis]
	sealed class ZFormWithBoundGridOnTabPage : ZForm
	{
		private ZTabControl TabControl;
		private ZTabPage InitiallyBoundTabPage;
		private ZTabPage LazyBoundTabPage;
		public ZGrid Grid;
		private readonly System.ComponentModel.Container components;

		public ZFormWithBoundGridOnTabPage()
		{
		}

		public ZFormWithBoundGridOnTabPage(DummyWithDependentsBusinessObject businessEntity)
			: base(businessEntity)
		{
		}

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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			this.TabControl = new ZTabControl();
			this.InitiallyBoundTabPage = new ZTabPage();
			this.LazyBoundTabPage = new ZTabPage();
			this.Grid = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.TabControl.SuspendLayout();
			this.LazyBoundTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = new System.Drawing.Point(0, 242);
			this.MainStatusBar.Name = "MainStatusBar";
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.InitiallyBoundTabPage);
			this.TabControl.Controls.Add(this.LazyBoundTabPage);
			this.TabControl.Location = new System.Drawing.Point(16, 16);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = new System.Drawing.Size(256, 200);
			this.TabControl.TabIndex = 1;
			// 
			// InitiallyBoundTabPage
			// 
			this.InitiallyBoundTabPage.CheckForNotifications = true;
			this.InitiallyBoundTabPage.Location = new System.Drawing.Point(4, 23);
			this.InitiallyBoundTabPage.Name = "InitiallyBoundTabPage";
			this.InitiallyBoundTabPage.Size = new System.Drawing.Size(248, 173);
			this.InitiallyBoundTabPage.TabIndex = 0;
			this.InitiallyBoundTabPage.Text = "InitiallyBoundTabPage";
			// 
			// LazyBoundTabPage
			// 
			this.LazyBoundTabPage.CheckForNotifications = true;
			this.LazyBoundTabPage.Controls.Add(this.Grid);
			this.LazyBoundTabPage.Location = new System.Drawing.Point(4, 23);
			this.LazyBoundTabPage.Name = "LazyBoundTabPage";
			this.LazyBoundTabPage.Size = new System.Drawing.Size(248, 173);
			this.LazyBoundTabPage.TabIndex = 1;
			this.LazyBoundTabPage.Text = "LazyBoundTabPage";
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.Grid.BindTo = "Dependents";
			this.Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "DummyDependent.ZD1_Code";
			zTextBoxColumnStyleInfo1.ColumnName = "ZD1_Code";
			zTextBoxColumnStyleInfo1.Width = 150;
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.EnableToolTips = false;
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "Grid";
			this.Grid.Location = new System.Drawing.Point(16, 16);
			this.Grid.Name = "Grid";
			this.Grid.Size = new System.Drawing.Size(216, 144);
			this.Grid.TabIndex = 0;
			// 
			// ZFormWithBoundGridOnTabPage
			// 

			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.TabControl);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Testing.DummyWithDependents";
			this.Name = "ZFormWithBoundGridOnTabPage";
			this.Text = "ZFormWithBoundGridOnTabPage";
			this.Controls.SetChildIndex(this.TabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.LazyBoundTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
