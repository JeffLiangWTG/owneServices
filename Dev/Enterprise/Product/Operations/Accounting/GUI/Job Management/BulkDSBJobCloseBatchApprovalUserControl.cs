using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class BulkDSBJobCloseBatchApprovalUserControl : ZUserControl
	{
		public BulkDSBJobCloseBatchApprovalUserControl()
		{
			InitializeComponent();
		}

		public ZArchitecture.ZGrid JobGrid;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		readonly System.ComponentModel.IContainer components;

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
	}
}

