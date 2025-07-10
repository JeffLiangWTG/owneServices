using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class JobManagementAmountFilterControl : ZUserControl
	{
		public JobManagementAmountFilterControl()
		{
			InitializeComponent();
		}

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

