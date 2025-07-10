using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class ImportSiscomexOfficesUserControl : ZUserControl
	{
		public ImportSiscomexOfficesUserControl()
		{
			InitializeComponent();
		}

		void WarehouseAreaIDButton_Click(object sender, System.EventArgs e)
		{
			var declaration = DataSource as JobDeclaration;
			if (declaration != null)
			{
				WarehouseAreaIDsForm.ShowDialog(declaration);
				declaration.WarehouseAreasConcatenatedInfo.RefreshBinding();
			}
		}
	}
}
