using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.GUI.Ccsuk;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.GUI.ChiefExportConsolIntegration
{
	public partial class ChiefExportConsolIntegrationUserControl : ZUserControl
	{
		CustomsExportConsolIntegrationWrapper ConsolIntegrationWrapper { get; set; }

		public ChiefExportConsolIntegrationUserControl(CustomsExportConsolIntegrationWrapper chiefExportConsolIntegrationWrapper)
		{
			InitializeComponent();
			this.MawbExportAddInfoUserControl.Load += MawbExportAddInfoUserControl_Load;
			ConsolIntegrationWrapper = MawbExportAddInfoUserControl.ConsolWrapper = chiefExportConsolIntegrationWrapper;
		}

		void MawbExportAddInfoUserControl_Load(object sender, System.EventArgs e)
		{
			ConsolIntegrationWrapper.CalculateMasterUCR();
		}

		void MessagesGrid_DoubleClick(object sender, System.EventArgs e)
		{
			OpenDeclarationFormInEditMode();
		}

		void OpenDeclarationFormInEditMode()
		{
			var helper = new DependentObjectControllerHelper(new DependentFormPresenter());
			if (DeclarationMessagesGrid.SelectedElements.Length == 1)
			{
				var message = DeclarationMessagesGrid.SelectedElements[0] as ChiefExportConsolIntegrationNPBO;
				if (message != null && message.Entry != null && message.Entry.Declaration != null)
				{
					if (message.Entry.Declaration.Shipment != null)
					{
						helper.EditExisting(message.Entry.Declaration, ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
					}
					else
					{
						helper.EditExisting(message.Entry.Declaration, ControllerIDs.Customs.JobDeclaration);
					}
				}
			}
		}
	}
}
