#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.MasterFiles.GUI
{
	public abstract partial class ExportProductsToCSVForm : ExportToCSVForm
	{
		public ExportProductsToCSVForm() { }

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
