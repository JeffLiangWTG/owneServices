namespace Enterprise.ZArchitecture.GUI
{
	public partial class FilterGroupNameForm : ZChildForm
	{
		public FilterGroupNameForm()
		{
			InitializeComponent();
		}

		internal ZTextBox SaveFiltersTextBox;
		protected ZButton CloseButton;
		protected ZButton SaveButton;

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
