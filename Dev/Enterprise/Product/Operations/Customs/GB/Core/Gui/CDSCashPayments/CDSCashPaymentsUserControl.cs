using Enterprise.Customs.GB.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.GUI.CDSCashPayments
{
	public partial class CDSCashPaymentsUserControl : ZUserControl
	{
		public CDSCashPaymentsUserControl()
		{
			InitializeComponent();
		}

		void OpenDeclarationButton_Click(object sender, System.EventArgs e)
		{
			var declaration = (DataSource as CusEntryPayInfo)?.EntryHeader?.Declaration;
			if (declaration != null)
			{
				JobDeclarationController.ShowViewForm(declaration);
			}
		}

		public ZController JobDeclarationController => jobDeclarationController ??
			(jobDeclarationController = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration));
		ZController jobDeclarationController;
	}
}
