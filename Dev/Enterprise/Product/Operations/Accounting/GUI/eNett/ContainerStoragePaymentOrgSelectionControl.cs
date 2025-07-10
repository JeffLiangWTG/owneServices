using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.eNett;
using Enterprise.Accounting.GUI.PeriodManagement;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.eNett
{
	public partial class ContainerStoragePaymentOrgSelectionControl : ZUserControl
	{
		public ContainerStoragePaymentOrgSelectionControl()
		{
			InitializeComponent();
			ContextMenu menu = OrganisationGrid.ContextMenu.GetContextMenu();
			menu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("16bb2ce3-4db3-46da-9de2-a933caef82f8", "Allocate Organization"), new EventHandler(ComPayGrid_AllocateOrganisation)));
		}

		public event EventHandler CloseParentForm;

		new ComPayRegisteredOrganisationDataSource DataSource
		{
			get { return (ComPayRegisteredOrganisationDataSource)base.DataSource; }
		}

		void FindButton_Click(object sender, EventArgs e)
		{
			DataSource.PopulateCollection();
			OrganisationGrid.Refresh();
		}

		void ComPayGrid_AllocateOrganisation(object sender, EventArgs e)
		{
			if (OrganisationGrid.SelectedElements != null && OrganisationGrid.SelectedElements.Length == 1)
			{
				AllocateToOrganisationForm form = new AllocateToOrganisationForm((ComPayRegisteredOrganisation)OrganisationGrid.SelectedElements[0]);
				form.Show();
			}
		}

		void ContainerStoragePaymentOrgSelectionControl_Load(object sender, EventArgs e)
		{
			if (DataSource != null)
			{
				DataSource.PopulateCollection();
				OrganisationGrid.Refresh();
			}
		}

		void OrganisationGrid_DoubleClick(object sender, EventArgs e)
		{
			SelectOrganisation();
		}

		void SelectButton_Click(object sender, EventArgs e)
		{
			SelectOrganisation();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			RaiseCloseParentFormEvent();
		}

		void SelectOrganisation()
		{
			if (OrganisationGrid.SelectedElements.Length > 0)
			{
				DataSource.SelectedOrganisation = (ComPayRegisteredOrganisation)OrganisationGrid.SelectedElements[0];
				if (CloseParentForm != null)
				{
					CloseParentForm(null, new EventArgs());
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("7670f3bb-3ab9-4aaf-9c52-f19e07c2f2a0", "Please select an organization."));
			}
		}

		void RaiseCloseParentFormEvent()
		{
			if (CloseParentForm != null)
			{
				CloseParentForm(null, new EventArgs());
			}
		}
	}
}
