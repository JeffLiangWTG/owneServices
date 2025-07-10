using System;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed partial class ZFormWithMultipleSplitters : ZForm
	{
		public ZFormWithMultipleSplitters()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SplitterLayoutStrategy.RestoreSplittersLayoutCore(this);
		}

		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components;

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

		public ZTabControl zTabControl1;
		ZTabPage zTabPage1;
		ZTabPage zTabPage2;
		ZLabel zLabel1;
		public CargoWise.Windows.UI.KSplitContainer splitContainerHavingSubContainers;
		public CargoWise.Windows.UI.KSplitContainer splitContainerInsideAnotherContainer;
		public CargoWise.Windows.UI.KSplitContainer kSplitContainer3;
		public CargoWise.Windows.UI.KSplitContainer kSplitContainer4;
		public CargoWise.Windows.UI.KSplitter kSplitter1;
	}
}
