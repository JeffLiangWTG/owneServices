using System;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed partial class ZFormWithMultipleSplitters
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.zTabControl1 = new ZTabControl();
			this.zTabPage1 = new ZTabPage();
			this.zTabPage2 = new ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zTabControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 458, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 24, true);
			// 
			// zTabControl1
			// 
			this.zTabControl1.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left);
			this.zTabControl1.Controls.Add(this.zTabPage1);
			this.zTabControl1.Controls.Add(this.zTabPage2);
			this.zTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 458, true);
			this.zTabControl1.TabIndex = 1;
			// 
			// zTabPage1
			// 
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(526, 431, true);
			this.zTabPage1.TabIndex = 0;
			this.zTabPage1.Text = "zTabPage1";
			this.zTabPage1.UseVisualStyleBackColor = true;
			this.zTabPage1.RunWhenBindingOrFirstShown(new EventHandler(this.zTabPage1_InitializeTab));
			// 
			// zTabPage2
			// 
			this.zTabPage2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage2.Name = "zTabPage2";
			this.zTabPage2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(526, 411, true);
			this.zTabPage2.TabIndex = 1;
			this.zTabPage2.Text = "zTabPage2";
			this.zTabPage2.UseVisualStyleBackColor = true;
			this.zTabPage2.RunWhenBindingOrFirstShown(new EventHandler(this.zTabPage2_InitializeTab));
			// 
			// ZFormWithMultipleSplitters
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 482, true);
			this.Controls.Add(this.zTabControl1);
			this.Name = "ZFormWithMultipleSplitters";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "ZFormWithMultipleSplitters";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zTabControl1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		void zTabPage1_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.splitContainerHavingSubContainers = new CargoWise.Windows.UI.KSplitContainer();
			this.kSplitter1 = new CargoWise.Windows.UI.KSplitter();
			this.zLabel1 = new ZLabel();
			this.splitContainerInsideAnotherContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.zTabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerHavingSubContainers)).BeginInit();
			this.splitContainerHavingSubContainers.Panel1.SuspendLayout();
			this.splitContainerHavingSubContainers.Panel2.SuspendLayout();
			this.splitContainerHavingSubContainers.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerInsideAnotherContainer)).BeginInit();
			this.splitContainerInsideAnotherContainer.SuspendLayout();
			this.zTabPage1.Controls.Add(this.splitContainerHavingSubContainers);
			// 
			// splitContainerHavingSubContainers
			// 
			this.splitContainerHavingSubContainers.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerHavingSubContainers.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.splitContainerHavingSubContainers.Name = "splitContainerHavingSubContainers";
			// 
			// splitContainerHavingSubContainers.Panel1
			// 
			this.splitContainerHavingSubContainers.Panel1.Controls.Add(this.kSplitter1);
			this.splitContainerHavingSubContainers.Panel1.Controls.Add(this.zLabel1);
			// 
			// splitContainerHavingSubContainers.Panel2
			// 
			this.splitContainerHavingSubContainers.Panel2.Controls.Add(this.splitContainerInsideAnotherContainer);
			this.splitContainerHavingSubContainers.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 425, true);
			this.splitContainerHavingSubContainers.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(173);
			this.splitContainerHavingSubContainers.TabIndex = 0;
			// 
			// kSplitter1
			// 
			this.kSplitter1.Dock = System.Windows.Forms.DockStyle.Top;
			this.kSplitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.kSplitter1.Name = "kSplitter1";
			this.kSplitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 14, true);
			this.kSplitter1.TabIndex = 2;
			this.kSplitter1.TabStop = false;
			// 
			// zLabel1
			// 
			this.zLabel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zLabel1.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 23, true);
			this.zLabel1.TabIndex = 1;
			this.zLabel1.Text = "zLabel1";
			// 
			// splitContainerInsideAnotherContainer
			// 
			this.splitContainerInsideAnotherContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerInsideAnotherContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerInsideAnotherContainer.Name = "splitContainerInsideAnotherContainer";
			this.splitContainerInsideAnotherContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 425, true);
			this.splitContainerInsideAnotherContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(163);
			this.splitContainerInsideAnotherContainer.TabIndex = 0;
			this.zTabPage1.PerformLayout();
			this.splitContainerHavingSubContainers.Panel1.ResumeLayout(false);
			this.splitContainerHavingSubContainers.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerHavingSubContainers)).EndInit();
			this.splitContainerHavingSubContainers.ResumeLayout(false);
			this.splitContainerHavingSubContainers.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerInsideAnotherContainer)).EndInit();
			this.splitContainerInsideAnotherContainer.ResumeLayout(false);
			this.splitContainerInsideAnotherContainer.PerformLayout();
			this.zTabPage1.ResumeLayout(true);
		}

		void zTabPage2_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.kSplitContainer3 = new CargoWise.Windows.UI.KSplitContainer();
			this.kSplitContainer4 = new CargoWise.Windows.UI.KSplitContainer();
			this.zTabPage2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer3)).BeginInit();
			this.kSplitContainer3.Panel2.SuspendLayout();
			this.kSplitContainer3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer4)).BeginInit();
			this.kSplitContainer4.SuspendLayout();
			this.zTabPage2.Controls.Add(this.kSplitContainer3);
			// 
			// kSplitContainer3
			// 
			this.kSplitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.kSplitContainer3.Name = "kSplitContainer3";
			// 
			// kSplitContainer3.Panel2
			// 
			this.kSplitContainer3.Panel2.Controls.Add(this.kSplitContainer4);
			this.kSplitContainer3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 385, true);
			this.kSplitContainer3.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(173);
			this.kSplitContainer3.TabIndex = 0;
			// 
			// kSplitContainer4
			// 
			this.kSplitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer4.IsSplitterFixed = true;
			this.kSplitContainer4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kSplitContainer4.Name = "kSplitContainer4";
			this.kSplitContainer4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 385, true);
			this.kSplitContainer4.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
			this.kSplitContainer4.TabIndex = 0;
			this.zTabPage2.PerformLayout();
			this.kSplitContainer3.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer3)).EndInit();
			this.kSplitContainer3.ResumeLayout(false);
			this.kSplitContainer3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer4)).EndInit();
			this.kSplitContainer4.ResumeLayout(false);
			this.kSplitContainer4.PerformLayout();
			this.zTabPage2.ResumeLayout(true);
		}
	}
}
