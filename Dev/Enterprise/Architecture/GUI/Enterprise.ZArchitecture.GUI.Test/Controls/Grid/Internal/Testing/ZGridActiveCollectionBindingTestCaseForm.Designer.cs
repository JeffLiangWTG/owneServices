using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed partial class ZGridActiveCollectionBindingTestCaseForm
	{
		#region Component Designer generated code

		internal ZGrid gridMaster;
		internal ZGrid gridDetail;
		private ZButton btnClose;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		new void InitializeComponent()
		{
			var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			this.gridMaster = new ZGrid();
			this.gridDetail = new ZGrid();
			this.btnClose = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridMaster)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridDetail)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = new System.Drawing.Point(0, 565);
			this.MainStatusBar.Size = new System.Drawing.Size(574, 24);
			// 
			// gridMaster
			// 
			this.gridMaster.AllowNavigation = false;
			this.gridMaster.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.gridMaster.BindTo = ".";
			this.gridMaster.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Code";
			zTextBoxColumnStyleInfo1.ColumnName = "Z0_Code";
			this.gridMaster.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.gridMaster.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridMaster.LayoutKey = "zGrid1";
			this.gridMaster.Location = new System.Drawing.Point(12, 12);
			this.gridMaster.Name = "gridMaster";
			this.gridMaster.Size = new System.Drawing.Size(550, 195);
			this.gridMaster.TabIndex = 0;
			// 
			// gridDetail
			// 
			this.gridDetail.AllowNavigation = false;
			this.gridDetail.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.gridDetail.BindTo = "ActiveDependents";
			this.gridDetail.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.Caption = "Code";
			zTextBoxColumnStyleInfo2.ColumnName = "ZD1_Code";
			this.gridDetail.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.gridDetail.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridDetail.LayoutKey = "zGrid1";
			this.gridDetail.Location = new System.Drawing.Point(12, 213);
			this.gridDetail.Name = "gridDetail";
			this.gridDetail.Size = new System.Drawing.Size(550, 311);
			this.gridDetail.TabIndex = 1;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			// 
			// btnClose
			// 
			this.btnClose.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.btnClose.Location = new System.Drawing.Point(487, 530);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(75, 23);
			this.btnClose.TabIndex = 2;
			this.btnClose.Text = "Close";
			this.btnClose.UseVisualStyleBackColor = true;
			// 
			// ZGridActiveCollectionBindingTestCaseForm
			// 
			this.ClientSize = new System.Drawing.Size(574, 589);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.gridDetail);
			this.Controls.Add(this.gridMaster);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Testing.DummyWithDependents";
			this.Name = "ZGridActiveCollectionBindingTestCaseForm";
			this.Controls.SetChildIndex(this.gridMaster, 0);
			this.Controls.SetChildIndex(this.gridDetail, 0);
			this.Controls.SetChildIndex(this.btnClose, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridMaster)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridDetail)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
